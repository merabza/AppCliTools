using System;

namespace LibDataInput;

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
            var ch = Console.ReadKey(true);
            var key = ch.Key == ConsoleKey.Enter ? _defaultValue ? "y" : "n" : ch.Key.Value().ToLower();
            if (key == "y" || key == "n")
            {
                Value = key == "y";
                Console.Write(key);
                Console.WriteLine();
                return true;
            }

            if (ch.Key == ConsoleKey.Escape)
                throw new DataInputEscapeException("Escape");

            Console.WriteLine("Answer must be 'y' or 'n'");
            Console.Write($"{_fieldName} (y/n)[{(_defaultValue ? "y" : "n")}]: ");
        }
    }
}