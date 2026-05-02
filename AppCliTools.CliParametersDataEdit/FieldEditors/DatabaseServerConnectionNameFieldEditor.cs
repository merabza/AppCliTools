using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersDataEdit.Cruders;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersDataEdit.FieldEditors;

public sealed class DatabaseServerConnectionNameFieldEditor : FieldEditor<string>
{
    private readonly IApplication _application;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;
    private readonly bool _useNone;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DatabaseServerConnectionNameFieldEditor(IApplication application, ILogger logger,
        IHttpClientFactory httpClientFactory, string propertyName, IParametersManager parametersManager,
        bool useNone = false, bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate)
    {
        _application = application;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _useNone = useNone;
    }

    public override async ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        string? currentDatabaseServerConnectionName = GetValue(recordForUpdate);

        var databaseServerConnectionCruder =
            DatabaseServerConnectionCruder.Create(_application, _logger, _httpClientFactory, _parametersManager);

        SetValue(recordForUpdate,
            await databaseServerConnectionCruder.GetNameWithPossibleNewName(FieldName,
                currentDatabaseServerConnectionName, null, _useNone, cancellationToken));
    }

    public override string GetValueStatus(object? record)
    {
        string? val = GetValue(record);

        if (val is null)
        {
            return string.Empty;
        }

        var databaseServerConnectionCruder =
            DatabaseServerConnectionCruder.Create(_application, _logger, _httpClientFactory, _parametersManager);

        string status = databaseServerConnectionCruder.GetStatusFor(val);
        return $"{val} {(string.IsNullOrWhiteSpace(status) ? string.Empty : $"({status})")}";
    }
}
