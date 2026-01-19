using System.Net.Http;
using CliParameters.FieldEditors;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace CliParametersApiClientsEdit.FieldEditors;

public sealed class ApiClientNameFieldEditor : FieldEditor<string>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;
    private readonly bool _useNone;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ApiClientNameFieldEditor(string propertyName, ILogger logger, IHttpClientFactory httpClientFactory,
        IParametersManager parametersManager, bool useNone = false, bool enterFieldDataOnCreate = false) : base(
        propertyName, enterFieldDataOnCreate)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _useNone = useNone;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        var currentApiClientName = GetValue(recordForUpdate);

        var apiClientCruder = ApiClientCruder.Create(_logger, _httpClientFactory, _parametersManager);

        var newValue = apiClientCruder.GetNameWithPossibleNewName(FieldName, currentApiClientName, null, _useNone);

        SetValue(recordForUpdate, newValue);
    }

    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record);

        if (val == null)
        {
            return string.Empty;
        }

        var apiClientCruder = ApiClientCruder.Create(_logger, _httpClientFactory, _parametersManager);

        var status = apiClientCruder.GetStatusFor(val);
        return $"{val} {(string.IsNullOrWhiteSpace(status) ? string.Empty : $"({status})")}";
    }
}
