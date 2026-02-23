using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.LibMenuInput;

namespace AppCliTools.CliParametersEdit.FieldEditors;

public sealed class FileStoragePathFieldEditor : FieldEditor<string>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public FileStoragePathFieldEditor(string propertyName, bool enterFieldDataOnCreate = false) : base(propertyName,
        enterFieldDataOnCreate)
    {
    }

    public override ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        SetValue(recordForUpdate, MenuInputer.InputFolderPath(FieldName, GetValue(recordForUpdate)));
        return ValueTask.CompletedTask;
    }
}
