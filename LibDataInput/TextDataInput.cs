using System;
using System.Text;

namespace LibDataInput;

public sealed class TextDataInput : DataInput
{
    private readonly string? _defaultValue;
    private readonly string _fieldName;
    private readonly char _passwordCharacter;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TextDataInput(string fieldName, string? defaultValue = default, char passwordCharacter = default)
    {
        _fieldName = fieldName;
        _defaultValue = defaultValue;
        _passwordCharacter = passwordCharacter;
    }

    public string? Text { get; private set; }

    public override bool DoInput()
    {
        var showDefValue = _defaultValue ?? "";
        if (_passwordCharacter != default && _defaultValue != default)
            showDefValue = new string(_passwordCharacter, showDefValue.Length);
        var prompt = $"Enter {_fieldName} {(_defaultValue == default ? "" : $"[{showDefValue}]")}: ";
        Console.Write(prompt);

        var promptLength = prompt.Length;
        StringBuilder sb = new();
        while (true)
        {
            var ch = Console.ReadKey(true);
            switch (ch.Key)
            {
                case ConsoleKey.Enter when sb.Length == 0:
                    Console.WriteLine(showDefValue);
                    Text = _defaultValue;
                    return true;
                case ConsoleKey.Enter:
                {
                    if (sb.Length > 0)
                    {
                        Text = sb.ToString();
                        Console.WriteLine();
                        return true;
                    }

                    break;
                }
                case ConsoleKey.Escape:
                    throw new DataInputEscapeException("Escape");
                case ConsoleKey.Backspace:
                    if (sb.Length > 0)
                    {
                        sb.Remove(sb.Length - 1, 1);
                        ClearCurrentInput(promptLength);
                        Console.Write(_passwordCharacter == default
                            ? sb.ToString()
                            : new string(_passwordCharacter, sb.Length));
                    }

                    break;
                case ConsoleKey.Delete:
                    if (sb.Length > 0)
                    {
                        sb.Clear();
                        ClearCurrentInput(promptLength);
                    }
                    else
                    {
                        if (Inputer.InputBool("Delete entire text?", false))
                        {
                            Text = "";
                            return true;
                        }
                    }
                    break;
                default:
                    if (ch.KeyChar >= 32)
                        sb.Append(ch.KeyChar);
                    //Console.Write(ch.KeyChar);
                    Console.Write(_passwordCharacter == default ? ch.KeyChar : _passwordCharacter);
                    break;
            }
        }
    }
}