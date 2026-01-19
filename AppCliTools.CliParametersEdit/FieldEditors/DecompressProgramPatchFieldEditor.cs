using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliTools.ArchiverTools;
using AppCliTools.LibMenuInput;
using ParametersManagement.LibFileParameters.Models;

namespace AppCliTools.CliParametersEdit.FieldEditors;

public sealed class DecompressProgramPatchFieldEditor : FilePathFieldEditor
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public DecompressProgramPatchFieldEditor(string propertyName) : base(propertyName)
    {
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        string? fileExtension = GetValue<string>(recordForUpdate, nameof(ArchiverData.FileExtension));
        string? compressProgramPatch = GetValue<string>(recordForUpdate, nameof(ArchiverData.CompressProgramPatch));

        string? def = null;

        if (fileExtension is not null)
        {
            ArchiverDetector? archiverDetector = ArchiverDetectorFactory.Create(true, fileExtension);
            if (archiverDetector != null)
            {
                ArchiverDetectorResults? archiverDetectorResults = archiverDetector.Run();
                def = archiverDetectorResults?.CompressProgramPatch ?? compressProgramPatch;
            }
        }

        SetValue(recordForUpdate, MenuInputer.InputFolderPath(FieldName, GetValue(recordForUpdate, def)));
    }
}
