using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParametersDataEdit.FieldEditors;
using DatabaseTools.DbTools;
using DatabaseTools.DbTools.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;
using ToolsManagement.DatabasesManagement;

namespace AppCliTools.CliParametersDataEdit.Tests.FieldEditors;

[Collection(ConsoleCaptureCollection.Name)]
public sealed class DatabaseNameFieldEditorTests : IDisposable
{
    private const string ConnectionName = "DevSqlServer";
    private const string NewDatabaseNameItem = "New Database Name";
    private const string EnteredDatabaseName = "AppDbNew";

    private readonly List<string?> _connectionNames = [];
    private readonly StringWriter _consoleOutput = new(CultureInfo.InvariantCulture);
    private readonly Mock<IDatabaseManager> _databaseManager = new();
    private readonly FakeMenuListInput _menuListInput = new();
    private readonly TextWriter _originalConsoleOutput;

    //as DatabaseParametersEditor edits it: a database on the server of a database server connection
    private readonly DatabaseParameters _record = new() { DbConnectionName = ConnectionName, DatabaseName = "AppDb" };

    private readonly List<string> _textInputFieldNames = [];
    private Result<IDatabaseManager> _createDatabaseManagerResult;

    public DatabaseNameFieldEditorTests()
    {
        _databaseManager.Setup(x => x.GetDatabaseNames(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(DatabaseInfos("master", "AppDb", "AppDbCopy")));
        _createDatabaseManagerResult = Result.Success(_databaseManager.Object);
        _originalConsoleOutput = Console.Out;
        Console.SetOut(_consoleOutput);
    }

    public void Dispose()
    {
        Console.SetOut(_originalConsoleOutput);
        _consoleOutput.Dispose();
    }

    //the public constructor reads the record's connection from the parameters and selects on the console: OleDb has no
    //database manager yet, so only New Database Name is offered, and the test host has no real console, so the
    //selection throws after the prompt with the current name is printed, and nothing is changed
    [Fact]
    public async Task PublicConstructor_WhenUpdatingField_PromptsWithCurrentDatabaseName()
    {
        // Arrange
        var parameters = new Mock<IParametersWithDatabaseServerConnectionsAndApiClients>();
        parameters.SetupGet(x => x.DatabaseServerConnections).Returns(
            new Dictionary<string, DatabaseServerConnectionData>
            {
                [ConnectionName] = new() { DatabaseServerProvider = EDatabaseProvider.OleDb }
            });
        parameters.SetupGet(x => x.ApiClients).Returns([]);
        var parametersManager = new Mock<IParametersManager>();
        parametersManager.SetupGet(x => x.Parameters).Returns(parameters.Object);
        var sut = new DatabaseNameFieldEditor("TestApp", NullLogger.Instance, Mock.Of<IHttpClientFactory>(),
            nameof(DatabaseParameters.DatabaseName), parametersManager.Object,
            nameof(DatabaseParameters.DbConnectionName), true);

        // Act
        await Assert.ThrowsAnyAsync<Exception>(async () =>
            await sut.UpdateField(null, _record, CancellationToken.None));

        // Assert
        string output = _consoleOutput.ToString();
        Assert.Contains("CreateOleDatabaseManager Is Not Implemented", output, StringComparison.Ordinal);
        Assert.Contains("Select Database Name [AppDb]: ", output, StringComparison.Ordinal);
        Assert.Equal("AppDb", _record.DatabaseName);
    }

    //the databases of the record's connection, after New Database Name when the editor offers it
    [Theory]
    [InlineData(true, new[] { NewDatabaseNameItem, "master", "AppDb", "AppDbCopy" })]
    [InlineData(false, new[] { "master", "AppDb", "AppDbCopy" })]
    public async Task UpdateField_WhenSelecting_OffersDatabasesOfRecordConnection(bool canUseNewDatabaseName,
        string[] expectedNames)
    {
        // Act
        await CreateSut(canUseNewDatabaseName).UpdateField(null, _record, CancellationToken.None);

        // Assert
        Assert.Equal([ConnectionName], _connectionNames);
        (string fieldName, CliMenuSet menuSet, string? currentName) = Assert.Single(_menuListInput.Selections);
        Assert.Equal("Database Name", fieldName);
        Assert.Equal("AppDb", currentName);
        Assert.Equal(expectedNames, CliMenuTestAccess.GetMenuItems(menuSet).Select(x => x.MenuItemName));
    }

    //Enter used to throw "Selected invalid ID": the items had no id, so the list returned -1 for the current one
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateField_WhenEnterPressed_KeepsCurrentDatabaseName(bool canUseNewDatabaseName)
    {
        // Arrange
        _menuListInput.Answer = FakeMenuListInput.EnterAnswer;

        // Act
        await CreateSut(canUseNewDatabaseName).UpdateField(null, _record, CancellationToken.None);

        // Assert
        Assert.Equal("AppDb", _record.DatabaseName);
        Assert.Empty(_textInputFieldNames);
    }

    //New Database Name, when offered, is the first item, so the databases follow it
    [Theory]
    [InlineData(true, "master")]
    [InlineData(true, "AppDbCopy")]
    [InlineData(false, "master")]
    [InlineData(false, "AppDbCopy")]
    public async Task UpdateField_WhenOtherDatabaseSelected_StoresIt(bool canUseNewDatabaseName, string databaseName)
    {
        // Arrange
        _menuListInput.Answer = databaseName;

        // Act
        await CreateSut(canUseNewDatabaseName).UpdateField(null, _record, CancellationToken.None);

        // Assert
        Assert.Equal(databaseName, _record.DatabaseName);
        Assert.Empty(_textInputFieldNames);
    }

    [Fact]
    public async Task UpdateField_WhenNewDatabaseNameSelected_StoresEnteredName()
    {
        // Arrange
        _menuListInput.Answer = NewDatabaseNameItem;

        // Act
        await CreateSut(true).UpdateField(null, _record, CancellationToken.None);

        // Assert
        Assert.Equal([NewDatabaseNameItem], _textInputFieldNames);
        Assert.Equal(EnteredDatabaseName, _record.DatabaseName);
    }

    //without a database manager (e.g. OleDb) the databases are unknown: a new name can still be entered
    [Fact]
    public async Task UpdateField_WhenDatabaseManagerNotCreated_OffersOnlyNewDatabaseName()
    {
        // Arrange
        _createDatabaseManagerResult = Error.Problem("TestError", "Test database manager error");
        _menuListInput.Answer = NewDatabaseNameItem;

        // Act
        await CreateSut(true).UpdateField(null, _record, CancellationToken.None);

        // Assert
        Assert.Contains("Test database manager error", _consoleOutput.ToString(), StringComparison.Ordinal);
        Assert.Equal([NewDatabaseNameItem], OfferedNames());
        Assert.Equal(EnteredDatabaseName, _record.DatabaseName);
    }

    //an editor without New Database Name has nothing to offer then: nothing is asked and the name stays
    [Fact]
    public async Task UpdateField_WhenDatabaseManagerNotCreatedWithoutNewDatabaseName_AsksNothing()
    {
        // Arrange
        _createDatabaseManagerResult = Error.Problem("TestError", "Test database manager error");

        // Act
        await CreateSut(false).UpdateField(null, _record, CancellationToken.None);

        // Assert
        Assert.Contains("Test database manager error", _consoleOutput.ToString(), StringComparison.Ordinal);
        Assert.Empty(_menuListInput.Selections);
        Assert.Equal("AppDb", _record.DatabaseName);
    }

    [Fact]
    public async Task UpdateField_WhenDatabaseListNotReceived_OffersOnlyNewDatabaseName()
    {
        // Arrange
        _databaseManager.Setup(x => x.GetDatabaseNames(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<DatabaseInfoModel>>(Error.Problem("TestError", "Test list error")));
        _menuListInput.Answer = NewDatabaseNameItem;

        // Act
        await CreateSut(true).UpdateField(null, _record, CancellationToken.None);

        // Assert
        Assert.Contains("Test list error", _consoleOutput.ToString(), StringComparison.Ordinal);
        Assert.Equal([NewDatabaseNameItem], OfferedNames());
        Assert.Equal(EnteredDatabaseName, _record.DatabaseName);
    }

    private DatabaseNameFieldEditor CreateSut(bool canUseNewDatabaseName)
    {
        return new DatabaseNameFieldEditor(nameof(DatabaseParameters.DatabaseName),
            nameof(DatabaseParameters.DbConnectionName), canUseNewDatabaseName, (connectionName, _) =>
            {
                _connectionNames.Add(connectionName);
                return Task.FromResult(_createDatabaseManagerResult);
            }, _menuListInput.InputIdFromMenuList, fieldName =>
            {
                _textInputFieldNames.Add(fieldName);
                return EnteredDatabaseName;
            });
    }

    private List<string> OfferedNames()
    {
        return
        [
            .. CliMenuTestAccess.GetMenuItems(Assert.Single(_menuListInput.Selections).MenuSet)
                .Select(x => x.MenuItemName)
        ];
    }

    private static List<DatabaseInfoModel> DatabaseInfos(params string[] names)
    {
        return [.. names.Select(x => new DatabaseInfoModel(x, EDatabaseRecoveryModel.Full, false))];
    }
}
