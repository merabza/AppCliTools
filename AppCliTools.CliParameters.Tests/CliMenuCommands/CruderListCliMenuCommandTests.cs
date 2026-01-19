using System;
using System.Collections.Generic;
using System.Linq;
using CliMenu;
using CliParameters.CliMenuCommands;
using CliParameters.Cruders;
using LibParameters;

namespace CliParameters.Tests.CliMenuCommands;

public sealed class CruderListCliMenuCommandTests
{
    [Fact]
    public void Constructor_WithValidCruder_InitializesCorrectly()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items", 5);

        // Act
        var command = new CruderListCliMenuCommand(cruder);

        // Assert
        Assert.Equal("Items", command.Name);
        Assert.Equal(EMenuAction.Nothing, command.MenuAction);
        Assert.Equal(EMenuAction.LoadSubMenu, command.MenuActionOnBodySuccess);
    }

    [Fact]
    public void Constructor_WithNullCruder_ThrowsNullReferenceException()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(() => new CruderListCliMenuCommand(null!));
    }

    [Fact]
    public void GetSubMenu_ReturnsCliMenuSetFromCruder()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items", 3);
        var command = new CruderListCliMenuCommand(cruder);

        // Act
        var result = command.GetSubMenu();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, cruder.GetListMenuCallCount);
    }

    [Fact]
    public void GetSubMenu_CalledMultipleTimes_InvokesCruderEachTime()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items", 2);
        var command = new CruderListCliMenuCommand(cruder);

        // Act
        var result1 = command.GetSubMenu();
        var result2 = command.GetSubMenu();

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);

        //GetSubMenu მენიუს ქმნის მხოლოდ იმ შემთხვევაში თუ ჯერ არ არის შექმნილი. ამიტომ GetListMenuCallCount უნდა იყოს 1
        Assert.Equal(1, cruder.GetListMenuCallCount);
    }

    [Fact]
    public void GetStatus_ReturnsCountAsString()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items", 42);
        var command = new CruderListCliMenuCommand(cruder);

        // Act
        command.CountStatus();

        // Assert
        Assert.Equal("42", command.Status);
    }

    [Fact]
    public void GetStatus_WithZeroCount_ReturnsZero()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items", 0);
        var command = new CruderListCliMenuCommand(cruder);

        // Act
        command.CountStatus();

        // Assert
        Assert.Equal("0", command.Status);
    }

    [Fact]
    public void GetStatus_WithSmallCount_ReturnsCorrectValue()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items", 1);
        var command = new CruderListCliMenuCommand(cruder);

        // Act
        command.CountStatus();

        // Assert
        Assert.Equal("1", command.Status);
    }

    [Fact]
    public void GetStatus_WithLargeCount_ReturnsCorrectString()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items", 999999);
        var command = new CruderListCliMenuCommand(cruder);

        // Act
        command.CountStatus();

        // Assert
        Assert.Equal("999999", command.Status);
    }

    [Fact]
    public void Constructor_UsesCrudNamePluralAsCommandName()
    {
        // Arrange
        var cruder = new TestCruder("Person", "People", 10);

        // Act
        var command = new CruderListCliMenuCommand(cruder);

        // Assert
        Assert.Equal("People", command.Name);
    }

    [Fact]
    public void Constructor_SetsMenuActionToLoadSubMenu()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items", 5);

        // Act
        var command = new CruderListCliMenuCommand(cruder);

        // Assert - MenuAction starts as Nothing, MenuActionOnBodySuccess is LoadSubMenu
        Assert.Equal(EMenuAction.Nothing, command.MenuAction);
        Assert.Equal(EMenuAction.LoadSubMenu, command.MenuActionOnBodySuccess);
    }

    [Fact]
    public void CountStatus_UpdatesStatusProperty()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items", 15);
        var command = new CruderListCliMenuCommand(cruder);

        // Verify initial state
        Assert.Null(command.Status);

        // Act
        command.CountStatus();

        // Assert
        Assert.NotNull(command.Status);
        Assert.Equal("15", command.Status);
    }

    [Fact]
    public void CountStatus_CalledMultipleTimes_ReflectsCurrentCount()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items", 10);
        var command = new CruderListCliMenuCommand(cruder);

        // Act & Assert - First call
        command.CountStatus();
        Assert.Equal("10", command.Status);

        // Update the count
        cruder.SetKeyCount(20);
        command.CountStatus();
        Assert.Equal("20", command.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void GetStatus_WithVariousCounts_ReturnsCorrectString(int count)
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items", count);
        var command = new CruderListCliMenuCommand(cruder);

        // Act
        command.CountStatus();

        // Assert
        Assert.Equal(count.ToString(), command.Status);
    }

    [Fact]
    public void Name_MatchesCruderCrudNamePlural()
    {
        // Arrange
        var cruder = new TestCruder("Database", "Databases", 7);

        // Act
        var command = new CruderListCliMenuCommand(cruder);

        // Assert
        Assert.Equal(cruder.CrudNamePlural, command.Name);
    }

    [Fact]
    public void GetSubMenu_ReturnsValidCliMenuSet()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items", 3);
        var command = new CruderListCliMenuCommand(cruder);

        // Act
        var result = command.GetSubMenu();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CliMenuSet>(result);
    }

    [Fact]
    public void MenuActionOnBodySuccess_HasDefaultValue()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items", 5);
        var command = new CruderListCliMenuCommand(cruder);

        // Assert - Constructor passes LoadSubMenu as menuActionOnBodySuccess
        Assert.Equal(EMenuAction.LoadSubMenu, command.MenuActionOnBodySuccess);
    }

    [Fact]
    public void MenuActionOnBodyFail_HasDefaultValue()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items", 5);
        var command = new CruderListCliMenuCommand(cruder);

        // Assert - Default value from base class constructor is Reload
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
    }

    [Fact]
    public void GetStatus_MultipleCallsWithSameCount_ReturnsSameValue()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items", 7);
        var command = new CruderListCliMenuCommand(cruder);

        // Act
        command.CountStatus();
        var firstStatus = command.Status;
        command.CountStatus();
        var secondStatus = command.Status;

        // Assert
        Assert.Equal(firstStatus, secondStatus);
        Assert.Equal("7", firstStatus);
    }

    [Fact]
    public void Constructor_WithEmptyPluralName_InitializesWithEmptyName()
    {
        // Arrange
        var cruder = new TestCruder("Item", "", 5);

        // Act
        var command = new CruderListCliMenuCommand(cruder);

        // Assert
        Assert.Equal("", command.Name);
    }

    [Fact]
    public void GetStatus_AfterCountChanges_ReturnsUpdatedValue()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items", 5);
        var command = new CruderListCliMenuCommand(cruder);

        command.CountStatus();
        Assert.Equal("5", command.Status);

        // Act - Change the count
        cruder.SetKeyCount(3);
        command.CountStatus();

        // Assert
        Assert.Equal("3", command.Status);
    }

    [Fact]
    public void GetSubMenu_WithDifferentCruderNames_ReturnsCorrectMenu()
    {
        // Arrange
        var cruder1 = new TestCruder("User", "Users", 5);
        var cruder2 = new TestCruder("Role", "Roles", 3);

        var command1 = new CruderListCliMenuCommand(cruder1);
        var command2 = new CruderListCliMenuCommand(cruder2);

        // Act
        var menu1 = command1.GetSubMenu();
        var menu2 = command2.GetSubMenu();

        // Assert
        Assert.NotNull(menu1);
        Assert.NotNull(menu2);
        Assert.NotSame(menu1, menu2);
    }

    [Fact]
    public void CountStatus_BeforeAndAfter_ShowsCorrectTransition()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items", 0);
        var command = new CruderListCliMenuCommand(cruder);

        // Initially null
        Assert.Null(command.Status);

        // Act - First count
        command.CountStatus();
        Assert.Equal("0", command.Status);

        // Add items
        cruder.SetKeyCount(5);
        command.CountStatus();

        // Assert
        Assert.Equal("5", command.Status);
    }

    /// <summary>
    ///     Test implementation of Cruder for testing purposes
    /// </summary>
    private sealed class TestCruder : Cruder
    {
        private readonly Dictionary<string, ItemData> _items = new();

        public TestCruder(string crudName, string crudNamePlural, int keyCount) : base(crudName, crudNamePlural)
        {
            SetKeyCount(keyCount);
        }

        public int GetListMenuCallCount { get; private set; }

        protected override Dictionary<string, ItemData> GetCrudersDictionary()
        {
            GetListMenuCallCount++;
            return _items;
        }

        public override List<string> GetKeys()
        {
            return _items.Keys.ToList();
        }

        public void SetKeyCount(int count)
        {
            _items.Clear();
            for (var i = 0; i < count; i++) _items.Add($"Key{i}", new TestItemData());
        }

        private sealed class TestItemData : ItemData
        {
        }
    }
}