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

            listSet.AddMenuItem(new CliMenuCommand(), "(Save and exit)");
            foreach (var listItem in SourceListWithChecks)
                listSet.AddMenuItem(new CliMenuCommand(listItem.Key),
                    $"{(listItem.Value ? "√" : "×")} {listItem.Key}");
            var key = ConsoleKey.Escape.Value().ToLower();
            listSet.AddMenuItem(key, "(Exit without saving)", new CliMenuCommand(), key.Length);

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
            if (text is null || !SourceListWithChecks.ContainsKey(text))
            {
                Console.WriteLine($"Something is wrong with {_fieldName}.");
                StShared.Pause();
                return false;
            }

            SourceListWithChecks[text] = !SourceListWithChecks[text];
        }
    }
}