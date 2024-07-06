using System;
using CliMenu;
using LibDataInput;

namespace LibMenuInput;

public sealed class SelectFromMenuListInput : DataInput
{
    private readonly string? _defaultValue;
    private readonly string _fieldName;
    private readonly CliMenuSet _listSet;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SelectFromMenuListInput(string fieldName, CliMenuSet listSet, string? defaultValue = default)
    {
        _fieldName = fieldName;
        _defaultValue = defaultValue;
        _listSet = listSet;
    }

    public int Id { get; private set; }

    public override bool DoInput()
    {
        var prompt = $"Select {_fieldName} {(_defaultValue == default ? string.Empty : $"[{_defaultValue}]")}: ";
        Console.Write(prompt);
        _listSet.Show(false);

        while (true)
        {
            var ch = Console.ReadKey(true);
            var menuItem = _listSet.GetMenuItemByKey(ch);
            if (menuItem != null)
            {
                var key = ch.Key.Value();
                if (menuItem.CountedKey == "-")
                {
                    Id = -1;
                    return true;
                }

                Id = menuItem.CountedId;
                Console.Write(key);
                Console.WriteLine();
                Console.WriteLine($"{_fieldName} is: {menuItem.MenuItemName}");
                return true;
            }

            switch (ch.Key)
            {
                case ConsoleKey.Enter when _defaultValue == default:
                    continue;
                case ConsoleKey.Enter:
                {
                    menuItem = _listSet.GetMenuItemWithName(_defaultValue);

                    if (menuItem == null)
                        continue;

                    Id = menuItem.CountedId;
                    Console.WriteLine();
                    Console.WriteLine($"{_fieldName} is: {_defaultValue}");
                    return true;
                }
                case ConsoleKey.Escape:
                    throw new DataInputEscapeException("Escape");
            }
        }
    }
}