﻿using System;

namespace LibDataInput;

public static class Inputer
{
    public static EDialogResult InputAnswer(string fieldName, EDialogResult defaultValue, bool useException = true)
    {
        var answerInput = new AnswerInput(fieldName, defaultValue);
        if (answerInput.DoInput())
            return answerInput.Value;
        if (useException)
            throw new DataInputException($"Invalid input of {fieldName}");
        return defaultValue;
    }

    public static bool InputBool(string fieldName, bool defaultValue, bool useException = true)
    {
        var boolInput = new BoolDataInput(fieldName, defaultValue);
        if (boolInput.DoInput())
            return boolInput.Value;
        if (useException)
            throw new DataInputException($"Invalid input of {fieldName}");
        return false;
    }

    public static int InputInt(string fieldName, int defaultValue)
    {
        var intInput = new IntDataInput(fieldName, defaultValue);
        if (!intInput.DoInput())
            throw new DataInputException($"Invalid input of {fieldName}");
        return intInput.Value;
    }

    public static string? InputText(string fieldName, string? defaultValue, char passwordCharacter = default)
    {
        var textDataInput = new TextDataInput(fieldName, defaultValue, passwordCharacter);
        if (!textDataInput.DoInput())
            throw new DataInputException($"Invalid input of {fieldName}");
        return textDataInput.Text;
    }

    public static string InputTextRequired(string fieldName, string? defaultValue = null,
        char passwordCharacter = default)
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
        if (!dateTimeInput.DoInput())
            throw new DataInputException($"Invalid input of {fieldName}");
        return dateTimeInput.Value;
    }

    public static DateTime InputDate(string fieldName, DateTime defaultValue = default)
    {
        var dateInput = new DateInput(fieldName, defaultValue);
        if (!dateInput.DoInput())
            throw new DataInputException($"Invalid input of {fieldName}");
        return dateInput.Value;
    }

    public static DateTime InputTime(string fieldName, DateTime defaultValue = default)
    {
        var startAtDateTimeInput = new TimeInput(fieldName, defaultValue);
        if (!startAtDateTimeInput.DoInput())
            throw new DataInputException($"Invalid input of {fieldName}");
        return startAtDateTimeInput.Value;
    }

    public static TimeSpan InputTimeSpan(string fieldName, TimeSpan defaultValue)
    {
        var timeSpanInput = new TimeSpanInput(fieldName, defaultValue);
        if (!timeSpanInput.DoInput())
            throw new DataInputException($"Invalid input of {fieldName}");
        return timeSpanInput.Value;
    }
}