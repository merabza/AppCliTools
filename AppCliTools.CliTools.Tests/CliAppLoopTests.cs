using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliTools.CliMenuCommands;
using AppCliTools.CliTools.Services.MenuBuilder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliTools.Tests;

//a menu link (InfoCliMenuCommand) goes from the main menu through the menu items that it names. After every command
//the loop builds the current menu again with the command that opened it, so the link has to leave its commands chosen
//on their levels, as if the user had chosen them
public sealed class CliAppLoopTests : IDisposable
{
    private const string EscapeName = "Exit to level up menu";
    private const string LinkName = "Group/Project/Dev";

    private readonly StringWriter _consoleOutput = new(CultureInfo.InvariantCulture);
    private readonly TextWriter _originalConsoleOutput;

    public CliAppLoopTests()
    {
        _originalConsoleOutput = Console.Out;
        Console.SetOut(_consoleOutput);
    }

    public void Dispose()
    {
        Console.SetOut(_originalConsoleOutput);
        _consoleOutput.Dispose();
    }

    //the loop used to build the menus of the user's own path (Editor/Connections) on the levels of the link
    [Fact]
    public async Task MenuLink_AfterTheUserOpenedOtherMenus_ShowsTheLinkedMenu()
    {
        // Arrange
        CliAppLoopDriver sut = await CliAppLoopDriver.Start(LinkName);
        await sut.Choose("Editor");
        await sut.Choose("Connections");

        // Act
        await sut.Choose(LinkName);

        // Assert
        Assert.Equal("Dev", sut.CurrentMenu.Caption);
        Assert.Equal(3, sut.Level);
        Assert.Empty(_consoleOutput.ToString());
    }

    //a command and a list item have the same name: the link goes through the list item, which comes after the commands
    [Fact]
    public async Task MenuLink_WhenACommandHasTheNameOfTheLinkedItem_GoesThroughTheLastItem()
    {
        // Arrange
        CliAppLoopDriver sut = await CliAppLoopDriver.Start(LinkName);
        await sut.Choose("Editor");
        await sut.Choose("Connections");

        // Act
        await sut.Choose(LinkName);

        // Assert
        Assert.Equal(["Main Menu", "Group", "Project", "Dev"], sut.MenusToTheCurrentLevel.Select(x => x.Caption));
    }

    [Fact]
    public async Task MenuLink_ThenLevelUp_ShowsTheMenusOfTheLinkPath()
    {
        // Arrange
        CliAppLoopDriver sut = await CliAppLoopDriver.Start(LinkName);
        await sut.Choose("Editor");
        await sut.Choose("Connections");
        await sut.Choose(LinkName);

        // Act
        await sut.Choose(EscapeName);
        string? projectMenuCaption = sut.CurrentMenu.Caption;
        await sut.Choose(EscapeName);

        // Assert
        Assert.Equal("Project", projectMenuCaption);
        Assert.Equal("Group", sut.CurrentMenu.Caption);
        Assert.Equal(1, sut.Level);
    }

    //the link stops in the last menu found and names the missing menu item
    [Theory]
    [InlineData("Group/Missing/Dev", "Missing", "Group", 1)]
    [InlineData("Missing", "Missing", "Main Menu", 0)]
    public async Task MenuLink_WhenAMenuItemIsMissing_StaysInTheLastMenuFound(string link, string missingItemName,
        string expectedCaption, int expectedLevel)
    {
        // Arrange
        CliAppLoopDriver sut = await CliAppLoopDriver.Start(link);
        await sut.Choose("Editor");
        await sut.Choose("Connections");

        // Act
        await sut.Choose(link);

        // Assert
        Assert.Equal(expectedCaption, sut.CurrentMenu.Caption);
        Assert.Equal(expectedLevel, sut.Level);
        Assert.Contains($"Menu item {missingItemName} of the menu link {link} not found", _consoleOutput.ToString(),
            StringComparison.Ordinal);
    }

    private sealed class SubMenuCliMenuCommand : CliMenuCommand
    {
        private readonly Func<CliMenuSet> _createSubMenu;

        // ReSharper disable once ConvertToPrimaryConstructor
        public SubMenuCliMenuCommand(string name, Func<CliMenuSet> createSubMenu) : base(name,
            EMenuAction.LoadSubMenu)
        {
            _createSubMenu = createSubMenu;
        }

        public override CliMenuSet GetSubMenu()
        {
            return _createSubMenu();
        }
    }

