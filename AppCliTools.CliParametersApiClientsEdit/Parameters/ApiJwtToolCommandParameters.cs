using ParametersManagement.LibApiClientParameters;
using ToolsManagement.ApiClientsManagement;

namespace AppCliTools.CliParametersApiClientsEdit.Parameters;

public sealed class ApiJwtToolCommandParameters : ApiToolCommandParameters
{
    private ApiJwtToolCommandParameters(ApiClientSettingsDomain api, string folderForLocalStorage, string apiName) :
        base(api)
    {
        FolderForLocalStorage = folderForLocalStorage;
        ApiName = apiName;
    }

    public string FolderForLocalStorage { get; set; }
    public string ApiName { get; set; }

    public static ApiJwtToolCommandParameters CreateJwtParameters(IParametersWithApiClients parameters, string apiName,
        string folderForLocalStorage)
    {
        ApiClientSettingsDomain api = GetApiClientRequired(parameters, apiName);

        return new ApiJwtToolCommandParameters(api, folderForLocalStorage, apiName);
    }
}
