using System.Threading;
using System.Threading.Tasks;
using AppCliTools.LibDataInput;

namespace AppCliTools.CliParameters.FieldEditors;

public sealed class BoolNullableFieldEditor : FieldEditor<bool?>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public BoolNullableFieldEditor(string propertyName, bool? defaultValue = false,
        bool autoUsageOfDefaultValue = false) : base(propertyName, true, defaultValue, autoUsageOfDefaultValue)
    {
    }

    public override ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        SetValue(recordForUpdate, Inputer.InputBool(FieldName, GetValue(recordForUpdate, DefaultValue) ?? false));
        return ValueTask.CompletedTask;
    }

    public override string GetValueStatus(object? record)
    {
        bool? val = GetValueOrDefault(record);
        return val?.ToString() ?? string.Empty;
    }
}
