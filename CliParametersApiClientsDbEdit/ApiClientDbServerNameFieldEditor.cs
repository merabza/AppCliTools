//using CliMenu;
//using CliParameters;
//using CliParameters.FieldEditors;
//using CliParameters.MenuCommands;
//using CliParametersApiClientsEdit;
//using CliParametersApiClientsEdit.Models;
//using DatabaseApiClients;
//using DatabaseManagementClients;
//using Installer.AgentClients;
//using LibDataInput;
//using Microsoft.Extensions.Logging;

//namespace CliParametersApiClientsDbEdit;

//public sealed class ApiClientDbServerNameFieldEditor : FieldEditor<string>
//{
//    private readonly IApiClientsFabric _apiClientsFabric;
//    private readonly string _databaseApiClientNameFieldName;
//    private readonly ILogger _logger;
//    private readonly IParametersManager _parametersManager;

//    public ApiClientDbServerNameFieldEditor(ILogger logger, string propertyName,
//        IParametersManager parametersManager, IApiClientsFabric apiClientsFabric,
//        string databaseApiClientNameFieldName) : base(propertyName)
//    {
//        _logger = logger;
//        _parametersManager = parametersManager;
//        _apiClientsFabric = apiClientsFabric;
//        _databaseApiClientNameFieldName = databaseApiClientNameFieldName;
//    }

//    public override void UpdateField(string? recordKey, object recordForUpdate)
//    {
//        var currentDatabaseName = GetValue(recordForUpdate);

//        var databaseApiClientName = GetValue<string>(recordForUpdate, _databaseApiClientNameFieldName);

//        ApiClientCruder apiClientCruder = new(_parametersManager, _logger, _apiClientsFabric);

//        var apiClientSettings = string.IsNullOrWhiteSpace(databaseApiClientName)
//            ? null
//            : (ApiClientSettings?)apiClientCruder.GetItemByName(databaseApiClientName);

//        DatabaseManagementClient? databaseClient = null;

//        if (apiClientSettings != null)
//            databaseClient = DatabaseApiClient.Create(_logger, true, apiClientSettings);

//        var serverNames = databaseClient?.GetDatabaseServerNames();

//        if (serverNames == null || serverNames.Count == 0)
//            throw new DataInputException("Can not get server Names");

//        CliMenuSet serverNamesMenuSet = new();
//        foreach (var listItem in serverNames)
//            serverNamesMenuSet.AddMenuItem(new MenuCommandWithStatus(listItem), listItem);

//        var index = Inputer.InputIdFromMenuList(FieldName, serverNamesMenuSet, currentDatabaseName);

//        if (index < 0 || index >= serverNames.Count)
//            throw new DataInputException("Selected invalid ID. ");

//        SetValue(recordForUpdate, serverNames[index]);
//    }

//    public override string GetValueStatus(object? record)
//    {
//        return GetValue(record) ?? "";
//    }
//}

