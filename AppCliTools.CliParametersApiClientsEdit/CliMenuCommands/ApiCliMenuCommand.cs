using AppCliTools.CliMenu;
using AppCliTools.CliParametersApiClientsEdit.Parameters;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersApiClientsEdit.CliMenuCommands;

public /*open*/ class ApiCliMenuCommand : CliMenuCommand
{
    protected readonly IParametersManager ParametersManager;
    protected readonly string TaskName;

    protected ApiCliMenuCommand(IParametersManager parametersManager, string taskName, string actionName,
        EMenuAction menuActionOnBodySuccess = EMenuAction.Reload,
        EMenuAction menuActionOnBodyFail = EMenuAction.Reload) : base(actionName, menuActionOnBodySuccess,
        menuActionOnBodyFail, null, true)
    {
        ParametersManager = parametersManager;
        TaskName = taskName;
    }

    protected ApiToolCommandParameters? CreateApiParameters(string apiName)
    {
        var apiToolCommandParameters =
            ApiToolCommandParameters.Create((IParametersWithApiClients)ParametersManager.Parameters, apiName);
        if (apiToolCommandParameters is not null)
        {
            return apiToolCommandParameters;
        }

        StShared.WriteErrorLine($"{nameof(ApiToolCommandParameters)} is null", true);
        return null;
    }

    protected ApiJwtToolCommandParameters CreateApiJwtParameters(string apiName, string folderForLocalStorage)
    {
        return ApiJwtToolCommandParameters.CreateJwtParameters((IParametersWithApiClients)ParametersManager.Parameters,
            apiName, folderForLocalStorage);
    }
}
