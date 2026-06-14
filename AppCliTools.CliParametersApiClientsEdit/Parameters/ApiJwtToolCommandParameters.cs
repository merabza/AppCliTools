using AppCliTools.CliParametersApiClientsEdit.Models;
using ParametersManagement.LibApiClientParameters;

namespace AppCliTools.CliParametersApiClientsEdit.Parameters;

public sealed class ApiJwtToolCommandParameters : ApiToolCommandParameters
{
    // ReSharper disable once ConvertToPrimaryConstructor
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
