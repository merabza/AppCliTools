using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AppCliTools.LibDataInput;
using SystemTools.SystemToolsShared;

namespace AppCliTools.LibMenuInput;

public /*open*/ class PathInput : DataInput
{
    private readonly bool _addDirectorySeparatorChar;
    private readonly string? _defaultValue;

    private readonly string _fieldName;
    private readonly bool _warningIfFileDoesNotExists;

    protected PathInput(string fieldName, bool addDirectorySeparatorChar, string? defaultValue = null,
        bool warningIfFileDoesNotExists = true)
    {
        _fieldName = fieldName;
        _addDirectorySeparatorChar = addDirectorySeparatorChar;
        _defaultValue = defaultValue;
        _warningIfFileDoesNotExists = warningIfFileDoesNotExists;
    }

    protected string? EnteredPath { get; private set; }

    public override bool DoInput()
    {
        Console.WriteLine($"Enter {_fieldName}: ");

        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(_defaultValue))
        {
            string? def = _defaultValue;
            if (_addDirectorySeparatorChar)
            {
                def = _defaultValue.AddNeedLastPart(Path.DirectorySeparatorChar);
            }

            Console.Write(def);
            sb.Append(def);
        }

        ConsoleKeyInfo input = Console.ReadKey(true);

        while (true)
        {
            string currentInput = sb.ToString();
            switch (input.Key)
            {
                case ConsoleKey.Escape:
                    throw new DataInputEscapeException("Escape");
                case ConsoleKey.Tab:
                    {
                        string? dirName = Path.GetDirectoryName(currentInput);
                        string fileName = Path.GetFileName(currentInput);
                        if (string.IsNullOrWhiteSpace(dirName) || string.IsNullOrWhiteSpace(fileName) ||
                            !Directory.Exists(dirName))
                        {
                            input = Console.ReadKey(true);
                            continue;
                        }

                        var dir = new DirectoryInfo(dirName);
                        List<string> names = dir.GetDirectories($"{fileName}*")
                            .Select(s => s.Name + Path.DirectorySeparatorChar).ToList();
                        AddFiles(dir, fileName, names);
                        names = [.. names.OrderBy(o => o)];

                        string? candidate = names.MinBy(o => o);
                        if (candidate != null && names.Count > 1)
                        {
                            int minimumLength = names.Min(x => x.Length);
                            int commonChars = Enumerable.Range(0, minimumLength)
                                .Count(i => names.All(y => y[i] == names[0][i]));
                            candidate = candidate[..commonChars];
                        }

                        if (string.IsNullOrWhiteSpace(candidate))
                        {
                            input = Console.ReadKey(true);
                            continue;
                        }

                        ClearCurrentLine();
                        sb.Clear();

                        string newVersion = Path.Combine(dirName, candidate);

                        Console.Write(newVersion);
                        sb.Append(newVersion);
                        break;
                    }
                case ConsoleKey.Backspace:
                    if (currentInput.Length > 0)
                    {
                        sb.Remove(sb.Length - 1, 1);
                        ClearCurrentLine();
                        currentInput = currentInput.Remove(currentInput.Length - 1);
                        Console.Write(currentInput);
                    }

                    break;
                case ConsoleKey.Delete:
                    if (sb.Length > 0)
                    {
                        ClearCurrentLine();
                        sb.Clear();
                    }
                    else
                    {
                        if (Inputer.InputBool("Delete entire text?", false))
                        {
                            EnteredPath = string.Empty;
                            return true;
                        }
                    }

                    break;
                case ConsoleKey.Enter:
                    Console.Write(input.KeyChar);
                    EnteredPath = sb.ToString();
                    Console.WriteLine($"Entered {_fieldName} is: {EnteredPath}");
                    if (_warningIfFileDoesNotExists && !CheckExists())
                    {
                        StShared.WriteWarningLine($"{EnteredPath} does not exists", true);
                    }

                    return true;
                default:
                    {
                        char key = input.KeyChar;
                        sb.Append(key);
                        Console.Write(key);
                        break;
                    }
            }

            input = Console.ReadKey(true);
        }
    }

    protected virtual bool CheckExists()
    {
        return false;
    }

    protected virtual void AddFiles(DirectoryInfo dir, string fileName, List<string> names)
    {
        //List<string> fileNames = dir.GetFiles($"{fileName}*").Select(s => s.Name).ToList();
        //names.AddRange(fileNames);
    }

    private static void ClearCurrentLine()
    {
        int currentLine = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLine);
    }
}
