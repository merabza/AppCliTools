using System;
using System.Data.Common;
using System.Data.OleDb;
using CliParametersDataEdit.Models;
using DbTools;
using LibDatabaseParameters;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using SystemToolsShared;

namespace CliParametersDataEdit;

public static class DbConnectionFabric
{
    private const string JetOleDbDatabasePasswordKey = "Jet OLEDB:Database Password";

    public static DbConnectionParameters? GetDbConnectionParameters(EDataProvider dataProvider,
        string? connectionString)
    {
        switch (dataProvider)
        {
            case EDataProvider.None:
                return null;
            case EDataProvider.Sql:
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
            case EDataProvider.SqLite:
                var sltConBuilder = new SqliteConnectionStringBuilder(connectionString);
                var sqLitePar = new SqLiteConnectionParameters
                {
                    DatabaseFilePath = sltConBuilder.DataSource, Password = sltConBuilder.Password
                };
                return sqLitePar;
            case EDataProvider.OleDb:
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

    public static (EDataProvider?, string?) GetDataProviderAndConnectionString(DatabasesParameters? databasesParameters,
        string projectName, DatabaseServerConnections databaseServerConnections)
    {
        if (databasesParameters is null)
        {
            StShared.WriteErrorLine($"databasesParameters does not specified for Project {projectName}", true);
            return (null, null);
        }

        if (databasesParameters.DbConnectionName is null)
        {
            StShared.WriteErrorLine(
                $"databasesParameters.DbConnectionName does not specified for Project {projectName}", true);
            return (null, null);
        }

        var databaseConnectionData =
            databaseServerConnections.GetDatabaseServerConnectionByKey(databasesParameters.DbConnectionName);

        if (databaseConnectionData is null)
        {
            StShared.WriteErrorLine("DatabaseConnectionData Data is not Created", true);
            return (null, null);
        }

        var devConnectionString = GetDbConnectionString(databasesParameters, databaseConnectionData);

        if (!string.IsNullOrWhiteSpace(devConnectionString))
            return (databasesParameters.DataProvider, devConnectionString);

        StShared.WriteErrorLine($"could not Created Dev Connection String form Project with name {projectName}", true);
        return (null, null);
    }


    private static string? GetDbConnectionString(DatabasesParameters databasesParameters,
        DatabaseServerConnectionData databaseServerConnection)
    {
        var dbConnectionStringBuilder = GetDbConnectionStringBuilder(databasesParameters, databaseServerConnection);
        return dbConnectionStringBuilder?.ConnectionString;
    }

    private static DbConnectionStringBuilder? GetDbConnectionStringBuilder(DatabasesParameters databasesParameters,
        DatabaseServerConnectionData databaseServerConnection)
    {
        var dataProvider = databasesParameters.DataProvider;
        switch (dataProvider)
        {
            case EDataProvider.None:
                return null;
            case EDataProvider.Sql:

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
                    sqlConBuilder.InitialCatalog = databasesParameters.DatabaseName;

                return sqlConBuilder;
            case EDataProvider.SqLite:
                var sltConBuilder = new SqliteConnectionStringBuilder
                {
                    DataSource = databasesParameters.DatabaseFilePath
                };
                if (!string.IsNullOrWhiteSpace(databasesParameters.DatabasePassword))
                    sltConBuilder.Password = databasesParameters.DatabasePassword;
                return sltConBuilder;
            case EDataProvider.OleDb:

                if (!SystemStat.IsWindows())
                {
                    StShared.WriteErrorLine("OleDb Data Provider is not valid for non windows operation systems", true);
                    return null;
                }

                if (string.IsNullOrWhiteSpace(databasesParameters.DatabaseFilePath))
                {
                    StShared.WriteErrorLine("DatabaseFilePath is not specified for Ole Database", true);
                    return null;
                }

#pragma warning disable CA1416

                var oleDbConBuilder = new OleDbConnectionStringBuilder
                {
                    DataSource = databasesParameters.DatabaseFilePath, Provider = "Microsoft.ACE.OLEDB.12.0"
                };
                if (string.IsNullOrWhiteSpace(databasesParameters.DatabasePassword))
                    return oleDbConBuilder;

                oleDbConBuilder.PersistSecurityInfo = true;
                oleDbConBuilder.Add(JetOleDbDatabasePasswordKey, databasesParameters.DatabasePassword);
                return oleDbConBuilder;
#pragma warning restore CA1416

            default:
                throw new ArgumentOutOfRangeException();
        }
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