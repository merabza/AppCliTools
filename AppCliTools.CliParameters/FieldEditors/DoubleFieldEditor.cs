using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.LibDataInput;

namespace AppCliTools.CliParameters.FieldEditors;

// ReSharper disable once UnusedType.Global
public sealed class DoubleFieldEditor : FieldEditor<double>
{
    private readonly double _defaultValue;

    
    public DoubleFieldEditor(string propertyName, double defaultValue = 0, bool enterFieldDataOnCreate = false) : base(
        propertyName, enterFieldDataOnCreate)
    {
        _defaultValue = defaultValue;
    }

    public override ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        double currentValue = GetValue(recordForUpdate, _defaultValue);
        while (true)
        {
            string? input = Inputer.InputText(FieldName, currentValue.ToString(CultureInfo.InvariantCulture));
            if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                SetValue(recordForUpdate, value);
                return ValueTask.CompletedTask;
            }

            Console.WriteLine($"Invalid value for {FieldName}. Enter a number like 41.715137 (decimal separator is .)");
        }
    }

    public override string GetValueStatus(object? record)
    {
        return GetValue(record, _defaultValue).ToString(CultureInfo.InvariantCulture);
    }
}
