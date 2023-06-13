using System;
using System.Text;

namespace LibDataInput;

public sealed class IntDataInput : DataInput
{
    private readonly int _defaultValue;
    private readonly string _fieldName;

    public IntDataInput(string fieldName, int defaultValue = default)
    {
        _fieldName = fieldName;
        _defaultValue = defaultValue;
    }

    public int Value { get; private set; }


    public override bool DoInput()
    {
        var prompt = $"{_fieldName} {(_defaultValue == default ? "" : $"[{_defaultValue}]")}: ";
        Console.Write(prompt);

        var promptLength = prompt.Length;
        StringBuilder sb = new();
        while (true)
        {
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
                        if (int.TryParse(sb.ToString(), out var result))
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

            var key = ch.Key.IntValue();
            if (key == null)
                continue;

            sb.Append(key);
            Console.Write(key);
        }
    }
}