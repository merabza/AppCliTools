using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliTools.ArchiverTools;
using AppCliTools.LibMenuInput;
using ParametersManagement.LibFileParameters.Models;

namespace AppCliTools.CliParametersEdit.FieldEditors;

public sealed class CompressProgramPatchFieldEditor : FilePathFieldEditor
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public CompressProgramPatchFieldEditor(string propertyName) : base(propertyName)
    {
    }

    public override ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        string? fileExtension = GetValue<string>(recordForUpdate, nameof(ArchiverData.FileExtension));

        string? def = null;
        ArchiverDetector? archiverDetector = null;
        if (fileExtension is not null)
        {
            archiverDetector = ArchiverDetectorFactory.Create(true, fileExtension);
        }

        if (archiverDetector is not null)
        {
            ArchiverDetectorResults? archiverDetectorResults = archiverDetector.Run();
            def = archiverDetectorResults?.CompressProgramPatch;
        }

        SetValue(recordForUpdate, MenuInputer.InputFolderPath(FieldName, GetValue(recordForUpdate, def)));
        return ValueTask.CompletedTask;
    }
}
