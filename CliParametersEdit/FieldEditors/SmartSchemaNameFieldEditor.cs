using CliParameters.FieldEditors;
using CliParametersEdit.Cruders;
using LibParameters;

namespace CliParametersEdit.FieldEditors;

public sealed class SmartSchemaNameFieldEditor : FieldEditor<string>
{
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SmartSchemaNameFieldEditor(string propertyName, IParametersManager parametersManager,
        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate)
    {
        _parametersManager = parametersManager;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate) //, object currentRecord
    {
        var smartSchemaCruder = SmartSchemaCruder.Create(_parametersManager);
        SetValue(recordForUpdate, smartSchemaCruder.GetNameWithPossibleNewName(FieldName, GetValue(recordForUpdate)));
    }
}