﻿using CliParameters.FieldEditors;
using CliParametersExcludeSetsEdit.Cruders;
using LibParameters;

namespace CliParametersExcludeSetsEdit.FieldEditors;

public sealed class ExcludeSetNameFieldEditor : FieldEditor<string>
{
    private readonly IParametersManager _parametersManager;
    private readonly bool _useNone;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ExcludeSetNameFieldEditor(string propertyName, IParametersManager parametersManager, bool useNone = false,
        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate)
    {
        _parametersManager = parametersManager;
        _useNone = useNone;
    }

    public override void UpdateField(string? recordName, object recordForUpdate) //, object currentRecord
    {
        var excludeSetCruder = ExcludeSetCruder.Create(_parametersManager);
        SetValue(recordForUpdate,
            excludeSetCruder.GetNameWithPossibleNewName(FieldName, GetValue(recordForUpdate), null, _useNone));
    }
}