using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters;
using AppCliTools.CliParametersDataEdit.CliMenuCommands;
using AppCliTools.CliParametersDataEdit.FieldEditors;
using AppCliTools.LibDataInput;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersDataEdit.Tests.CliMenuCommands;

//the Delete item of the database parameters editor, here for the new database of a server
[Collection(ConsoleCaptureCollection.Name)]
public sealed class DeleteDatabaseParametersCliMenuCommandTests : IDisposable
{
    private const string PropertyName = nameof(TestRecord.NewDatabaseParameters);

    private readonly StringWriter _consoleOutput = new(CultureInfo.InvariantCulture);

    private readonly DatabaseParameters _databaseParameters =
        new() { DbConnectionName = "ProdSqlServer", DatabaseName = "AppDb" };

    private readonly TextWriter _originalConsoleOutput;
    private readonly TestDatabaseServerParameters _parameters = new();
    private readonly Mock<IParametersManager> _parametersManager = new();
    private readonly TestRecord _record;
    private readonly List<string> _saveMessages = [];
    private readonly DeleteDatabaseParametersCliMenuCommand _sut;

    public DeleteDatabaseParametersCliMenuCommandTests()
    {
        _record = new TestRecord { NewDatabaseParameters = _databaseParameters };
        _parametersManager.Setup(x => x.Parameters).Returns(_parameters);
        _parametersManager
            .Setup(x => x.Save(_parameters, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .Callback((IParameters _, string message, string? _, CancellationToken _) => _saveMessages.Add(message))
            .ReturnsAsync(true);
        var fieldEditor = new DatabaseParametersFieldEditor(Mock.Of<IApplication>(x => x.AppName == "TestApp"),
            NullLogger.Instance, Mock.Of<IHttpClientFactory>(), PropertyName, _parametersManager.Object);
        var subParametersManager = new SubParametersManager<DatabaseParameters>(_databaseParameters,
            _parametersManager.Object, fieldEditor, _record);
        _sut = new DeleteDatabaseParametersCliMenuCommand(subParametersManager, PropertyName);

        _originalConsoleOutput = Console.Out;
        Console.SetOut(_consoleOutput);
    }

    public void Dispose()
    {
        Console.SetOut(_originalConsoleOutput);
        _consoleOutput.Dispose();
    }

    [Fact]
    public void Constructor_NamesTheCommandDeleteAndGoesUpAfterTheDeletion()
    {
        // Assert
        Assert.Equal("Delete", _sut.Name);
        Assert.Equal(EMenuAction.LevelUp, _sut.MenuActionOnBodySuccess);
        Assert.Equal(EMenuAction.Reload, _sut.MenuActionOnBodyFail);
    }

    [Fact]
    public async Task Run_WhenConfirmed_DeletesTheRecordSavesTheParentAndGoesUp()
    {
        // Act
        await RunAnsweringYes();

        // Assert
        Assert.Null(_record.NewDatabaseParameters);
        Assert.Equal($"{PropertyName} deleted", Assert.Single(_saveMessages));
        Assert.Equal(EMenuAction.LevelUp, _sut.MenuAction);
        string output = _consoleOutput.ToString();
        Assert.Contains($"This process will delete {PropertyName}", output, StringComparison.Ordinal);
        Assert.Contains("Are you sure, you want to run this action?", output, StringComparison.Ordinal);
    }

    //the test host has no console, so the question cannot be answered: nothing is deleted or saved
    [Fact]
    public async Task Run_WhenTheQuestionIsNotAnswered_KeepsTheRecord()
    {
        // Act
        await _sut.Run();

        // Assert
        Assert.Same(_databaseParameters, _record.NewDatabaseParameters);
        Assert.Empty(_saveMessages);
    }

    [Fact]
    public async Task Run_WhenTheSaveFails_ReloadsTheMenu()
    {
        // Arrange
        _parametersManager
            .Setup(x => x.Save(_parameters, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await RunAnsweringYes();

        // Assert
        Assert.Equal(EMenuAction.Reload, _sut.MenuAction);
    }

    //Run asks "Are you sure...?" on the console; yes to all answers it as the user would
    private async Task RunAnsweringYes()
    {
        Inputer.AnswerYesToAll = true;
        try
        {
            await _sut.Run();
        }
        finally
        {
            Inputer.AnswerYesToAll = false;
        }
    }

    //as ServerInfoModel keeps it: an init-only property that the field editor sets by reflection
    private sealed class TestRecord
    {
        public DatabaseParameters? NewDatabaseParameters { get; init; }
    }
}
