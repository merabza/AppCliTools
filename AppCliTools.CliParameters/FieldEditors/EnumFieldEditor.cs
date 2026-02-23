using System;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.LibMenuInput;

namespace AppCliTools.CliParameters.FieldEditors;

public /*open*/ class EnumFieldEditor<TEnum> : FieldEditor<TEnum> where TEnum : struct, Enum
{
    private readonly TEnum _defaultValue;

    // ReSharper disable once ConvertToPrimaryConstructor
    public EnumFieldEditor(string propertyName, TEnum defaultValue, bool enterFieldDataOnCreate = false) : base(
        propertyName, enterFieldDataOnCreate)
    {
        _defaultValue = defaultValue;
    }

    public override ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        TEnum current = GetValue(recordForUpdate, _defaultValue);
        SetValue(recordForUpdate, MenuInputer.InputFromEnumList(FieldName, current));
        return ValueTask.CompletedTask;
    }
}
