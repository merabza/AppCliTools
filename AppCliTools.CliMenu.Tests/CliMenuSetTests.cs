using System;
using System.Collections.Generic;
using System.Reflection;

namespace AppCliTools.CliMenu.Tests;

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
        CliMenuItem? item = set.GetMenuItemWithName("Item1");
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
        CliMenuItem? item = set.GetMenuItemWithName("Item1");
        Assert.NotNull(item);
        Assert.Equal("A", item.Key);
    }

    //the third argument is the id of the item: the overload that checked the key length is gone,
    //Show itself cuts a key to its first 3 characters
    [Fact]
    public void AddMenuItem_WithKeyAndId_KeepsTheKeyAndTheId()
    {
        var set = new CliMenuSet();
        var command = new CliMenuCommand("Item1");
        set.AddMenuItem("ABCD", command, 2);
        CliMenuItem? item = set.GetMenuItemWithName("Item1");
        Assert.NotNull(item);
        Assert.Equal("ABCD", item.Key);
        Assert.Equal(2, item.CountedId);
    }

    [Fact]
    public void GetMenuItemWithName_ReturnsNullIfNotFound()
    {
        var set = new CliMenuSet();
        Assert.Null(set.GetMenuItemWithName("NotExist"));
    }

    //keys select from the page the menu showed last, so nothing can be selected before Show
    [Fact]
    public void GetMenuItemByKey_WhenMenuWasNotShown_ReturnsNull()
    {
        var set = new CliMenuSet();
        set.AddMenuItem(new CliMenuCommand("Item1"), 1);
        var keyInfo = new ConsoleKeyInfo('0', ConsoleKey.D0, false, false, false);
        Assert.Null(set.GetMenuItemByKey(keyInfo));
    }

    //a digit selects an item without its own key by its number on the shown page
    [Fact]
    public void GetMenuItemByKey_WhenItemIsShown_ReturnsItByItsNumber()
    {
        var set = new CliMenuSet();
        var command = new CliMenuCommand("Item1");
        set.AddMenuItem(command, 1);
        ShowAllItems(set);
        var keyInfo = new ConsoleKeyInfo('0', ConsoleKey.D0, false, false, false);
        CliMenuItem? item = set.GetMenuItemByKey(keyInfo);
        Assert.NotNull(item);
        Assert.Same(command, item.CliMenuCommand);
        Assert.Null(item.Key);
        Assert.Equal("0", item.CountedKey);
        Assert.Equal(0, item.CountedId);
    }

    [Fact]
    public void GetMenuItemByKey_ReturnsNullForInvalidKey()
    {
        var set = new CliMenuSet();
        var keyInfo = new ConsoleKeyInfo('Z', ConsoleKey.Z, false, false, false);
        CliMenuItem? item = set.GetMenuItemByKey(keyInfo);
        Assert.Null(item);
    }

    //Show needs a real console (window size, Console.Clear), so the shown page is set the way Show sets it
    //when all items fit on one page
    private static void ShowAllItems(CliMenuSet set)
    {
        PropertyInfo menuItems =
            typeof(CliMenuSet).GetProperty("MenuItems", BindingFlags.Instance | BindingFlags.NonPublic)!;
        PropertyInfo menuItemsShown =
            typeof(CliMenuSet).GetProperty("MenuItemsShown", BindingFlags.Instance | BindingFlags.NonPublic)!;
        menuItemsShown.SetValue(set, new List<CliMenuItem>((List<CliMenuItem>)menuItems.GetValue(set)!));
    }
}
