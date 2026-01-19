//using LibDataInput;
//using LibParameters;

//namespace CliParameters.FieldEditors;

//public sealed class BoolNullableFieldEditor : FieldEditor<bool?>
//{
//    private readonly bool? _defaultValue;

//    // ReSharper disable once ConvertToPrimaryConstructor
//    public BoolNullableFieldEditor(string propertyName, bool? defaultValue, bool autoUsageOfDefaultValue = false) : base(
//        propertyName, true, autoUsageOfDefaultValue)
//    {
//        _defaultValue = defaultValue;
//    }

//    public override void UpdateField(string? recordKey, object recordForUpdate) //, object currentRecord
//    {
//        SetValue(recordForUpdate, Inputer.InputBool(FieldName, GetValue(recordForUpdate, _defaultValue) ?? false));
//    }

//    public override string GetValueStatus(object? record)
//    {
//        bool? val;
//        if (AutoUsageOfDefaultValue && _defaultValue is not null)
//            val = GetValue(record, _defaultValue);
//        else
//            val = GetValue(record);
//        return val is null ? string.Empty : val.ToString() ?? string.Empty;
//    }

//    //public override void SetDefault(ItemData currentItem)
//    //{
//    //    SetValue(currentItem, _defaultValue);
//    //}
//}


