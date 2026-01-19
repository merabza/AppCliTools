namespace AppCliTools.CliMenu.Tests;

public sealed class CliMenuCommandTests
{
    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Arrange
        const string name = "TestCommand";
        const string parentMenuName = "ParentMenu";
        var command = new CliMenuCommand(name, EMenuAction.LevelUp, EMenuAction.Reload, parentMenuName, true,
            EStatusView.Table, true);

        // Assert
        Assert.Equal(name, command.Name);
        Assert.Equal(EMenuAction.Nothing, command.MenuAction);
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
        Assert.Equal(parentMenuName, command.ParentMenuName);
        Assert.Equal(EStatusView.Table, command.StatusView);
        Assert.True(command.NameIsStatus);
    }

    [Fact]
    public void CountStatus_SetsStatusProperty()
    {
        // Arrange
        var command = new CliMenuCommand("Test");

        // Act
        command.CountStatus();

        // Assert
        Assert.Null(command.StatusString); // Default GetStatus returns null
    }

    [Fact]
    public void Run_SetsMenuActionOnSuccess()
    {
        // Arrange
        var command = new TestCliMenuCommand("Test", EMenuAction.LevelUp, EMenuAction.Reload);

        // Act
        command.Run();

        // Assert
        Assert.Equal(EMenuAction.LevelUp, command.MenuAction);
    }

    [Fact]
    public void Run_SetsMenuActionOnFail()
    {
        // Arrange
        var command = new TestCliMenuCommand("Test", EMenuAction.LevelUp, EMenuAction.Reload, false);

        // Act
        command.Run();

        // Assert
        Assert.Equal(EMenuAction.Reload, command.MenuAction);
    }

    [Fact]
    public void GetSubMenu_ReturnsNullByDefault()
    {
        // Arrange
        var command = new CliMenuCommand("Test");

        // Act
        CliMenuSet? subMenu = command.GetSubMenu();

        // Assert
        Assert.Null(subMenu);
    }

    [Fact]
    public void GetMenuLinkToGo_ReturnsNullByDefault()
    {
        // Arrange
        var command = new CliMenuCommand("Test");

        // Act
        string? link = command.GetMenuLinkToGo();

        // Assert
        Assert.Null(link);
    }

    // Helper class to override RunBody for testing
    private sealed class TestCliMenuCommand : CliMenuCommand
    {
        private readonly bool _runBodyResult;

        // ReSharper disable once ConvertToPrimaryConstructor
        public TestCliMenuCommand(string name, EMenuAction success, EMenuAction fail, bool runBodyResult = true) : base(
            name, success, fail)
        {
            _runBodyResult = runBodyResult;
        }

        protected override bool RunBody()
        {
            return _runBodyResult;
        }
    }
}
