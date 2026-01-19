//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using CliMenu;
//using CliParameters.FieldEditors;
//using CliParametersDataEdit.Cruders;
//using LibDatabaseParameters;
//using LibParameters;
//using Microsoft.Extensions.Logging;

//namespace CliParametersDataEdit.FieldEditors;

//public sealed class DatabaseServerConnectionsFieldEditor : FieldEditor<Dictionary<string, DatabaseServerConnectionData>>
//{
//    private readonly IHttpClientFactory _httpClientFactory;
//    private readonly ILogger _logger;
//    private readonly IParametersManager _parametersManager;

//    // ReSharper disable once ConvertToPrimaryConstructor
//    public DatabaseServerConnectionsFieldEditor(ILogger logger, IHttpClientFactory httpClientFactory,
//        IParametersManager parametersManager, string propertyName, bool enterFieldDataOnCreate = false) : base(
//        propertyName, enterFieldDataOnCreate, null, false, null, true)
//    {
//        _parametersManager = parametersManager;
//        _logger = logger;
//        _httpClientFactory = httpClientFactory;
//    }

//    public override CliMenuSet GetSubMenu(object record)
//    {
//        DatabaseServerConnectionCruder databaseServerConnectionsCruder =
//            new(_logger, _httpClientFactory, _parametersManager);
//        var menuSet = databaseServerConnectionsCruder.GetListMenu();

//        return menuSet;
//    }

//    public override string GetValueStatus(object? record)
//    {
//        var val = GetValue(record);

//        if (val is null || val.Count <= 0)
//            return "No Details";

//        if (val.Count > 1)
//            return $"{val.Count} Details";

//        var kvp = val.Single();
//        return $"{kvp.Key} - {kvp.Value.DatabaseServerProvider} {kvp.Value.ServerAddress}";
//    }
//}


