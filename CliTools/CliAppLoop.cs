using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CliMenu;
using CliTools.CliMenuCommands;
using Figgle.Fonts;
using LibParameters;
using LibToolActions.BackgroundTasks;
using SystemToolsShared;

namespace CliTools;

public abstract class CliAppLoop
{
    private readonly string? _header;
    private readonly List<CliMenuSet> _menuSetsList = [];
    private readonly IParametersWithRecentData? _par;
    private readonly IProcesses? _processes;
    private readonly List<CliMenuCommand> _selectedMenuCommandsList = [];
    private int _currentMenuSetLevel;
    private RecentCommands _recentCommands = new();

    // ReSharper disable once ConvertToPrimaryConstructor
    protected CliAppLoop(IParametersWithRecentData? par = null, string? header = null, IProcesses? processes = null)
    {
        _par = par;
        _header = header;
        _processes = processes;
    }

    public abstract CliMenuSet BuildMainMenu();

    private void ShowMenu(bool inFirstTime)
    {
        _menuSetsList[_currentMenuSetLevel].Show(!inFirstTime);
    }

    protected IEnumerable<RecentCommandCliMenuCommand> GetRecentCommands()
    {
        return _recentCommands.Rc.OrderByDescending(x => x.Value)
            .Select(rcKvp => new RecentCommandCliMenuCommand(this, rcKvp.Key));
    }

    public bool Run()
    {
        Console.Clear();
        Console.CancelKeyPress += Console_CancelKeyPress;

        if (_header == null)
        {
            var header = $"{ProgramAttributes.Instance.AppName} {Assembly.GetEntryAssembly()?.GetName().Version}";
            Console.WriteLine(FiggleFonts.Standard.Render(header));
        }
        else
        {
            Console.WriteLine(_header);
        }

        Console.WriteLine();

        var refreshList = true;
        _currentMenuSetLevel = 0;
        var inFirstTime = true;

        LoadRecent();

        while (true)
        {
            if (refreshList)
            {
                if (!ReloadCurrentMenu())
                {
                    StShared.WriteErrorLine("Menu could not loaded", true);
                    return false;
                }

                ShowMenu(inFirstTime);
                inFirstTime = false;
            }

            if (_processes != null && _processes.IsBusy())
            {
                try
                {
                    _processes.WaitForFinishAll().Wait();
                }
                catch (TaskCanceledException e)
                {
                    StShared.WriteException(e, true);
                }
                catch (OperationCanceledException e)
                {
                    StShared.WriteException(e, true);
                }
                catch (Exception e)
                {
                    StShared.WriteException(e, true);
                }

                refreshList = true;
                Console.WriteLine("Process finished");
                StShared.Pause();
                continue;
            }

            var ch = Console.ReadKey(true);

            refreshList = true;

            var menuItem = _menuSetsList[_currentMenuSetLevel].GetMenuItemByKey(ch);

            if (menuItem == null)
            {
                //თუ არაფერი არ მოხდა, მენიუსაც არ ვცვლით
                refreshList = false;
                continue;
            }

            Console.Write(menuItem.CountedKey);
            Console.WriteLine();

            var menuCommand = menuItem.CliMenuCommand;
            menuCommand.Run();

            if (menuCommand is not RecentCommandCliMenuCommand)
                AddSelectedCommand(menuCommand);

            var menuAction = menuCommand.MenuAction;
            //თუ მენიუს ცვლილება მოთხოვნილი არ არის, ვაგრძელებთ ჩვეულებრივად

            switch (menuAction)
            {
                case EMenuAction.Nothing:
                    refreshList = false;
                    continue;
                //თუ მოთხოვნილია პროგრამიდან გასვლა, გავდივართ
                case EMenuAction.Exit:
                    return true;
                case EMenuAction.LoadSubMenu:
                    if (!AddSubMenu(menuCommand.GetSubMenu()))
                        return false;
                    break;
                case EMenuAction.LevelUp when _currentMenuSetLevel <= 0:
                    return true;
                case EMenuAction.LevelUp:
                    _currentMenuSetLevel--;
                    break;
                case EMenuAction.Reload:
                    SaveRecent(menuCommand);
                    StShared.Pause();
                    break;
                case EMenuAction.ReloadWithoutPause:
                    SaveRecent(menuCommand);
                    break;
                case EMenuAction.GoToMenuLink:
                    if (!GoToMenu(menuCommand.GetMenuLinkToGo()))
                        if (!ReloadCurrentMenu())
                            return false;
                    break;
                default:
                    throw new UnreachableException($"EMenuAction {menuAction} did rot realized");
            }

            //თუ სხვა არაფერი არ იყო, აქედან მუშაობს EMenuAction.Reload
        }
    }

    private void LoadRecent()
    {
        if (_par is null || string.IsNullOrWhiteSpace(_par.RecentCommandsFileName) || _par.RecentCommandsCount < 1)
            return;
        var parLoader = new ParametersLoader<RecentCommands>();
        if (!parLoader.TryLoadParameters(_par.RecentCommandsFileName, false) || parLoader.Par is null)
            return;
        _recentCommands = (RecentCommands)parLoader.Par;
        _recentCommands.Rc = _recentCommands.Rc.OrderByDescending(x => x.Key).ToDictionary(k => k.Key, v => v.Value);
    }

