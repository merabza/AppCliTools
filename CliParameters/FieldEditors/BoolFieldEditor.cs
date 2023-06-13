using LibDataInput;
using LibParameters;

namespace CliParameters.FieldEditors;

public sealed class BoolFieldEditor : FieldEditor<bool>
{
    private readonly bool _defaultValue;

    public BoolFieldEditor(string propertyName, bool defaultValue) : base(propertyName)
    {
        _defaultValue = defaultValue;
    }

    public override void UpdateField(string? recordName, object recordForUpdate) //, object currentRecord
    {
        SetValue(recordForUpdate, Inputer.InputBool(FieldName, GetValue(recordForUpdate, _defaultValue)));
    }

    public override void SetDefault(ItemData currentItem)
    {
        SetValue(currentItem, _defaultValue);
    }
}