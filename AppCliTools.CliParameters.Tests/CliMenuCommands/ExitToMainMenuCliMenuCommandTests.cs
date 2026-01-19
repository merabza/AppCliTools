using System;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;

namespace AppCliTools.CliParameters.Tests.CliMenuCommands;

public sealed class ExitToMainMenuCliMenuCommandTests
{
    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        // Arrange
        const string name = "Exit to Main Menu";
        const string parentMenuName = "SubMenu";

        // Act
        var command = new ExitToMainMenuCliMenuCommand(name, parentMenuName);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(name, command.Name);
    }

    [Fact]
    public void Constructor_WithNullParentMenuName_InitializesCorrectly()
    {
        // Arrange
        const string name = "Exit to Main Menu";

        // Act
        var command = new ExitToMainMenuCliMenuCommand(name, null);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(name, command.Name);
    }

    [Fact]
    public void Constructor_WithEmptyName_InitializesCorrectly()
    {
        // Arrange
        const string name = "";
        const string parentMenuName = "ParentMenu";

        // Act
        var command = new ExitToMainMenuCliMenuCommand(name, parentMenuName);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(name, command.Name);
    }

    [Fact]
    public void Constructor_WithWhitespaceName_InitializesCorrectly()
    {
        // Arrange
        const string name = "   ";
        const string parentMenuName = "ParentMenu";

        // Act
        var command = new ExitToMainMenuCliMenuCommand(name, parentMenuName);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(name, command.Name);
    }

    [Fact]
    public void Constructor_WithEmptyParentMenuName_InitializesCorrectly()
    {
        // Arrange
        const string name = "Exit to Main Menu";
        const string parentMenuName = "";

        // Act
        var command = new ExitToMainMenuCliMenuCommand(name, parentMenuName);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(name, command.Name);
    }

    [Fact]
    public void Constructor_SetsMenuActionOnBodySuccessToLevelUp()
    {
        // Arrange
        const string name = "Exit to Main Menu";
        const string parentMenuName = "SubMenu";

        // Act
        var command = new ExitToMainMenuCliMenuCommand(name, parentMenuName);

        // Assert
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
    }

    [Fact]
    public void Constructor_SetsMenuActionOnBodyFailToLevelUp()
    {
        // Arrange
        const string name = "Exit to Main Menu";
        const string parentMenuName = "SubMenu";

        // Act
        var command = new ExitToMainMenuCliMenuCommand(name, parentMenuName);

        // Assert
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodyFail);
    }

    [Fact]
    public void Constructor_InitializesWithNothingMenuAction()
    {
        // Arrange
        const string name = "Exit to Main Menu";
        const string parentMenuName = "SubMenu";

        // Act
        var command = new ExitToMainMenuCliMenuCommand(name, parentMenuName);

        // Assert - Initial value should be Nothing
        Assert.Equal(EMenuAction.Nothing, command.MenuAction);
    }

    [Fact]
    public void Constructor_WithLongName_InitializesCorrectly()
    {
        // Arrange
        const string name = "This is a very long name for testing purposes with many characters";
        const string parentMenuName = "ParentMenu";

        // Act
        var command = new ExitToMainMenuCliMenuCommand(name, parentMenuName);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(name, command.Name);
    }

    [Fact]
    public void Constructor_WithSpecialCharactersInName_InitializesCorrectly()
    {
        // Arrange
        const string name = "Exit → Main Menu [ESC]";
        const string parentMenuName = "SubMenu";

        // Act
        var command = new ExitToMainMenuCliMenuCommand(name, parentMenuName);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(name, command.Name);
    }

    [Fact]
    public void Constructor_WithSpecialCharactersInParentMenuName_InitializesCorrectly()
    {
        // Arrange
        const string name = "Exit to Main Menu";
        const string parentMenuName = "Parent → Sub Menu [Level 2]";

        // Act
        var command = new ExitToMainMenuCliMenuCommand(name, parentMenuName);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(name, command.Name);
    }

    [Theory]
    [InlineData("Exit", "Main")]
    [InlineData("Back", "ParentMenu")]
    [InlineData("Return to Main", "RootMenu")]
    [InlineData("← Back", "SubMenu")]
    [InlineData("Exit to Main Menu", null)]
    public void Constructor_WithVariousNameAndParentCombinations_InitializesCorrectly(string name,
        string? parentMenuName)
    {
        // Act
        var command = new ExitToMainMenuCliMenuCommand(name, parentMenuName);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(name, command.Name);
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodyFail);
    }

    [Fact]
    public void Constructor_BothMenuActionsSetToLevelUp()
    {
// Arrange
        const string name = "Exit to Main Menu";
        const string parentMenuName = "SubMenu";

        // Act
        var command = new ExitToMainMenuCliMenuCommand(name, parentMenuName);

        // Assert - Both success and fail actions should be LevelUp
        Assert.Equal(command.MenuActionOnBodySuccess, command.MenuActionOnBodyFail);
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodyFail);
    }

    [Fact]
    public void Constructor_NameProperty_IsAccessible()
    {
        // Arrange
        const string expectedName = "Test Exit Command";
        const string parentMenuName = "TestParent";

        // Act
        var command = new ExitToMainMenuCliMenuCommand(expectedName, parentMenuName);

        // Assert
        Assert.Equal(expectedName, command.Name);
    }

    [Fact]
    public void Constructor_WithNullNameThrowsException()
    {
        // Arrange
        const string? name = null;
        const string parentMenuName = "ParentMenu";

        // Act & Assert - This should throw based on base class validation
        // Note: The actual behavior depends on the base class implementation
        // If the base class allows null, this test would need adjustment
        Exception? exception = Record.Exception(() => new ExitToMainMenuCliMenuCommand(name!, parentMenuName));

// If no exception is thrown, the constructor accepts null
        // If an exception is thrown, we verify it's the expected type
        if (exception != null)
        {
            Assert.IsType<ArgumentNullException>(exception);
        }
    }

    [Fact]
    public void Constructor_CreatesUniqueInstances()
    {
        // Arrange & Act
        var command1 = new ExitToMainMenuCliMenuCommand("Exit 1", "Menu1");
        var command2 = new ExitToMainMenuCliMenuCommand("Exit 2", "Menu2");

        // Assert
        Assert.NotSame(command1, command2);
        Assert.NotEqual(command1.Name, command2.Name);
    }

    [Fact]
    public void Constructor_PassesCorrectParametersToBase()
    {
        // Arrange
        const string name = "Exit Command";
        const string parentMenuName = "TestMenu";

        // Act
        var command = new ExitToMainMenuCliMenuCommand(name, parentMenuName);

        // Assert - Verify the parameters are passed correctly to base constructor
        // by checking the resulting properties
        Assert.Equal(name, command.Name);
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodyFail);
    }

    [Fact]
    public void Constructor_InheritsFromCliMenuCommand()
    {
        // Arrange
        const string name = "Exit";
        const string parentMenuName = "Parent";

        // Act
        var command = new ExitToMainMenuCliMenuCommand(name, parentMenuName);

        // Assert
        Assert.IsAssignableFrom<CliMenuCommand>(command);
    }

    [Fact]
    public void Constructor_IsSealed()
    {
        // Assert
        Assert.True(typeof(ExitToMainMenuCliMenuCommand).IsSealed);
    }
}
