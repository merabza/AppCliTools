using CliParameters.FieldEditors;
using LibMenuInput;

namespace CliParametersEdit.FieldEditors;

public sealed class FileStoragePathFieldEditor : FieldEditor<string>
{
    public FileStoragePathFieldEditor(string propertyName) : base(propertyName)
    {
    }

    public override void UpdateField(string? recordName, object recordForUpdate)
    {
        SetValue(recordForUpdate, MenuInputer.InputFolderPath(FieldName, GetValue(recordForUpdate)));
    }
}