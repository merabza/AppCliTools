using System;
using System.Globalization;
using System.IO;
using SystemTools.SystemToolsShared;

namespace LibDataInput;

public static class Stat
{
    public static string Value(this ConsoleKey key)
    {
        return key switch
        {
            ConsoleKey.OemMinus or ConsoleKey.Subtract => "-",
            >= ConsoleKey.D0 and <= ConsoleKey.D9 => ((int)key - (int)ConsoleKey.D0).ToString(CultureInfo
                .InvariantCulture),
            >= ConsoleKey.NumPad0 and <= ConsoleKey.NumPad9 => ((int)key - (int)ConsoleKey.NumPad0).ToString(CultureInfo
                .InvariantCulture),
            _ => key.ToString().ToLower(CultureInfo.CurrentCulture)
        };
    }

    public static string? IntValue(this ConsoleKey key)
    {
        string? value = key switch
        {
            >= ConsoleKey.D0 and <= ConsoleKey.D9 => ((int)key - (int)ConsoleKey.D0).ToString(CultureInfo
                .InvariantCulture),
            >= ConsoleKey.NumPad0 and <= ConsoleKey.NumPad9 => ((int)key - (int)ConsoleKey.NumPad0).ToString(CultureInfo
                .InvariantCulture),
            _ => null
        };
        return value;
    }

    public static bool CheckRequiredFolder(bool useConsole, string path, bool askQuestion = true)
    {
        var dir = new DirectoryInfo(path);

        if (!dir.Exists)
        {
            return true;
        }

        StShared.WriteWarningLine(
            $"Folder with name {path} already exists.{(askQuestion ? " and will be deleted." : string.Empty)}",
            useConsole);

        if (askQuestion && !Inputer.InputBool($"Delete folder {path}?", false))
        {
            return false;
        }

        FileStat.DeleteDirectoryWithNormaliseAttributes(path);

        return true;
    }
}
