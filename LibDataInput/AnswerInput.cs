using System;
using System.Linq;

namespace LibDataInput;

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
        var defaultAnswer = _defaultValue.ToString().Take(1).ToString();
        var prompt = Enum.GetValues<EDialogResult>().Aggregate($"{_fieldName} (",
                         (current, dialogResult) =>
                             $"{current}/{dialogResult.ToString().First(char.IsUpper).ToString().ToLower()}{dialogResult}") +
                     $")[{defaultAnswer}]: ";
        Console.Write(prompt);

        var mainLetters = Enum.GetValues<EDialogResult>()
            .ToDictionary(k => k.ToString().First(char.IsUpper).ToString(), v => v);

        while (true)
        {
            var ch = Console.ReadKey(true);
            var key = ch.Key == ConsoleKey.Enter ? defaultAnswer : ch.Key.Value().ToLower();
            if (key != null && mainLetters.TryGetValue(key, out var letter))
            {
                Value = letter;
                Console.Write(key);
                Console.WriteLine();
                return true;
            }

            if (ch.Key == ConsoleKey.Escape)
                throw new DataInputEscapeException("Escape");

            var answerMustBe = Enum.GetValues<EDialogResult>().Aggregate("Answer must be",
                (current, dialogResult) =>
                    $"{current} '{dialogResult.ToString().First(char.IsUpper).ToString().ToLower()}, ");
            Console.WriteLine(answerMustBe);
            Console.Write(prompt);
        }
    }
}