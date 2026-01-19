using AppCliTools.LibMenuInput;

namespace AppCliTools.CliParameters.FieldEditors;

public sealed class FolderPathFieldEditor : FieldEditor<string>
{
    private readonly string? _defaultValue;

    // ReSharper disable once ConvertToPrimaryConstructor
    public FolderPathFieldEditor(string propertyName, string? defaultValue = null, bool enterFieldDataOnCreate = false)
        : base(propertyName, enterFieldDataOnCreate)
    {
        _defaultValue = defaultValue;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        SetValue(recordForUpdate, MenuInputer.InputFolderPath(FieldName, GetValue(recordForUpdate, _defaultValue)));
    }

    //public override void SetDefault(ItemData currentItem)
    //{
    //    SetValue(currentItem, _defaultValue);
    //}
}
