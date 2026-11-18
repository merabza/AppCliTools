using ParametersManagement.LibDatabaseParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersDataEdit.Tests;

public sealed class DatabaseServerConnectionStringConverterTests
{
    private const string InvalidPrefix = "Connection string is not valid: ";

    [Theory]
    [InlineData(EDatabaseProvider.SqlServer, true)]
    [InlineData(EDatabaseProvider.MariaDb, true)]
    [InlineData(EDatabaseProvider.None, false)]
    [InlineData(EDatabaseProvider.SqLite, false)]
    [InlineData(EDatabaseProvider.OleDb, false)]
    [InlineData(EDatabaseProvider.WebAgent, false)]
    public void CanCreateConnectionString_OnlyForServerProviders(EDatabaseProvider provider, bool expected)
    {
        Assert.Equal(expected, DatabaseServerConnectionStringConverter.CanCreateConnectionString(provider));
    }

    //WebAgent: DbConnectionFactory would throw for it, so the provider check must come first
    [Theory]
    [InlineData(EDatabaseProvider.None)]
    [InlineData(EDatabaseProvider.SqLite)]
    [InlineData(EDatabaseProvider.OleDb)]
    [InlineData(EDatabaseProvider.WebAgent)]
    public void CreateConnectionString_WhenProviderCanNotCreate_ReturnsNull(EDatabaseProvider provider)
    {
        var data = new DatabaseServerConnectionData { DatabaseServerProvider = provider, ServerPass = "TestPass" };

        Assert.Null(DatabaseServerConnectionStringConverter.CreateConnectionString(data));
    }

    [Fact]
    public void CreateConnectionString_SqlServer_ContainsParametersWithoutDatabase()
    {
        string? connectionString = DatabaseServerConnectionStringConverter.CreateConnectionString(SqlServerData());

        Assert.Equal(
            "Data Source=TestSqlServer,1444;Integrated Security=False;User ID=TestUser;Password=TestPass;" +
            "Connect Timeout=30;Encrypt=True;Trust Server Certificate=True", connectionString);
    }

    [Fact]
    public void CreateConnectionString_SqlServerWithDatabaseName_ContainsTheDatabase()
    {
        string? connectionString =
            DatabaseServerConnectionStringConverter.CreateConnectionString(SqlServerData("AppDb"));

        Assert.Equal(
            "Data Source=TestSqlServer,1444;Initial Catalog=AppDb;Integrated Security=False;User ID=TestUser;" +
            "Password=TestPass;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True", connectionString);
    }

    [Fact]
    public void CreateConnectionString_MariaDbWithDatabaseName_ContainsTheDatabase()
    {
        DatabaseServerConnectionData data = MariaDbData("maria.local:3307");
        data.DatabaseName = "ErrataDB";

        string? connectionString = DatabaseServerConnectionStringConverter.CreateConnectionString(data);

        Assert.Contains("Database=ErrataDB", connectionString, System.StringComparison.Ordinal);
    }

