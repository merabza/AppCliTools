using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersDataEdit.Cruders;
using DatabaseTools.DbTools.Models;
using Microsoft.Extensions.Logging;
using Moq;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;
using ToolsManagement.DatabasesManagement;

namespace AppCliTools.CliParametersDataEdit.Tests.Cruders;

[Collection(ConsoleCaptureCollection.Name)]
public sealed class DatabaseServerConnectionCruderTests : IDisposable
{
    private const string ConnectionName = "TestConnection";
    private const string GetFoldersCommandName = "Get Database Server Folders and save in parameters";
    private const string WebAgentName = "TestAgent";
    private const string ValidationExceptionMessage =
        "An exception occurred while validating the database server connection.";

    //nothing listens on port 1, so a connection attempt fails at once
    private const string UnreachableMariaDbAddress = "127.0.0.1:1";

    private static readonly Error TestConnectionError = Error.Failure("TestConnection", "Test connection failed");
    private static readonly Error ServerInfoError = Error.Failure("ServerInfo", "Server info is not available");

    private readonly StringWriter _consoleOutput = new(CultureInfo.InvariantCulture);
    private readonly Mock<IDatabaseManager> _databaseManager = new();
    private readonly Mock<ILogger> _logger = new();
    private readonly TextWriter _originalConsoleOutput;
    private readonly TestDatabaseServerParameters _parameters = new();
    private readonly Mock<IParametersManager> _parametersManager = new();
    private readonly StubHttpMessageHandler _webAgent = new();

