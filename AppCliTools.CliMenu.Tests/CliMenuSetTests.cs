using System;

namespace CliMenu.Tests;

public sealed class CliMenuSetTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        var set = new CliMenuSet("TestCaption", 5);
        Assert.Equal(5, set.MenuVersion);
    }

    [Fact]
    public void AddMenuItem_AddsItemToMenu()
    {
        var set = new CliMenuSet();
        var command = new CliMenuCommand("Item1");
        set.AddMenuItem(command);
        var item = set.GetMenuItemWithName("Item1");
        Assert.NotNull(item);
        Assert.Equal("Item1", item.MenuItemName);
        Assert.Equal(command, item.CliMenuCommand);
    }

    [Fact]
    public void InsertMenuItem_InsertsItemAtIndex()
    {
        var set = new CliMenuSet();
        var command1 = new CliMenuCommand("Item1");
        var command2 = new CliMenuCommand("Item2");
        set.AddMenuItem(command1);
        set.InsertMenuItem(0, command2);
        Assert.Equal("Item2", set.GetMenuItemWithName("Item2")?.MenuItemName);
        Assert.Equal("Item1", set.GetMenuItemWithName("Item1")?.MenuItemName);
    }

    [Fact]
    public void AddMenuItem_WithKey_AddsItemWithKey()
    {
        var set = new CliMenuSet();
        var command = new CliMenuCommand("Item1");
        set.AddMenuItem("A", command, 1);
        var item = set.GetMenuItemWithName("Item1");
        Assert.NotNull(item);
        Assert.Equal("A", item.Key);
    }

    [Fact]
    public void AddMenuItem_WithKey_InvalidLength_AddsError()
    {
        var set = new CliMenuSet();
        var command = new CliMenuCommand("Item1");
        set.AddMenuItem("ABC", command, 2);
        // Error message is written to _errorMessages, but not exposed publicly.
        // This test ensures item is not added.
        Assert.Null(set.GetMenuItemWithName("Item1"));
    }

    [Fact]
    public void GetMenuItemWithName_ReturnsNullIfNotFound()
    {
        var set = new CliMenuSet();
        Assert.Null(set.GetMenuItemWithName("NotExist"));
    }

    [Fact]
    public void GetMenuItemByKey_ReturnsCorrectItemByKey()
    {
        var set = new CliMenuSet();
        var command = new CliMenuCommand("Item1");
        set.AddMenuItem(command, 1);
        var keyInfo = new ConsoleKeyInfo('0', ConsoleKey.D0, false, false, false);
        var item = set.GetMenuItemByKey(keyInfo);
        Assert.NotNull(item);
        Assert.Null(item.Key);
    }

    [Fact]
    public void GetMenuItemByKey_ReturnsNullForInvalidKey()
    {
        var set = new CliMenuSet();
        var keyInfo = new ConsoleKeyInfo('Z', ConsoleKey.Z, false, false, false);
        var item = set.GetMenuItemByKey(keyInfo);
        Assert.Null(item);
    }
}