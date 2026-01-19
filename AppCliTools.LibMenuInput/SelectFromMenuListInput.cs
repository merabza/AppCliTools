using System;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;

namespace AppCliTools.LibMenuInput;

public sealed class SelectFromMenuListInput : DataInput
{
    private readonly string? _defaultValue;
    private readonly string _fieldName;
    private readonly CliMenuSet _listSet;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SelectFromMenuListInput(string fieldName, CliMenuSet listSet, string? defaultValue = null)
    {
        _fieldName = fieldName;
        _defaultValue = defaultValue;
        _listSet = listSet;
    }

    public int Id { get; private set; }

    public CliMenuItem? SelectedCliMenuItem { get; private set; }

    public override bool DoInput()
    {
        string prompt = $"Select {_fieldName} {(_defaultValue == null ? string.Empty : $"[{_defaultValue}]")}: ";
        Console.Write(prompt);
        _listSet.Show(false);

        while (true)
        {
            ConsoleKeyInfo ch = Console.ReadKey(true);
            CliMenuItem? menuItem = _listSet.GetMenuItemByKey(ch);
            if (menuItem != null)
            {
                SelectedCliMenuItem = menuItem;
                string key = ch.Key.Value();
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
                case ConsoleKey.Enter when _defaultValue == null:
                    continue;
                case ConsoleKey.Enter:
                    {
                        menuItem = _listSet.GetMenuItemWithName(_defaultValue);

                        if (menuItem == null)
                        {
                            continue;
                        }

                        SelectedCliMenuItem = menuItem;
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
