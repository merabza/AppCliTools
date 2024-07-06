using CliParameters.FieldEditors;
using CliParametersEdit.Cruders;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParametersEdit.FieldEditors;

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
        var currentFileStorageName = GetValue(recordForUpdate);

        FileStorageCruder fileStorageCruder = new(_logger, _parametersManager);

        SetValue(recordForUpdate, fileStorageCruder.GetNameWithPossibleNewName(FieldName, currentFileStorageName));
    }

    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record);

        if (val == null)
            return string.Empty;

        FileStorageCruder fileStorageCruder = new(_logger, _parametersManager);

        var status = fileStorageCruder.GetStatusFor(val);
        return $"{val} {(string.IsNullOrWhiteSpace(status) ? string.Empty : $"({status})")}";
    }
}