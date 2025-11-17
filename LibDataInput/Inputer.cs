using System;

namespace LibDataInput;

public static class Inputer
{
    public static EDialogResult InputAnswer(string fieldName, EDialogResult defaultValue, bool useException = true)
    {
        var answerInput = new AnswerInput(fieldName, defaultValue);
        if (answerInput.DoInput())
            return answerInput.Value;
        return useException ? throw new DataInputException($"Invalid input of {fieldName}") : defaultValue;
    }

    public static bool InputBool(string fieldName, bool defaultValue, bool useException = true)
    {
        var boolInput = new BoolDataInput(fieldName, defaultValue);
        if (boolInput.DoInput())
            return boolInput.Value;
        return useException ? throw new DataInputException($"Invalid input of {fieldName}") : false;
    }

    public static int InputInt(string fieldName, int defaultValue)
    {
        var intInput = new IntDataInput(fieldName, defaultValue);
        return !intInput.DoInput() ? throw new DataInputException($"Invalid input of {fieldName}") : intInput.Value;
    }

    public static string? InputText(string fieldName, string? defaultValue, char passwordCharacter = '\0')
    {
        var textDataInput = new TextDataInput(fieldName, defaultValue, passwordCharacter);
        return !textDataInput.DoInput()
            ? throw new DataInputException($"Invalid input of {fieldName}")
            : textDataInput.Text;
    }

    public static string InputTextRequired(string fieldName, string? defaultValue = null, char passwordCharacter = '\0')
    {
        var result = string.Empty;
        while (result == string.Empty)
        {
            Console.WriteLine($"{fieldName} is required");
            result = InputText(fieldName, defaultValue, passwordCharacter) ?? string.Empty;
        }

        return result;
    }

    public static DateTime InputDateTime(string fieldName, DateTime defaultValue = default)
    {
        var dateTimeInput = new DateTimeInput(fieldName, defaultValue);
        return !dateTimeInput.DoInput()
            ? throw new DataInputException($"Invalid input of {fieldName}")
            : dateTimeInput.Value;
    }

    public static DateTime InputDate(string fieldName, DateTime defaultValue = default)
    {
        var dateInput = new DateInput(fieldName, defaultValue);
        return !dateInput.DoInput() ? throw new DataInputException($"Invalid input of {fieldName}") : dateInput.Value;
    }

    public static DateTime InputTime(string fieldName, DateTime defaultValue = default)
    {
        var startAtDateTimeInput = new TimeInput(fieldName, defaultValue);
        return !startAtDateTimeInput.DoInput()
            ? throw new DataInputException($"Invalid input of {fieldName}")
            : startAtDateTimeInput.Value;
    }

    public static TimeSpan InputTimeSpan(string fieldName, TimeSpan defaultValue)
    {
        var timeSpanInput = new TimeSpanInput(fieldName, defaultValue);
        return !timeSpanInput.DoInput()
            ? throw new DataInputException($"Invalid input of {fieldName}")
            : timeSpanInput.Value;
    }
}