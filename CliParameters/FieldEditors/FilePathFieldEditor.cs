using LibMenuInput;
using LibParameters;

namespace CliParameters.FieldEditors;

public /*open*/ class FilePathFieldEditor : FieldEditor<string>
{
    private readonly string? _defaultValue;

    // ReSharper disable once ConvertToPrimaryConstructor
    public FilePathFieldEditor(string propertyName, string? defaultValue = default, bool enterFieldDataOnCreate = false)
        : base(propertyName, enterFieldDataOnCreate)
    {
        _defaultValue = defaultValue;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        SetValue(recordForUpdate, MenuInputer.InputFilePath(FieldName, GetValue(recordForUpdate, _defaultValue)));
    }

    public override void SetDefault(ItemData currentItem)
    {
        SetValue(currentItem, _defaultValue);
    }
}