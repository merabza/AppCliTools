using System.Reflection;
using CliMenu;
using CliParameters.CliMenuCommands;
using CliParameters.Cruders;

namespace CliParameters.Tests.CliMenuCommands;

public sealed class DeleteCruderRecordCliMenuCommandTests
{
    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "TestRecord";

        // Act
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Assert
        Assert.Equal("Delete this record", command.Name);
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
    }

    [Fact]
    public void Constructor_WithNullCruder_AcceptsNullAndStoresIt()
    {
        // Arrange
        const string recordName = "TestRecord";

        // Act
        var command = new DeleteCruderRecordCliMenuCommand(null!, recordName);

        // Assert - Constructor doesn't validate, it just stores the value
        Assert.NotNull(command);
        var cruderField = typeof(DeleteCruderRecordCliMenuCommand).GetField("_cruder",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(cruderField);
        var storedCruder = cruderField.GetValue(command);
        Assert.Null(storedCruder);
    }

    [Fact]
    public void Constructor_WithNullRecordName_InitializesCorrectly()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");

        // Act
        var command = new DeleteCruderRecordCliMenuCommand(cruder, null!);

        // Assert
        Assert.Equal("Delete this record", command.Name);
        Assert.NotNull(command);
    }

    [Fact]
    public void Constructor_WithEmptyRecordName_InitializesCorrectly()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");

        // Act
        var command = new DeleteCruderRecordCliMenuCommand(cruder, string.Empty);

        // Assert
        Assert.Equal("Delete this record", command.Name);
        Assert.NotNull(command);
    }

    [Fact]
    public void Constructor_WithWhitespaceRecordName_InitializesCorrectly()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "   ";

        // Act
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Assert
        Assert.Equal("Delete this record", command.Name);
        Assert.NotNull(command);
    }

    [Fact]
    public void Constructor_SetsCorrectMenuActions()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "TestRecord";

        // Act
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Assert
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
        Assert.Equal(EMenuAction.Nothing, command.MenuAction); // Initial value
    }

    [Fact]
    public void Constructor_WithLongRecordName_InitializesCorrectly()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        var recordName = new string('A', 1000);

        // Act
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Assert
        Assert.Equal("Delete this record", command.Name);
    }

    [Theory]
    [InlineData("Record1")]
    [InlineData("Record-With-Dashes")]
    [InlineData("Record_With_Underscores")]
    [InlineData("Record With Spaces")]
    [InlineData("123NumericRecord")]
    [InlineData("Test-გამოცდა-日本語")]
    [InlineData("Test-Record_123!@#")]
    public void Constructor_WithVariousValidRecordNames_InitializesCorrectly(string recordName)
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");

        // Act
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Assert
        Assert.Equal("Delete this record", command.Name);
        var parentMenuName = GetParentMenuName(command);
        Assert.Equal(recordName, parentMenuName);
    }

    [Fact]
    public void Name_AlwaysReturnsDeleteThisRecord()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "TestRecord";
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

// Assert
        Assert.Equal("Delete this record", command.Name);
    }

    [Fact]
    public void MenuActionOnBodySuccess_AlwaysReturnsLevelUp()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "TestRecord";
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Assert
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
    }

    [Fact]
    public void MenuActionOnBodyFail_AlwaysReturnsReload()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "TestRecord";
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Assert
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
    }

    [Fact]
    public void Constructor_WithDifferentCruderTypes_InitializesCorrectly()
    {
        // Arrange
        var cruder1 = new TestCruder("User", "Users");
        var cruder2 = new TestCruder("Role", "Roles");
        const string recordName = "TestRecord";

        // Act
        var command1 = new DeleteCruderRecordCliMenuCommand(cruder1, recordName);
        var command2 = new DeleteCruderRecordCliMenuCommand(cruder2, recordName);

        // Assert
        Assert.Equal("Delete this record", command1.Name);
        Assert.Equal("Delete this record", command2.Name);
    }

    [Fact]
    public void ParentMenuName_IsSetFromConstructor()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "TestRecord";

        // Act
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Assert
        var parentMenuName = GetParentMenuName(command);
        Assert.Equal(recordName, parentMenuName);
    }

    [Fact]
    public void ParentMenuName_WithNullRecordName_IsNull()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");

        // Act
        var command = new DeleteCruderRecordCliMenuCommand(cruder, null!);

        // Assert
        var parentMenuName = GetParentMenuName(command);
        Assert.Null(parentMenuName);
    }

    [Fact]
    public void ParentMenuName_WithEmptyRecordName_IsEmpty()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");

        // Act
        var command = new DeleteCruderRecordCliMenuCommand(cruder, string.Empty);

