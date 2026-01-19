using System.Collections.Generic;
using AppCliTools.CliMenu;

namespace AppCliTools.LibMenuInput.CliMenuCommands;

public sealed class MultipleSelectFromListElementCliMenuCommand : CliMenuCommand
{
    private readonly KeyValuePair<string, bool> _listItem;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MultipleSelectFromListElementCliMenuCommand(KeyValuePair<string, bool> listItem) : base(
        $"{(listItem.Value ? "√" : "×")} {listItem.Key}")
    {
        _listItem = listItem;
    }

    public string Key => _listItem.Key;
}
