using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.IO;
using AppCliTools.CliParametersDataEdit.Models;
using DatabaseTools.DbTools.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using ParametersManagement.LibDatabaseParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersDataEdit.Tests;

//error paths write to the console (StShared.WriteErrorLine), so the class must not run beside console-capturing ones
[Collection(ConsoleCaptureCollection.Name)]
public sealed class DbConnectionFactoryTests : IDisposable
{
    private const string ConnectionName = "TestConnection";
    private const string FoldersSetName = "Default";
    private const string DataFolder = @"C:\TestData";
    private const string NotCreatedMessage = "could not Created Connection String";

    private readonly Dictionary<string, DatabaseServerConnectionData> _connections = [];
    private readonly StringWriter _consoleOutput = new(CultureInfo.InvariantCulture);
    private readonly TextWriter _originalConsoleOutput;

    public DbConnectionFactoryTests()
    {
        _originalConsoleOutput = Console.Out;
        Console.SetOut(_consoleOutput);
    }

    public void Dispose()
    {
        Console.SetOut(_originalConsoleOutput);
        _consoleOutput.Dispose();
    }

    // ---------- GetDbConnectionParameters ----------

    [Fact]
    public void GetDbConnectionParameters_WhenProviderIsNone_ReturnsNull()
    {
        Assert.Null(DbConnectionFactory.GetDbConnectionParameters(EDatabaseProvider.None, "Data Source=x"));
    }

    [Fact]
    public void GetDbConnectionParameters_SqlServer_ReadsEveryField()
    {
        // Act
        DbConnectionParameters? result = DbConnectionFactory.GetDbConnectionParameters(EDatabaseProvider.SqlServer,
            "Data Source=TestSqlServer;Initial Catalog=AppDb;User ID=TestUser;Password=TestPass;" +
            "Connect Timeout=30;Encrypt=True;Trust Server Certificate=True");

        // Assert
        SqlServerConnectionParameters par = Assert.IsType<SqlServerConnectionParameters>(result);
        Assert.Equal("TestSqlServer", par.ServerAddress);
        Assert.False(par.WindowsNtIntegratedSecurity);
        Assert.Equal("TestUser", par.ServerUser);
        Assert.Equal("TestPass", par.ServerPass);
        Assert.Equal("AppDb", par.DatabaseName);
        Assert.Equal(30, par.ConnectionTimeOut);
        Assert.True(par.Encrypt);
        Assert.True(par.TrustServerCertificate);
    }

    [Fact]
    public void GetDbConnectionParameters_SqlServerIntegratedSecurity_ReadsIntegratedSecurity()
    {
        // Act
        DbConnectionParameters? result = DbConnectionFactory.GetDbConnectionParameters(EDatabaseProvider.SqlServer,
            "Data Source=.;Integrated Security=True;Encrypt=False");

        // Assert
        SqlServerConnectionParameters par = Assert.IsType<SqlServerConnectionParameters>(result);
        Assert.True(par.WindowsNtIntegratedSecurity);
        Assert.False(par.Encrypt);
        Assert.False(par.TrustServerCertificate);
    }

    [Fact]
    public void GetDbConnectionParameters_SqLite_ReadsFileAndPassword()
    {
        // Act
        DbConnectionParameters? result = DbConnectionFactory.GetDbConnectionParameters(EDatabaseProvider.SqLite,
            @"Data Source=C:\TestData\App.db;Password=TestPass");

        // Assert
        SqLiteConnectionParameters par = Assert.IsType<SqLiteConnectionParameters>(result);
        Assert.Equal(@"C:\TestData\App.db", par.DatabaseFilePath);
        Assert.Equal("TestPass", par.Password);
    }

