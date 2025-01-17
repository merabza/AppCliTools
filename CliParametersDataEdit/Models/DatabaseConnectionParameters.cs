//using DbTools;
//using LibDatabaseParameters;
//using LibParameters;

//namespace CliParametersDataEdit.Models;

//public sealed class DatabaseConnectionParameters : ParametersWithStatus
//{
//    public EDatabaseProvider DataProvider { get; set; }
//    public string? ConnectionString { get; set; }
//    public int CommandTimeOut { get; set; }

//    public override string GetStatus()
//    {
//        var dataProvider = DataProvider;
//        var status = $"Data Provider: {dataProvider}";
//        var dbConnectionParameters =
//            DbConnectionFabric.GetDbConnectionParameters(dataProvider, ConnectionString);
//        status +=
//            $", Connection: {(dbConnectionParameters == null ? "(invalid)" : dbConnectionParameters.GetStatus())}";
//        status += $", CommandTimeOut: {CommandTimeOut}";
//        return status;
//    }
//}

