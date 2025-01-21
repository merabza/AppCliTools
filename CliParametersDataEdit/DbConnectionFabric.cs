using System;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using CliParametersDataEdit.Models;
using LibDatabaseParameters;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using SystemToolsShared;

namespace CliParametersDataEdit;

public static class DbConnectionFabric
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
                SqlServerConnectionParameters sqlSerPar = new()
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
                    throw new ArgumentOutOfRangeException(nameof(dataProvider), dataProvider, null);
#pragma warning disable CA1416
                var msaConBuilder = new OleDbConnectionStringBuilder(connectionString);
                var msAccessPar = new OleDbConnectionParameters
                {
                    DatabaseFilePath = msaConBuilder.DataSource,
                    Provider = msaConBuilder.Provider,
                    PersistSecurityInfo = msaConBuilder.PersistSecurityInfo
                };

                if (msAccessPar.PersistSecurityInfo && msaConBuilder.ContainsKey(JetOleDbDatabasePasswordKey))
                    msAccessPar.Password = msaConBuilder[JetOleDbDatabasePasswordKey].ToString();

                return msAccessPar;
#pragma warning restore CA1416
            default:
                throw new ArgumentOutOfRangeException(nameof(dataProvider), dataProvider, null);
        }
    }

    public static (EDatabaseProvider?, string?) GetDataProviderAndConnectionString(
        DatabaseParameters? databasesParameters, DatabaseServerConnections databaseServerConnections)
    {
        if (databasesParameters is null)
        {
            StShared.WriteErrorLine("databasesParameters does not specified", true);
            return (null, null);
        }

        if (databasesParameters.DbConnectionName is null)
        {
            StShared.WriteErrorLine("databasesParameters.DbConnectionName does not specified", true);
            return (null, null);
        }

        var databaseConnectionData =
            databaseServerConnections.GetDatabaseServerConnectionByKey(databasesParameters.DbConnectionName);

        if (databaseConnectionData is null)
        {
            StShared.WriteErrorLine("DatabaseConnectionData Data is not Created", true);
            return (null, null);
        }

        var connectionString = GetDbConnectionString(databasesParameters, databaseConnectionData);

        if (!string.IsNullOrWhiteSpace(connectionString))
            return (databaseConnectionData.DatabaseServerProvider, connectionString);

        StShared.WriteErrorLine("could not Created Connection String", true);
        return (null, null);
    }


    private static string? GetDbConnectionString(DatabaseParameters databasesParameters,
        DatabaseServerConnectionData databaseServerConnection)
    {
        var dbConnectionStringBuilder = GetDbConnectionStringBuilder(databasesParameters, databaseServerConnection);
        return dbConnectionStringBuilder?.ConnectionString;
    }

    private static DbConnectionStringBuilder? GetDbConnectionStringBuilder(DatabaseParameters databasesParameters,
        DatabaseServerConnectionData databaseServerConnection)
    {
        var dataProvider = databaseServerConnection.DatabaseServerProvider;
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
                    sqlConBuilder.UserID = databaseServerConnection.User;
                    sqlConBuilder.Password = databaseServerConnection.Password;
                }

                if (databasesParameters.DatabaseName != null)
                    sqlConBuilder.InitialCatalog = databasesParameters.DatabaseName;

                return sqlConBuilder;
            case EDatabaseProvider.SqLite:

                var (databaseFilePath, password) =
                    GetOneFileDbConnectionStringBuilderParameters(databasesParameters, databaseServerConnection);

                if (string.IsNullOrWhiteSpace(databaseFilePath))
                    return null;

                var sltConBuilder = new SqliteConnectionStringBuilder { DataSource = databaseFilePath };
                if (!string.IsNullOrWhiteSpace(password))
                    sltConBuilder.Password = password;
                return sltConBuilder;
            case EDatabaseProvider.OleDb:

                if (!SystemStat.IsWindows())
                {
                    StShared.WriteErrorLine("OleDb Data Provider is not valid for non windows operation systems", true);
                    return null;
                }

                var (oleDatabaseFilePath, olePassword) =
                    GetOneFileDbConnectionStringBuilderParameters(databasesParameters, databaseServerConnection);

                if (string.IsNullOrWhiteSpace(oleDatabaseFilePath))
                    return null;

#pragma warning disable CA1416

                var oleDbConBuilder = new OleDbConnectionStringBuilder
                {
                    DataSource = oleDatabaseFilePath, Provider = "Microsoft.ACE.OLEDB.12.0"
                };
                if (string.IsNullOrWhiteSpace(olePassword))
                    return oleDbConBuilder;

                oleDbConBuilder.PersistSecurityInfo = true;
                oleDbConBuilder.Add(JetOleDbDatabasePasswordKey, olePassword);
                return oleDbConBuilder;
#pragma warning restore CA1416

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static (string?, string?) GetOneFileDbConnectionStringBuilderParameters(
        DatabaseParameters databasesParameters, DatabaseServerConnectionData databaseServerConnection)
    {
        if (databasesParameters.DbServerFoldersSetName is null ||
            !databaseServerConnection.DatabaseFoldersSets.TryGetValue(databasesParameters.DbServerFoldersSetName,
                out var databaseFoldersSet))
        {
            StShared.WriteErrorLine(
                $"DatabaseFoldersSets does not contain key {databasesParameters.DbServerFoldersSetName}", true);
            return (null, null);
        }

        var dataPath = databaseFoldersSet.Data;

        if (string.IsNullOrWhiteSpace(dataPath))
        {
            StShared.WriteErrorLine("dataPath is not specified", true);
            return (null, null);
        }

        var databaseName = databasesParameters.DatabaseName;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            StShared.WriteErrorLine("databaseName is not specified", true);
            return (null, null);
        }

        var databaseFilePath = Path.Combine(dataPath, databaseName);
        var password = databaseServerConnection.Password;
        return (databaseFilePath, password);
    }

    public static string? GetDbConnectionString(DbConnectionParameters dbConnectionParameters)
    {
        var dbConnectionStringBuilder = GetDbConnectionStringBuilder(dbConnectionParameters);
        return dbConnectionStringBuilder?.ConnectionString;
    }

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
                sqlConBuilder.InitialCatalog = par.DatabaseName;

            return sqlConBuilder;
        }

        if (dbConnectionParameters is not SqLiteConnectionParameters slPar)
            return null;

        var sltConBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = slPar.DatabaseFilePath, Password = slPar.Password
        };
        return sltConBuilder;
    }
}