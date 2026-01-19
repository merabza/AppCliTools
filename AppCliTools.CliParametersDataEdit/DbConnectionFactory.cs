using System;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using CliParametersDataEdit.Models;
using DatabaseTools.DbTools.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using ParametersManagement.LibDatabaseParameters;
using SystemTools.SystemToolsShared;

namespace CliParametersDataEdit;

public static class DbConnectionFactory
{
    private const string JetOleDbDatabasePasswordKey = "Jet OLEDB:Database Password";

    public static DbConnectionParameters? GetDbConnectionParameters(EDatabaseProvider dataProvider,
        string? connectionString)
    {
        switch (dataProvider)
        {
            case EDatabaseProvider.None:
                return null;
            case EDatabaseProvider.SqlServer:
                var sqlConBuilder = new SqlConnectionStringBuilder(connectionString);
                var sqlSerPar = new SqlServerConnectionParameters
                {
                    ServerAddress = sqlConBuilder.DataSource,
                    WindowsNtIntegratedSecurity = sqlConBuilder.IntegratedSecurity,
                    ServerUser = sqlConBuilder.UserID,
                    ServerPass = sqlConBuilder.Password,
                    DatabaseName = sqlConBuilder.InitialCatalog,
                    ConnectionTimeOut = sqlConBuilder.ConnectTimeout,
                    Encrypt = sqlConBuilder.Encrypt,
                    TrustServerCertificate = sqlConBuilder.TrustServerCertificate
                };
                return sqlSerPar;
            case EDatabaseProvider.SqLite:
                var sltConBuilder = new SqliteConnectionStringBuilder(connectionString);
                var sqLitePar = new SqLiteConnectionParameters
                {
                    DatabaseFilePath = sltConBuilder.DataSource, Password = sltConBuilder.Password
                };
                return sqLitePar;
            case EDatabaseProvider.OleDb:
                if (!SystemStat.IsWindows())
                {
                    throw new ArgumentOutOfRangeException(nameof(dataProvider), dataProvider, null);
                }
#pragma warning disable CA1416
                var msaConBuilder = new OleDbConnectionStringBuilder(connectionString);
                var msAccessPar = new OleDbConnectionParameters
                {
                    DatabaseFilePath = msaConBuilder.DataSource,
                    Provider = msaConBuilder.Provider,
                    PersistSecurityInfo = msaConBuilder.PersistSecurityInfo
                };

                if (msAccessPar.PersistSecurityInfo && msaConBuilder.ContainsKey(JetOleDbDatabasePasswordKey))
                {
                    msAccessPar.Password = msaConBuilder[JetOleDbDatabasePasswordKey].ToString();
                }

                return msAccessPar;
#pragma warning restore CA1416
            default:
                throw new ArgumentOutOfRangeException(nameof(dataProvider), dataProvider, "Unsupported database provider.");
        }
    }

    public static (EDatabaseProvider?, string?, int) GetDataProviderConnectionStringCommandTimeOut(
        DatabaseParameters? databasesParameters, DatabaseServerConnections databaseServerConnections)
    {
        if (databasesParameters is null)
        {
            StShared.WriteErrorLine("databasesParameters does not specified", true);
            return (null, null, -1);
        }

        if (databasesParameters.DbConnectionName is null)
        {
            StShared.WriteErrorLine("databasesParameters.DbConnectionName does not specified", true);
            return (null, null, -1);
        }

        DatabaseServerConnectionData? databaseConnectionData =
            databaseServerConnections.GetDatabaseServerConnectionByKey(databasesParameters.DbConnectionName);

        if (databaseConnectionData is null)
        {
            StShared.WriteErrorLine("DatabaseConnectionData Data is not Created", true);
            return (null, null, -1);
        }

        string? connectionString = GetDbConnectionString(databasesParameters, databaseConnectionData);

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return (databaseConnectionData.DatabaseServerProvider, connectionString,
                databasesParameters.CommandTimeOut);
        }

