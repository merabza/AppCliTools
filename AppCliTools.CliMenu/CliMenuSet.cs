using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliMenu;

public sealed class CliMenuSet
{
    private readonly List<string> _errorMessages = [];

    // ReSharper disable once ConvertToPrimaryConstructor
    public CliMenuSet(string? caption = null, int menuVersion = 0)
    {
        Caption = caption;
        MenuVersion = menuVersion;
    }

    public int MenuVersion { get; set; }

    //public string Name => _caption ?? string.Empty;

    private List<CliMenuItem> MenuItems { get; } = [];
    public CliMenuSet? ParentMenu { get; set; }
    public string? Caption { get; }

    public CliMenuItem? GetMenuItemWithName(string menuItemName)
    {
        return MenuItems.SingleOrDefault(a => a.MenuItemName == menuItemName);
    }

    public CliMenuItem? GetMenuItemByKey(ConsoleKeyInfo consoleKeyInfo)
    {
        int keyId = GetNoKeyId(consoleKeyInfo);
        if (keyId > -1)
        {
            List<CliMenuItem> menuItemsWithNoKey = MenuItems.Where(w => w.Key == null).ToList();
            if (menuItemsWithNoKey.Count <= keyId)
            {
                return null;
            }

            menuItemsWithNoKey[keyId].CountedKey = consoleKeyInfo.KeyChar.ToString();
            menuItemsWithNoKey[keyId].CountedId = keyId;
            return menuItemsWithNoKey[keyId];
        }

        string key = char.IsSymbol(consoleKeyInfo.KeyChar) || consoleKeyInfo.KeyChar == '-'
            ? consoleKeyInfo.KeyChar.ToString()
            : consoleKeyInfo.Key.ToString().ToLower(CultureInfo.CurrentCulture);

        CliMenuItem? menuItem = MenuItems.SingleOrDefault(w =>
            w.Key != null && w.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (menuItem == null)
        {
            return null;
        }

        menuItem.CountedKey = key;
        menuItem.CountedId = -1;
        return menuItem;
    }

    private static int GetNoKeyId(ConsoleKeyInfo consoleKeyInfo)
    {
        if (char.IsDigit(consoleKeyInfo.KeyChar))
        {
            return consoleKeyInfo.KeyChar - '0';
        }

        if (IsInRange(consoleKeyInfo.KeyChar, 'a', '~'))
        {
            return consoleKeyInfo.KeyChar - 'a' + 10;
        }

        if (IsInRange(consoleKeyInfo.KeyChar, 'A', 'Z'))
        {
            return consoleKeyInfo.KeyChar - 'A' + 40;
        }

        return -1;
    }

    private static bool IsInRange(char c, char min, char max)
    {
        return (uint)(c - min) <= (uint)(max - min);
    }

    private static string UseLength(string? strFrom, int length, bool addSpaces = true)
    {
        string str = strFrom ?? string.Empty;
        int strLength = str.Length;
        int spacesLength = length > strLength ? length - strLength : 0;
        string spaces = addSpaces ? new string(' ', spacesLength) : string.Empty;
        return strLength > length ? str[..length] : $"{str}{spaces}";
    }

    private string? GetCaption()
    {
        if (Caption == null)
        {
            return null;
        }

        string? parentCaption = ParentMenu?.GetCaption();
        return parentCaption != null ? $"{parentCaption}/{Caption}" : $"/{Caption}";
    }

    public void Show(bool clearBefore = true)
    {
        if (clearBefore)
        {
            Console.Clear();
        }

        string? caption = GetCaption();
        if (caption is not null)
        {
            Console.WriteLine(caption);
        }

        Console.WriteLine();

        foreach (CliMenuItem menuItem in MenuItems)
        {
            menuItem.CliMenuCommand.CountStatus();
        }

        int width = Console.WindowWidth - 6;
        int max1 = 0;
        int max2 = 0;
        if (MenuItems.Any(w => w.CliMenuCommand.StatusView == EStatusView.Table))
        {
            max1 = MenuItems.Where(w => w.CliMenuCommand.StatusView == EStatusView.Table)
                .Max(m => m.MenuItemName.Length);
            max2 = MenuItems.Where(w => w.CliMenuCommand.StatusView == EStatusView.Table)
                .Max(m => m.CliMenuCommand.StatusString?.Length ?? 0);
            double k = (max1 + max2 + 3.0) / width;
            if (k > 1)
            {
                max1 = (int)(max1 / k);
                max2 = (int)(max2 / k);
            }
        }

        int menuId = 0;
        foreach (CliMenuItem menuItem in MenuItems)
        {
            string? key = menuItem.Key;
            if (key == null)
            {
                key = GetKey(menuId);
                menuId++;
            }

            if (key == null)
            {
                _errorMessages.Add("Too many lines in menu");
                break;
            }

            if (key.Length > 3)
            {
                key = key[..3];
            }

            string preSpace = new(' ', 4 - key.Length);
            //Console.WriteLine($"{preSpace}{key}. {menuItem.MenuItemName} {menuItem.CliMenuCommand.GetStatus() ?? string.Empty}");

            Console.Write($"{preSpace}{key}. ");
            if (menuItem.CliMenuCommand.NameIsStatus)
            {
                ConsoleColor currentColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(UseLength(menuItem.MenuItemName, width, false));
                Console.ForegroundColor = currentColor;
            }
            else if (menuItem.CliMenuCommand.StatusString == null)
            {
                Console.WriteLine(UseLength(menuItem.MenuItemName, width, false));
            }
            else
            {
                if (menuItem.CliMenuCommand.StatusView == EStatusView.Table)
                {
                    Console.Write($"{UseLength(menuItem.MenuItemName, max1)}: ");
                    ConsoleColor currentColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(UseLength(menuItem.CliMenuCommand.StatusString, max2, false));
                    Console.ForegroundColor = currentColor;
                }
                else
                {
                    Console.WriteLine(UseLength(
                        $"{menuItem.MenuItemName} {(string.IsNullOrWhiteSpace(menuItem.CliMenuCommand.StatusString) ? string.Empty : $"({menuItem.CliMenuCommand.StatusString})")}",
                        width, false));
                }
            }
        }

        foreach (string errorMessage in _errorMessages)
        {
            StShared.WriteErrorLine(errorMessage, true);
        }

        Console.WriteLine(string.Empty);
        Console.Write("enter your choice: ");
    }

    public void AddMenuItem(CliMenuCommand menuCommand, int useId = -1)
    {
        var menuItem = new CliMenuItem(menuCommand, useId);
        MenuItems.Add(menuItem);
    }

    //საჭიროა ApAgent პროექტში
    // ReSharper disable once UnusedMember.Global
    public void InsertMenuItem(int index, CliMenuCommand menuCommand)
    {
        var menuItem = new CliMenuItem(menuCommand);

        MenuItems.Insert(index, menuItem);
    }

    private static string? GetKey(int keyId)
    {
        return keyId switch
        {
            < 10 => keyId.ToString(CultureInfo.InvariantCulture),
            < 40 => ((char)('a' + (keyId - 10))).ToString(),
            < 70 => ((char)('A' + (keyId - 40))).ToString(),
            < 103 => ((char)('ა' + (keyId - 70))).ToString(),
            _ => null
        };
    }

    public void AddMenuItem(string key, CliMenuCommand menuCommand, int checkKeyLength, int useId = -1)
    {
        if (key.Length > checkKeyLength)
        {
            _errorMessages.Add($"Length of key {key} is not valid. menu line \"{menuCommand.Name}\" not added");
            return;
        }

        var menuItem = new CliMenuItem(key, menuCommand, useId);

        MenuItems.Add(menuItem);
    }

    public void FixMenuElements()
    {
        foreach (CliMenuItem cliMenuItem in MenuItems)
        {
            cliMenuItem.SetMenuSet(this);
        }
    }
}
