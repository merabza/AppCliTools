using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.LibDataInput;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParameters.Tests;

[Collection(ConsoleCaptureCollection.Name)]
public sealed class ParametersEditorTests : IDisposable
{
    private const string EditorName = "Test";
    private const string EditInSequenceName = "Edit Parameters in sequence";
    private const string EscapeName = "Exit to level up menu";
    private const string SaveAsFilePath = "Copy.json";

    private readonly StringWriter _consoleOutput = new(CultureInfo.InvariantCulture);
    private readonly TextWriter _originalConsoleOutput;
    private readonly RecordingParametersManager _parametersManager = new();
    private readonly List<(string PropertyName, object Record)> _updates = [];

    public ParametersEditorTests()
    {
        _originalConsoleOutput = Console.Out;
        Console.SetOut(_consoleOutput);
    }

    public void Dispose()
    {
        Console.SetOut(_originalConsoleOutput);
        _consoleOutput.Dispose();
    }

    [Fact]
    public void Constructor_WithParametersManager_EditsItsParameters()
    {
        // Act
        var sut = new TestParametersEditor(EditorName, _parametersManager);

        // Assert
        Assert.Equal(EditorName, sut.Name);
        Assert.Same(_parametersManager.Parameters, sut.Parameters);
    }

    [Fact]
    public void Constructor_WithOwnParameters_EditsThemInsteadOfTheManagerParameters()
    {
        // Arrange
        var parameters = new TestParameters();

        // Act
        var sut = new TestParametersEditor(EditorName, parameters, _parametersManager);

        // Assert
        Assert.Equal(EditorName, sut.Name);
        Assert.Same(parameters, sut.Parameters);
    }

    [Fact]
    public void GetStatus_ReturnsNull()
    {
        // Act
        string? status = ParametersEditor.GetStatus();

        // Assert
        Assert.Null(status);
    }

    [Fact]
    public void GetParametersMainMenu_ListsEditInSequenceTheFieldsAndTheEscapeCommand()
    {
        // Arrange
        var sut = new TestParametersEditor(EditorName, _parametersManager, Field("First"), Field("Second"));

        // Act
        CliMenuSet menu = sut.GetParametersMainMenu();

        // Assert
        Assert.Equal(EditorName, menu.Caption);
        string[] expected = [EditInSequenceName, "First", "Second", EscapeName];
        Assert.Equal(expected, MenuItemNames(menu));
    }

    [Fact]
    public void GetParametersMainMenu_PutsAdditionalCommandsAfterTheFieldsAndBeforeTheEscapeCommand()
    {
        // Arrange
        var sut = new EditorWithAdditionalCommand(_parametersManager, Field("First"));

        // Act
        List<string> names = MenuItemNames(sut.GetParametersMainMenu());

        // Assert
        string[] expected = [EditInSequenceName, "First", EditorWithAdditionalCommand.CommandName, EscapeName];
        Assert.Equal(expected, names);
    }

    [Fact]
    public void FillDetailsSubMenu_AddsEditInSequenceAndTheFieldsWithoutTheEscapeCommand()
    {
        // Arrange
        var sut = new TestParametersEditor(EditorName, _parametersManager, Field("Plain"), Field("Sub", true));
        var menu = new CliMenuSet();

        // Act
        sut.FillDetailsSubMenu(menu);

        // Assert
        List<CliMenuItem> items = MenuItems(menu);
        string[] expectedNames = [EditInSequenceName, "Plain", "Sub"];
        Assert.Equal(expectedNames, items.Select(x => x.MenuItemName));
        Assert.IsType<EditParametersInSequenceCliMenuCommand>(items[0].CliMenuCommand);
        Assert.IsType<ParameterFieldEditorCliMenuCommand>(items[1].CliMenuCommand);
        Assert.IsType<ParameterSubObjectFieldEditorCliMenuCommand>(items[2].CliMenuCommand);
    }