    public DatabaseServerConnectionCruderTests()
    {
        _parameters.DatabaseServerConnections[ConnectionName] =
            DatabaseServerConnectionStringConverterTests.SqlServerData();
        _parametersManager.Setup(x => x.Parameters).Returns(_parameters);

        _databaseManager.Setup(x => x.TestConnection(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _databaseManager.Setup(x => x.GetDatabaseServerInfo(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(ServerInfo(true)));
        _databaseManager.Setup(x => x.IsServerLocal(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(true));

        _originalConsoleOutput = Console.Out;
        Console.SetOut(_consoleOutput);
    }

    private DatabaseServerConnectionData Record => _parameters.DatabaseServerConnections[ConnectionName];

    public void Dispose()
    {
        Console.SetOut(_originalConsoleOutput);
        _consoleOutput.Dispose();
        _webAgent.Dispose();
    }

    // ---------- menus ----------

    [Fact]
    public void Create_UsesTheConnectionsOfTheParameters()
    {
        // Act
        DatabaseServerConnectionCruder sut = DatabaseServerConnectionCruder.Create(Application(), _logger.Object,
            null, _parametersManager.Object);

        // Assert
        Assert.Equal([ConnectionName], sut.GetKeys());
    }

    [Fact]
    public void GetListMenu_NamesTheRecordsDatabaseServerConnections()
    {
        // Act
        CliMenuSet listMenu = CreateSut().GetListMenu();

        // Assert
        Assert.Equal("Database Server Connections", listMenu.Caption);
        Assert.Equal("New Database Server Connection", CliMenuTestAccess.GetMenuItems(listMenu)[0].MenuItemName);
    }

    [Fact]
    public void GetStatusFor_ShowsProviderAndServerAddress()
    {
        Assert.Equal("SqlServer: TestSqlServer,1444", CreateSut().GetStatusFor(ConnectionName));
    }

    //the defaults that "New Database Server Connection" applies before asking for the fields
    [Fact]
    public void FieldEditors_SetDefault_GivesTheDefaultsOfANewRecord()
    {
        // Arrange
        var record = new DatabaseServerConnectionData
        {
            DatabaseServerProvider = EDatabaseProvider.SqlServer, Encrypt = true, ConnectionTimeOut = 99
        };

        // Act
        foreach (FieldEditor fieldEditor in FieldEditors(CreateSut()))
        {
            fieldEditor.SetDefault(record);
        }

        // Assert
        Assert.Equal(EDatabaseProvider.None, record.DatabaseServerProvider);
        Assert.True(record.TrustServerCertificate);
        Assert.False(record.Encrypt);
        Assert.Equal(15, record.ConnectionTimeOut);
    }

    [Fact]
    public void GetDetailsSubMenu_WhenProviderIsNone_ShowsOnlyTheProvider()
    {
        // Arrange
        Record.DatabaseServerProvider = EDatabaseProvider.None;

        // Act & Assert
        Assert.Equal(["Record Name", "Database Server Provider"], FieldNames(CreateSut(true)));
    }

    [Fact]
    public void GetDetailsSubMenu_SqlServerWithSqlAuthentication_ShowsServerFields()
    {
        Assert.Equal(
        [
            "Record Name", "Database Server Provider", "Server Address", "Windows Nt Integrated Security",
            "Server User", "Server Pass", "Trust Server Certificate", "Encrypt", "Connection Time Out",
            "Database Folders Sets"
        ], FieldNames(CreateSut(true)));
    }

    [Fact]
    public void GetDetailsSubMenu_SqlServerWithIntegratedSecurity_HidesUserAndPassword()
    {
        // Arrange
        Record.WindowsNtIntegratedSecurity = true;

        // Act
        List<string> names = FieldNames(CreateSut(true));

        // Assert
        Assert.DoesNotContain("Server User", names);
        Assert.DoesNotContain("Server Pass", names);
        Assert.Contains("Windows Nt Integrated Security", names);
    }

    [Fact]
    public void GetDetailsSubMenu_MariaDb_HidesSqlServerOnlyFields()
    {
        // Arrange
        Record.DatabaseServerProvider = EDatabaseProvider.MariaDb;

        // Act & Assert
        Assert.Equal(
        [
            "Record Name", "Database Server Provider", "Server Address", "Server User", "Server Pass",
            "Connection Time Out", "Database Folders Sets"
        ], FieldNames(CreateSut(true)));
    }

    [Theory]
    [InlineData(EDatabaseProvider.SqLite)]
    [InlineData(EDatabaseProvider.OleDb)]
    public void GetDetailsSubMenu_FileDb_ShowsOnlyPasswordAndFolders(EDatabaseProvider provider)
    {
        // Arrange
        Record.DatabaseServerProvider = provider;

        // Act & Assert
        Assert.Equal(["Record Name", "Database Server Provider", "Server Pass", "Database Folders Sets"],
            FieldNames(CreateSut(true)));
    }

    [Fact]
    public void GetDetailsSubMenu_WebAgent_ShowsOnlyWebAgentFields()
    {
        // Arrange
        Record.DatabaseServerProvider = EDatabaseProvider.WebAgent;

        // Act & Assert
        Assert.Equal(["Record Name", "Database Server Provider", "Db Web Agent Name", "Remote Db Connection Name"],
            FieldNames(CreateSut(true)));
    }

    [Fact]
    public void GetDetailsSubMenu_WithoutHttpClientFactory_HasNoWebAgentFields()
    {
        // Arrange
        Record.DatabaseServerProvider = EDatabaseProvider.WebAgent;

        // Act & Assert
        Assert.Equal(["Record Name", "Database Server Provider"], FieldNames(CreateSut()));
    }

    [Fact]
    public void GetItemMenu_ShowsTheFieldsBeforeTheCommands()
    {
        // Act
        List<string> names = ItemMenuNames(CreateSut());

        // Assert
        Assert.True(names.IndexOf("Server Address") >= 0);
        Assert.True(names.IndexOf("Server Address") < names.IndexOf(GetFoldersCommandName));
    }

    [Theory]
    [InlineData(EDatabaseProvider.SqlServer, true)]
    [InlineData(EDatabaseProvider.SqLite, true)]
    [InlineData(EDatabaseProvider.WebAgent, false)]
    public void GetItemMenu_OffersGetFoldersExceptForWebAgent(EDatabaseProvider provider, bool expected)
    {
        // Arrange
        Record.DatabaseServerProvider = provider;

        // Act & Assert
        Assert.Equal(expected, ItemMenuNames(CreateSut()).Contains(GetFoldersCommandName));
    }

    // ---------- CheckValidation with the real DatabaseManagersFactory ----------

    [Fact]
    public void CheckValidation_WhenProviderIsNone_PrintsTheFactoryError()
    {
        // Arrange
        Record.DatabaseServerProvider = EDatabaseProvider.None;

        // Act
        bool result = CreateSut().CheckValidation(Record);

        // Assert
        Assert.False(result);
        Assert.Contains("Database Provider is None", _consoleOutput.ToString(), StringComparison.Ordinal);
        Assert.Contains("dbManager could not created", _consoleOutput.ToString(), StringComparison.Ordinal);
    }

    //the factory is called with useConsole: its own messages reach the console
    [Fact]
    public void CheckValidation_MariaDbWithoutUser_FactoryWritesItsMessageToTheConsole()
    {
        // Arrange
        SetUnreachableMariaDb();
        Record.ServerUser = null;

        // Act
        bool result = CreateSut().CheckValidation(Record);

        // Assert
        Assert.False(result);
        Assert.Contains("windowsNtIntegratedSecurity is off and serverUser does not specified",
            _consoleOutput.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void CheckValidation_WhenServerDoesNotAnswer_ReturnsFalseAfterConnectionAttempt()
    {
        // Arrange
        SetUnreachableMariaDb();

        // Act
        bool result = CreateSut().CheckValidation(Record);

        // Assert
        Assert.False(result);
        Assert.Contains("Try connect to server...", _consoleOutput.ToString(), StringComparison.Ordinal);
    }

    //a web agent connection is valid when the web agent connects; the server folders are not read through it
    [Fact]
    public void CheckValidation_WebAgentThatConnects_ReturnsTrueWithoutReadingServerInfo()
    {
        // Arrange
        SetWebAgent(HttpStatusCode.OK);

        // Act
        bool result = CreateSut(true).CheckValidation(Record);

        // Assert
        Assert.True(result);
        HttpRequestMessage request = Assert.Single(_webAgent.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.EndsWith("/databases/testconnection", request.RequestUri!.AbsolutePath, StringComparison.Ordinal);
        Assert.Empty(Record.DatabaseFoldersSets!);
    }

    [Fact]
    public void CheckValidation_WebAgentThatFails_ReturnsFalse()
    {
        // Arrange
        SetWebAgent(HttpStatusCode.InternalServerError);

        // Act
        bool result = CreateSut(true).CheckValidation(Record);

        // Assert
        Assert.False(result);
        Assert.Single(_webAgent.Requests);
    }

    // ---------- CheckValidation with a database manager double ----------

    [Fact]
    public void CheckValidation_WhenManagerCanNotBeCreated_PrintsItsError()
    {
        // Act
        bool result = CreateSutWithManager((_, _, _) =>
            ValueTask.FromResult(Result.Failure<IDatabaseManager>(TestConnectionError))).CheckValidation(Record);

        // Assert
        Assert.False(result);
        Assert.Contains(TestConnectionError.Description, _consoleOutput.ToString(), StringComparison.Ordinal);
        Assert.Contains("dbManager could not created", _consoleOutput.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void CheckValidation_PassesTheRecordAndTheApiClientsToTheFactory()
    {
        // Arrange
        _parameters.ApiClients[WebAgentName] = new ApiClientSettings { Server = "http://127.0.0.1:0/" };
        DatabaseServerConnectionData? passedRecord = null;
        ApiClients? passedApiClients = null;

        // Act
        CreateSutWithManager((record, apiClients, _) =>
        {
            passedRecord = record;
            passedApiClients = apiClients;
            return ValueTask.FromResult(Result.Success(_databaseManager.Object));
        }).CheckValidation(Record);

        // Assert
        Assert.Same(Record, passedRecord);
        Assert.NotNull(passedApiClients!.GetApiClientByKey(WebAgentName));
    }

    [Fact]
    public void CheckValidation_WhenConnectionFails_PrintsTheErrorAndDoesNotReadServerInfo()
    {
        // Arrange
        _databaseManager.Setup(x => x.TestConnection(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(TestConnectionError));

        // Act
        bool result = CreateSutWithManager().CheckValidation(Record);

        // Assert
        Assert.False(result);
        Assert.Contains(TestConnectionError.Description, _consoleOutput.ToString(), StringComparison.Ordinal);
        _databaseManager.Verify(x => x.GetDatabaseServerInfo(It.IsAny<CancellationToken>()), Times.Never);
        Assert.Empty(Record.DatabaseFoldersSets!);
    }

    [Fact]
    public void CheckValidation_WhenServerInfoFails_PrintsTheErrorAndReturnsFalse()
    {
        // Arrange
        _databaseManager.Setup(x => x.GetDatabaseServerInfo(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<DbServerInfo>(ServerInfoError));

        // Act
        bool result = CreateSutWithManager().CheckValidation(Record);

        // Assert
        Assert.False(result);
        Assert.Contains(ServerInfoError.Description, _consoleOutput.ToString(), StringComparison.Ordinal);
        Assert.Empty(Record.DatabaseFoldersSets!);
    }

    [Fact]
    public void CheckValidation_LocalServerWithCompression_PrintsServerInfoAndSetsDefaultFolders()
    {
        // Act
        bool result = CreateSutWithManager().CheckValidation(Record);

        // Assert
        Assert.True(result);
        Assert.Equal(Lines("Try connect to server...", "Server Name is TestServer",
            "Server is  Allows Compression", "Server is  local", "Server Product Version is 16.0.1000",
            "Server Instance Name is MSSQLSERVER", @"Backup Directory is C:\Sql\Backup",
            @"Default Data Directory is C:\Sql\Data", @"Default Log Directory is C:\Sql\Log"),
            _consoleOutput.ToString());
        DatabaseFoldersSet defaultSet = Record.DatabaseFoldersSets!["Default"];
        Assert.Equal(@"C:\Sql\Backup", defaultSet.Backup);
        Assert.Equal(@"C:\Sql\Data", defaultSet.Data);
        Assert.Equal(@"C:\Sql\Log", defaultSet.DataLog);
    }

    [Fact]
    public void CheckValidation_RemoteServerWithoutCompression_PrintsNot()
    {
        // Arrange
        _databaseManager.Setup(x => x.GetDatabaseServerInfo(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(ServerInfo(false)));
        _databaseManager.Setup(x => x.IsServerLocal(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(false));

        // Act
        bool result = CreateSutWithManager().CheckValidation(Record);

        // Assert
        Assert.True(result);
        string output = _consoleOutput.ToString();
        Assert.Contains(Lines("Server is NOT  Allows Compression", "Server is NOT  local"), output,
            StringComparison.Ordinal);
    }

    //reading the value of the failed result used to throw, so a server that answered was reported as invalid
    [Fact]
    public void CheckValidation_WhenServerLocalityIsUnknown_SaysSoAndStaysValid()
    {
        // Arrange
        _databaseManager.Setup(x => x.IsServerLocal(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<bool>(ServerInfoError));

        // Act
        bool result = CreateSutWithManager().CheckValidation(Record);

        // Assert
        Assert.True(result);
        Assert.Contains(Lines("Server is  Allows Compression", "Server is local or not is not detected",
            "Server Product Version is 16.0.1000"), _consoleOutput.ToString(), StringComparison.Ordinal);
        Assert.Equal(@"C:\Sql\Data", Record.DatabaseFoldersSets!["Default"].Data);
    }

    [Fact]
    public void CheckValidation_WhenCanceled_ReportsCancellation()
    {
        // Act
        bool result = CreateSutWithManager((_, _, _) => throw new OperationCanceledException())
            .CheckValidation(Record);

        // Assert
        Assert.False(result);
        Assert.Equal(Lines("Operation was canceled."), _consoleOutput.ToString());
    }

    [Fact]
    public void CheckValidation_WhenAnExceptionIsThrown_LogsItAndReturnsFalse()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        bool result = CreateSutWithManager((_, _, _) => throw exception).CheckValidation(Record);

        // Assert
        Assert.False(result);
#pragma warning disable CA1873
        _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, _) => v!.ToString() == ValidationExceptionMessage), exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
#pragma warning restore CA1873
    }

    // ---------- CheckFieldsEnables ----------

    [Theory]
    [InlineData(nameof(DatabaseServerConnectionData.ServerUser))]
    [InlineData(nameof(DatabaseServerConnectionData.ServerPass))]
    [InlineData(nameof(DatabaseServerConnectionData.ServerAddress))]
    public void CheckFieldsEnables_AfterEditingAConnectionFieldWithAllFilled_ChecksTheConnection(
        string lastEditedFieldName)
    {
        // Arrange
        SetUnreachableMariaDb();

        // Act
        CheckFieldsEnables(CreateSut(), lastEditedFieldName);

        // Assert
        Assert.Contains("Try connect to server...", _consoleOutput.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void CheckFieldsEnables_AfterEditingAnotherField_DoesNotCheckTheConnection()
    {
        // Arrange
        SetUnreachableMariaDb();

        // Act
        CheckFieldsEnables(CreateSut(), nameof(DatabaseServerConnectionData.ConnectionTimeOut));

        // Assert
        Assert.Empty(_consoleOutput.ToString());
    }

    [Theory]
    [InlineData(nameof(DatabaseServerConnectionData.ServerAddress))]
    [InlineData(nameof(DatabaseServerConnectionData.ServerUser))]
    [InlineData(nameof(DatabaseServerConnectionData.ServerPass))]
    public void CheckFieldsEnables_WhenAConnectionFieldIsEmpty_DoesNotCheckTheConnection(string emptyFieldName)
    {
        // Arrange
        SetUnreachableMariaDb();
        typeof(DatabaseServerConnectionData).GetProperty(emptyFieldName)!.SetValue(Record, " ");

        // Act
        CheckFieldsEnables(CreateSut(), nameof(DatabaseServerConnectionData.ServerUser));

        // Assert
        Assert.Empty(_consoleOutput.ToString());
    }

    [Fact]
    public void CheckFieldsEnables_WhenProviderIsNone_DoesNotCheckTheConnection()
    {
        // Arrange
        SetUnreachableMariaDb();
        Record.DatabaseServerProvider = EDatabaseProvider.None;

        // Act
        CheckFieldsEnables(CreateSut(), nameof(DatabaseServerConnectionData.ServerUser));

        // Assert
        Assert.Empty(_consoleOutput.ToString());
    }

    [Fact]
    public void CheckFieldsEnables_WhenProviderIsUnknown_Throws()
    {
        // Arrange
        Record.DatabaseServerProvider = (EDatabaseProvider)999;

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() => CheckFieldsEnables(CreateSut(), null));
        var inner = Assert.IsType<ArgumentOutOfRangeException>(exception.InnerException);
        Assert.StartsWith("Unexpected database server provider value.", inner.Message, StringComparison.Ordinal);
    }

    // ---------- helpers ----------

    private void SetUnreachableMariaDb()
    {
        Record.DatabaseServerProvider = EDatabaseProvider.MariaDb;
        Record.ServerAddress = UnreachableMariaDbAddress;
        Record.ConnectionTimeOut = 1;
    }

    //port 0: the message hub of the web agent's api client fails at once instead of waiting for a server
    private void SetWebAgent(HttpStatusCode statusCode)
    {
        _webAgent.StatusCode = statusCode;
        _parameters.ApiClients[WebAgentName] = new ApiClientSettings { Server = "http://127.0.0.1:0/" };
        Record.DatabaseServerProvider = EDatabaseProvider.WebAgent;
        Record.DbWebAgentName = WebAgentName;
    }

    private void CheckFieldsEnables(DatabaseServerConnectionCruder sut, string? lastEditedFieldName)
    {
        typeof(DatabaseServerConnectionCruder)
            .GetMethod("CheckFieldsEnables", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(sut, [Record, lastEditedFieldName]);
    }

    private static List<FieldEditor> FieldEditors(DatabaseServerConnectionCruder sut)
    {
        return (List<FieldEditor>)typeof(CliParameters.Cruders.Cruder)
            .GetField("FieldEditors", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(sut)!;
    }

    private static List<string> FieldNames(DatabaseServerConnectionCruder sut)
    {
        return [.. sut.GetDetailsSubMenu(ConnectionName).Select(x => x.Name)];
    }

    private static List<string> ItemMenuNames(DatabaseServerConnectionCruder sut)
    {
        return [.. CliMenuTestAccess.GetMenuItems(sut.GetItemMenu(ConnectionName)).Select(x => x.MenuItemName)];
    }

    private static string Lines(params string[] lines)
    {
        return string.Concat(lines.Select(x => x + Environment.NewLine));
    }

    private static DbServerInfo ServerInfo(bool allowsCompression)
    {
        return new DbServerInfo("16.0.1000", "MSSQLSERVER", @"C:\Sql\Backup", @"C:\Sql\Data", @"C:\Sql\Log",
            allowsCompression, "TestServer");
    }

    private DatabaseServerConnectionCruder CreateSut(bool withHttpClientFactory = false)
    {
        IHttpClientFactory? httpClientFactory = null;
        if (withHttpClientFactory)
        {
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(_webAgent, false));
            httpClientFactory = factory.Object;
        }

        return new DatabaseServerConnectionCruder(Application(), _logger.Object, httpClientFactory,
            _parametersManager.Object, _parameters.DatabaseServerConnections);
    }

    private DatabaseServerConnectionCruder CreateSutWithManager(
        Func<DatabaseServerConnectionData, ApiClients, CancellationToken, ValueTask<Result<IDatabaseManager>>>?
            createDatabaseManager = null)
    {
        return new DatabaseServerConnectionCruder(Application(), _logger.Object, null, _parametersManager.Object,
            _parameters.DatabaseServerConnections,
            createDatabaseManager ?? ((_, _, _) => ValueTask.FromResult(Result.Success(_databaseManager.Object))));
    }

    private static IApplication Application()
    {
        return Mock.Of<IApplication>(x => x.AppName == "TestApp");
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(new HttpResponseMessage(StatusCode) { Content = new StringContent(string.Empty) });
        }
    }
}
