using System;
using System.Collections.Generic;
using System.IO;

namespace LibMenuInput;

public sealed class FolderPathInput : PathInput
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public FolderPathInput(string fieldName, string? defaultValue = null, bool warningIfFileDoesNotExists = true) :
        base(fieldName, true, defaultValue, warningIfFileDoesNotExists)
    {
    }

    public string? FolderPath => EnteredPath;


    protected override void AddFiles(DirectoryInfo dir, string fileName, List<string> names)
    {
    }


    protected override bool CheckExists()
    {
        return !IsFileSchema(EnteredPath) || Directory.Exists(EnteredPath);
    }


    private static bool IsFileSchema(string? path)
    {
        var uriCreated = Uri.TryCreate(path, UriKind.Absolute, out var uri);
        return !uriCreated || uri is null || uri.Scheme.ToLower() == "file";
    }
}