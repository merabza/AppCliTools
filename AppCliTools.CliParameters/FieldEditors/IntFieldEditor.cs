using LibDataInput;

namespace CliParameters.FieldEditors;

public sealed class IntFieldEditor : FieldEditor<int>
{
    private readonly int _defaultValue;

    // ReSharper disable once ConvertToPrimaryConstructor
    public IntFieldEditor(string propertyName, int defaultValue = 0, bool enterFieldDataOnCreate = false) : base(
        propertyName, enterFieldDataOnCreate)
    {
        _defaultValue = defaultValue;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        SetValue(recordForUpdate, Inputer.InputInt(FieldName, GetValue(recordForUpdate, _defaultValue)));
        //SetValue(recordForUpdate, Inputer.InputInt(FieldName, GetValue(recordForUpdate, true, _defaultValue)));//20220811
    }

    //public override void SetDefault(ItemData currentItem)
    //{
    //    SetValue(currentItem, _defaultValue);
    //}
}