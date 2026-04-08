using System.Net.Http;
using AppCliTools.CliParameters;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersDataEdit.ParametersEditors;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParametersDataEdit.FieldEditors;

public sealed class DatabaseParametersFieldEditor : ParametersFieldEditor<DatabaseParameters, DatabaseParametersEditor>
{
    private readonly string _appName;
    private readonly IHttpClientFactory _httpClientFactory;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DatabaseParametersFieldEditor(string appName, ILogger logger, IHttpClientFactory httpClientFactory,
        string propertyName, IParametersManager parametersManager) : base(propertyName, logger, parametersManager)
    {
        _appName = appName;
        _httpClientFactory = httpClientFactory;
    }

    protected override DatabaseParametersEditor CreateEditor(object record, DatabaseParameters currentValue)
    {
        var serverDatabasesExchangeParametersManager =
            new SubParametersManager<DatabaseParameters>(currentValue, ParametersManager, this, record);

        return new DatabaseParametersEditor(_appName, Logger, _httpClientFactory,
            serverDatabasesExchangeParametersManager, ParametersManager, PropertyName);
    }
}
