using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.LibDataInput;
using ParametersManagement.LibFileParameters.Models;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersEdit.FieldEditors;

public sealed class ArchiverFileExtensionFieldEditor : TextFieldEditor
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ArchiverFileExtensionFieldEditor(string propertyName) : base(propertyName)
    {
    }

    public override ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        var archiveType = GetValue<EArchiveType>(recordForUpdate, nameof(ArchiverData.Type));

        string extensionCandidate = archiveType.ToString();
        extensionCandidate =
            (extensionCandidate.Length > 2 ? extensionCandidate[..3] : extensionCandidate).ToLower(CultureInfo
                .CurrentCulture);

        SetValue(recordForUpdate, Inputer.InputText(FieldName, GetValue(recordForUpdate, extensionCandidate)));
        return ValueTask.CompletedTask;
    }
}