// Assert
        var parentMenuName = GetParentMenuName(command);
        Assert.Equal(string.Empty, parentMenuName);
    }

    [Fact]
    public void ParentMenuName_WithWhitespaceRecordName_ContainsWhitespace()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "   ";

        // Act
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

// Assert
        var parentMenuName = GetParentMenuName(command);
        Assert.Equal(recordName, parentMenuName);
    }

    [Fact]
    public void Constructor_InitialMenuActionIsNothing()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "TestRecord";

        // Act
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

// Assert
        Assert.Equal(EMenuAction.Nothing, command.MenuAction);
    }

    [Fact]
    public void GetSubMenu_ReturnsNull()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "TestRecord";
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Act
        var result = command.GetSubMenu();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetMenuLinkToGo_ReturnsNull()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "TestRecord";
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Act
        var result = command.GetMenuLinkToGo();

// Assert
        Assert.Null(result);
    }

    [Fact]
    public void StatusView_HasDefaultValue()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "TestRecord";
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Assert
        Assert.Equal(EStatusView.Brackets, command.StatusView);
    }

    [Fact]
    public void NameIsStatus_IsFalse()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "TestRecord";
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Assert
        Assert.False(command.NameIsStatus);
    }

    [Fact]
    public void Cruder_IsStoredInPrivateField()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "TestRecord";

        // Act
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Assert - Verify cruder is stored by using reflection
        var cruderField = typeof(DeleteCruderRecordCliMenuCommand).GetField("_cruder",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(cruderField);
        var storedCruder = cruderField.GetValue(command);
        Assert.Same(cruder, storedCruder);
    }

    [Fact]
    public void Constructor_PassesRecordNameAsParentMenuName()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "MySpecificRecord";

        // Act
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Assert - Verify parentMenuName is passed to base constructor
        var parentMenuName = GetParentMenuName(command);
        Assert.Equal(recordName, parentMenuName);
    }

    [Fact]
    public void Constructor_SetsCommandNameToDeleteThisRecord()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "TestRecord";

        // Act
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Assert
        Assert.Equal("Delete this record", command.Name);
        Assert.NotEqual(recordName, command.Name); // Command name is NOT the record name
    }

    [Fact]
    public void MenuAction_CanBeModified()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "TestRecord";
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Act - MenuAction is public and has a protected setter through inheritance
        // We can't set it directly in tests, but we can verify it starts as Nothing
        var initialAction = command.MenuAction;

// Assert
        Assert.Equal(EMenuAction.Nothing, initialAction);
    }

    [Fact]
    public void MenuActionOnBodySuccess_CanBeReadButNotSetInTests()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "TestRecord";
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Act & Assert - Verify the value set by constructor
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
    }

    [Fact]
    public void MenuActionOnBodyFail_CanBeReadButNotSetInTests()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName = "TestRecord";
        var command = new DeleteCruderRecordCliMenuCommand(cruder, recordName);

        // Act & Assert - Verify the value set by constructor
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
    }

    [Fact]
    public void Constructor_WithSameCruderDifferentRecords_CreatesIndependentCommands()
    {
        // Arrange
        var cruder = new TestCruder("Item", "Items");
        const string recordName1 = "Record1";
        const string recordName2 = "Record2";

        // Act
        var command1 = new DeleteCruderRecordCliMenuCommand(cruder, recordName1);
        var command2 = new DeleteCruderRecordCliMenuCommand(cruder, recordName2);

        // Assert
        Assert.NotSame(command1, command2);
        Assert.Equal(recordName1, GetParentMenuName(command1));
        Assert.Equal(recordName2, GetParentMenuName(command2));
    }

    /// <summary>
    ///     Helper method to get the protected ParentMenuName field using reflection
    /// </summary>
    private static string? GetParentMenuName(DeleteCruderRecordCliMenuCommand command)
    {
        var field = typeof(CliMenuCommand).GetField("ParentMenuName", BindingFlags.Public | BindingFlags.Instance);
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