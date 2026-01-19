namespace CliMenu.Tests;

public sealed class CliMenuItemTests
{
    [Fact]
    public void Constructor_WithCommandAndCountedId_InitializesProperties()
    {
        // Arrange
        var command = new CliMenuCommand("TestCommand");
        const int countedId = 5;

        // Act
        var menuItem = new CliMenuItem(command, countedId);

        // Assert
        Assert.Equal(command, menuItem.CliMenuCommand);
        Assert.Equal(countedId, menuItem.CountedId);
        Assert.Null(menuItem.Key);
        Assert.Null(menuItem.CountedKey);
        Assert.Equal("TestCommand", menuItem.MenuItemName);
    }

    [Fact]
    public void Constructor_WithCommandOnly_InitializesProperties()
    {
        // Arrange
        var command = new CliMenuCommand("TestCommand");

        // Act
        var menuItem = new CliMenuItem(command);

        // Assert
        Assert.Equal(command, menuItem.CliMenuCommand);
        Assert.Equal(0, menuItem.CountedId); // Default value for int
        Assert.Null(menuItem.Key);
        Assert.Null(menuItem.CountedKey);
        Assert.Equal("TestCommand", menuItem.MenuItemName);
    }

    [Fact]
    public void Constructor_WithKeyCommandAndCountedId_InitializesProperties()
    {
        // Arrange
        var command = new CliMenuCommand("TestCommand");
        const string key = "A";
        const int countedId = 10;

        // Act
        var menuItem = new CliMenuItem(key, command, countedId);

        // Assert
        Assert.Equal(command, menuItem.CliMenuCommand);
        Assert.Equal(countedId, menuItem.CountedId);
        Assert.Equal(key, menuItem.Key);
        Assert.Null(menuItem.CountedKey);
        Assert.Equal("TestCommand", menuItem.MenuItemName);
    }

    [Fact]
    public void Constructor_WithNullKey_InitializesProperties()
    {
        // Arrange
        var command = new CliMenuCommand("TestCommand");
        const int countedId = 7;

        // Act
        var menuItem = new CliMenuItem(null, command, countedId);

        // Assert
        Assert.Equal(command, menuItem.CliMenuCommand);
        Assert.Equal(countedId, menuItem.CountedId);
        Assert.Null(menuItem.Key);
        Assert.Null(menuItem.CountedKey);
        Assert.Equal("TestCommand", menuItem.MenuItemName);
    }

    [Fact]
    public void MenuItemName_ReturnsCommandName()
    {
        // Arrange
        const string commandName = "MyCommand";
        var command = new CliMenuCommand(commandName);
        var menuItem = new CliMenuItem(command);

        // Act
        var menuItemName = menuItem.MenuItemName;

        // Assert
        Assert.Equal(commandName, menuItemName);
    }

    [Fact]
    public void CountedKey_CanBeSetAndGet()
    {
        // Arrange
        var command = new CliMenuCommand("TestCommand");
        var menuItem = new CliMenuItem(command);
        const string countedKey = "1";

        // Act
        menuItem.CountedKey = countedKey;

        // Assert
        Assert.Equal(countedKey, menuItem.CountedKey);
    }

    [Fact]
    public void CountedKey_CanBeSetToNull()
    {
        // Arrange
        var command = new CliMenuCommand("TestCommand");
        var menuItem = new CliMenuItem(command);
        menuItem.CountedKey = "1";

        // Act
        menuItem.CountedKey = null;

        // Assert
        Assert.Null(menuItem.CountedKey);
    }

    [Fact]
    public void CountedId_CanBeSetAndGet()
    {
        // Arrange
        var command = new CliMenuCommand("TestCommand");
        var menuItem = new CliMenuItem(command);
        const int newCountedId = 42;

        // Act
        menuItem.CountedId = newCountedId;

        // Assert
        Assert.Equal(newCountedId, menuItem.CountedId);
    }

    [Fact]
    public void Key_CanBeSetAndGet()
    {
        // Arrange
        var command = new CliMenuCommand("TestCommand");
        var menuItem = new CliMenuItem(command);
        const string key = "B";

        // Act
        menuItem.Key = key;

        // Assert
        Assert.Equal(key, menuItem.Key);
    }

