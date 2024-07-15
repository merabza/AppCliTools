using System;
using System.IO;
using SystemToolsShared;

namespace LibDataInput;

public static class Stat
{
    public static string Value(this ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.OemMinus:
            case ConsoleKey.Subtract:
                return "-";
            case >= ConsoleKey.D0 and <= ConsoleKey.D9:
                return ((int)key - (int)ConsoleKey.D0).ToString();
            case >= ConsoleKey.NumPad0 and <= ConsoleKey.NumPad9:
                return ((int)key - (int)ConsoleKey.NumPad0).ToString();
            default:
                return key.ToString().ToLower();
        }
    }

    public static string? IntValue(this ConsoleKey key)
    {
        var value = key switch
        {
            >= ConsoleKey.D0 and <= ConsoleKey.D9 => ((int)key - (int)ConsoleKey.D0).ToString(),
            >= ConsoleKey.NumPad0 and <= ConsoleKey.NumPad9 => ((int)key - (int)ConsoleKey.NumPad0).ToString(),
            _ => null
        };
        return value;
    }


    public static bool CheckRequiredFolder(bool useConsole, string path, bool askQuestion = true)
    {
        var dir = new DirectoryInfo(path);

        if (!dir.Exists)
            return true;

        StShared.WriteWarningLine(
            $"Folder with name {path} already exists.{(askQuestion ? " and will be deleted." : string.Empty)}",
            useConsole);

        if (askQuestion && !Inputer.InputBool($"Delete folder {path}?", false))
            return false;

        FileStat.DeleteDirectory(path);

        return true;
    }
}