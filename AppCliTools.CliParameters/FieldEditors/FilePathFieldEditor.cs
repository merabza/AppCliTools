using System.Threading;
using System.Threading.Tasks;
using AppCliTools.LibMenuInput;

namespace AppCliTools.CliParameters.FieldEditors;

public /*open*/ class FilePathFieldEditor : FieldEditor<string>
{
    private readonly string? _defaultValue;

    // ReSharper disable once ConvertToPrimaryConstructor
    public FilePathFieldEditor(string propertyName, string? defaultValue = null, bool enterFieldDataOnCreate = false) :
        base(propertyName, enterFieldDataOnCreate)
    {
        _defaultValue = defaultValue;
    }

    public override ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        SetValue(recordForUpdate, MenuInputer.InputFilePath(FieldName, GetValue(recordForUpdate, _defaultValue)));
        return ValueTask.CompletedTask;
    }
}
