using System;
using System.Data.Common;
using System.Data.SqlClient;
using CliParametersDataEdit.Models;
using DbTools;
using Microsoft.Data.Sqlite;

namespace CliParametersDataEdit;

public static class DbConnectionFabric
{
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
                    DatabaseFilePath = sltConBuilder.DataSource,
                    Password = sltConBuilder.Password
                };
                return sqLitePar;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataProvider), dataProvider, null);
        }
    }

    public static string? GetDbConnectionString(DbConnectionParameters dbConnectionParameters)
    {
        var dbConnectionStringBuilder = GetDbConnectionStringBuilder(dbConnectionParameters);
        return dbConnectionStringBuilder?.ConnectionString;
    }

    public static DbConnectionStringBuilder? GetDbConnectionStringBuilder(
        DbConnectionParameters dbConnectionParameters)
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