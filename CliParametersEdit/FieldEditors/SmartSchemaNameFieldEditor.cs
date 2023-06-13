using CliParameters.FieldEditors;
using CliParametersEdit.Cruders;
using LibParameters;

namespace CliParametersEdit.FieldEditors;

public sealed class SmartSchemaNameFieldEditor : FieldEditor<string>
{
    private readonly IParametersManager _parametersManager;

    public SmartSchemaNameFieldEditor(string propertyName, IParametersManager parametersManager) : base(
        propertyName)
    {
        _parametersManager = parametersManager;
    }

    public override void UpdateField(string? recordName, object recordForUpdate) //, object currentRecord
    {
        SmartSchemaCruder smartSchemaCruder = new(_parametersManager);
        SetValue(recordForUpdate,
            smartSchemaCruder.GetNameWithPossibleNewName(FieldName, GetValue(recordForUpdate)));
    }
}