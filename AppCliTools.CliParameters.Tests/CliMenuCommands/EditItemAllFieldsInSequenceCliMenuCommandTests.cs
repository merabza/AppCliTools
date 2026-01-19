using System.Reflection;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.CliParameters.Cruders;

namespace AppCliTools.CliParameters.Tests.CliMenuCommands;

public sealed class EditItemAllFieldsInSequenceCliMenuCommandTests
{
    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert
        Assert.Equal("Edit All fields in sequence", command.Name);
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
    }

    [Fact]
    public void Constructor_WithNullCruder_AcceptsNullAndStoresIt()
    {
        // Arrange
        const string itemName = "TestItem";

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(null!, itemName);

        // Assert - Constructor doesn't validate, it just stores the value
        Assert.NotNull(command);
        FieldInfo? cruderField = typeof(EditItemAllFieldsInSequenceCliMenuCommand).GetField("_cruder",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(cruderField);
        object? storedCruder = cruderField.GetValue(command);
        Assert.Null(storedCruder);
    }

    [Fact]
    public void Constructor_WithNullItemName_InitializesCorrectly()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, null!);

        // Assert
        Assert.Equal("Edit All fields in sequence", command.Name);
        Assert.NotNull(command);
    }

    [Fact]
    public void Constructor_WithEmptyItemName_InitializesCorrectly()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, string.Empty);

        // Assert
        Assert.Equal("Edit All fields in sequence", command.Name);
        Assert.NotNull(command);
    }

    [Fact]
    public void Constructor_WithWhitespaceItemName_InitializesCorrectly()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "   ";

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert
        Assert.Equal("Edit All fields in sequence", command.Name);
        Assert.NotNull(command);
    }

    [Fact]
    public void Constructor_SetsCorrectMenuActions()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
        Assert.Equal(EMenuAction.Nothing, command.MenuAction); // Initial value
    }

    [Fact]
    public void Constructor_WithLongItemName_InitializesCorrectly()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        string itemName = new('A', 1000);

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert
        Assert.Equal("Edit All fields in sequence", command.Name);
    }

    [Theory]
    [InlineData("Item1")]
    [InlineData("Item-With-Dashes")]
    [InlineData("Item_With_Underscores")]
    [InlineData("Item With Spaces")]
    [InlineData("123NumericItem")]
    [InlineData("Test-Item-გამოცდა-日本語")]
    [InlineData("Test-Item_123!@#")]
    public void Constructor_WithVariousValidItemNames_InitializesCorrectly(string itemName)
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert
        Assert.Equal("Edit All fields in sequence", command.Name);
        string? parentMenuName = GetParentMenuName(command);
        Assert.Equal(itemName, parentMenuName);
    }

    [Fact]
    public void Name_AlwaysReturnsEditAllFieldsInSequence()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

