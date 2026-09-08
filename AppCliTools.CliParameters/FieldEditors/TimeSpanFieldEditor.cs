using System;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.LibDataInput;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters.FieldEditors;

public sealed class TimeSpanFieldEditor : FieldEditor<TimeSpan>
{
    private readonly TimeSpan _defaultValue;

    public TimeSpanFieldEditor(string propertyName, TimeSpan defaultValue, bool enterFieldDataOnCreate = false) : base(
        propertyName, enterFieldDataOnCreate)
    {
        _defaultValue = defaultValue;
    }

    public override ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        SetValue(recordForUpdate, Inputer.InputTimeSpan(FieldName, GetValue(recordForUpdate, _defaultValue)));
        return ValueTask.CompletedTask;
    }

    public override void SetDefault(ItemData currentItem)
    {
        SetValue(currentItem, _defaultValue);
    }
}
