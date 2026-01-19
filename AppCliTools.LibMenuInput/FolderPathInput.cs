using System;
using System.IO;

namespace AppCliTools.LibMenuInput;

public sealed class FolderPathInput : PathInput
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public FolderPathInput(string fieldName, string? defaultValue = null, bool warningIfFileDoesNotExists = true) :
        base(fieldName, true, defaultValue, warningIfFileDoesNotExists)
    {
    }

    public string? FolderPath => EnteredPath;

    protected override bool CheckExists()
    {
        return !IsFileSchema(EnteredPath) || Directory.Exists(EnteredPath);
    }

    private static bool IsFileSchema(string? path)
    {
        bool uriCreated = Uri.TryCreate(path, UriKind.Absolute, out Uri? uri);
        return !uriCreated || uri is null || uri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase);
    }
}
