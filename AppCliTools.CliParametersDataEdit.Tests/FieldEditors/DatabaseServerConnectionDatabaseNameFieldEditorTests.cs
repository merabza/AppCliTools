using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParametersDataEdit.FieldEditors;
using AppCliTools.LibDataInput;
using DatabaseTools.DbTools;
using DatabaseTools.DbTools.Models;
using Moq;
using ParametersManagement.LibDatabaseParameters;
using SystemTools.SharedKernel;
using ToolsManagement.DatabasesManagement;

namespace AppCliTools.CliParametersDataEdit.Tests.FieldEditors;

[Collection(ConsoleCaptureCollection.Name)]
public sealed class DatabaseServerConnectionDatabaseNameFieldEditorTests : IDisposable
{
    private const string NoneItem = "(None)";
    private const string NewDatabaseNameItem = "New Database Name";
    private const string EnteredDatabaseName = "AppDbNew";

    private readonly StringWriter _consoleOutput = new(CultureInfo.InvariantCulture);
    private readonly Mock<IDatabaseManager> _databaseManager = new();
    private readonly FakeMenuListInput _menuListInput = new();
    private readonly TextWriter _originalConsoleOutput;
    private readonly List<DatabaseServerConnectionData> _passedRecords = [];

    private readonly DatabaseServerConnectionData _record =
        DatabaseServerConnectionStringConverterTests.SqlServerData("AppDb");

    private readonly List<string> _textInputFieldNames = [];
    private Result<IDatabaseManager> _createDatabaseManagerResult;

    public DatabaseServerConnectionDatabaseNameFieldEditorTests()
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

    //the record itself gives the server: a new or changed connection is not in the parameters yet
    [Fact]
    public async Task UpdateField_WhenSelecting_OffersNoneNewNameAndDatabasesOfTheRecordServer()
    {
        // Act
        await CreateSut().UpdateField(null, _record, CancellationToken.None);

        // Assert
        Assert.Equal([_record], _passedRecords);
        (string fieldName, CliMenuSet menuSet, string? currentName) = Assert.Single(_menuListInput.Selections);
        Assert.Equal("Database Name", fieldName);
        Assert.Equal("AppDb", currentName);
        Assert.Equal([NoneItem, NewDatabaseNameItem, "master", "AppDb", "AppDbCopy"],
            CliMenuTestAccess.GetMenuItems(menuSet).Select(x => x.MenuItemName));
    }

    [Fact]
    public async Task UpdateField_WhenEnterPressed_KeepsCurrentDatabaseName()
    {
        // Arrange
        _menuListInput.Answer = FakeMenuListInput.EnterAnswer;

        // Act
        await CreateSut().UpdateField(null, _record, CancellationToken.None);

        // Assert
        Assert.Equal("AppDb", _record.DatabaseName);
        Assert.Empty(_textInputFieldNames);
    }

    //New Database Name is the first item without a key, so the databases follow it
    [Theory]
    [InlineData("master")]
    [InlineData("AppDbCopy")]
    public async Task UpdateField_WhenOtherDatabaseSelected_StoresIt(string databaseName)
    {
        // Arrange
        _menuListInput.Answer = databaseName;

        // Act
        await CreateSut().UpdateField(null, _record, CancellationToken.None);

        // Assert
        Assert.Equal(databaseName, _record.DatabaseName);
        Assert.Empty(_textInputFieldNames);
    }

    //the connection may be saved without a database
    [Fact]
    public async Task UpdateField_WhenNoneSelected_RemovesDatabaseName()
    {
        // Arrange
        _menuListInput.Answer = FakeMenuListInput.NoneAnswer;

        // Act
        await CreateSut().UpdateField(null, _record, CancellationToken.None);

        // Assert
        Assert.Null(_record.DatabaseName);
        Assert.Empty(_textInputFieldNames);
    }

    [Fact]
    public async Task UpdateField_WhenNewDatabaseNameSelected_StoresEnteredName()
    {
        // Arrange
        _menuListInput.Answer = NewDatabaseNameItem;

        // Act
        await CreateSut().UpdateField(null, _record, CancellationToken.None);

        // Assert
        Assert.Equal([NewDatabaseNameItem], _textInputFieldNames);
        Assert.Equal(EnteredDatabaseName, _record.DatabaseName);
    }

    //without a database manager the databases are unknown: no database or a new name can still be chosen
    [Fact]
    public async Task UpdateField_WhenDatabaseManagerNotCreated_OffersOnlyNoneAndNewDatabaseName()
    {
        // Arrange
        _createDatabaseManagerResult = Error.Problem("TestError", "Test database manager error");
        _menuListInput.Answer = FakeMenuListInput.NoneAnswer;

        // Act
        await CreateSut().UpdateField(null, _record, CancellationToken.None);

        // Assert
        Assert.Contains("Test database manager error", _consoleOutput.ToString(), StringComparison.Ordinal);
        Assert.Equal([NoneItem, NewDatabaseNameItem], OfferedNames());
        Assert.Null(_record.DatabaseName);
    }

    [Fact]
    public async Task UpdateField_WhenDatabaseListNotReceived_OffersOnlyNoneAndNewDatabaseName()
    {
        // Arrange
        _databaseManager.Setup(x => x.GetDatabaseNames(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<DatabaseInfoModel>>(Error.Problem("TestError", "Test list error")));
        _menuListInput.Answer = NewDatabaseNameItem;

        // Act
        await CreateSut().UpdateField(null, _record, CancellationToken.None);

        // Assert
        Assert.Contains("Test list error", _consoleOutput.ToString(), StringComparison.Ordinal);
        Assert.Equal([NoneItem, NewDatabaseNameItem], OfferedNames());
        Assert.Equal(EnteredDatabaseName, _record.DatabaseName);
    }

    //an id that no item has (the real input never gives it) is refused and the name stays
    [Theory]
    [InlineData(-2)]
    [InlineData(4)]
    public async Task UpdateField_WhenIdIsNotOffered_ThrowsAndKeepsDatabaseName(int selectedId)
    {
        // Arrange
        var sut = new DatabaseServerConnectionDatabaseNameFieldEditor(
            nameof(DatabaseServerConnectionData.DatabaseName), DatabaseManagerFactory(), (_, _, _) => selectedId,
            _ => EnteredDatabaseName);

        // Act & Assert
        await Assert.ThrowsAsync<DataInputException>(async () =>
            await sut.UpdateField(null, _record, CancellationToken.None));
        Assert.Equal("AppDb", _record.DatabaseName);
    }

    private DatabaseServerConnectionDatabaseNameFieldEditor CreateSut()
    {
        return new DatabaseServerConnectionDatabaseNameFieldEditor(nameof(DatabaseServerConnectionData.DatabaseName),
            DatabaseManagerFactory(), _menuListInput.InputIdFromMenuList, fieldName =>
            {
                _textInputFieldNames.Add(fieldName);
                return EnteredDatabaseName;
            });
    }

    private Func<DatabaseServerConnectionData, CancellationToken, ValueTask<Result<IDatabaseManager>>>
        DatabaseManagerFactory()
    {
        return (record, _) =>
        {
            _passedRecords.Add(record);
            return ValueTask.FromResult(_createDatabaseManagerResult);
        };
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
