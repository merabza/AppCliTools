using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibDataInput;
using SystemToolsShared;

namespace LibMenuInput;

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

        StringBuilder sb = new();

        if (!string.IsNullOrWhiteSpace(_defaultValue))
        {
            var def = _defaultValue;
            if (_addDirectorySeparatorChar)
                def = _defaultValue.AddNeedLastPart(Path.DirectorySeparatorChar);
            Console.Write(def);
            sb.Append(def);
        }

        var input = Console.ReadKey(true);

        while (input.Key != ConsoleKey.Enter)
        {
            var currentInput = sb.ToString();
            switch (input.Key)
            {
                case ConsoleKey.Escape:
                    throw new DataInputEscapeException("Escape");
                case ConsoleKey.Tab:
                {
                    var dirName = Path.GetDirectoryName(currentInput);
                    var fileName = Path.GetFileName(currentInput);
                    if (string.IsNullOrWhiteSpace(dirName) || string.IsNullOrWhiteSpace(fileName) ||
                        !Directory.Exists(dirName))
                    {
                        input = Console.ReadKey(true);
                        continue;
                    }

                    DirectoryInfo dir = new(dirName);
                    var names = dir.GetDirectories($"{fileName}*")
                        .Select(s => s.Name + Path.DirectorySeparatorChar).ToList();
                    AddFiles(dir, fileName, names);
                    names = names.OrderBy(o => o).ToList();

                    var candidate = names.MinBy(o => o);
                    if (candidate != null && names.Count > 1)
                    {
                        var minimumLength = names.Min(x => x.Length);
                        var commonChars = Enumerable.Range(0, minimumLength)
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

                    var newVersion = Path.Combine(dirName, candidate);

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
                default:
                {
                    var key = input.KeyChar;
                    sb.Append(key);
                    Console.Write(key);
                    break;
                }
            }

            input = Console.ReadKey(true);
        }

        Console.Write(input.KeyChar);

        EnteredPath = sb.ToString();

        Console.WriteLine($"Entered {_fieldName} is: {EnteredPath}");

        if (_warningIfFileDoesNotExists && !CheckExists())
            StShared.WriteWarningLine($"{EnteredPath} does not exists", true);


        return true;
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
        var currentLine = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLine);
    }
}