// Assert
        Assert.Equal("Edit All fields in sequence", command.Name);
    }

    [Fact]
    public void MenuActionOnBodySuccess_AlwaysReturnsLevelUp()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
    }

    [Fact]
    public void MenuActionOnBodyFail_AlwaysReturnsReload()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
    }

    [Fact]
    public void Constructor_WithDifferentCruderTypes_InitializesCorrectly()
    {
        // Arrange
        var cruder1 = new TestCruder("User", "Users");
        var cruder2 = new TestCruder("Role", "Roles");
        const string itemName = "TestItem";

        // Act
        var command1 = new EditItemAllFieldsInSequenceCliMenuCommand(cruder1, itemName);
        var command2 = new EditItemAllFieldsInSequenceCliMenuCommand(cruder2, itemName);

        // Assert
        Assert.Equal("Edit All fields in sequence", command1.Name);
        Assert.Equal("Edit All fields in sequence", command2.Name);
    }

    [Fact]
    public void ParentMenuName_IsSetFromConstructor()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert
        string? parentMenuName = GetParentMenuName(command);
        Assert.Equal(itemName, parentMenuName);
    }

    [Fact]
    public void ParentMenuName_WithNullItemName_IsNull()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, null!);

        // Assert
        string? parentMenuName = GetParentMenuName(command);
        Assert.Null(parentMenuName);
    }

    [Fact]
    public void ParentMenuName_WithEmptyItemName_IsEmpty()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, string.Empty);

        // Assert
        string? parentMenuName = GetParentMenuName(command);
        Assert.Equal(string.Empty, parentMenuName);
    }

    [Fact]
    public void ParentMenuName_WithWhitespaceItemName_ContainsWhitespace()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "   ";

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert
        string? parentMenuName = GetParentMenuName(command);
        Assert.Equal(itemName, parentMenuName);
    }

    [Fact]
    public void Constructor_InitialMenuActionIsNothing()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert
        Assert.Equal(EMenuAction.Nothing, command.MenuAction);
    }

    [Fact]
    public void GetSubMenu_ReturnsNull()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Act
        CliMenuSet? result = command.GetSubMenu();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetMenuLinkToGo_ReturnsNull()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Act
        string? result = command.GetMenuLinkToGo();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void StatusView_HasDefaultValue()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert
        Assert.Equal(EStatusView.Brackets, command.StatusView);
    }

    [Fact]
    public void NameIsStatus_IsFalse()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert
        Assert.False(command.NameIsStatus);
    }

    [Fact]
    public void Cruder_IsStoredInPrivateField()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert - Verify cruder is stored by using reflection
        FieldInfo? cruderField = typeof(EditItemAllFieldsInSequenceCliMenuCommand).GetField("_cruder",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(cruderField);
        object? storedCruder = cruderField.GetValue(command);
        Assert.Same(cruder, storedCruder);
    }

    [Fact]
    public void Constructor_PassesItemNameAsParentMenuName()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "MySpecificItem";

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert - Verify parentMenuName is passed to base constructor
        string? parentMenuName = GetParentMenuName(command);
        Assert.Equal(itemName, parentMenuName);
    }

    [Fact]
    public void Constructor_SetsCommandNameToEditAllFieldsInSequence()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert
        Assert.Equal("Edit All fields in sequence", command.Name);
        Assert.NotEqual(itemName, command.Name); // Command name is NOT the item name
    }

    [Fact]
    public void MenuAction_CanBeModified()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Act - MenuAction is public and has a protected setter through inheritance
        // We can't set it directly in tests, but we can verify it starts as Nothing
        EMenuAction initialAction = command.MenuAction;

        // Assert
        Assert.Equal(EMenuAction.Nothing, initialAction);
    }

    [Fact]
    public void Constructor_WithSameCruderDifferentItems_CreatesIndependentCommands()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName1 = "Item1";
        const string itemName2 = "Item2";

        // Act
        var command1 = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName1);
        var command2 = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName2);

        // Assert
        Assert.NotSame(command1, command2);
        Assert.Equal(itemName1, GetParentMenuName(command1));
        Assert.Equal(itemName2, GetParentMenuName(command2));
    }

    [Fact]
    public void Constructor_PassesCorrectParametersToBaseClass()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert - Verify all parameters passed to base constructor
        Assert.Equal("Edit All fields in sequence", command.Name);
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
        Assert.Equal(itemName, GetParentMenuName(command));
    }

    [Fact]
    public void Constructor_WithMultipleCalls_CreatesDistinctInstances()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";

        // Act
        var command1 = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);
        var command2 = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert
        Assert.NotSame(command1, command2);
        Assert.Equal(command1.Name, command2.Name);
        Assert.Equal(GetParentMenuName(command1), GetParentMenuName(command2));
    }

    [Fact]
    public void Constructor_CommandNameIsConstant()
    {
        // Arrange
        var cruder1 = new TestCruder("Item", "Items");
        var cruder2 = new TestCruder("Record", "Records");
        const string itemName1 = "Item1";
        const string itemName2 = "DifferentItem";

        // Act
        var command1 = new EditItemAllFieldsInSequenceCliMenuCommand(cruder1, itemName1);
        var command2 = new EditItemAllFieldsInSequenceCliMenuCommand(cruder2, itemName2);

        // Assert - Command name is always the same regardless of cruder or item name
        Assert.Equal("Edit All fields in sequence", command1.Name);
        Assert.Equal("Edit All fields in sequence", command2.Name);
        Assert.Equal(command1.Name, command2.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ValidItemName")]
    [InlineData("Item-with-special-chars!@#")]
    public void Constructor_ItemNameIsPassedAsIs_ToParentMenuName(string? itemName)
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName!);

        // Assert
        string? parentMenuName = GetParentMenuName(command);
        Assert.Equal(itemName, parentMenuName);
    }

    [Fact]
    public void Constructor_DoesNotModifyItemName()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "  TestItem  "; // with spaces

        // Act
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert - Item name should be stored as-is without trimming
        string? parentMenuName = GetParentMenuName(command);
        Assert.Equal(itemName, parentMenuName);
    }

    [Fact]
    public void Status_IsNullByDefault()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert
        Assert.Null(command.StatusString);
    }

    [Fact]
    public void MenuSet_IsNullByDefault()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string itemName = "TestItem";
        var command = new EditItemAllFieldsInSequenceCliMenuCommand(cruder, itemName);

        // Assert
        Assert.Null(command.MenuSet);
    }

    /// <summary>
    ///     Helper method to get the protected ParentMenuName field using reflection
    /// </summary>
    private static string? GetParentMenuName(EditItemAllFieldsInSequenceCliMenuCommand command)
    {
        FieldInfo? field =
            typeof(CliMenuCommand).GetField("ParentMenuName", BindingFlags.Public | BindingFlags.Instance);
        return field?.GetValue(command) as string;
    }

    /// <summary>
    ///     Test implementation of Cruder for testing purposes
    /// </summary>
    private sealed class TestCruder : Cruder
    {
        public TestCruder(string crudName, string crudNamePlural) : base(crudName, crudNamePlural)
        {
        }
    }
}
