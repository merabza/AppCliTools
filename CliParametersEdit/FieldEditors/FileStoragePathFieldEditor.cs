using CliParameters.FieldEditors;
using LibMenuInput;

namespace CliParametersEdit.FieldEditors;

public sealed class FileStoragePathFieldEditor : FieldEditor<string>
{
    public FileStoragePathFieldEditor(string propertyName, bool enterFieldDataOnCreate = false) : base(propertyName,
        enterFieldDataOnCreate)
    {
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        SetValue(recordForUpdate, MenuInputer.InputFolderPath(FieldName, GetValue(recordForUpdate)));
    }
}