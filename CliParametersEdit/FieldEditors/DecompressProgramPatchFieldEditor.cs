using CliParameters.FieldEditors;
using CliTools.ArchiverTools;
using LibFileParameters.Models;
using LibMenuInput;

namespace CliParametersEdit.FieldEditors;

public sealed class DecompressProgramPatchFieldEditor : FilePathFieldEditor
{
    public DecompressProgramPatchFieldEditor(string propertyName) : base(propertyName)
    {
    }

    public override void UpdateField(string? recordName, object recordForUpdate)
    {
        var fileExtension = GetValue<string>(recordForUpdate, nameof(ArchiverData.FileExtension));
        var compressProgramPatch = GetValue<string>(recordForUpdate, nameof(ArchiverData.CompressProgramPatch));

        string? def = null;

        if (fileExtension is not null)
        {
            var archiverDetector = ArchiverDetectorFabric.Create(true, fileExtension);
            if (archiverDetector != null)
            {
                var archiverDetectorResults = archiverDetector.Run();
                def = archiverDetectorResults?.CompressProgramPatch ?? compressProgramPatch;
            }
        }

        SetValue(recordForUpdate, MenuInputer.InputFolderPath(FieldName, GetValue(recordForUpdate, def)));
    }
}