    [Fact]
    public void EnableFieldByName_WhenDisabled_LeavesTheFieldOutOfTheMenuUntilEnabledAgain()
    {
        // Arrange
        var sut = new TestParametersEditor(EditorName, _parametersManager, Field("First"), Field("Second"));

        // Act
        sut.EnableFieldByName("First", false);
        List<string> namesWhileDisabled = MenuItemNames(sut.GetParametersMainMenu());
        sut.EnableFieldByName("First");
        List<string> namesAfterEnabled = MenuItemNames(sut.GetParametersMainMenu());

        // Assert
        string[] expectedWhileDisabled = [EditInSequenceName, "Second", EscapeName];
        string[] expectedAfterEnabled = [EditInSequenceName, "First", "Second", EscapeName];
        Assert.Equal(expectedWhileDisabled, namesWhileDisabled);
        Assert.Equal(expectedAfterEnabled, namesAfterEnabled);
    }

    [Fact]
    public void EnableFieldByName_WhenNoFieldHasTheName_ChangesNothing()
    {
        // Arrange
        var sut = new TestParametersEditor(EditorName, _parametersManager, Field("First"));

        // Act
        sut.EnableFieldByName("Missing", false);

        // Assert
        string[] expected = [EditInSequenceName, "First", EscapeName];
        Assert.Equal(expected, MenuItemNames(sut.GetParametersMainMenu()));
    }

    [Fact]
    public async Task Save_SavesTheManagerParametersWithTheMessageAndTheFileName()
    {
        // Arrange
        var sut = new TestParametersEditor(EditorName, _parametersManager);

        // Act
        await sut.Save("Saved", SaveAsFilePath, CancellationToken.None);

        // Assert
        (IParameters parameters, string message) = Assert.Single(_parametersManager.Saves);
        Assert.Same(_parametersManager.Parameters, parameters);
        Assert.Equal("Saved", message);
        Assert.Equal(SaveAsFilePath, Assert.Single(_parametersManager.SaveAsFilePaths));
    }

    //an editor of a sub object still saves the whole parameters of its manager
    [Fact]
    public async Task Save_WithOwnParameters_SavesTheManagerParameters()
    {
        // Arrange
        var sut = new TestParametersEditor(EditorName, new TestParameters(), _parametersManager);

        // Act
        await sut.Save("Saved");

        // Assert
        (IParameters parameters, string _) = Assert.Single(_parametersManager.Saves);
        Assert.Same(_parametersManager.Parameters, parameters);
    }

    [Fact]
    public async Task Save_WhenTheManagerHasNoParameters_Throws()
    {
        // Arrange
        var sut = new TestParametersEditor(EditorName, _parametersManager);
        _parametersManager.Parameters = null!;

        // Act
        Exception exception = await Assert.ThrowsAsync<Exception>(async () => await sut.Save("Saved"));

        // Assert
        Assert.Equal("Invalid parameters for save", exception.Message);
        Assert.Empty(_parametersManager.Saves);
    }

