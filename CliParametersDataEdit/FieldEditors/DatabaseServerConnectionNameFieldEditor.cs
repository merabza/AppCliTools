﻿using CliParameters.FieldEditors;
using CliParametersDataEdit.Cruders;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParametersDataEdit.FieldEditors;

public sealed class DatabaseServerConnectionNameFieldEditor : FieldEditor<string>
{
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;
    private readonly bool _useNone;

    public DatabaseServerConnectionNameFieldEditor(ILogger logger, string propertyName,
        IParametersManager parametersManager, bool useNone = false) : base(propertyName)
    {
        _logger = logger;
        _parametersManager = parametersManager;
        _useNone = useNone;
    }

    public override void UpdateField(string? recordName, object recordForUpdate) //, object currentRecord
    {
        var currentDatabaseServerConnectionName = GetValue(recordForUpdate);

        //if (currentDatabaseServerConnectionName is null)
        //    throw new Exception("currentDatabaseServerConnectionName is null");

        DatabaseServerConnectionCruder databaseServerConnectionCruder = new(_parametersManager, _logger);

        SetValue(recordForUpdate,
            databaseServerConnectionCruder.GetNameWithPossibleNewName(FieldName,
                currentDatabaseServerConnectionName, null,
                _useNone));
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
            return "";

        DatabaseServerConnectionCruder databaseServerConnectionCruder = new(_parametersManager, _logger);

        var status = databaseServerConnectionCruder.GetStatusFor(val);
        return $"{val} {(string.IsNullOrWhiteSpace(status) ? "" : $"({status})")}";
    }
}