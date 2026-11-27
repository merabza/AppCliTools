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

    //კავშირის ბაზის სახელით, თუ ის მითითებულია, და ბაზის სახელის გარეშე, თუ არა
    public static string? CreateConnectionString(DatabaseServerConnectionData databaseServerConnection)
    {
        return CanCreateConnectionString(databaseServerConnection.DatabaseServerProvider)
            ? DbConnectionFactory.GetServerConnectionString(databaseServerConnection)
            : null;
    }

    //აბრუნებს შეცდომას (null - წარმატება). პარამეტრები იცვლება მხოლოდ მაშინ, როცა ConnectionString სწორია. ბაზის
    //სახელიც ConnectionString-იდან აიღება: ბაზის გარეშე ConnectionString კავშირის ბაზის სახელს წაშლის
    public static string? ApplyConnectionString(DatabaseServerConnectionData databaseServerConnection,
        string connectionString)
    {
        try
        {
            return databaseServerConnection.DatabaseServerProvider switch
            {
                EDatabaseProvider.SqlServer => ApplySqlServerConnectionString(databaseServerConnection,
                    new SqlConnectionStringBuilder(connectionString)),
                EDatabaseProvider.MariaDb => ApplyMariaDbConnectionString(databaseServerConnection,
                    new MySqlConnectionStringBuilder(connectionString)),
                _ => $"Connection string can not be used for {databaseServerConnection.DatabaseServerProvider} provider"
            };
        }
        catch (ArgumentException e)
        {
            return $"Connection string is not valid: {e.Message}";
        }
        catch (FormatException e)
        {
            return $"Connection string is not valid: {e.Message}";
        }
    }

    private static string? ApplySqlServerConnectionString(DatabaseServerConnectionData databaseServerConnection,
        SqlConnectionStringBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(builder.DataSource))
        {
            return "Connection string does not contain Data Source";
        }

        databaseServerConnection.ServerAddress = builder.DataSource;
        databaseServerConnection.WindowsNtIntegratedSecurity = builder.IntegratedSecurity;
        databaseServerConnection.ServerUser = builder.IntegratedSecurity ? null : EmptyToNull(builder.UserID);
        databaseServerConnection.ServerPass = builder.IntegratedSecurity ? null : EmptyToNull(builder.Password);
        databaseServerConnection.ConnectionTimeOut = builder.ConnectTimeout;
        databaseServerConnection.Encrypt = builder.Encrypt;
        databaseServerConnection.TrustServerCertificate = builder.TrustServerCertificate;
        databaseServerConnection.DatabaseName = EmptyToNull(builder.InitialCatalog);
        return null;
    }

    private static string? ApplyMariaDbConnectionString(DatabaseServerConnectionData databaseServerConnection,
        MySqlConnectionStringBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(builder.Server))
        {
            return "Connection string does not contain Server";
        }

        //სერვერის მისამართის ფორმატია "host" ან "host:port" (MariaDbConnectionStringFactory)
        databaseServerConnection.ServerAddress =
            builder.Port == MariaDbDefaultPort ? builder.Server : $"{builder.Server}:{builder.Port}";
        databaseServerConnection.ServerUser = EmptyToNull(builder.UserID);
        databaseServerConnection.ServerPass = EmptyToNull(builder.Password);
        databaseServerConnection.ConnectionTimeOut = (int)builder.ConnectionTimeout;
        databaseServerConnection.DatabaseName = EmptyToNull(builder.Database);
        return null;
    }

    private static string? EmptyToNull(string? value)
    {
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
