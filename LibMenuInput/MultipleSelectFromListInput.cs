using System;
using System.Collections.Generic;
using CliMenu;
using LibDataInput;
using LibMenuInput.CliMenuCommands;
using SystemTools.SystemToolsShared;
using System.Globalization;

namespace LibMenuInput;

public sealed class MultipleSelectFromListInput : DataInput
{
    private readonly string _fieldName;
    public Dictionary<string, bool> SourceListWithChecks { get; set; }

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

            foreach (KeyValuePair<string, bool> listItem in SourceListWithChecks)
            {
                listSet.AddMenuItem(
                    new MultipleSelectFromListElementCliMenuCommand(
                        listItem));
            }

            string key = ConsoleKey.Escape.Value().ToUpperInvariant();
            listSet.AddMenuItem(key, new CliMenuCommand("(Exit without saving)"), key.Length);

            listSet.Show();
            ConsoleKeyInfo ch = Console.ReadKey(true);

            CliMenuItem? menuItem = listSet.GetMenuItemByKey(ch);

            if (menuItem == null)
            {
                continue;
            }

            if (menuItem.CountedKey == "0")
            {
                return true;
            }

            if (ch.Key == ConsoleKey.Escape)
            {
                throw new DataInputEscapeException("Escape");
            }

            var menuCommand = (MultipleSelectFromListElementCliMenuCommand)menuItem.CliMenuCommand;
            string text = menuCommand.Key;
            if (!SourceListWithChecks.TryGetValue(text, out bool value))
            {
                Console.WriteLine($"Something is wrong with {_fieldName}.");
                StShared.Pause();
                return false;
            }

            SourceListWithChecks[text] = SourceListWithChecks[text] = !value;
        }
    }
}
