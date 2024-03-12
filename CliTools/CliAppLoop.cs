using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using CliMenu;
using Figgle;
using LibToolActions.BackgroundTasks;
using SystemToolsShared;

namespace CliTools;

public /*open*/ class CliAppLoop
{
    private readonly string? _header;
    private readonly List<CliMenuSet> _menuSetsList = new();
    private readonly IProcesses? _processes;
    private readonly List<CliMenuCommand> _selectedMenuCommandsList = new();
    private int _currentMenuSetLevel;

    protected CliAppLoop(string? header = null, IProcesses? processes = null)
    {
        _header = header;
        _processes = processes;
    }

    protected virtual bool BuildMainMenu()
    {
        return false;
    }

    private void ShowMenu(bool inFirstTime)
    {
        _menuSetsList[_currentMenuSetLevel].Show(!inFirstTime);
    }


    public bool Run()
    {
        Console.Clear();
        Console.CancelKeyPress += Console_CancelKeyPress;


        if (_header == null)
        {
            var header =
                $"{ProgramAttributes.Instance.GetAttribute<string>("AppName")} {Assembly.GetEntryAssembly()?.GetName().Version}";
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

        while (true)
        {
            if (refreshList)
            {
                ReloadCurrentMenu();
                //if (_currentMenuSetLevel == 0)
                //{
                //    if (!BuildMainMenu())
                //        return false;
                //}
                //else
                //{
                //    if (!ReBuildMenu(_currentMenuSetLevel))
                //        return false;
                //}

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
                    if (!AddSubMenu(menuCommand.GetSubmenu()))
                        return false;
                    break;
                case EMenuAction.LevelUp when _currentMenuSetLevel <= 0:
                    return true;
                case EMenuAction.LevelUp:
                    _currentMenuSetLevel--;
                    break;
                case EMenuAction.Reload:
                    if (!ReloadCurrentMenu())
                        return false;
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

        if (menuLine.Length > 0 && menuLine[0] == string.Empty)
        {
            _currentMenuSetLevel = 0;
            BuildMainMenu();
        }

        if (menuLine.Length <= 1 || menuLine[1] != _menuSetsList[_currentMenuSetLevel].Name)
            return true;

        for (var i = 2; i < menuLine.Length; i++)
        {
            var menuItem = _menuSetsList[_currentMenuSetLevel].GetMenuItemWithName(menuLine[i]);
            if (menuItem is null)
                return false;

            if (!AddSubMenu(menuItem.CliMenuCommand.GetSubmenu()))
                return false;
        }

        return true;
    }

    private bool ReloadCurrentMenu()
    {
        if (_currentMenuSetLevel == 0)
        {
            BuildMainMenu();
        }
        else
        {
            var selectedMenuCommand = _selectedMenuCommandsList[_currentMenuSetLevel - 1];
            if (!AddChangeMenu(selectedMenuCommand.GetSubmenu()))
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

    protected bool AddChangeMenu(CliMenuSet? menuSet)
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