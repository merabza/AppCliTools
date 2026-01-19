using CliParameters.FieldEditors;
using CliTools.ArchiverTools;
using LibMenuInput;
using ParametersManagement.LibFileParameters.Models;

namespace CliParametersEdit.FieldEditors;

public sealed class DecompressProgramPatchFieldEditor : FilePathFieldEditor
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public DecompressProgramPatchFieldEditor(string propertyName) : base(propertyName)
    {
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        var fileExtension = GetValue<string>(recordForUpdate, nameof(ArchiverData.FileExtension));
        var compressProgramPatch = GetValue<string>(recordForUpdate, nameof(ArchiverData.CompressProgramPatch));

        string? def = null;

        if (fileExtension is not null)
        {
            var archiverDetector = ArchiverDetectorFactory.Create(true, fileExtension);
            if (archiverDetector != null)
            {
                var archiverDetectorResults = archiverDetector.Run();
                def = archiverDetectorResults?.CompressProgramPatch ?? compressProgramPatch;
            }
        }

        SetValue(recordForUpdate, MenuInputer.InputFolderPath(FieldName, GetValue(recordForUpdate, def)));
    }
}
