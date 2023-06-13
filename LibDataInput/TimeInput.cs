using System;
using System.Globalization;

namespace LibDataInput;

public sealed class TimeInput : DataInput
{
    private readonly DateTime _defaultValue;
    private readonly string _fieldName;

    public TimeInput(string fieldName, DateTime defaultValue = default)
    {
        _fieldName = fieldName;
        _defaultValue = defaultValue;
    }

    private string? Text { get; set; }
    public DateTime Value { get; private set; }


    public override bool DoInput()
    {
        var prompt = $"Enter {_fieldName} [{_defaultValue.ToString("T", CultureInfo.InvariantCulture)}]: ";

        while (true)
        {
            Console.Write(prompt);
            Text = Console.ReadLine();

            if (Text == "")
            {
                Value = _defaultValue;
                break;
            }

            if (!DateTime.TryParseExact(Text, "T", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var userDateTime))
            {
                if (!Inputer.InputBool("Entered date time is not valid try again?", false, false))
                    return false;
                continue;
            }

            Value = userDateTime;
            break;
        }

        Text = Value.ToString(CultureInfo.InvariantCulture);

        Console.WriteLine($"{_fieldName} is: {Text}");

        return true;
    }
}