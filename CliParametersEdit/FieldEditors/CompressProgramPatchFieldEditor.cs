using CliParameters.FieldEditors;
using CliTools.ArchiverTools;
using LibFileParameters.Models;
using LibMenuInput;

namespace CliParametersEdit.FieldEditors;

public sealed class CompressProgramPatchFieldEditor : FilePathFieldEditor
{
    public CompressProgramPatchFieldEditor(string propertyName) : base(propertyName)
    {
    }

    public override void UpdateField(string? recordKey, object recordForUpdate)
    {
        var fileExtension = GetValue<string>(recordForUpdate, nameof(ArchiverData.FileExtension));


        string? def = null;
        ArchiverDetector? archiverDetector = null;
        if (fileExtension is not null) archiverDetector = ArchiverDetectorFabric.Create(true, fileExtension);

        if (archiverDetector is not null)
        {
            var archiverDetectorResults = archiverDetector.Run();
            def = archiverDetectorResults?.CompressProgramPatch;
        }

        SetValue(recordForUpdate, MenuInputer.InputFolderPath(FieldName, GetValue(recordForUpdate, def)));
    }
}