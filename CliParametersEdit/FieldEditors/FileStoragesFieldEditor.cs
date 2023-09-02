using System.Collections.Generic;
using System.Linq;
using CliMenu;
using CliParameters.FieldEditors;
using CliParametersEdit.Cruders;
using LibFileParameters.Models;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParametersEdit.FieldEditors;

public sealed class FileStoragesFieldEditor : FieldEditor<Dictionary<string, FileStorageData>>
{
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;

    public FileStoragesFieldEditor(ILogger logger, string propertyName, IParametersManager parametersManager) : base(
        propertyName, false, null, true)
    {
        _logger = logger;
        _parametersManager = parametersManager;
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        FileStorageCruder fileStorageCruder = new(_logger, _parametersManager);
        var menuSet = fileStorageCruder.GetListMenu();
        return menuSet;
    }


    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record);

        if (val is null || val.Count <= 0)
            return "No Details";

        if (val.Count > 1)
            return $"{val.Count} Details";

        var kvp = val.Single();
        return $"{kvp.Key} - {kvp.Value.FileStoragePath} {kvp.Value.UserName ?? ""}";
    }
}