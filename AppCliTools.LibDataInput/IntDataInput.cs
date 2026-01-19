using System;
using System.Text;

namespace LibDataInput;

public sealed class IntDataInput : DataInput
{
    private readonly int _defaultValue;
    private readonly string _fieldName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public IntDataInput(string fieldName, int defaultValue = 0)
    {
        _fieldName = fieldName;
        _defaultValue = defaultValue;
    }

    public int Value { get; private set; }

    public override bool DoInput()
    {
        string prompt = $"{_fieldName} {(_defaultValue == 0 ? string.Empty : $"[{_defaultValue}]")}: ";
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
                        if (sb.Length > 0 && int.TryParse(sb.ToString(), out int result))
                        {
                            Value = result;
                            Console.WriteLine();
                            return true;
                        }

                        break;
                    }
                case ConsoleKey.Escape:
                    return false;
                case ConsoleKey.Backspace:
                    if (sb.Length > 0)
                    {
                        sb.Remove(sb.Length - 1, 1);
                        ClearCurrentInput(promptLength);
                        Console.Write(sb.ToString());
                    }

                    break;
            }

            string? key = ch.Key.IntValue();
            if (key == null)
            {
                continue;
            }

            sb.Append(key);
            Console.Write(key);
        }
    }
}
