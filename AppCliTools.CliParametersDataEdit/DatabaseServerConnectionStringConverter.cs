using System;
using Microsoft.Data.SqlClient;
using MySqlConnector;
using ParametersManagement.LibDatabaseParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersDataEdit;

//ConnectionString-ის გაცვლა სერვერთან კავშირის პარამეტრებთან. მხოლოდ სერვერის ტიპის პროვაიდერებისთვის:
//SqLite-ისა და OleDb-ის ConnectionString ბაზის ფაილს შეიცავს, რომელიც სერვერთან კავშირის პარამეტრებში არ ინახება
public static class DatabaseServerConnectionStringConverter
{
    private const uint MariaDbDefaultPort = 3306;

    public static bool CanCreateConnectionString(EDatabaseProvider databaseProvider)
    {
        return databaseProvider is EDatabaseProvider.SqlServer or EDatabaseProvider.MariaDb;
    }

    //ბაზის სახელის გარეშე
    public static string? CreateConnectionString(DatabaseServerConnectionData databaseServerConnection)
    {
        return CanCreateConnectionString(databaseServerConnection.DatabaseServerProvider)
            ? DbConnectionFactory.GetServerConnectionString(databaseServerConnection)
            : null;
    }

    //პარამეტრები იცვლება მხოლოდ მაშინ, როცა ConnectionString სწორია
    public static ConnectionStringApplyResult ApplyConnectionString(
        DatabaseServerConnectionData databaseServerConnection, string connectionString)
    {
        try
        {
            return databaseServerConnection.DatabaseServerProvider switch
            {
                EDatabaseProvider.SqlServer => ApplySqlServerConnectionString(databaseServerConnection,
                    new SqlConnectionStringBuilder(connectionString)),
                EDatabaseProvider.MariaDb => ApplyMariaDbConnectionString(databaseServerConnection,
                    new MySqlConnectionStringBuilder(connectionString)),
                _ => new ConnectionStringApplyResult(
                    $"Connection string can not be used for {databaseServerConnection.DatabaseServerProvider} provider",
                    null)
            };
        }
        catch (ArgumentException e)
        {
            return new ConnectionStringApplyResult($"Connection string is not valid: {e.Message}", null);
        }
        catch (FormatException e)
        {
            return new ConnectionStringApplyResult($"Connection string is not valid: {e.Message}", null);
        }
    }

    private static ConnectionStringApplyResult ApplySqlServerConnectionString(
        DatabaseServerConnectionData databaseServerConnection, SqlConnectionStringBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(builder.DataSource))
        {
            return new ConnectionStringApplyResult("Connection string does not contain Data Source", null);
        }

        databaseServerConnection.ServerAddress = builder.DataSource;
        databaseServerConnection.WindowsNtIntegratedSecurity = builder.IntegratedSecurity;
        databaseServerConnection.ServerUser = builder.IntegratedSecurity ? null : EmptyToNull(builder.UserID);
        databaseServerConnection.ServerPass = builder.IntegratedSecurity ? null : EmptyToNull(builder.Password);
        databaseServerConnection.ConnectionTimeOut = builder.ConnectTimeout;
        databaseServerConnection.Encrypt = builder.Encrypt;
        databaseServerConnection.TrustServerCertificate = builder.TrustServerCertificate;
        return new ConnectionStringApplyResult(null, EmptyToNull(builder.InitialCatalog));
    }

    private static ConnectionStringApplyResult ApplyMariaDbConnectionString(
        DatabaseServerConnectionData databaseServerConnection, MySqlConnectionStringBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(builder.Server))
        {
            return new ConnectionStringApplyResult("Connection string does not contain Server", null);
        }

        //სერვერის მისამართის ფორმატია "host" ან "host:port" (MariaDbConnectionStringFactory)
        databaseServerConnection.ServerAddress =
            builder.Port == MariaDbDefaultPort ? builder.Server : $"{builder.Server}:{builder.Port}";
        databaseServerConnection.ServerUser = EmptyToNull(builder.UserID);
        databaseServerConnection.ServerPass = EmptyToNull(builder.Password);
        databaseServerConnection.ConnectionTimeOut = (int)builder.ConnectionTimeout;
        return new ConnectionStringApplyResult(null, EmptyToNull(builder.Database));
    }

    private static string? EmptyToNull(string? value)
    {
        return string.IsNullOrEmpty(value) ? null : value;
    }
}

//Error - შეცდომა (null - წარმატება); IgnoredDatabaseName - ConnectionString-ში მითითებული ბაზა, რომელიც არ ინახება
public sealed record ConnectionStringApplyResult(string? Error, string? IgnoredDatabaseName);
