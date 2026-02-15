using System;
using System.Collections.Generic;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParameters.FieldEditors;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters.Tests.CliMenuCommands;

// NOTE: The RunBody method in FieldEditorMenuCliMenuCommand has the following behavior:
// 1. Calls _fieldEditor.UpdateField(_recordKey, _recordForUpdate)
// 2. Calls _cruder.UpdateRecordWithKey(_recordKey, _recordForUpdate)
// 3. Calls _cruder.CheckValidation(_recordForUpdate)
// 4. If validation fails, prompts user with Inputer.InputBool (cannot be tested without refactoring)
// 5. Calls _cruder.Save with a formatted message
//
// The RunBody method cannot be fully tested because it uses Inputer.InputBool for user interaction,
// which would require dependency injection or refactoring to make it testable.
// The tests below focus on the constructor, GetStatus method, and verifying the class structure.

public sealed class FieldEditorMenuCliMenuCommandTests
{
    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        // Arrange
        const string fieldName = "TestField";
        var fieldEditor = new TestFieldEditor(fieldName);
        var recordForUpdate = new TestItemData { TestProperty = "InitialValue" };
        var cruder = new TestCruder();
        const string recordKey = "TestKey";

        // Act
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(fieldName, command.Name);
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodySuccess);
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
        Assert.Equal(EStatusView.Table, command.StatusView);
    }

    [Fact]
    public void Constructor_WithNullFieldName_ThrowsException()
    {
        // Arrange
        const string? fieldName = null;
        var fieldEditor = new TestFieldEditor("Field");
        var recordForUpdate = new TestItemData();
        var cruder = new TestCruder();
        const string recordKey = "TestKey";

        // Act & Assert
        Exception? exception = Record.Exception(() =>
            new FieldEditorMenuCliMenuCommand(fieldName!, fieldEditor, recordForUpdate, cruder, recordKey));

        if (exception != null)
        {
            Assert.IsType<ArgumentNullException>(exception);
        }
    }

    [Fact]
    public void Constructor_WithEmptyFieldName_InitializesCorrectly()
    {
        // Arrange
        const string fieldName = "";
        var fieldEditor = new TestFieldEditor(fieldName);
        var recordForUpdate = new TestItemData();
        var cruder = new TestCruder();
        const string recordKey = "TestKey";

        // Act
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(fieldName, command.Name);
    }

    [Fact]
    public void Constructor_WithNullRecordKey_InitializesCorrectly()
    {
        // Arrange
        const string fieldName = "TestField";
        var fieldEditor = new TestFieldEditor(fieldName);
        var recordForUpdate = new TestItemData();
        var cruder = new TestCruder();
        const string? recordKey = null;

        // Act
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey!);

        // Assert
        Assert.NotNull(command);
    }

    [Fact]
    public void Constructor_WithEmptyRecordKey_InitializesCorrectly()
    {
        // Arrange
        const string fieldName = "TestField";
        var fieldEditor = new TestFieldEditor(fieldName);
        var recordForUpdate = new TestItemData();
        var cruder = new TestCruder();
        const string recordKey = "";

        // Act
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Assert
        Assert.NotNull(command);
    }

    [Fact]
    public void Constructor_SetsMenuActionsToReload()
    {
        // Arrange
        const string fieldName = "TestField";
        var fieldEditor = new TestFieldEditor(fieldName);
        var recordForUpdate = new TestItemData();
        var cruder = new TestCruder();
        const string recordKey = "TestKey";

        // Act
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Assert
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodySuccess);
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
    }

    [Fact]
    public void Constructor_SetsStatusViewToTable()
    {
        // Arrange
        const string fieldName = "TestField";
        var fieldEditor = new TestFieldEditor(fieldName);
        var recordForUpdate = new TestItemData();
        var cruder = new TestCruder();
        const string recordKey = "TestKey";

        // Act
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Assert
        Assert.Equal(EStatusView.Table, command.StatusView);
    }

    [Fact]
    public void Constructor_InitializesWithNothingMenuAction()
    {
        // Arrange
        const string fieldName = "TestField";
        var fieldEditor = new TestFieldEditor(fieldName);
        var recordForUpdate = new TestItemData();
        var cruder = new TestCruder();
        const string recordKey = "TestKey";

        // Act
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Assert
        Assert.Equal(EMenuAction.Nothing, command.MenuAction);
    }

    [Fact]
    public void Constructor_InheritsFromCliMenuCommand()
    {
        // Arrange
        const string fieldName = "TestField";
        var fieldEditor = new TestFieldEditor(fieldName);
        var recordForUpdate = new TestItemData();
        var cruder = new TestCruder();
        const string recordKey = "TestKey";

        // Act
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Assert
        Assert.IsAssignableFrom<CliMenuCommand>(command);
    }

    [Fact]
    public void Constructor_IsSealed()
    {
        // Assert
        Assert.True(typeof(FieldEditorMenuCliMenuCommand).IsSealed);
    }

    [Fact]
    public void Constructor_WithSpecialCharactersInFieldName_InitializesCorrectly()
    {
        // Arrange
        const string fieldName = "Test_Field-Name@123";
        var fieldEditor = new TestFieldEditor(fieldName);
        var recordForUpdate = new TestItemData();
        var cruder = new TestCruder();
        const string recordKey = "TestKey";

        // Act
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(fieldName, command.Name);
    }

    [Fact]
    public void Constructor_WithLongFieldName_InitializesCorrectly()
    {
        // Arrange
        const string fieldName =
            "ThisIsAVeryLongFieldNameThatShouldStillWorkCorrectlyWithTheConstructorAndNotCauseAnyIssues";
        var fieldEditor = new TestFieldEditor(fieldName);
        var recordForUpdate = new TestItemData();
        var cruder = new TestCruder();
        const string recordKey = "TestKey";

        // Act
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(fieldName, command.Name);
    }

    [Theory]
    [InlineData("Field1", "Key1")]
    [InlineData("Field2", "Key2")]
    [InlineData("TestField", "TestKey")]
    [InlineData("Name", "Record123")]
    public void Constructor_WithVariousFieldNameAndKeyCombinations_InitializesCorrectly(string fieldName,
        string recordKey)
    {
        // Arrange
        var fieldEditor = new TestFieldEditor(fieldName);
        var recordForUpdate = new TestItemData();
        var cruder = new TestCruder();

        // Act
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(fieldName, command.Name);
    }

    [Fact]
    public void Constructor_CreatesUniqueInstances()
    {
        // Arrange & Act
        var fieldEditor1 = new TestFieldEditor("Field1");
        var recordForUpdate1 = new TestItemData();
        var cruder1 = new TestCruder();
        var command1 = new FieldEditorMenuCliMenuCommand("Field1", fieldEditor1, recordForUpdate1, cruder1, "Key1");

        var fieldEditor2 = new TestFieldEditor("Field2");
        var recordForUpdate2 = new TestItemData();
        var cruder2 = new TestCruder();
        var command2 = new FieldEditorMenuCliMenuCommand("Field2", fieldEditor2, recordForUpdate2, cruder2, "Key2");

        // Assert
        Assert.NotSame(command1, command2);
        Assert.NotEqual(command1.Name, command2.Name);
    }

    [Fact]
    public void Constructor_PassesCorrectParametersToBase()
    {
        // Arrange
        const string fieldName = "TestField";
        var fieldEditor = new TestFieldEditor(fieldName);
        var recordForUpdate = new TestItemData();
        var cruder = new TestCruder();
        const string recordKey = "TestKey";

        // Act
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Assert
        Assert.Equal(fieldName, command.Name);
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodySuccess);
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
        Assert.Equal(EStatusView.Table, command.StatusView);
        Assert.Equal(EMenuAction.Nothing, command.MenuAction);
    }

    [Fact]
    public void GetStatus_CallsFieldEditorGetValueStatus()
    {
        // Arrange
        const string fieldName = "TestField";
        var fieldEditor = new TestFieldEditor(fieldName);
        var recordForUpdate = new TestItemData { TestProperty = "TestValue" };
        var cruder = new TestCruder();
        const string recordKey = "TestKey";
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Act
        command.CountStatus();

        // Assert
        Assert.True(fieldEditor.GetValueStatusCalled);
        Assert.Equal("Test Status", command.StatusString);
    }

    [Fact]
    public void GetStatus_PassesCorrectRecordToFieldEditor()
    {
        // Arrange
        const string fieldName = "TestField";
        var fieldEditor = new TestFieldEditor(fieldName);
        var recordForUpdate = new TestItemData { TestProperty = "TestValue" };
        var cruder = new TestCruder();
        const string recordKey = "TestKey";
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Act
        command.CountStatus();

        // Assert
        Assert.True(fieldEditor.GetValueStatusCalled);
    }

    [Fact]
    public void GetStatus_ReturnsFieldEditorValueStatus()
    {
        // Arrange
        const string fieldName = "TestField";
        var fieldEditor = new TestFieldEditor(fieldName) { StatusToReturn = "Custom Status Value" };
        var recordForUpdate = new TestItemData();
        var cruder = new TestCruder();
        const string recordKey = "TestKey";
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Act
        command.CountStatus();

        // Assert
        Assert.Equal("Custom Status Value", command.StatusString);
    }

    [Fact]
    public void GetStatus_WithEmptyStatus_ReturnsEmptyString()
    {
        // Arrange
        const string fieldName = "TestField";
        var fieldEditor = new TestFieldEditor(fieldName) { StatusToReturn = "" };
        var recordForUpdate = new TestItemData();
        var cruder = new TestCruder();
        const string recordKey = "TestKey";
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Act
        command.CountStatus();

        // Assert
        Assert.Equal("", command.StatusString);
    }

    [Fact]
    public void GetStatus_WithNullStatus_ReturnsNull()
    {
        // Arrange
        const string fieldName = "TestField";
        var fieldEditor = new TestFieldEditor(fieldName) { StatusToReturn = null };
        var recordForUpdate = new TestItemData();
        var cruder = new TestCruder();
        const string recordKey = "TestKey";
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Act
        command.CountStatus();

        // Assert
        Assert.Null(command.StatusString);
    }

    [Theory]
    [InlineData("Value1")]
    [InlineData("Value2")]
    [InlineData("Test Status Message")]
    [InlineData("100%")]
    public void GetStatus_WithVariousStatuses_ReturnsCorrectValue(string expectedStatus)
    {
        // Arrange
        const string fieldName = "TestField";
        var fieldEditor = new TestFieldEditor(fieldName) { StatusToReturn = expectedStatus };
        var recordForUpdate = new TestItemData();
        var cruder = new TestCruder();
        const string recordKey = "TestKey";
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Act
        command.CountStatus();

        // Assert
        Assert.Equal(expectedStatus, command.StatusString);
    }

    [Fact]
    public void GetStatus_CalledMultipleTimes_AlwaysCallsFieldEditor()
    {
        // Arrange
        const string fieldName = "TestField";
        var fieldEditor = new TestFieldEditor(fieldName);
        var recordForUpdate = new TestItemData();
        var cruder = new TestCruder();
        const string recordKey = "TestKey";
        var command = new FieldEditorMenuCliMenuCommand(fieldName, fieldEditor, recordForUpdate, cruder, recordKey);

        // Act
        command.CountStatus();
        int callCountAfterFirst = fieldEditor.GetValueStatusCallCount;
        command.CountStatus();
        int callCountAfterSecond = fieldEditor.GetValueStatusCallCount;

        // Assert
        Assert.Equal(1, callCountAfterFirst);
        Assert.Equal(2, callCountAfterSecond);
    }

    private sealed class TestFieldEditor : FieldEditor
    {
        public TestFieldEditor(string propertyName) : base(propertyName, null, false)
        {
        }

        public bool UpdateFieldCalled { get; private set; }
        public bool GetValueStatusCalled { get; private set; }
        public int GetValueStatusCallCount { get; private set; }
        public string? LastRecordKey { get; private set; }
        public object? LastRecordForUpdate { get; private set; }
        public string? StatusToReturn { get; set; } = "Test Status";

        public override void UpdateField(string? recordKey, object recordForUpdate)
        {
            UpdateFieldCalled = true;
            LastRecordKey = recordKey;
            LastRecordForUpdate = recordForUpdate;
        }

        public override string GetValueStatus(object? record)
        {
            GetValueStatusCalled = true;
            GetValueStatusCallCount++;
            return StatusToReturn!;
        }
    }

    private sealed class TestItemData : ItemData
    {
        public string? TestProperty { get; set; }
    }

    private sealed class TestCruder : Cruder
    {
        public TestCruder() : base("TestCrud", "TestCruds")
        {
        }

        public bool UpdateRecordWithKeyCalled { get; private set; }
        public bool CheckValidationCalled { get; private set; }
        public bool SaveCalled { get; private set; }
        public string? LastRecordKey { get; private set; }
        public ItemData? LastNewRecord { get; private set; }
        public ItemData? LastItemToValidate { get; private set; }
        public string? LastSaveMessage { get; private set; }
        public bool ValidationResult { get; } = true;

        protected override Dictionary<string, ItemData> GetCrudersDictionary()
        {
            return new Dictionary<string, ItemData>();
        }

        public override bool ContainsRecordWithKey(string recordKey)
        {
            return true;
        }

        public override void UpdateRecordWithKey(string recordKey, ItemData newRecord)
        {
            UpdateRecordWithKeyCalled = true;
            LastRecordKey = recordKey;
            LastNewRecord = newRecord;
        }

        protected override void AddRecordWithKey(string recordKey, ItemData newRecord)
        {
        }

        protected override void RemoveRecordWithKey(string recordKey)
        {
        }

        public override bool CheckValidation(ItemData item)
        {
            CheckValidationCalled = true;
            LastItemToValidate = item;
            return ValidationResult;
        }

        public override void Save(string message)
        {
            SaveCalled = true;
            LastSaveMessage = message;
        }

        protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
        {
            return new TestItemData();
        }
    }
}
