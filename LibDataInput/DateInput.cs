using System;
using System.Globalization;
using System.Text;
using LibDataInput.InputParsers;

namespace LibDataInput;

public sealed class DateInput : DataInput
{
    private readonly DateTime _defaultValue;
    private readonly string _fieldName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DateInput(string fieldName, DateTime defaultValue = default)
    {
        _fieldName = fieldName;
        _defaultValue = defaultValue;
    }

    //public string Text { get; private set; }
    public DateTime Value { get; private set; }

    public override bool DoInput()
    {
        var prompt =
            $"{_fieldName} {(_defaultValue == default ? string.Empty : $"[{_defaultValue.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}]")}: ";
        Console.Write(prompt);

        var promptLength = prompt.Length;
        StringBuilder sb = new();
        while (true)
        {
            //string currentInput = sb.ToString();

            var ch = Console.ReadKey(true);
            switch (ch.Key)
            {
                case ConsoleKey.Enter when sb.Length == 0:
                    Console.WriteLine(_defaultValue);
                    Value = _defaultValue;
                    return true;
                case ConsoleKey.Enter:
                {
                    if (sb.Length > 0)
                        if (DateTime.TryParse(sb.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None,
                                out var result))
                        {
                            Value = result.Date;
                            Console.WriteLine();
                            return true;
                        }

                    break;
                }
                case ConsoleKey.Escape:
                    throw new DataInputEscapeException("Escape");
                case ConsoleKey.Backspace:
                    if (sb.Length > 0)
                    {
                        sb.Remove(sb.Length - 1, 1);
                        ClearCurrentInput(promptLength);
                        Console.Write(sb.ToString());
                    }

                    break;
            }

            DateParser dateParser = new();
            var res = dateParser.TryAddNextChar(sb.ToString(), ch.KeyChar);
            if (res == null)
                continue;
            sb.Clear();
            sb.Append(res);
        }
    }
}