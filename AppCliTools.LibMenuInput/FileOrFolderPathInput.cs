using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibMenuInput;

public sealed class FileOrFolderPathInput : PathInput
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public FileOrFolderPathInput(string fieldName, string? defaultValue = null, bool warningIfFileDoesNotExists = true)
        : base(fieldName, true, defaultValue, warningIfFileDoesNotExists)
    {
    }

    public string? FileOrFolderPath => EnteredPath;

    protected override void AddFiles(DirectoryInfo dir, string fileName, List<string> names)
    {
        List<string> fileNames = dir.GetFiles($"{fileName}*").Select(s => s.Name).ToList();
        names.AddRange(fileNames);
    }

    protected override bool CheckExists()
    {
        return File.Exists(EnteredPath) || !IsFileSchema(EnteredPath) || Directory.Exists(EnteredPath);
    }

    private static bool IsFileSchema(string? path)
    {
        bool uriCreated = Uri.TryCreate(path, UriKind.Absolute, out Uri? uri);
        return !uriCreated || uri is null || uri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase);
    }
}
