using System.Reflection;
using CliMenu;
using CliParameters.CliMenuCommands;
using LibParameters;

namespace CliParameters.Tests.CliMenuCommands;

public sealed class EditParametersInSequenceCliMenuCommandTests
{
    [Fact]
    public void Constructor_WithValidParametersEditor_InitializesCorrectly()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");

        // Act
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Assert
        Assert.Equal("Edit Parameters in sequence", command.Name);
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
    }

    [Fact]
    public void Constructor_WithNullParametersEditor_AcceptsNullAndStoresIt()
    {
        // Arrange & Act
        var command = new EditParametersInSequenceCliMenuCommand(null!);

        // Assert - Constructor doesn't validate, it just stores the value
        Assert.NotNull(command);
        var parametersEditorField = typeof(EditParametersInSequenceCliMenuCommand).GetField("_parametersEditor",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(parametersEditorField);
        var storedParametersEditor = parametersEditorField.GetValue(command);
        Assert.Null(storedParametersEditor);
    }

    [Fact]
    public void Constructor_SetsCorrectMenuActions()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");

        // Act
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Assert
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
        Assert.Equal(EMenuAction.Nothing, command.MenuAction); // Initial value
    }

    [Fact]
    public void Constructor_PassesCorrectParametersToBaseClass()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");

        // Act
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Assert - Verify all parameters passed to base constructor
        Assert.Equal("Edit Parameters in sequence", command.Name);
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
    }

    [Fact]
    public void Constructor_StoresParametersEditorReference()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");

        // Act
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Assert - Verify parametersEditor is stored by using reflection
        var parametersEditorField = typeof(EditParametersInSequenceCliMenuCommand).GetField("_parametersEditor",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(parametersEditorField);
        var storedParametersEditor = parametersEditorField.GetValue(command);
        Assert.Same(parametersEditor, storedParametersEditor);
    }

    [Fact]
    public void Name_AlwaysReturnsEditParametersInSequence()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Assert
        Assert.Equal("Edit Parameters in sequence", command.Name);
    }

    [Fact]
    public void MenuActionOnBodySuccess_AlwaysReturnsLevelUp()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Assert
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
    }

    [Fact]
    public void MenuActionOnBodyFail_AlwaysReturnsReload()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Assert
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
    }

    [Fact]
    public void Constructor_InitialMenuActionIsNothing()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");

        // Act
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Assert
        Assert.Equal(EMenuAction.Nothing, command.MenuAction);
    }

    [Fact]
    public void GetSubMenu_ReturnsNull()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Act
        var result = command.GetSubMenu();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetMenuLinkToGo_ReturnsNull()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Act
        var result = command.GetMenuLinkToGo();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void StatusView_HasDefaultValue()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Assert
        Assert.Equal(EStatusView.Brackets, command.StatusView);
    }

    [Fact]
    public void NameIsStatus_IsFalse()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Assert
        Assert.False(command.NameIsStatus);
    }

    [Fact]
    public void Constructor_WithDifferentParametersEditors_CreatesIndependentCommands()
    {
        // Arrange
        var parametersEditor1 = new TestParametersEditor("Parameters1");
        var parametersEditor2 = new TestParametersEditor("Parameters2");

        // Act
        var command1 = new EditParametersInSequenceCliMenuCommand(parametersEditor1);
        var command2 = new EditParametersInSequenceCliMenuCommand(parametersEditor2);

        // Assert
        Assert.NotSame(command1, command2);
        var field1 = typeof(EditParametersInSequenceCliMenuCommand).GetField("_parametersEditor",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.Same(parametersEditor1, field1?.GetValue(command1));
        Assert.Same(parametersEditor2, field1?.GetValue(command2));
    }

    [Fact]
    public void Constructor_SetsSealedClassCorrectly()
    {
        // Arrange & Act
        var parametersEditor = new TestParametersEditor("TestParameters");
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Assert - Verify class is sealed
        var type = command.GetType();
        Assert.True(type.IsSealed);
    }

    [Fact]
    public void ParentMenuName_IsNullByDefault()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Act
        var parentMenuName = GetParentMenuName(command);

        // Assert
        Assert.Null(parentMenuName);
    }

    [Fact]
    public void Constructor_InheritsFromCliMenuCommand()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");

        // Act
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Assert
        Assert.IsAssignableFrom<CliMenuCommand>(command);
    }

    [Theory]
    [InlineData("DatabaseParameters")]
    [InlineData("ApiClientParameters")]
    [InlineData("FileStorageParameters")]
    [InlineData("ConnectionParameters")]
    public void Constructor_WithVariousParametersEditorNames_InitializesCorrectly(string parameterName)
    {
// Arrange
        var parametersEditor = new TestParametersEditor(parameterName);

        // Act
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Assert
        Assert.Equal("Edit Parameters in sequence", command.Name);
        Assert.NotNull(command);
    }

    [Fact]
    public void MenuAction_CanBeRead()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Act
        var menuAction = command.MenuAction;

        // Assert
        Assert.Equal(EMenuAction.Nothing, menuAction);
    }

    [Fact]
    public void MenuActionOnBodySuccess_CanBeReadButNotSetInTests()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Act & Assert - Verify the value set by constructor
        Assert.Equal(EMenuAction.LevelUp, command.MenuActionOnBodySuccess);
    }

    [Fact]
    public void MenuActionOnBodyFail_CanBeReadButNotSetInTests()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");
        var command = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Act & Assert - Verify the value set by constructor
        Assert.Equal(EMenuAction.Reload, command.MenuActionOnBodyFail);
    }

    [Fact]
    public void Constructor_WithSameParametersEditor_CreatesIndependentInstances()
    {
        // Arrange
        var parametersEditor = new TestParametersEditor("TestParameters");

        // Act
        var command1 = new EditParametersInSequenceCliMenuCommand(parametersEditor);
        var command2 = new EditParametersInSequenceCliMenuCommand(parametersEditor);

        // Assert
        Assert.NotSame(command1, command2);
        var field = typeof(EditParametersInSequenceCliMenuCommand).GetField("_parametersEditor",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.Same(parametersEditor, field?.GetValue(command1));
        Assert.Same(parametersEditor, field?.GetValue(command2));
    }

    private static string? GetParentMenuName(CliMenuCommand command)
    {
        var field = typeof(CliMenuCommand).GetField("ParentMenuName", BindingFlags.Public | BindingFlags.Instance);
        return field?.GetValue(command) as string;
    }

    /// <summary>
    ///     Test implementation of ParametersEditor for testing purposes
    /// </summary>
    private sealed class TestParametersEditor : ParametersEditor
    {
        public TestParametersEditor(string name) : base(name, new TestParametersManager())
        {
        }

        public int EditParametersInSequenceCallCount { get; private set; }
    }

    /// <summary>
    ///     Test implementation of IParametersManager for testing purposes
    /// </summary>
    private sealed class TestParametersManager : IParametersManager
    {
        public TestParametersManager()
        {
            Parameters = new TestParameters();
        }

        public IParameters Parameters { get; set; }

        public void Save(IParameters parameters, string message, string? saveAsFilePath = null)
        {
            // No-op for testing
        }
    }

    /// <summary>
    ///     Test implementation of IParameters for testing purposes
    /// </summary>
    private sealed class TestParameters : IParameters
    {
        public bool CheckBeforeSave()
        {
            return true;
        }
    }
}