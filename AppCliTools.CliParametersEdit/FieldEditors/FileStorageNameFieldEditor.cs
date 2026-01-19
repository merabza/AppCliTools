using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersEdit.Cruders;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParametersEdit.FieldEditors;

public sealed class FileStorageNameFieldEditor : FieldEditor<string>
{
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public FileStorageNameFieldEditor(ILogger logger, string propertyName, IParametersManager parametersManager,
        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate)
    {
        _logger = logger;
        _parametersManager = parametersManager;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        string? currentFileStorageName = GetValue(recordForUpdate);

        var fileStorageCruder = FileStorageCruder.Create(_logger, _parametersManager);

        SetValue(recordForUpdate, fileStorageCruder.GetNameWithPossibleNewName(FieldName, currentFileStorageName));
    }

    public override string GetValueStatus(object? record)
    {
        string? val = GetValue(record);

        if (val == null)
        {
            return string.Empty;
        }

        var fileStorageCruder = FileStorageCruder.Create(_logger, _parametersManager);

        string? status = fileStorageCruder.GetStatusFor(val);
        return $"{val} {(string.IsNullOrWhiteSpace(status) ? string.Empty : $"({status})")}";
    }
}
