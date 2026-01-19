using System;
using System.Collections.Generic;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;

namespace AppCliTools.LibMenuInput;

public sealed class SelectFromListInput : DataInput
{
    private readonly string? _defaultValue;
    private readonly string _fieldName;
    private readonly List<string> _sourceList;
    private readonly bool _useNone;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SelectFromListInput(string fieldName, List<string> sourceList, string? defaultValue = default,
        bool useNone = false)
    {
        _fieldName = fieldName;
        _defaultValue = defaultValue;
        _sourceList = sourceList;
        _useNone = useNone;
    }

    public string? Text { get; private set; }

    public override bool DoInput()
    {
        string prompt = $"Select for {_fieldName} {(_defaultValue == null ? string.Empty : $"[{_defaultValue}]")}: ";
        Console.Write(prompt);
        var listSet = new CliMenuSet();

        if (_useNone)
        {
            listSet.AddMenuItem("-", new CliMenuCommand("(None)"), 1);
        }

        foreach (string listItem in _sourceList)
        {
            listSet.AddMenuItem(new CliMenuCommand(listItem));
        }

        listSet.Show(false);

        while (true)
        {
            ConsoleKeyInfo ch = Console.ReadKey(true);
            CliMenuItem? menuItem = listSet.GetMenuItemByKey(ch);

            if (menuItem != null)
            {
                if (menuItem.CountedKey == "-")
                {
                    Text = null;
                    return true;
                }

                Text = menuItem.MenuItemName;
                Console.Write(menuItem.CountedKey);
                Console.WriteLine();
                Console.WriteLine($"{_fieldName} is: {Text}");
                return true;
            }

            switch (ch.Key)
            {
                case ConsoleKey.Enter when _defaultValue == null:
                    continue;
                case ConsoleKey.Enter:
                    {
                        if (listSet.GetMenuItemWithName(_defaultValue) == null)
                        {
                            continue;
                        }

                        Text = _defaultValue;
                        Console.WriteLine();
                        Console.WriteLine($"{_fieldName} is: {Text}");
                        return true;
                    }
                case ConsoleKey.Escape:
                    throw new DataInputEscapeException("Escape");
            }
        }
    }
}
