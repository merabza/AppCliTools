using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliTools.CliMenuCommands;
using AppCliTools.CliTools.Services.MenuBuilder;
using AppCliTools.CliTools.Services.RecentCommands;
using Figgle.Fonts;
using SystemTools.BackgroundTasks;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliTools;

public class CliAppLoop
{
    private readonly IApplication _app;
    private readonly IMenuBuilder _menuBuilder;
    private readonly List<CliMenuSet> _menuSetsList = [];
    private readonly IProcesses? _processes;
    private readonly IRecentCommandsService? _recentCommandsService;
    private readonly List<CliMenuCommand> _selectedMenuCommandsList = [];
    private int _currentMenuSetLevel;

    // ReSharper disable once ConvertToPrimaryConstructor
    private CliAppLoop(IApplication app, IMenuBuilder menuBuilder, IRecentCommandsService? recentCommandsService = null,
        IProcesses? processes = null)
    {
        _app = app;
        _menuBuilder = menuBuilder;
        _recentCommandsService = recentCommandsService;
        _processes = processes;
    }

    public CliAppLoop(CliAppLoopParameters clp) : this(clp.App, clp.MenuBuilder, clp.RecentCommandsService,
        clp.Processes)
    {
    }

    private void ShowMenu(bool inFirstTime)
    {
        _menuSetsList[_currentMenuSetLevel].Show(!inFirstTime);
    }

    public async ValueTask<bool> Run()
    {
        Console.Clear();
        Console.CancelKeyPress += Console_CancelKeyPress;

        string header = $"{_app.Name} {Assembly.GetEntryAssembly()?.GetName().Version}";
        Console.WriteLine(FiggleFonts.Standard.Render(header));

        Console.WriteLine();

        bool refreshList = true;
        _currentMenuSetLevel = 0;
        bool inFirstTime = true;

        _recentCommandsService?.LoadRecent();

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
                    await _processes.WaitForFinishAll();
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

            ConsoleKeyInfo ch = Console.ReadKey(true);

            refreshList = true;

            CliMenuItem? menuItem = _menuSetsList[_currentMenuSetLevel].GetMenuItemByKey(ch);

            if (menuItem == null)
            {
                //თუ არაფერი არ მოხდა, მენიუსაც არ ვცვლით
                refreshList = false;
                continue;
            }

            Console.Write(menuItem.CountedKey);
            Console.WriteLine();

            CliMenuCommand menuCommand = menuItem.CliMenuCommand;


            await menuCommand.Run();

            if (menuCommand is not RecentCommandCliMenuCommand)
            {
                AddSelectedCommand(menuCommand);
            }

            EMenuAction menuAction = menuCommand.MenuAction;
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
                    {
                        return false;
                    }

                    break;
                case EMenuAction.LevelUp when _currentMenuSetLevel <= 0:
                    return true;
                case EMenuAction.LevelUp:
                    _currentMenuSetLevel--;
                    break;
                case EMenuAction.Reload:
                    if (_recentCommandsService != null)
                    {
                        await _recentCommandsService.SaveRecent(menuCommand);
                    }

                    StShared.Pause();
                    break;
                case EMenuAction.ReloadWithoutPause:
                    if (_recentCommandsService != null)
                    {
                        await _recentCommandsService.SaveRecent(menuCommand);
                    }

                    break;
                case EMenuAction.GoToMenuLink:
                    if (!GoToMenu(menuCommand.GetMenuLinkToGo()) && !ReloadCurrentMenu())
                    {
                        return false;
                    }

                    break;
                case EMenuAction.PageUp:
                    _menuSetsList[_currentMenuSetLevel].PageUp();
                    refreshList = false;
                    break;
                case EMenuAction.PageDown:
                    _menuSetsList[_currentMenuSetLevel].PageDown();
                    refreshList = false;
                    break;
                default:
                    throw new UnreachableException($"EMenuAction {menuAction} did rot realized");
            }

            //თუ სხვა არაფერი არ იყო, აქედან მუშაობს EMenuAction.Reload
        }
    }

    private void CountCommandLink(CliMenuCommand menuCommand)
    {
        menuCommand.CommandLink = menuCommand switch
        {
            RecentCommandCliMenuCommand => menuCommand.Name,
            _ => _currentMenuSetLevel < 1
                ? null
                : string.Join('/', _selectedMenuCommandsList.Take(_currentMenuSetLevel + 1).Select(x => x.Name))
        };
    }

    private bool GoToMenu(string? menuLinkToGo)
    {
        if (menuLinkToGo is null)
        {
            return false;
        }

        string[] menuLine = menuLinkToGo.Split('/');

        if (menuLine.Length > 0)
        {
            _currentMenuSetLevel = 0;
            AddChangeMenu(_menuBuilder.BuildMainMenu());
        }

        foreach (string menuName in menuLine)
        {
            CliMenuItem? menuItem = _menuSetsList[_currentMenuSetLevel].GetMenuItemWithName(menuName);
            if (menuItem is null)
            {
                return false;
            }

            if (!AddSubMenu(menuItem.CliMenuCommand.GetSubMenu()))
            {
                return false;
            }
        }

        return true;
    }

    private bool ReloadCurrentMenu()
    {
        if (_currentMenuSetLevel == 0)
        {
            AddChangeMenu(_menuBuilder.BuildMainMenu());
        }
        else
        {
            if (_currentMenuSetLevel > _selectedMenuCommandsList.Count)
            {
                StShared.WriteErrorLine($"Wrong menu build. missed selected command on level {_currentMenuSetLevel}",
                    true);
                return false;
            }

            CliMenuCommand selectedMenuCommand = _selectedMenuCommandsList[_currentMenuSetLevel - 1];
            if (!AddChangeMenu(selectedMenuCommand.GetSubMenu()))
            {
                return false;
            }
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
        {
            return AddChangeMenu(menuSet);
        }

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
        {
            _menuSetsList.Add(menuSet);
        }
        else if (_menuSetsList.Count > _currentMenuSetLevel)
        {
            _menuSetsList[_currentMenuSetLevel] = menuSet;
        }

        if (_currentMenuSetLevel > 0)
        {
            menuSet.ParentMenu = _menuSetsList[_currentMenuSetLevel - 1];
        }

        menuSet.FixMenuElements();
        return true;
    }

    private void AddSelectedCommand(CliMenuCommand menuCommand)
    {
        if (_selectedMenuCommandsList.Count == _currentMenuSetLevel)
        {
            _selectedMenuCommandsList.Add(menuCommand);
        }
        else if (_selectedMenuCommandsList.Count > _currentMenuSetLevel)
        {
            _selectedMenuCommandsList[_currentMenuSetLevel] = menuCommand;
        }

        CountCommandLink(menuCommand);
    }
}
