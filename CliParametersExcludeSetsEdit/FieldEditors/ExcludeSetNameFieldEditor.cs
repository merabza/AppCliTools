using CliParameters.FieldEditors;
using CliParametersExcludeSetsEdit.Cruders;
using LibParameters;

namespace CliParametersExcludeSetsEdit.FieldEditors;

public sealed class ExcludeSetNameFieldEditor : FieldEditor<string>
{
    private readonly IParametersManager _parametersManager;
    private readonly bool _useNone;

    public ExcludeSetNameFieldEditor(string propertyName, IParametersManager parametersManager, bool useNone = false,
        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate)
    {
        _parametersManager = parametersManager;
        _useNone = useNone;
    }

    public override void UpdateField(string? recordName, object recordForUpdate) //, object currentRecord
    {
        ExcludeSetCruder excludeSetCruder = new(_parametersManager);
        SetValue(recordForUpdate,
            excludeSetCruder.GetNameWithPossibleNewName(FieldName, GetValue(recordForUpdate), null, _useNone));
    }
}