    //a database name that is only white space (e.g. typed into the parameters file) means no database
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateConnectionString_WhenDatabaseNameIsEmpty_ContainsNoDatabase(string databaseName)
    {
        Assert.Equal(DatabaseServerConnectionStringConverter.CreateConnectionString(SqlServerData()),
            DatabaseServerConnectionStringConverter.CreateConnectionString(SqlServerData(databaseName)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("AppDb")]
    public void ApplyConnectionString_SqlServerGeneratedString_RestoresAllParameters(string? databaseName)
    {
        DatabaseServerConnectionData source = SqlServerData(databaseName);
        string connectionString = DatabaseServerConnectionStringConverter.CreateConnectionString(source)!;
        var target = new DatabaseServerConnectionData { DatabaseServerProvider = EDatabaseProvider.SqlServer };

        string? error = DatabaseServerConnectionStringConverter.ApplyConnectionString(target, connectionString);

        Assert.Null(error);
        AssertServerParametersEqual(source, target);
    }

    [Fact]
    public void ApplyConnectionString_SqlServerIntegratedSecurity_ClearsUserAndPasswordAndStoresDatabase()
    {
        DatabaseServerConnectionData data = SqlServerData();

        string? error = DatabaseServerConnectionStringConverter.ApplyConnectionString(data,
            "Server=.\\SQL2022;Database=AppDb;Integrated Security=SSPI;User ID=Ignored;Password=Ignored;Encrypt=False");

        Assert.Null(error);
        Assert.Equal(".\\SQL2022", data.ServerAddress);
        Assert.True(data.WindowsNtIntegratedSecurity);
        Assert.Null(data.ServerUser);
        Assert.Null(data.ServerPass);
        Assert.False(data.Encrypt);
        Assert.False(data.TrustServerCertificate);
        //the SqlClient default
        Assert.Equal(15, data.ConnectionTimeOut);
        Assert.Equal("AppDb", data.DatabaseName);
    }

    //the connection string describes the whole connection, so a string without a database removes the old one
    [Theory]
    [InlineData(EDatabaseProvider.SqlServer, "Data Source=NewServer;Integrated Security=True")]
    [InlineData(EDatabaseProvider.MariaDb, "Server=maria.local;User ID=root")]
    public void ApplyConnectionString_WhenStringHasNoDatabase_RemovesTheDatabaseName(EDatabaseProvider provider,
        string connectionString)
    {
        DatabaseServerConnectionData data =
            provider == EDatabaseProvider.SqlServer ? SqlServerData() : MariaDbData("maria.local:3307");
        data.DatabaseName = "OldDb";

        string? error = DatabaseServerConnectionStringConverter.ApplyConnectionString(data, connectionString);

        Assert.Null(error);
        Assert.Null(data.DatabaseName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("ErrataDB")]
    public void ApplyConnectionString_MariaDbGeneratedString_RestoresAllParameters(string? databaseName)
    {
        DatabaseServerConnectionData source = MariaDbData("maria.local:3307");
        source.DatabaseName = databaseName;
        string connectionString = DatabaseServerConnectionStringConverter.CreateConnectionString(source)!;
        var target = new DatabaseServerConnectionData { DatabaseServerProvider = EDatabaseProvider.MariaDb };

        string? error = DatabaseServerConnectionStringConverter.ApplyConnectionString(target, connectionString);

        Assert.Null(error);
        Assert.Contains("Port=3307", connectionString, System.StringComparison.Ordinal);
        AssertServerParametersEqual(source, target);
    }

    [Fact]
    public void ApplyConnectionString_MariaDbDefaultPort_StoresAddressWithoutPortAndStoresDatabase()
    {
        DatabaseServerConnectionData data = MariaDbData("old.local:3310");

        string? error = DatabaseServerConnectionStringConverter.ApplyConnectionString(data,
            "Server=maria.local;Port=3306;Database=ErrataDB;User ID=root;Password=TestPass");

        Assert.Null(error);
        Assert.Equal("maria.local", data.ServerAddress);
        Assert.Equal("root", data.ServerUser);
        Assert.Equal("TestPass", data.ServerPass);
        //the MySqlConnector default
        Assert.Equal(15, data.ConnectionTimeOut);
        Assert.Equal("ErrataDB", data.DatabaseName);
    }

    [Theory]
    [InlineData(EDatabaseProvider.SqlServer, "Data Source", InvalidPrefix)]
    [InlineData(EDatabaseProvider.SqlServer, "Unknown Key=1", InvalidPrefix)]
    [InlineData(EDatabaseProvider.SqlServer, "User ID=TestUser", "Connection string does not contain Data Source")]
    [InlineData(EDatabaseProvider.SqlServer, "Data Source=x;Connect Timeout=abc", InvalidPrefix)]
    [InlineData(EDatabaseProvider.MariaDb, "Unknown Key=1", InvalidPrefix)]
    [InlineData(EDatabaseProvider.MariaDb, "User ID=root", "Connection string does not contain Server")]
    [InlineData(EDatabaseProvider.MariaDb, "Server=x;Port=abc", InvalidPrefix)]
    public void ApplyConnectionString_WhenInvalid_ReturnsErrorAndKeepsParameters(EDatabaseProvider provider,
        string connectionString, string expectedErrorStart)
    {
        DatabaseServerConnectionData data =
            provider == EDatabaseProvider.SqlServer ? SqlServerData("AppDb") : MariaDbData("maria.local:3307");
        DatabaseServerConnectionData original =
            provider == EDatabaseProvider.SqlServer ? SqlServerData("AppDb") : MariaDbData("maria.local:3307");

        string? error = DatabaseServerConnectionStringConverter.ApplyConnectionString(data, connectionString);

        Assert.StartsWith(expectedErrorStart, error, System.StringComparison.Ordinal);
        AssertServerParametersEqual(original, data);
    }

    //the message of the builder follows the prefix, so the reason is not lost
    [Fact]
    public void ApplyConnectionString_WhenInvalid_KeepsTheReasonOfTheBuilder()
    {
        string? error = DatabaseServerConnectionStringConverter.ApplyConnectionString(SqlServerData(), "Unknown Key=1");

        Assert.True(error!.Length > InvalidPrefix.Length);
    }

    [Fact]
    public void ApplyConnectionString_WhenProviderCanNotCreate_ReturnsError()
    {
        var data = new DatabaseServerConnectionData { DatabaseServerProvider = EDatabaseProvider.SqLite };

        string? error = DatabaseServerConnectionStringConverter.ApplyConnectionString(data, "Data Source=a.db");

        Assert.Equal("Connection string can not be used for SqLite provider", error);
        Assert.Null(data.ServerAddress);
    }

    internal static DatabaseServerConnectionData SqlServerData(string? databaseName = null)
    {
        return new DatabaseServerConnectionData
        {
            DatabaseServerProvider = EDatabaseProvider.SqlServer,
            ServerAddress = "TestSqlServer,1444",
            WindowsNtIntegratedSecurity = false,
            ServerUser = "TestUser",
            ServerPass = "TestPass",
            ConnectionTimeOut = 30,
            Encrypt = true,
            TrustServerCertificate = true,
            DatabaseName = databaseName
        };
    }

    private static DatabaseServerConnectionData MariaDbData(string serverAddress)
    {
        return new DatabaseServerConnectionData
        {
            DatabaseServerProvider = EDatabaseProvider.MariaDb,
            ServerAddress = serverAddress,
            ServerUser = "root",
            ServerPass = "TestPass",
            ConnectionTimeOut = 20
        };
    }

    private static void AssertServerParametersEqual(DatabaseServerConnectionData expected,
        DatabaseServerConnectionData actual)
    {
        Assert.Equal(expected.ServerAddress, actual.ServerAddress);
        Assert.Equal(expected.WindowsNtIntegratedSecurity, actual.WindowsNtIntegratedSecurity);
        Assert.Equal(expected.ServerUser, actual.ServerUser);
        Assert.Equal(expected.ServerPass, actual.ServerPass);
        Assert.Equal(expected.ConnectionTimeOut, actual.ConnectionTimeOut);
        Assert.Equal(expected.Encrypt, actual.Encrypt);
        Assert.Equal(expected.TrustServerCertificate, actual.TrustServerCertificate);
        Assert.Equal(expected.DatabaseName, actual.DatabaseName);
    }
}