    [Fact]
    public async Task EditParametersInSequence_UpdatesTheEnabledFieldsInOrderAndSaves()
    {
        // Arrange
        var sut = new TestParametersEditor(EditorName, _parametersManager, Field("First"), Field("Disabled"),
            Field("Last"));
        sut.EnableFieldByName("Disabled", false);

        // Act
        bool result = await sut.EditParametersInSequence(CancellationToken.None);

        // Assert
        Assert.True(result);
        string[] expectedUpdates = ["First", "Last"];
        Assert.Equal(expectedUpdates, _updates.Select(x => x.PropertyName));
        Assert.All(_updates, x => Assert.Same(_parametersManager.Parameters, x.Record));
        (IParameters parameters, string message) = Assert.Single(_parametersManager.Saves);
        Assert.Same(_parametersManager.Parameters, parameters);
        Assert.Equal($"{EditorName} parameters Updated", message);
        Assert.Null(Assert.Single(_parametersManager.SaveAsFilePaths));
        Assert.Contains($"Edit {EditorName} parameters started", _consoleOutput.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task EditParametersInSequence_WhenAFieldIsEscaped_StopsWithoutSaving()
    {
        // Arrange
        var sut = new TestParametersEditor(EditorName, _parametersManager,
            Field("First", exceptionToThrow: new DataInputEscapeException("Escape")), Field("Last"));

        // Act
        bool result = await sut.EditParametersInSequence(CancellationToken.None);

        // Assert
        Assert.False(result);
        string[] expectedUpdates = ["First"];
        Assert.Equal(expectedUpdates, _updates.Select(x => x.PropertyName));
        Assert.Empty(_parametersManager.Saves);
        Assert.Contains(
            $"Edit {EditorName} parameters started{Environment.NewLine}{Environment.NewLine}Escape... {Environment.NewLine}",
            _consoleOutput.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task EditParametersInSequence_WhenAFieldFails_ShowsTheErrorAndStopsWithoutSaving()
    {
        // Arrange
        var sut = new TestParametersEditor(EditorName, _parametersManager,
            Field("First", exceptionToThrow: new InvalidOperationException("Field failed")), Field("Last"));

        // Act
        bool result = await sut.EditParametersInSequence(CancellationToken.None);

        // Assert
        Assert.False(result);
        string[] expectedUpdates = ["First"];
        Assert.Equal(expectedUpdates, _updates.Select(x => x.PropertyName));
        Assert.Empty(_parametersManager.Saves);
        Assert.Contains("InvalidOperationException thrown with message: Field failed", _consoleOutput.ToString(),
            StringComparison.Ordinal);
    }

    private RecordingFieldEditor Field(string propertyName, bool isSubObject = false,
        Exception? exceptionToThrow = null)
    {
        return new RecordingFieldEditor(propertyName, isSubObject, _updates, exceptionToThrow);
    }

    //CliMenuSet keeps its items only in private state
    private static List<CliMenuItem> MenuItems(CliMenuSet menuSet)
    {
        return (List<CliMenuItem>)typeof(CliMenuSet)
            .GetProperty("MenuItems", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(menuSet)!;
    }

    private static List<string> MenuItemNames(CliMenuSet menuSet)
    {
        return [.. MenuItems(menuSet).Select(x => x.MenuItemName)];
    }

    private sealed class TestParametersEditor : ParametersEditor
    {
        public TestParametersEditor(string name, IParametersManager parametersManager,
            params FieldEditor[] fieldEditors) : base(name, parametersManager)
        {
            FieldEditors.AddRange(fieldEditors);
        }

        public TestParametersEditor(string name, IParameters parameters, IParametersManager parametersManager) : base(
            name, parameters, parametersManager)
        {
        }
    }

    //an editor that adds its own command after the fields, as DatabaseParametersEditor adds Delete
    private sealed class EditorWithAdditionalCommand : ParametersEditor
    {
        public const string CommandName = "Additional";

        public EditorWithAdditionalCommand(IParametersManager parametersManager, FieldEditor fieldEditor) : base(
            EditorName, parametersManager)
        {
            FieldEditors.Add(fieldEditor);
        }

        protected override void FillMainMenuAdditional(CliMenuSet parametersEditorMenuSet)
        {
            parametersEditorMenuSet.AddMenuItem(new CliMenuCommand(CommandName));
        }
    }

    //records which field was asked for in which record, or fails as the user's input can
    private sealed class RecordingFieldEditor : FieldEditor<string>
    {
        private readonly Exception? _exceptionToThrow;
        private readonly List<(string PropertyName, object Record)> _updates;

        public RecordingFieldEditor(string propertyName, bool isSubObject,
            List<(string PropertyName, object Record)> updates, Exception? exceptionToThrow) : base(propertyName,
            isSubObject: isSubObject)
        {
            _updates = updates;
            _exceptionToThrow = exceptionToThrow;
        }

        public override ValueTask UpdateField(string? recordKey, object recordForUpdate,
            CancellationToken cancellationToken = default)
        {
            _updates.Add((PropertyName, recordForUpdate));
            if (_exceptionToThrow is not null)
            {
                throw _exceptionToThrow;
            }

            return ValueTask.CompletedTask;
        }
    }

    private sealed class TestParameters : IParameters
    {
        public bool CheckBeforeSave()
        {
            return true;
        }
    }
}