        StShared.WriteErrorLine("could not Created Connection String", true);
        return (null, null, -1);
    }

    private static string? GetDbConnectionString(DatabaseParameters databasesParameters,
        DatabaseServerConnectionData databaseServerConnection)
    {
        DbConnectionStringBuilder? dbConnectionStringBuilder =
            GetDbConnectionStringBuilder(databasesParameters, databaseServerConnection);
        return dbConnectionStringBuilder?.ConnectionString;
    }

    private static DbConnectionStringBuilder? GetDbConnectionStringBuilder(DatabaseParameters databasesParameters,
        DatabaseServerConnectionData databaseServerConnection)
    {
        EDatabaseProvider dataProvider = databaseServerConnection.DatabaseServerProvider;
        switch (dataProvider)
        {
            case EDatabaseProvider.None:
                return null;
            case EDatabaseProvider.SqlServer:

                var sqlConBuilder = new SqlConnectionStringBuilder
                {
                    DataSource = databaseServerConnection.ServerAddress,
                    IntegratedSecurity = databaseServerConnection.WindowsNtIntegratedSecurity,
                    ConnectTimeout = databaseServerConnection.ConnectionTimeOut,
                    Encrypt = databaseServerConnection.Encrypt,
                    TrustServerCertificate = databaseServerConnection.TrustServerCertificate
                };
                if (!databaseServerConnection.WindowsNtIntegratedSecurity)
                {
                    sqlConBuilder.UserID = databaseServerConnection.ServerUser;
                    sqlConBuilder.Password = databaseServerConnection.ServerPass;
                }

                if (databasesParameters.DatabaseName != null)
                {
                    sqlConBuilder.InitialCatalog = databasesParameters.DatabaseName;
                }

                return sqlConBuilder;
            case EDatabaseProvider.SqLite:

                (string? databaseFilePath, string? password) =
                    GetOneFileDbConnectionStringBuilderParameters(databasesParameters, databaseServerConnection);

                if (string.IsNullOrWhiteSpace(databaseFilePath))
                {
                    return null;
                }

                var sltConBuilder = new SqliteConnectionStringBuilder { DataSource = databaseFilePath };
                if (!string.IsNullOrWhiteSpace(password))
                {
                    sltConBuilder.Password = password;
                }

                return sltConBuilder;
            case EDatabaseProvider.OleDb:

                if (!SystemStat.IsWindows())
                {
                    StShared.WriteErrorLine("OleDb Data Provider is not valid for non windows operation systems", true);
                    return null;
                }

                (string? oleDatabaseFilePath, string? olePassword) =
                    GetOneFileDbConnectionStringBuilderParameters(databasesParameters, databaseServerConnection);

                if (string.IsNullOrWhiteSpace(oleDatabaseFilePath))
                {
                    return null;
                }

#pragma warning disable CA1416

                var oleDbConBuilder = new OleDbConnectionStringBuilder
                {
                    DataSource = oleDatabaseFilePath, Provider = "Microsoft.ACE.OLEDB.12.0"
                };
                if (string.IsNullOrWhiteSpace(olePassword))
                {
                    return oleDbConBuilder;
                }

                oleDbConBuilder.PersistSecurityInfo = true;
                oleDbConBuilder.Add(JetOleDbDatabasePasswordKey, olePassword);
                return oleDbConBuilder;
#pragma warning restore CA1416

            case EDatabaseProvider.WebAgent:
            default:
                throw new ArgumentOutOfRangeException(nameof(databasesParameters), dataProvider, "Unsupported database provider.");
        }
    }

    private static (string?, string?) GetOneFileDbConnectionStringBuilderParameters(
        DatabaseParameters databasesParameters, DatabaseServerConnectionData databaseServerConnection)
    {
        if (databasesParameters.DbServerFoldersSetName is null ||
            databaseServerConnection.DatabaseFoldersSets is null ||
            !databaseServerConnection.DatabaseFoldersSets.TryGetValue(databasesParameters.DbServerFoldersSetName,
                out DatabaseFoldersSet? databaseFoldersSet))
        {
            StShared.WriteErrorLine(
                $"DatabaseFoldersSets does not contain key {databasesParameters.DbServerFoldersSetName}", true);
            return (null, null);
        }

        string? dataPath = databaseFoldersSet.Data;

        if (string.IsNullOrWhiteSpace(dataPath))
        {
            StShared.WriteErrorLine("dataPath is not specified", true);
            return (null, null);
        }

        string? databaseName = databasesParameters.DatabaseName;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            StShared.WriteErrorLine("databaseName is not specified", true);
            return (null, null);
        }

        string databaseFilePath = Path.Combine(dataPath, databaseName);
        string? password = databaseServerConnection.ServerPass;
        return (databaseFilePath, password);
    }

    //გავაუქმე, რადგან არ ვიცით სად გამოიყენება. თუ აღმოაჩინდება, რომ გამოიყენება, მაშინ დავაბრუნებ.
    //public static string? GetDbConnectionString(DbConnectionParameters dbConnectionParameters)
    //{
    //    var dbConnectionStringBuilder = GetDbConnectionStringBuilder(dbConnectionParameters);
    //    return dbConnectionStringBuilder?.ConnectionString;
    //}

    public static DbConnectionStringBuilder? GetDbConnectionStringBuilder(DbConnectionParameters dbConnectionParameters)
    {
        if (dbConnectionParameters is SqlServerConnectionParameters par)
        {
            var sqlConBuilder = new SqlConnectionStringBuilder
            {
                DataSource = par.ServerAddress,
                IntegratedSecurity = par.WindowsNtIntegratedSecurity,
                ConnectTimeout = par.ConnectionTimeOut,
                Encrypt = par.Encrypt,
                TrustServerCertificate = par.TrustServerCertificate
            };
            if (!par.WindowsNtIntegratedSecurity)
            {
                sqlConBuilder.UserID = par.ServerUser;
                sqlConBuilder.Password = par.ServerPass;
            }

            if (par.DatabaseName != null)
            {
                sqlConBuilder.InitialCatalog = par.DatabaseName;
            }

            return sqlConBuilder;
        }

        if (dbConnectionParameters is not SqLiteConnectionParameters slPar)
        {
            return null;
        }

        var sltConBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = slPar.DatabaseFilePath, Password = slPar.Password
        };
        return sltConBuilder;
    }
}
