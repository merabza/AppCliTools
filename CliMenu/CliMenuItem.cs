namespace CliMenu;

public sealed class CliMenuItem
{
    public CliMenuItem(CliMenuCommand cliMenuCommand, int countedId)
    {
        //MenuItemName = menuItemName;
        CliMenuCommand = cliMenuCommand;
        CountedId = countedId;
    }

    public CliMenuItem(CliMenuCommand cliMenuCommand)
    {
        //MenuItemName = menuItemName;
        CliMenuCommand = cliMenuCommand;
    }

    // ReSharper disable once ConvertToPrimaryConstructor
    public CliMenuItem(string? key, CliMenuCommand cliMenuCommand, int countedId)
    {
        Key = key;
        //MenuItemName = menuItemName;
        CliMenuCommand = cliMenuCommand;
        CountedId = countedId;
    }

    public string? CountedKey { get; set; }
    public int CountedId { get; set; }
    public string? Key { get; set; }
    public string MenuItemName => CliMenuCommand.Name;
    public CliMenuCommand CliMenuCommand { get; set; }

    public void SetMenuSet(CliMenuSet menuSet)
    {
        CliMenuCommand.MenuSet = menuSet;
    }
}