using System;
using System.Collections.Generic;
using System.Linq;

namespace AppCliTools.LibDataInput;

public sealed class AnswerInput : DataInput
{
    private readonly EDialogResult _defaultValue;
    private readonly string _fieldName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public AnswerInput(string fieldName, EDialogResult defaultValue = EDialogResult.Yes)
    {
        _fieldName = fieldName;
        _defaultValue = defaultValue;
    }

    public EDialogResult Value { get; private set; }

    public override bool DoInput()
    {
        string? defaultAnswer = _defaultValue.ToString().Take(1).ToString();
        string prompt = Enum.GetValues<EDialogResult>().Aggregate($"{_fieldName} (",
                            (current, dialogResult) =>
                                $"{current}/{dialogResult.ToString().First(char.IsUpper).ToString().ToUpperInvariant()}{dialogResult}") +
                        $")[{defaultAnswer}]: ";
        Console.Write(prompt);

        Dictionary<string, EDialogResult> mainLetters = Enum.GetValues<EDialogResult>()
            .ToDictionary(k => k.ToString().First(char.IsUpper).ToString(), v => v);

        while (true)
        {
            ConsoleKeyInfo ch = Console.ReadKey(true);
            string? key = ch.Key == ConsoleKey.Enter ? defaultAnswer : ch.Key.Value().ToUpperInvariant();
            if (key != null && mainLetters.TryGetValue(key, out EDialogResult letter))
            {
                Value = letter;
                Console.Write(key);
                Console.WriteLine();
                return true;
            }

            if (ch.Key == ConsoleKey.Escape)
            {
                throw new DataInputEscapeException("Escape");
            }

            string answerMustBe = Enum.GetValues<EDialogResult>().Aggregate("Answer must be",
                (current, dialogResult) =>
                    $"{current} '{dialogResult.ToString().First(char.IsUpper).ToString().ToUpperInvariant()}, ");
            Console.WriteLine(answerMustBe);
            Console.Write(prompt);
        }
    }
}