    private void SaveRecent(CliMenuCommand menuCommand)
    {
        if (_par is null || string.IsNullOrWhiteSpace(_par.RecentCommandsFileName) || _par.RecentCommandsCount < 1)
            return;

        string commLink;
        if (menuCommand is RecentCommandCliMenuCommand)
        {
            commLink = menuCommand.Name;
        }
        else
        {
            if (_currentMenuSetLevel < 1)
                return;
            commLink = string.Join('/', _selectedMenuCommandsList.Take(_currentMenuSetLevel + 1).Select(x => x.Name));
        }

        _recentCommands.Rc[commLink] = DateTime.Now;

        _recentCommands.Rc = _recentCommands.Rc.OrderByDescending(x => x.Value).ToDictionary(k => k.Key, v => v.Value);

        if (_recentCommands.Rc.Count > _par.RecentCommandsCount)
            _recentCommands.Rc = _recentCommands.Rc.OrderByDescending(x => x.Value).Take(_par.RecentCommandsCount)
                .ToDictionary(k => k.Key, v => v.Value);

        var parMan = new ParametersManager(_par.RecentCommandsFileName, _recentCommands);
        parMan.Save(_recentCommands);
    }

    private bool GoToMenu(string? menuLinkToGo)
    {
        if (menuLinkToGo is null)
            return false;

        var menuLine = menuLinkToGo.Split('/');

        //Console.WriteLine("GoToMenu Start");
        //var line = 0;
        //foreach (var s in menuLine)
        //{
        //    Console.WriteLine($"{line}. {s}");    
        //    line++;
        //}
        //Console.WriteLine("GoToMenu Finish");
        //StShared.Pause();
        ////return false;

        if (menuLine.Length > 0) // && menuLine[0] == string.Empty
        {
            _currentMenuSetLevel = 0;
            AddChangeMenu(BuildMainMenu());
        }

        //if (menuLine.Length <= 1 || menuLine[1] != _menuSetsList[_currentMenuSetLevel].Name)
        //    return true;

        foreach (var menuName in menuLine)
        {
            var menuItem = _menuSetsList[_currentMenuSetLevel].GetMenuItemWithName(menuName);
            if (menuItem is null)
                return false;

            //AddSelectedCommand(menuItem.CliMenuCommand);

            if (!AddSubMenu(menuItem.CliMenuCommand.GetSubMenu()))
                return false;
        }

        return true;
    }

    private bool ReloadCurrentMenu()
    {
        if (_currentMenuSetLevel == 0)
        {
            AddChangeMenu(BuildMainMenu());
        }
        else
        {
            if (_currentMenuSetLevel > _selectedMenuCommandsList.Count)
            {
                StShared.WriteErrorLine($"Wrong menu build. missed selected command on level {_currentMenuSetLevel}",
                    true);
                return false;
            }

            var selectedMenuCommand = _selectedMenuCommandsList[_currentMenuSetLevel - 1];
            if (!AddChangeMenu(selectedMenuCommand.GetSubMenu()))
                return false;
        }

        return true;
    }

    private void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        Console.WriteLine("Console_CancelKeyPress start");
        e.Cancel = true;

        if (_processes != null && _processes.IsBusy())
        {
            _processes.CancelProcesses();
            Console.WriteLine("Process finished");
            StShared.Pause();
        }
        else
        {
            Environment.Exit(0);
        }

        Console.WriteLine("Console_CancelKeyPress Finish");
    }

    private bool AddSubMenu(CliMenuSet? menuSet)
    {
        _currentMenuSetLevel++;
        if (_menuSetsList.Count >= _currentMenuSetLevel)
            return AddChangeMenu(menuSet);

        StShared.WriteErrorLine($"Wrong menu build. missed menu level {_currentMenuSetLevel}", true);
        return false;
    }

    private bool AddChangeMenu(CliMenuSet? menuSet)
    {
        if (menuSet == null)
        {
            StShared.WriteErrorLine($"Menu set is null on level {_currentMenuSetLevel}", true);
            return false;
        }

        if (_menuSetsList.Count == _currentMenuSetLevel)
            _menuSetsList.Add(menuSet);
        else if (_menuSetsList.Count > _currentMenuSetLevel)
            _menuSetsList[_currentMenuSetLevel] = menuSet;
        if (_currentMenuSetLevel > 0)
            menuSet.ParentMenu = _menuSetsList[_currentMenuSetLevel - 1];
        menuSet.FixMenuElements();
        return true;
    }

    private void AddSelectedCommand(CliMenuCommand menuCommand)
    {
        if (_selectedMenuCommandsList.Count == _currentMenuSetLevel)
            _selectedMenuCommandsList.Add(menuCommand);
        else if (_selectedMenuCommandsList.Count > _currentMenuSetLevel)
            _selectedMenuCommandsList[_currentMenuSetLevel] = menuCommand;
    }
}