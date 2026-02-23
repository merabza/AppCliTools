using System.Threading;
using System.Threading.Tasks;
using AppCliTools.LibDataInput;

namespace AppCliTools.CliParameters.FieldEditors;

public sealed class BoolFieldEditor : FieldEditor<bool>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public BoolFieldEditor(string propertyName, bool defaultValue = false, bool autoUsageOfDefaultValue = false) : base(
        propertyName, true, defaultValue, autoUsageOfDefaultValue)
    {
    }

    public override ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        SetValue(recordForUpdate, Inputer.InputBool(FieldName, GetValue(recordForUpdate, DefaultValue)));
        return ValueTask.CompletedTask;
    }
}
