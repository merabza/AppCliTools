using System.Globalization;
using CliParameters.FieldEditors;
using LibDataInput;
using ParametersManagement.LibFileParameters.Models;
using SystemTools.SystemToolsShared;

namespace CliParametersEdit.FieldEditors;

public sealed class ArchiverFileExtensionFieldEditor : TextFieldEditor
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ArchiverFileExtensionFieldEditor(string propertyName) : base(propertyName)
    {
    }

    public override void UpdateField(string? recordKey, object recordForUpdate) //, object currentRecord
    {
        var archiveType = GetValue<EArchiveType>(recordForUpdate, nameof(ArchiverData.Type));

        var extensionCandidate = archiveType.ToString();
        extensionCandidate =
            (extensionCandidate.Length > 2 ? extensionCandidate[..3] : extensionCandidate).ToLower(CultureInfo
                .CurrentCulture);

        SetValue(recordForUpdate, Inputer.InputText(FieldName, GetValue(recordForUpdate, extensionCandidate)));
    }
}
