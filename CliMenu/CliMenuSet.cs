using System;
using System.Collections.Generic;
using System.Linq;
using SystemToolsShared;

namespace CliMenu;

public sealed class CliMenuSet
{
    private readonly string? _caption;
    private readonly List<string> _errorMessages = [];

    // ReSharper disable once ConvertToPrimaryConstructor
    public CliMenuSet(string? caption = null)
    {
        _caption = caption;
    }

    public string Name => _caption ?? string.Empty;

    private List<CliMenuItem> MenuItems { get; } = [];
    public CliMenuSet? ParentMenu { get; set; }

    public CliMenuItem? GetMenuItemWithName(string menuItemName)
    {
        return MenuItems.SingleOrDefault(a => a.MenuItemName == menuItemName);
    }

    public CliMenuItem? GetMenuItemByKey(ConsoleKeyInfo consoleKeyInfo)
    {
        var keyId = GetNoKeyId(consoleKeyInfo);
        if (keyId > -1)
        {
            var menuItemsWithNoKey = MenuItems.Where(w => w.Key == null).ToList();
            if (menuItemsWithNoKey.Count <= keyId)
                return null;
            menuItemsWithNoKey[keyId].CountedKey = consoleKeyInfo.KeyChar.ToString();
            menuItemsWithNoKey[keyId].CountedId = keyId;
            return menuItemsWithNoKey[keyId];
        }

        var key = char.IsSymbol(consoleKeyInfo.KeyChar) || consoleKeyInfo.KeyChar == '-'
            ? consoleKeyInfo.KeyChar.ToString()
            : consoleKeyInfo.Key.ToString().ToLower();

        var menuItem = MenuItems.SingleOrDefault(w =>
            w.Key != null && w.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase));
        if (menuItem == null)
            return null;

        menuItem.CountedKey = key;
        menuItem.CountedId = -1;
        return menuItem;
    }

    private static int GetNoKeyId(ConsoleKeyInfo consoleKeyInfo)
    {
        if (char.IsDigit(consoleKeyInfo.KeyChar)) return consoleKeyInfo.KeyChar - '0';

        //if (IsInRange(char.ToLower(consoleKeyInfo.KeyChar), 'a', '~'))
        if (IsInRange(consoleKeyInfo.KeyChar, 'a', '~')) return consoleKeyInfo.KeyChar - 'a' + 10;

        if (IsInRange(consoleKeyInfo.KeyChar, 'A', 'Z')) return consoleKeyInfo.KeyChar - 'A' + 40;

        return -1;
    }

    private static bool IsInRange(char c, char min, char max)
    {
        return (uint)(c - min) <= (uint)(max - min);
    }

    private static string UseLength(string? strFrom, int length, bool addSpaces = true)
    {
        var str = strFrom ?? "";
        var strLength = str.Length;
        return strLength > length
            ? str[..length]
            : $"{str}{(addSpaces ? new string(' ', length - strLength) : "")}";
    }

    private string? GetCaption()
    {
        if (_caption == null)
            return null;
        var parentCaption = ParentMenu?.GetCaption();
        return parentCaption != null ? $"{parentCaption}/{_caption}" : $"/{_caption}";
    }

    public void Show(bool clearBefore = true)
    {
        if (clearBefore)
            Console.Clear();
        var caption = GetCaption();
        if (caption is not null)
            Console.WriteLine(caption);
        Console.WriteLine();

        foreach (var menuItem in MenuItems) 
            menuItem.CliMenuCommand.CountStatus();


        var width = Console.WindowWidth - 6;
        var max1 = 0;
        var max2 = 0;
        if (MenuItems.Any(w => w.CliMenuCommand.StatusView == EStatusView.Table))
        {
            max1 = MenuItems.Where(w => w.CliMenuCommand.StatusView == EStatusView.Table)
                .Max(m => m.MenuItemName?.Length ?? 0);
            max2 = MenuItems.Where(w => w.CliMenuCommand.StatusView == EStatusView.Table)
                .Max(m => m.CliMenuCommand.Status?.Length ?? 0);
            var k = (max1 + max2 + 3.0) / width;
            if (k > 1)
            {
                max1 = (int)(max1 / k);
                max2 = (int)(max2 / k);
            }
        }


        var menuId = 0;
        foreach (var menuItem in MenuItems)
        {
            var key = menuItem.Key;
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
                key = key.Substring(0, 3);
            string preSpace = new(' ', 4 - key.Length);
            //Console.WriteLine($"{preSpace}{key}. {menuItem.MenuItemName} {menuItem.CliMenuCommand.GetStatus() ?? ""}");

            Console.Write($"{preSpace}{key}. ");
            if (menuItem.CliMenuCommand.NameIsStatus)
            {
                var currentColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(UseLength(menuItem.MenuItemName, width, false));
                Console.ForegroundColor = currentColor;
            }
            else if (menuItem.CliMenuCommand.Status == null)
            {
                Console.WriteLine(UseLength(menuItem.MenuItemName, width, false));
            }
            else
            {
                if (menuItem.CliMenuCommand.StatusView == EStatusView.Table)
                {
                    Console.Write($"{UseLength(menuItem.MenuItemName, max1)}: ");
                    var currentColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(UseLength(menuItem.CliMenuCommand.Status, max2, false));
                    Console.ForegroundColor = currentColor;
                }
                else
                {
                    Console.WriteLine(UseLength(
                        $"{menuItem.MenuItemName} {(string.IsNullOrWhiteSpace(menuItem.CliMenuCommand.Status) ? "" : $"({menuItem.CliMenuCommand.Status})")}",
                        width, false));
                }
            }
        }

        foreach (var errorMessage in _errorMessages) StShared.WriteErrorLine(errorMessage, true);

        Console.WriteLine("");
        Console.Write("enter your choice: ");
    }

    public void AddMenuItemsRange(IEnumerable<CliMenuCommand> menuCommands, int useIdFrom = -1)
    {
        MenuItems.AddRange(menuCommands.Select((x, i) =>
            new CliMenuItem(x.Name, x, useIdFrom == -1 ? -1 : i + useIdFrom)));
    }

    public void AddMenuItem(CliMenuCommand menuCommand, string? menuItemName = null, int useId = -1)
    {
        var menuItem = new CliMenuItem(menuItemName ?? menuCommand.Name, menuCommand, useId);
        MenuItems.Add(menuItem);
    }


    public void InsertMenuItem(int index, CliMenuCommand menuCommand, string? menuItemName = null)
    {
        CliMenuItem menuItem = new(menuItemName, menuCommand);

        MenuItems.Insert(index, menuItem);
    }

    private static string? GetKey(int keyId)
    {
        return keyId switch
        {
            < 10 => keyId.ToString(),
            < 40 => ((char)('a' + (keyId - 10))).ToString(),
            < 70 => ((char)('A' + (keyId - 40))).ToString(),
            < 103 => ((char)('ა' + (keyId - 70))).ToString(),
            _ => null
        };
    }


    public void AddMenuItem(string key, string menuItemName, CliMenuCommand menuCommand, int checkKeyLength,
        int useId = -1)
    {
        if (key.Length > checkKeyLength)
        {
            _errorMessages.Add($"Length of key {key} is not valid. menu line \"{menuItemName}\" not added");
            return;
        }

        CliMenuItem menuItem = new(key, menuItemName, menuCommand, useId);

        MenuItems.Add(menuItem);
    }
}