    [Fact]
    public void Key_CanBeSetToNull()
    {
        // Arrange
        var command = new CliMenuCommand("TestCommand");
        var menuItem = new CliMenuItem("A", command, 1);

        // Act
        menuItem.Key = null;

        // Assert
        Assert.Null(menuItem.Key);
    }

    [Fact]
    public void CliMenuCommand_CanBeSetAndGet()
    {
        // Arrange
        var originalCommand = new CliMenuCommand("Original");
        var menuItem = new CliMenuItem(originalCommand);
        var newCommand = new CliMenuCommand("NewCommand");

        // Act
        menuItem.CliMenuCommand = newCommand;

        // Assert
        Assert.Equal(newCommand, menuItem.CliMenuCommand);
        Assert.Equal("NewCommand", menuItem.MenuItemName);
    }

    [Fact]
    public void SetMenuSet_SetsMenuSetOnCommand()
    {
        // Arrange
        var command = new CliMenuCommand("TestCommand");
        var menuItem = new CliMenuItem(command);
        var menuSet = new CliMenuSet("TestMenuSet");

        // Act
        menuItem.SetMenuSet(menuSet);

        // Assert
        Assert.Equal(menuSet, menuItem.CliMenuCommand.MenuSet);
    }

    [Fact]
    public void SetMenuSet_WithNullMenuSet_SetsNullOnCommand()
    {
        // Arrange
        var command = new CliMenuCommand("TestCommand");
        var menuItem = new CliMenuItem(command);
        var menuSet = new CliMenuSet("TestMenuSet");
        menuItem.SetMenuSet(menuSet);

        // Act
        menuItem.SetMenuSet(null!);

        // Assert
        Assert.Null(menuItem.CliMenuCommand.MenuSet);
    }

    [Fact]
    public void SetMenuSet_UpdatesMenuSetReference()
    {
        // Arrange
        var command = new CliMenuCommand("TestCommand");
        var menuItem = new CliMenuItem(command);
        var menuSet1 = new CliMenuSet("MenuSet1");
        var menuSet2 = new CliMenuSet("MenuSet2");

        // Act
        menuItem.SetMenuSet(menuSet1);
        Assert.Equal(menuSet1, menuItem.CliMenuCommand.MenuSet);

        menuItem.SetMenuSet(menuSet2);

        // Assert
        Assert.Equal(menuSet2, menuItem.CliMenuCommand.MenuSet);
    }

    [Fact]
    public void MultipleMenuItems_CanShareSameCommand()
    {
        // Arrange
        var command = new CliMenuCommand("SharedCommand");

        // Act
        var menuItem1 = new CliMenuItem(command, 1);
        var menuItem2 = new CliMenuItem(command, 2);

        // Assert
        Assert.Equal(command, menuItem1.CliMenuCommand);
        Assert.Equal(command, menuItem2.CliMenuCommand);
        Assert.Same(menuItem1.CliMenuCommand, menuItem2.CliMenuCommand);
    }

    [Fact]
    public void MenuItemName_ReflectsCommandNameChange()
    {
        // Arrange
        var originalCommand = new CliMenuCommand("OriginalName");
        var menuItem = new CliMenuItem(originalCommand);
        var newCommand = new CliMenuCommand("NewName");

        // Act
        menuItem.CliMenuCommand = newCommand;

        // Assert
        Assert.Equal("NewName", menuItem.MenuItemName);
    }

    [Fact]
    public void Constructor_AllProperties_AreIndependent()
    {
        // Arrange
        var command1 = new CliMenuCommand("Command1");
        var command2 = new CliMenuCommand("Command2");

        // Act
        var menuItem1 = new CliMenuItem("A", command1, 1);
        var menuItem2 = new CliMenuItem("B", command2, 2);

        menuItem1.CountedKey = "1";
        menuItem2.CountedKey = "2";

        // Assert
        Assert.NotEqual(menuItem1.Key, menuItem2.Key);
        Assert.NotEqual(menuItem1.CountedId, menuItem2.CountedId);
        Assert.NotEqual(menuItem1.CountedKey, menuItem2.CountedKey);
        Assert.NotSame(menuItem1.CliMenuCommand, menuItem2.CliMenuCommand);
    }
}