    [Fact]
    public void GetDbConnectionParameters_OleDbWithPersistedPassword_ReadsPassword()
    {
        // Act
        DbConnectionParameters? result = DbConnectionFactory.GetDbConnectionParameters(EDatabaseProvider.OleDb,
            @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\TestData\App.accdb;Persist Security Info=True;" +
            "Jet OLEDB:Database Password=TestPass");

        // Assert
        OleDbConnectionParameters par = Assert.IsType<OleDbConnectionParameters>(result);
        Assert.Equal(@"C:\TestData\App.accdb", par.DatabaseFilePath);
        Assert.Equal("Microsoft.ACE.OLEDB.12.0", par.Provider);
        Assert.True(par.PersistSecurityInfo);
        Assert.Equal("TestPass", par.Password);
    }

    [Fact]
    public void GetDbConnectionParameters_OleDbWithoutPersistSecurityInfo_DoesNotReadPassword()
    {
        // Act
        DbConnectionParameters? result = DbConnectionFactory.GetDbConnectionParameters(EDatabaseProvider.OleDb,
            @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\TestData\App.accdb;" +
            "Jet OLEDB:Database Password=TestPass");

        // Assert
        OleDbConnectionParameters par = Assert.IsType<OleDbConnectionParameters>(result);
        Assert.False(par.PersistSecurityInfo);
        Assert.Null(par.Password);
    }

