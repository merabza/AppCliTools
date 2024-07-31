using System;
using System.Collections.Generic;
using CliMenu;
using LibDataInput;
using SystemToolsShared;

namespace LibMenuInput;

public sealed class MultipleSelectFromListInput : DataInput
{
    private readonly string _fieldName;
    public readonly Dictionary<string, bool> SourceListWithChecks;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MultipleSelectFromListInput(string fieldName, Dictionary<string, bool> sourceListWithChecks)
    {
        _fieldName = fieldName;
        SourceListWithChecks = sourceListWithChecks;
    }

    public override bool DoInput()
    {
        while (true)
        {
            var listSet = new CliMenuSet(_fieldName);

            listSet.AddMenuItem(new CliMenuCommand("(Save and exit)"));
            foreach (var listItem in SourceListWithChecks)
                //$"{(listItem.Value ? "√" : "×")} {listItem.Key}"
                listSet.AddMenuItem(new CliMenuCommand(listItem.Key));
            var key = ConsoleKey.Escape.Value().ToLower();
            listSet.AddMenuItem(key, new CliMenuCommand("(Exit without saving)"), key.Length);

            listSet.Show();
            var ch = Console.ReadKey(true);

            var menuItem = listSet.GetMenuItemByKey(ch);

            if (menuItem == null)
                continue;

            if (menuItem.CountedKey == "0")
                return true;

            if (ch.Key == ConsoleKey.Escape)
                throw new DataInputEscapeException("Escape");

            var text = menuItem.CliMenuCommand.Name;
            if (!SourceListWithChecks.TryGetValue(text, out var value))
            {
                Console.WriteLine($"Something is wrong with {_fieldName}.");
                StShared.Pause();
                return false;
            }

            SourceListWithChecks[text] = SourceListWithChecks[text] = !value;
        }
    }
}