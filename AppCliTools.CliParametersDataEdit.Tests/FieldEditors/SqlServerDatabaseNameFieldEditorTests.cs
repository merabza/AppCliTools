using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParametersDataEdit.FieldEditors;
using AppCliTools.CliParametersDataEdit.Models;
using DatabaseTools.DbTools;
using DatabaseTools.DbTools.Models;
using Microsoft.Extensions.Logging.Abstractions;
using SystemTools.SharedKernel;

namespace AppCliTools.CliParametersDataEdit.Tests.FieldEditors;

public sealed class SqlServerDatabaseNameFieldEditorTests
{
    private const string NewDatabaseNameItem = "New Database Name";
    private const string EnteredDatabaseName = "AppDbNew";

    private readonly List<SqlServerConnectionParameters> _connectionParameters = [];
    private readonly FakeMenuListInput _menuListInput = new();

    //as SqlServerDatabaseConnectionParametersEditor edits it, with SQL Server authentication and connection settings
    //that differ from the defaults
    private readonly SqlServerConnectionParameters _record = new()
    {
        ServerAddress = "TestSqlServer",
        WindowsNtIntegratedSecurity = false,
        ServerUser = "TestUser",
        ServerPass = "TestPass",
        DatabaseName = "AppDb",
        ConnectionTimeOut = 30,
        Encrypt = true,
        TrustServerCertificate = true
    };

    private readonly List<string> _textInputFieldNames = [];

    private Result<List<DatabaseInfoModel>> _getDatabaseInfosResult =
        Result.Success(DatabaseInfos("master", "AppDb", "AppDbCopy"));

    //the server's databases after New Database Name, read with the connection settings of the record
    [Fact]
    public async Task UpdateField_WhenSelecting_OffersNewDatabaseNameAndDatabasesReadWithRecordSettings()
    {
        // Act
        await CreateSut().UpdateField(null, _record, CancellationToken.None);

        // Assert
        SqlServerConnectionParameters connectionParameters = Assert.Single(_connectionParameters);
        Assert.Equal("TestSqlServer", connectionParameters.ServerAddress);
        Assert.False(connectionParameters.WindowsNtIntegratedSecurity);
        Assert.Equal("TestUser", connectionParameters.ServerUser);
        Assert.Equal("TestPass", connectionParameters.ServerPass);
        Assert.Equal(30, connectionParameters.ConnectionTimeOut);
        Assert.True(connectionParameters.Encrypt);
        Assert.True(connectionParameters.TrustServerCertificate);
        (string fieldName, CliMenuSet menuSet, string? currentName) = Assert.Single(_menuListInput.Selections);
        Assert.Equal("Database Name", fieldName);
        Assert.Equal("AppDb", currentName);
        Assert.Equal([NewDatabaseNameItem, "master", "AppDb", "AppDbCopy"],
            CliMenuTestAccess.GetMenuItems(menuSet).Select(x => x.MenuItemName));
    }

    //Enter used to throw "Selected invalid ID": the items had no id, so the list returned -1 for the current one
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

    //New Database Name is the first item, so the databases follow it
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

    //a server that cannot be read still lets a new name be entered
    [Fact]
    public async Task UpdateField_WhenDatabaseListNotReceived_OffersOnlyNewDatabaseName()
    {
        // Arrange
        _getDatabaseInfosResult = Error.Problem("TestError", "Test list error");
        _menuListInput.Answer = NewDatabaseNameItem;

        // Act
        await CreateSut().UpdateField(null, _record, CancellationToken.None);

        // Assert
        Assert.Equal([NewDatabaseNameItem],
            CliMenuTestAccess.GetMenuItems(Assert.Single(_menuListInput.Selections).MenuSet)
                .Select(x => x.MenuItemName));
        Assert.Equal(EnteredDatabaseName, _record.DatabaseName);
    }

