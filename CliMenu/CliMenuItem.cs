namespace CliMenu;

public sealed class CliMenuItem
{
    public CliMenuItem(string? menuItemName, CliMenuCommand cliMenuCommand, int countedId)
    {
        MenuItemName = menuItemName;
        CliMenuCommand = cliMenuCommand;
        CountedId = countedId;
    }

    public CliMenuItem(string? menuItemName, CliMenuCommand cliMenuCommand)
    {
        MenuItemName = menuItemName;
        CliMenuCommand = cliMenuCommand;
    }

    public CliMenuItem(string? key, string? menuItemName, CliMenuCommand cliMenuCommand, int countedId)
    {
        Key = key;
        MenuItemName = menuItemName;
        CliMenuCommand = cliMenuCommand;
        CountedId = countedId;
    }

    public string? CountedKey { get; set; }
    public int CountedId { get; set; }
    public string? Key { get; set; }
    public string? MenuItemName { get; set; }
    public CliMenuCommand CliMenuCommand { get; set; }
}