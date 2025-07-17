using System.Net.Http;
using CliParameters.FieldEditors;
using CliParametersDataEdit.Cruders;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParametersDataEdit.FieldEditors;

public sealed class DatabaseServerConnectionNameFieldEditor : FieldEditor<string>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;
    private readonly bool _useNone;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DatabaseServerConnectionNameFieldEditor(ILogger logger, IHttpClientFactory httpClientFactory,
        string propertyName, IParametersManager parametersManager, bool useNone = false,
        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _useNone = useNone;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate) //, object currentRecord
    {
        var currentDatabaseServerConnectionName = GetValue(recordForUpdate);

        //if (currentDatabaseServerConnectionName is null)
        //    throw new Exception("currentDatabaseServerConnectionName is null");

        var databaseServerConnectionCruder =
            DatabaseServerConnectionCruder.Create(_logger, _httpClientFactory, _parametersManager);

        SetValue(recordForUpdate,
            databaseServerConnectionCruder.GetNameWithPossibleNewName(FieldName, currentDatabaseServerConnectionName,
                null, _useNone));
    }

    public override string GetValueStatus(object? record)
    {
        //string? val = null;
        //try
        //{
        //    val = GetValue(record);
        //}
        //catch
        //{
        //    // ignored
        //}

        var val = GetValue(record);

        if (val is null)
            return string.Empty;

        var databaseServerConnectionCruder =
            DatabaseServerConnectionCruder.Create(_logger, _httpClientFactory, _parametersManager);

        var status = databaseServerConnectionCruder.GetStatusFor(val);
        return $"{val} {(string.IsNullOrWhiteSpace(status) ? string.Empty : $"({status})")}";
    }
}