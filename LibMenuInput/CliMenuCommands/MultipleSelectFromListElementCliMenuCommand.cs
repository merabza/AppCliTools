using System.Collections.Generic;
using CliMenu;

namespace LibMenuInput.CliMenuCommands;

public sealed class MultipleSelectFromListElementCliMenuCommand : CliMenuCommand
{
    private readonly KeyValuePair<string, bool> _listItem;

    public string Key => _listItem.Key;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MultipleSelectFromListElementCliMenuCommand(KeyValuePair<string, bool> listItem) : base(
        $"{(listItem.Value ? "√" : "×")} {listItem.Key}")
    {
        _listItem = listItem;
    }
}