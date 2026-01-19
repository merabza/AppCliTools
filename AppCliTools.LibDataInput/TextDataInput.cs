using System;
using System.Text;

namespace LibDataInput;

public sealed class TextDataInput : DataInput
{
    private readonly string? _defaultValue;
    private readonly string _fieldName;
    private readonly char _passwordCharacter;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TextDataInput(string fieldName, string? defaultValue = null, char passwordCharacter = '\0')
    {
        _fieldName = fieldName;
        _defaultValue = defaultValue;
        _passwordCharacter = passwordCharacter;
    }

    public string? Text { get; private set; }

    public override bool DoInput()
    {
        string showDefValue = _defaultValue ?? string.Empty;
        if (_passwordCharacter != 0 && _defaultValue != null)
        {
            showDefValue = new string(_passwordCharacter, showDefValue.Length);
        }

        string prompt = $"Enter {_fieldName} {(_defaultValue == null ? string.Empty : $"[{showDefValue}]")}: ";
        Console.Write(prompt);

        int promptLength = prompt.Length;
        var sb = new StringBuilder();
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
                        Console.Write(_passwordCharacter == 0
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
                            Text = string.Empty;
                            return true;
                        }
                    }

                    break;
                default:
                    if (ch.KeyChar >= 32)
                    {
                        sb.Append(ch.KeyChar);
                    }

                    //Console.Write(ch.KeyChar);
                    Console.Write(_passwordCharacter == 0 ? ch.KeyChar : _passwordCharacter);
                    break;
            }
        }
    }
}
