using System;
using System.Collections.Generic;
using System.Linq;
using CliMenu;
using LibDataInput;

namespace LibMenuInput;

public static class MenuInputer
{
    public static int InputIdFromMenuList(string fieldName, CliMenuSet listSet, string? defaultValue = null)
    {
        var selectMenuListInput = new SelectFromMenuListInput(fieldName, listSet, defaultValue);
        if (!selectMenuListInput.DoInput())
            throw new DataInputException($"Invalid input of {fieldName}");
        return selectMenuListInput.Id;
    }

    public static string? InputFromMenuList(string fieldName, CliMenuSet listSet, string? defaultValue = null)
    {
        var selectMenuListInput = new SelectFromMenuListInput(fieldName, listSet, defaultValue);
        if (!selectMenuListInput.DoInput())
            throw new DataInputException($"Invalid input of {fieldName}");
        return selectMenuListInput.SelectedCliMenuItem?.CliMenuCommand.Name;
    }

    public static TEnum? InputFromEnumNullableList<TEnum>(string fieldName, TEnum? defaultValue)
        where TEnum : struct, Enum
    {
        const string none = "__None__";
        var names = new List<string> { none };
        names.AddRange(Enum.GetNames<TEnum>());

        var selectInput = new SelectFromListInput(fieldName, names, defaultValue.ToString());
        if (!selectInput.DoInput())
            throw new DataInputException($"Input {fieldName} Escaped");
        var key = selectInput.Text;

        if (key == none)
            return null;

        if (!Enum.TryParse(key, out TEnum dataProvider))
            throw new DataInputException($"Invalid selection of {fieldName}");

        return dataProvider;
    }

    public static TEnum InputFromEnumList<TEnum>(string fieldName, TEnum defaultValue) where TEnum : struct, Enum
    {
        var selectInput = new SelectFromListInput(fieldName, [.. Enum.GetNames<TEnum>()], defaultValue.ToString());
        if (!selectInput.DoInput())
            throw new DataInputException($"Input {fieldName} Escaped");
        var key = selectInput.Text;

        if (!Enum.TryParse(key, out TEnum dataProvider))
            throw new DataInputException($"Invalid selection of {fieldName}");

        return dataProvider;
    }

    public static List<string> MultipleInputFromList(string caption, Dictionary<string, bool> oldChecks)
    {
        var multipleSelectFromListInput = new MultipleSelectFromListInput(caption, oldChecks);
        if (!multipleSelectFromListInput.DoInput())
            throw new DataInputException($"Input {caption} Escaped");
        return multipleSelectFromListInput.SourceListWithChecks.Where(w => w.Value).Select(s => s.Key).ToList();
    }

    public static string? InputFolderPath(string fieldName, string? defaultValue = null)
    {
        var folderInput = new FolderPathInput(fieldName, defaultValue);
        if (!folderInput.DoInput())
            throw new DataInputException($"Invalid input of {fieldName}");
        return folderInput.FolderPath;
    }

    public static string InputFolderPathRequired(string fieldName, string? defaultValue = null)
    {
        var result = string.Empty;
        while (result == string.Empty)
        {
            Console.WriteLine($"{fieldName} is required");
            result = InputFolderPath(fieldName, defaultValue) ?? string.Empty;
        }

        return result;
    }

    public static string? InputFileOrFolderPath(string fieldName, string? defaultValue,
        bool warningIfFileDoesNotExists = true)
    {
        var fileOrFolderPathInput = new FileOrFolderPathInput(fieldName, defaultValue, warningIfFileDoesNotExists);
        if (!fileOrFolderPathInput.DoInput())
            throw new DataInputException($"Invalid input of {fieldName}");
        return fileOrFolderPathInput.FileOrFolderPath;
    }

    public static string? InputFilePath(string fieldName, string? defaultValue, bool warningIfFileDoesNotExists = true)
    {
        var filePathInput = new FilePathInput(fieldName, defaultValue, warningIfFileDoesNotExists);
        if (!filePathInput.DoInput())
            throw new DataInputException($"Invalid input of {fieldName}");
        return filePathInput.FilePath;
    }
}