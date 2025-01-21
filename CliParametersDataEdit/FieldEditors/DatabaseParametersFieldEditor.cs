using System.Net.Http;
using CliParameters;
using CliParameters.FieldEditors;
using CliParametersDataEdit.ParametersEditors;
using LibDatabaseParameters;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParametersDataEdit.FieldEditors;

public sealed class DatabaseParametersFieldEditor : ParametersFieldEditor<DatabaseParameters, DatabaseParametersEditor>
{
    private readonly IHttpClientFactory _httpClientFactory;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DatabaseParametersFieldEditor(ILogger logger, IHttpClientFactory httpClientFactory, string propertyName,
        IParametersManager parametersManager) : base(logger, propertyName, parametersManager)
    {
        _httpClientFactory = httpClientFactory;
    }

    protected override DatabaseParametersEditor CreateEditor(object record, DatabaseParameters currentValue)
    {
        var serverDatabasesExchangeParametersManager =
            new SubParametersManager<DatabaseParameters>(currentValue, ParametersManager, this, record);

        return new DatabaseParametersEditor(Logger, _httpClientFactory, serverDatabasesExchangeParametersManager,
            ParametersManager);
    }
}