    //a connection timeout of 0 means no limit in SqlClient: an unreachable server used to block the prompt
    [Fact]
    public async Task UpdateField_WhenRecordConnectionTimeOutIsZero_ReadsDatabasesWithDefaultTimeOut()
    {
        // Arrange
        _record.ConnectionTimeOut = 0;

        // Act
        await CreateSut().UpdateField(null, _record, CancellationToken.None);

        // Assert
        Assert.Equal(15, Assert.Single(_connectionParameters).ConnectionTimeOut);
    }

    //Windows authentication needs no user and password: their absence used to stop the editor
    [Fact]
    public async Task UpdateField_WithWindowsAuthenticationWithoutUserAndPassword_OffersDatabases()
    {
        // Arrange
        _record.WindowsNtIntegratedSecurity = true;
        _record.ServerUser = null;
        _record.ServerPass = null;

        // Act
        await CreateSut().UpdateField(null, _record, CancellationToken.None);

        // Assert
        SqlServerConnectionParameters connectionParameters = Assert.Single(_connectionParameters);
        Assert.True(connectionParameters.WindowsNtIntegratedSecurity);
        Assert.Null(connectionParameters.ServerUser);
        Assert.Null(connectionParameters.ServerPass);
        Assert.Single(_menuListInput.Selections);
        Assert.Equal("AppDb", _record.DatabaseName);
    }

    [Theory]
    [InlineData(null, "TestPass")]
    [InlineData("TestUser", null)]
    public async Task UpdateField_WithSqlServerAuthenticationWithoutUserOrPassword_ThrowsWithoutReadingDatabases(
        string? serverUser, string? serverPass)
    {
        // Arrange
        _record.ServerUser = serverUser;
        _record.ServerPass = serverPass;

        // Act
        Exception exception = await Assert.ThrowsAsync<Exception>(async () =>
            await CreateSut().UpdateField(null, _record, CancellationToken.None));

        // Assert
        Assert.Equal("serverUser or serverPass is null", exception.InnerException?.Message);
        Assert.Empty(_connectionParameters);
        Assert.Empty(_menuListInput.Selections);
        Assert.Equal("AppDb", _record.DatabaseName);
    }

    [Fact]
    public async Task UpdateField_WithoutServerAddress_ThrowsWithoutReadingDatabases()
    {
        // Arrange
        _record.ServerAddress = null;

        // Act
        Exception exception = await Assert.ThrowsAsync<Exception>(async () =>
            await CreateSut().UpdateField(null, _record, CancellationToken.None));

        // Assert
        Assert.Equal("serverAddress is null", exception.InnerException?.Message);
        Assert.Empty(_connectionParameters);
        Assert.Empty(_menuListInput.Selections);
    }

    //as SqlServerDatabaseConnectionParametersEditor creates the editor
    private SqlServerDatabaseNameFieldEditor CreateSut()
    {
        return new SqlServerDatabaseNameFieldEditor(NullLogger.Instance,
            nameof(SqlServerConnectionParameters.DatabaseName), nameof(SqlServerConnectionParameters.ServerAddress),
            nameof(SqlServerConnectionParameters.WindowsNtIntegratedSecurity),
            nameof(SqlServerConnectionParameters.ServerUser), nameof(SqlServerConnectionParameters.ServerPass),
            nameof(SqlServerConnectionParameters.ConnectionTimeOut), nameof(SqlServerConnectionParameters.Encrypt),
            nameof(SqlServerConnectionParameters.TrustServerCertificate), false, (connectionParameters, _) =>
            {
                _connectionParameters.Add(connectionParameters);
                return Task.FromResult(_getDatabaseInfosResult);
            }, _menuListInput.InputIdFromMenuList, fieldName =>
            {
                _textInputFieldNames.Add(fieldName);
                return EnteredDatabaseName;
            });
    }

    private static List<DatabaseInfoModel> DatabaseInfos(params string[] names)
    {
        return [.. names.Select(x => new DatabaseInfoModel(x, EDatabaseRecoveryModel.Full, false))];
    }
}