    [Fact]
    public void GetDbConnectionParameters_OleDbWithPersistSecurityInfoButNoPassword_LeavesPasswordNull()
    {
        // Act
        DbConnectionParameters? result = DbConnectionFactory.GetDbConnectionParameters(EDatabaseProvider.OleDb,
            @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\TestData\App.accdb;Persist Security Info=True");

        // Assert
        OleDbConnectionParameters par = Assert.IsType<OleDbConnectionParameters>(result);
        Assert.Null(par.Password);
    }

    [Theory]
    [InlineData(EDatabaseProvider.MariaDb)]
    [InlineData(EDatabaseProvider.WebAgent)]
    public void GetDbConnectionParameters_UnsupportedProvider_Throws(EDatabaseProvider provider)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            DbConnectionFactory.GetDbConnectionParameters(provider, "Server=x"));
        Assert.StartsWith("Unsupported database provider.", exception.Message, StringComparison.Ordinal);
    }

    // ---------- GetDataProviderConnectionStringCommandTimeOut ----------

    [Fact]
    public void GetDataProviderConnectionStringCommandTimeOut_WhenParametersAreNull_Fails()
    {
        AssertFailure(DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(null, Connections()),
            "databasesParameters does not specified");
    }

    [Fact]
    public void GetDataProviderConnectionStringCommandTimeOut_WhenConnectionNameIsNull_Fails()
    {
        AssertFailure(DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(
            new DatabaseParameters { DatabaseName = "AppDb" }, Connections()),
            "databasesParameters.DbConnectionName does not specified");
    }

    [Fact]
    public void GetDataProviderConnectionStringCommandTimeOut_WhenConnectionIsMissing_Fails()
    {
        AssertFailure(DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(
            DatabaseParameters("AppDb"), Connections()), "DatabaseConnectionData Data is not Created");
    }

    [Fact]
    public void GetDataProviderConnectionStringCommandTimeOut_WhenProviderIsNone_Fails()
    {
        // Arrange
        _connections[ConnectionName] = new DatabaseServerConnectionData();

        // Act & Assert
        AssertFailure(DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(
            DatabaseParameters("AppDb"), Connections()), NotCreatedMessage);
    }

    [Fact]
    public void GetDataProviderConnectionStringCommandTimeOut_SqlServer_ReturnsConnectionStringWithDatabase()
    {
        // Arrange
        _connections[ConnectionName] = DatabaseServerConnectionStringConverterTests.SqlServerData();

        // Act
        (EDatabaseProvider? provider, string? connectionString, int commandTimeOut) =
            DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(DatabaseParameters("AppDb"),
                Connections());

        // Assert
        Assert.Equal(EDatabaseProvider.SqlServer, provider);
        Assert.Equal(120, commandTimeOut);
        var builder = new SqlConnectionStringBuilder(connectionString);
        Assert.Equal("TestSqlServer,1444", builder.DataSource);
        Assert.Equal("AppDb", builder.InitialCatalog);
        Assert.False(builder.IntegratedSecurity);
        Assert.Equal("TestUser", builder.UserID);
        Assert.Equal("TestPass", builder.Password);
        Assert.Equal(30, builder.ConnectTimeout);
        Assert.True(builder.Encrypt);
        Assert.True(builder.TrustServerCertificate);
    }

    [Fact]
    public void GetDataProviderConnectionStringCommandTimeOut_SqlServerIntegratedSecurity_OmitsUserAndDatabase()
    {
        // Arrange
        DatabaseServerConnectionData data = DatabaseServerConnectionStringConverterTests.SqlServerData();
        data.WindowsNtIntegratedSecurity = true;
        data.Encrypt = false;
        data.TrustServerCertificate = false;
        _connections[ConnectionName] = data;

        // Act
        (_, string? connectionString, _) =
            DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(DatabaseParameters(null),
                Connections());

        // Assert
        Assert.Equal("Data Source=TestSqlServer,1444;Integrated Security=True;Connect Timeout=30;Encrypt=False;" +
            "Trust Server Certificate=False",
            connectionString);
    }

    [Fact]
    public void GetDataProviderConnectionStringCommandTimeOut_SqLite_CombinesDataFolderAndDatabaseName()
    {
        // Arrange
        _connections[ConnectionName] = FileDbData(EDatabaseProvider.SqLite, "TestPass");

        // Act
        (EDatabaseProvider? provider, string? connectionString, _) =
            DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(DatabaseParameters("App.db"),
                Connections());

        // Assert
        Assert.Equal(EDatabaseProvider.SqLite, provider);
        var builder = new SqliteConnectionStringBuilder(connectionString);
        Assert.Equal(Path.Combine(DataFolder, "App.db"), builder.DataSource);
        Assert.Equal("TestPass", builder.Password);
    }

    [Fact]
    public void GetDataProviderConnectionStringCommandTimeOut_SqLiteWithoutPassword_OmitsPassword()
    {
        // Arrange
        _connections[ConnectionName] = FileDbData(EDatabaseProvider.SqLite, " ");

        // Act
        (_, string? connectionString, _) =
            DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(DatabaseParameters("App.db"),
                Connections());

        // Assert
        Assert.Equal($"Data Source={Path.Combine(DataFolder, "App.db")}", connectionString);
    }

    [Fact]
    public void GetDataProviderConnectionStringCommandTimeOut_OleDb_UsesAceProvider()
    {
        // Arrange
        _connections[ConnectionName] = FileDbData(EDatabaseProvider.OleDb, null);

        // Act
        (EDatabaseProvider? provider, string? connectionString, _) =
            DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(DatabaseParameters("App.accdb"),
                Connections());

        // Assert
        Assert.Equal(EDatabaseProvider.OleDb, provider);
        Assert.Equal($@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={DataFolder}\App.accdb", connectionString);
    }

    [Fact]
    public void GetDataProviderConnectionStringCommandTimeOut_OleDbWithPassword_PersistsThePassword()
    {
        // Arrange
        _connections[ConnectionName] = FileDbData(EDatabaseProvider.OleDb, "TestPass");

        // Act
        (_, string? connectionString, _) =
            DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(DatabaseParameters("App.accdb"),
                Connections());

        // Assert
        var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
        Assert.Equal("True", builder["Persist Security Info"]);
        Assert.Equal("TestPass", builder["Jet OLEDB:Database Password"]);
        Assert.Equal("Microsoft.ACE.OLEDB.12.0", builder["Provider"]);
    }

    [Theory]
    [InlineData(EDatabaseProvider.SqLite)]
    [InlineData(EDatabaseProvider.OleDb)]
    public void GetDataProviderConnectionStringCommandTimeOut_FileDbWithoutFoldersSetName_Fails(
        EDatabaseProvider provider)
    {
        // Arrange
        _connections[ConnectionName] = FileDbData(provider, null);
        DatabaseParameters parameters = DatabaseParameters("App.db");
        parameters.DbServerFoldersSetName = null;

        // Act & Assert
        AssertFailure(DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(parameters, Connections()),
            "DatabaseFoldersSets does not contain key ");
    }

    [Fact]
    public void GetDataProviderConnectionStringCommandTimeOut_FileDbWithoutFoldersSets_Fails()
    {
        // Arrange
        DatabaseServerConnectionData data = FileDbData(EDatabaseProvider.SqLite, null);
        data.DatabaseFoldersSets = null;
        _connections[ConnectionName] = data;

        // Act & Assert
        AssertFailure(DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(
            DatabaseParameters("App.db"), Connections()), $"DatabaseFoldersSets does not contain key {FoldersSetName}");
    }

    [Fact]
    public void GetDataProviderConnectionStringCommandTimeOut_FileDbWithUnknownFoldersSet_Fails()
    {
        // Arrange
        _connections[ConnectionName] = FileDbData(EDatabaseProvider.SqLite, null);
        DatabaseParameters parameters = DatabaseParameters("App.db");
        parameters.DbServerFoldersSetName = "Other";

        // Act & Assert
        AssertFailure(DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(parameters, Connections()),
            "DatabaseFoldersSets does not contain key Other");
    }

    [Theory]
    [InlineData(EDatabaseProvider.SqLite)]
    [InlineData(EDatabaseProvider.OleDb)]
    public void GetDataProviderConnectionStringCommandTimeOut_FileDbWithoutDataFolder_Fails(
        EDatabaseProvider provider)
    {
        // Arrange
        DatabaseServerConnectionData data = FileDbData(provider, null);
        data.DatabaseFoldersSets![FoldersSetName].Data = " ";
        _connections[ConnectionName] = data;

        // Act & Assert
        AssertFailure(DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(
            DatabaseParameters("App.db"), Connections()), "dataPath is not specified");
    }

    [Fact]
    public void GetDataProviderConnectionStringCommandTimeOut_FileDbWithoutDatabaseName_Fails()
    {
        // Arrange
        _connections[ConnectionName] = FileDbData(EDatabaseProvider.SqLite, null);

        // Act & Assert
        AssertFailure(DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(DatabaseParameters(" "),
            Connections()), "databaseName is not specified");
    }

    [Fact]
    public void GetDataProviderConnectionStringCommandTimeOut_MariaDb_ReturnsConnectionStringWithDatabase()
    {
        // Arrange
        _connections[ConnectionName] = new DatabaseServerConnectionData
        {
            DatabaseServerProvider = EDatabaseProvider.MariaDb,
            ServerAddress = "maria.local:3307",
            ServerUser = "root",
            ServerPass = "TestPass",
            ConnectionTimeOut = 20
        };

        // Act
        (EDatabaseProvider? provider, string? connectionString, _) =
            DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(DatabaseParameters("ErrataDB"),
                Connections());

        // Assert
        Assert.Equal(EDatabaseProvider.MariaDb, provider);
        var builder = new MySqlConnectionStringBuilder(connectionString!);
        Assert.Equal("maria.local", builder.Server);
        Assert.Equal(3307u, builder.Port);
        Assert.Equal("ErrataDB", builder.Database);
        Assert.Equal(20u, builder.ConnectionTimeout);
    }

    [Fact]
    public void GetDataProviderConnectionStringCommandTimeOut_WebAgent_Throws()
    {
        // Arrange
        _connections[ConnectionName] =
            new DatabaseServerConnectionData { DatabaseServerProvider = EDatabaseProvider.WebAgent };

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(DatabaseParameters("AppDb"),
                Connections()));
        Assert.StartsWith("Unsupported database provider.", exception.Message, StringComparison.Ordinal);
    }

    // ---------- GetDbConnectionStringBuilder(DbConnectionParameters) ----------

    [Fact]
    public void GetDbConnectionStringBuilder_SqlServer_SetsEveryField()
    {
        // Arrange
        var par = new SqlServerConnectionParameters
        {
            ServerAddress = "TestSqlServer",
            ServerUser = "TestUser",
            ServerPass = "TestPass",
            DatabaseName = "AppDb",
            ConnectionTimeOut = 30,
            Encrypt = true,
            TrustServerCertificate = true
        };

        // Act
        var builder = Assert.IsType<SqlConnectionStringBuilder>(DbConnectionFactory.GetDbConnectionStringBuilder(par));

        // Assert
        Assert.Equal("TestSqlServer", builder.DataSource);
        Assert.False(builder.IntegratedSecurity);
        Assert.Equal("TestUser", builder.UserID);
        Assert.Equal("TestPass", builder.Password);
        Assert.Equal("AppDb", builder.InitialCatalog);
        Assert.Equal(30, builder.ConnectTimeout);
        Assert.True(builder.Encrypt);
        Assert.True(builder.TrustServerCertificate);
    }

    [Fact]
    public void GetDbConnectionStringBuilder_SqlServerIntegratedSecurity_OmitsUserAndDatabase()
    {
        // Arrange
        var par = new SqlServerConnectionParameters
        {
            ServerAddress = "TestSqlServer",
            WindowsNtIntegratedSecurity = true,
            ServerUser = "TestUser",
            ServerPass = "TestPass",
            ConnectionTimeOut = 30
        };

        // Act
        DbConnectionStringBuilder? builder = DbConnectionFactory.GetDbConnectionStringBuilder(par);

        // Assert
        Assert.Equal("Data Source=TestSqlServer;Integrated Security=True;Connect Timeout=30;Encrypt=False;" +
            "Trust Server Certificate=False",
            builder?.ConnectionString);
    }

    [Fact]
    public void GetDbConnectionStringBuilder_SqLite_SetsFileAndPassword()
    {
        // Act
        var builder = Assert.IsType<SqliteConnectionStringBuilder>(DbConnectionFactory.GetDbConnectionStringBuilder(
            new SqLiteConnectionParameters { DatabaseFilePath = @"C:\TestData\App.db", Password = "TestPass" }));

        // Assert
        Assert.Equal(@"C:\TestData\App.db", builder.DataSource);
        Assert.Equal("TestPass", builder.Password);
    }

    [Fact]
    public void GetDbConnectionStringBuilder_OtherParameters_ReturnsNull()
    {
        Assert.Null(DbConnectionFactory.GetDbConnectionStringBuilder(new OleDbConnectionParameters()));
    }

    // ---------- helpers ----------

    private DatabaseServerConnections Connections()
    {
        return new DatabaseServerConnections(_connections);
    }

    private static DatabaseParameters DatabaseParameters(string? databaseName)
    {
        return new DatabaseParameters
        {
            DbConnectionName = ConnectionName,
            DatabaseName = databaseName,
            DbServerFoldersSetName = FoldersSetName,
            CommandTimeOut = 120
        };
    }

    private static DatabaseServerConnectionData FileDbData(EDatabaseProvider provider, string? password)
    {
        return new DatabaseServerConnectionData
        {
            DatabaseServerProvider = provider,
            ServerPass = password,
            DatabaseFoldersSets = new Dictionary<string, DatabaseFoldersSet>
            {
                [FoldersSetName] = new() { Data = DataFolder }
            }
        };
    }

    private void AssertFailure((EDatabaseProvider?, string?, int) result, string expectedMessage)
    {
        Assert.Equal((null, null, -1), result);
        Assert.Contains(expectedMessage, _consoleOutput.ToString(), StringComparison.Ordinal);
    }
}
