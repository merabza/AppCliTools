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
    public void ApplyConnectionString_SqlServerGeneratedString_RestoresAllParameters()
    {
        DatabaseServerConnectionData source = SqlServerData();
        string connectionString = DatabaseServerConnectionStringConverter.CreateConnectionString(source)!;
        var target = new DatabaseServerConnectionData { DatabaseServerProvider = EDatabaseProvider.SqlServer };

        ConnectionStringApplyResult result =
            DatabaseServerConnectionStringConverter.ApplyConnectionString(target, connectionString);

        Assert.Equal(new ConnectionStringApplyResult(null, null), result);
        AssertServerParametersEqual(source, target);
    }

    [Fact]
    public void ApplyConnectionString_SqlServerIntegratedSecurity_ClearsUserAndPasswordAndReportsDatabase()
    {
        DatabaseServerConnectionData data = SqlServerData();

        ConnectionStringApplyResult result = DatabaseServerConnectionStringConverter.ApplyConnectionString(data,
            "Server=.\\SQL2022;Database=AppDb;Integrated Security=SSPI;User ID=Ignored;Password=Ignored;Encrypt=False");

        Assert.Equal(new ConnectionStringApplyResult(null, "AppDb"), result);
        Assert.Equal(".\\SQL2022", data.ServerAddress);
        Assert.True(data.WindowsNtIntegratedSecurity);
        Assert.Null(data.ServerUser);
        Assert.Null(data.ServerPass);
        Assert.False(data.Encrypt);
        Assert.False(data.TrustServerCertificate);
        //the SqlClient default
        Assert.Equal(15, data.ConnectionTimeOut);
    }

    [Fact]
    public void ApplyConnectionString_MariaDbGeneratedString_RestoresAllParameters()
    {
        DatabaseServerConnectionData source = MariaDbData("maria.local:3307");
        string connectionString = DatabaseServerConnectionStringConverter.CreateConnectionString(source)!;
        var target = new DatabaseServerConnectionData { DatabaseServerProvider = EDatabaseProvider.MariaDb };

        ConnectionStringApplyResult result =
            DatabaseServerConnectionStringConverter.ApplyConnectionString(target, connectionString);

        Assert.Equal(new ConnectionStringApplyResult(null, null), result);
        Assert.Contains("Port=3307", connectionString, System.StringComparison.Ordinal);
        AssertServerParametersEqual(source, target);
    }

    [Fact]
    public void ApplyConnectionString_MariaDbDefaultPort_StoresAddressWithoutPortAndReportsDatabase()
    {
        DatabaseServerConnectionData data = MariaDbData("old.local:3310");

        ConnectionStringApplyResult result = DatabaseServerConnectionStringConverter.ApplyConnectionString(data,
            "Server=maria.local;Port=3306;Database=ErrataDB;User ID=root;Password=TestPass");

        Assert.Equal(new ConnectionStringApplyResult(null, "ErrataDB"), result);
        Assert.Equal("maria.local", data.ServerAddress);
        Assert.Equal("root", data.ServerUser);
        Assert.Equal("TestPass", data.ServerPass);
        //the MySqlConnector default
        Assert.Equal(15, data.ConnectionTimeOut);
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
            provider == EDatabaseProvider.SqlServer ? SqlServerData() : MariaDbData("maria.local:3307");
        DatabaseServerConnectionData original =
            provider == EDatabaseProvider.SqlServer ? SqlServerData() : MariaDbData("maria.local:3307");

        ConnectionStringApplyResult result =
            DatabaseServerConnectionStringConverter.ApplyConnectionString(data, connectionString);

        Assert.StartsWith(expectedErrorStart, result.Error, System.StringComparison.Ordinal);
        Assert.Null(result.IgnoredDatabaseName);
        AssertServerParametersEqual(original, data);
    }

    //the message of the builder follows the prefix, so the reason is not lost
    [Fact]
    public void ApplyConnectionString_WhenInvalid_KeepsTheReasonOfTheBuilder()
    {
        ConnectionStringApplyResult result =
            DatabaseServerConnectionStringConverter.ApplyConnectionString(SqlServerData(), "Unknown Key=1");

        Assert.True(result.Error!.Length > InvalidPrefix.Length);
    }

    [Fact]
    public void ApplyConnectionString_WhenProviderCanNotCreate_ReturnsError()
    {
        var data = new DatabaseServerConnectionData { DatabaseServerProvider = EDatabaseProvider.SqLite };

        ConnectionStringApplyResult result =
            DatabaseServerConnectionStringConverter.ApplyConnectionString(data, "Data Source=a.db");

        Assert.Equal("Connection string can not be used for SqLite provider", result.Error);
        Assert.Null(data.ServerAddress);
    }

    internal static DatabaseServerConnectionData SqlServerData()
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
            TrustServerCertificate = true
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
    }
}
