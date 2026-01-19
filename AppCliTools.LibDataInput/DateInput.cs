using System;
using System.Globalization;
using System.Text;
using AppCliTools.LibDataInput.InputParsers;

namespace AppCliTools.LibDataInput;

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
        string prompt =
            $"{_fieldName} {(_defaultValue == default ? string.Empty : $"[{_defaultValue.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}]")}: ";
        Console.Write(prompt);

        int promptLength = prompt.Length;
        var sb = new StringBuilder();
        while (true)
        {
            //string currentInput = sb.ToString();

            ConsoleKeyInfo ch = Console.ReadKey(true);
            switch (ch.Key)
            {
                case ConsoleKey.Enter when sb.Length == 0:
                    Console.WriteLine(_defaultValue);
                    Value = _defaultValue;
                    return true;
                case ConsoleKey.Enter:
                    {
                        if (sb.Length > 0 && DateTime.TryParse(sb.ToString(), CultureInfo.InvariantCulture,
                                DateTimeStyles.None, out DateTime result))
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

            var dateParser = new DateParser();
            string? res = dateParser.TryAddNextChar(sb.ToString(), ch.KeyChar);
            if (res == null)
            {
                continue;
            }

            sb.Clear();
            sb.Append(res);
        }
    }
}
