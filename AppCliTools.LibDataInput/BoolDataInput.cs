using System;

namespace AppCliTools.LibDataInput;

public sealed class BoolDataInput : DataInput
{
    private readonly bool _defaultValue;
    private readonly string _fieldName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public BoolDataInput(string fieldName, bool defaultValue = true)
    {
        _fieldName = fieldName;
        _defaultValue = defaultValue;
    }

    public bool Value { get; private set; }

    public override bool DoInput()
    {
        Console.Write($"{_fieldName} (y/n)[{(_defaultValue ? "y" : "n")}]: ");

        while (true)
        {
            ConsoleKeyInfo ch = Console.ReadKey(true);
            string key = ch.Key switch
            {
                ConsoleKey.Enter => _defaultValue ? "y" : "n",
                _ => ch.Key.Value().ToUpperInvariant()
            };

            if (key is "y" or "n")
            {
                Value = key == "y";
                Console.Write(key);
                Console.WriteLine();
                return true;
            }

            if (ch.Key == ConsoleKey.Escape)
            {
                throw new DataInputEscapeException("Escape");
            }

            Console.WriteLine("Answer must be 'y' or 'n'");
            Console.Write($"{_fieldName} (y/n)[{(_defaultValue ? "y" : "n")}]: ");
        }
    }
}
