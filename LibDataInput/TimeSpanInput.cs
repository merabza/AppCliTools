using System;
using System.Globalization;
using System.Text;
using LibDataInput.InputParsers;

namespace LibDataInput;

public sealed class TimeSpanInput : DataInput
{
    private readonly TimeSpan _defaultValue;
    private readonly string _fieldName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TimeSpanInput(string fieldName, TimeSpan defaultValue = default)
    {
        _fieldName = fieldName;
        _defaultValue = defaultValue;
    }

    public TimeSpan Value { get; private set; }

    public override bool DoInput()
    {
        string prompt = $"{_fieldName} {(_defaultValue == TimeSpan.Zero ? string.Empty : $"[{_defaultValue}]")}: ";
        Console.Write(prompt);

        int promptLength = prompt.Length;
        var sb = new StringBuilder();
        while (true)
        {
            ConsoleKeyInfo ch = Console.ReadKey(true);
            switch (ch.Key)
            {
                case ConsoleKey.Enter when sb.Length == 0:
                    Console.WriteLine(_defaultValue);
                    Value = _defaultValue;
                    return true;
                case ConsoleKey.Enter:
                    {
                        if (sb.Length > 0 && TimeSpan.TryParse(sb.ToString(), CultureInfo.InvariantCulture,
                                out TimeSpan result))
                        {
                            Value = result;
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

            var timeDelimiterParser = new TimeDelimiterParser();
            string? res = timeDelimiterParser.TryAddNextChar(sb.ToString(), ch.KeyChar);
            if (res == null)
            {
                continue;
            }

            sb.Clear();
            sb.Append(res);
        }
    }
}
