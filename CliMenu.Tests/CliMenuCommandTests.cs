namespace CliMenu.Tests;

public class CliMenuCommandTests
{
    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Arrange
        var name = "TestCommand";
        var parentMenuName = "ParentMenu";
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
        Assert.Null(command.Status); // Default GetStatus returns null
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
        var command = new TestCliMenuCommand("Test", EMenuAction.LevelUp, EMenuAction.Reload, runBodyResult: false);

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
        var subMenu = command.GetSubMenu();

        // Assert
        Assert.Null(subMenu);
    }

    [Fact]
    public void GetMenuLinkToGo_ReturnsNullByDefault()
    {
        // Arrange
        var command = new CliMenuCommand("Test");

        // Act
        var link = command.GetMenuLinkToGo();

        // Assert
        Assert.Null(link);
    }

    // Helper class to override RunBody for testing
    private class TestCliMenuCommand : CliMenuCommand
    {
        private readonly bool _runBodyResult;

        // ReSharper disable once ConvertToPrimaryConstructor
        public TestCliMenuCommand(string name, EMenuAction success, EMenuAction fail, bool runBodyResult = true) : base(
            name, success, fail)
        {
            _runBodyResult = runBodyResult;
        }

        protected override bool RunBody() => _runBodyResult;
    }
}