    //like the real menu builder, every call builds the main menu again. Main Menu: Editor/Connections holds the link,
    //Group/Project/Dev is a menu path that the link can name. Like the BaGetter command and the BaGetter project group of
    //SupportTools, a command of the main menu has the name of the group
    private sealed class FakeMenuBuilder : IMenuBuilder
    {
        private readonly string _link;

        // ReSharper disable once ConvertToPrimaryConstructor
        public FakeMenuBuilder(string link)
        {
            _link = link;
        }

        public Task<CliMenuSet?> BuildMainMenu()
        {
            var mainMenu = new CliMenuSet("Main Menu");
            mainMenu.AddMenuItem(new SubMenuCliMenuCommand("Editor",
                () => Menu("Editor",
                    new SubMenuCliMenuCommand("Connections",
                        () => Menu("Connections", new InfoCliMenuCommand(_link, _link))))));
            mainMenu.AddMenuItem(new SubMenuCliMenuCommand("Group", () => Menu("Group commands")));
            mainMenu.AddMenuItem(new SubMenuCliMenuCommand("Group",
                () => Menu("Group", new SubMenuCliMenuCommand("Project",
                    () => Menu("Project", new SubMenuCliMenuCommand("Dev", () => Menu("Dev")))))));
            return Task.FromResult<CliMenuSet?>(mainMenu);
        }

        private static CliMenuSet Menu(string caption, params CliMenuCommand[] commands)
        {
            var menuSet = new CliMenuSet(caption);
            foreach (CliMenuCommand command in commands)
            {
                menuSet.AddMenuItem(command);
            }

            menuSet.AddEscapeCommand(EscapeName);
            return menuSet;
        }
    }

    //CliAppLoop.Run reads its keys from the console, so the driver takes the steps of one turn of its loop for the
    //chosen menu item: run the command, note it as chosen on the current level, do its menu action and build the
    //current menu again
    private sealed class CliAppLoopDriver
    {
        private readonly CliAppLoop _loop;

        private CliAppLoopDriver(CliAppLoop loop)
        {
            _loop = loop;
        }

        public int Level => (int)Field("_currentMenuSetLevel").GetValue(_loop)!;

        public CliMenuSet CurrentMenu => MenusToTheCurrentLevel[Level];

        public List<CliMenuSet> MenusToTheCurrentLevel =>
            [.. ((List<CliMenuSet>)Field("_menuSetsList").GetValue(_loop)!).Take(Level + 1)];

        public static async Task<CliAppLoopDriver> Start(string link)
        {
            CliAppLoopParameters? parameters;
            // ReSharper disable once using
            await using (ServiceProvider serviceProvider = new ServiceCollection()
                             .AddSingleton<ILogger<CliAppLoopTests>>(NullLogger<CliAppLoopTests>.Instance)
                             .AddSingleton(Mock.Of<IApplication>())
                             .AddSingleton<IMenuBuilder>(new FakeMenuBuilder(link)).BuildServiceProvider())
            {
                (parameters, _) = CliAppLoopParameters.Create<CliAppLoopTests>(serviceProvider);
            }

            var driver = new CliAppLoopDriver(new CliAppLoop(parameters!));
            Assert.True(await driver.ReloadCurrentMenu());
            return driver;
        }

        public async Task Choose(string menuItemName)
        {
            CliMenuCommand command = CurrentMenu.GetMenuItemWithName(menuItemName)!.CliMenuCommand;
            await command.Run();
            Invoke("AddSelectedCommand", command);

            switch (command.MenuAction)
            {
                case EMenuAction.LoadSubMenu:
                    Assert.True((bool)Invoke("AddSubMenu", command.GetSubMenu())!);
                    break;
                case EMenuAction.LevelUp:
                    Field("_currentMenuSetLevel").SetValue(_loop, Level - 1);
                    break;
                case EMenuAction.GoToMenuLink:
                    if (!await (Task<bool>)Invoke("GoToMenu", command.GetMenuLinkToGo())!)
                    {
                        Assert.True(await ReloadCurrentMenu());
                    }

                    break;
                default:
                    throw new InvalidOperationException($"The menu action {command.MenuAction} is not expected");
            }

            //the loop builds the current menu again before it shows it
            Assert.True(await ReloadCurrentMenu());
        }

        private Task<bool> ReloadCurrentMenu()
        {
            return (Task<bool>)Invoke("ReloadCurrentMenu")!;
        }

        private object? Invoke(string methodName, params object?[] arguments)
        {
            return typeof(CliAppLoop).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(_loop, arguments);
        }

        private static FieldInfo Field(string fieldName)
        {
            return typeof(CliAppLoop).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!;
        }
    }
}
