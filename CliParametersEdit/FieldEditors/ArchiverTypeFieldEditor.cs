using CliParameters;
using CliParameters.FieldEditors;
using LibFileParameters.Models;
using SystemToolsShared;

namespace CliParametersEdit.FieldEditors;

public sealed class ArchiverTypeFieldEditor : EnumFieldEditor<EArchiveType>
{
    private readonly Cruder _cruder;

    public ArchiverTypeFieldEditor(string propertyName, Cruder cruder) : base(propertyName, EArchiveType.ZipClass)
    {
        _cruder = cruder;
    }

    public override void UpdateField(string? recordName, object recordForUpdate) //, object currentRecord
    {
        base.UpdateField(recordName, recordForUpdate);
        var currentArchiveType = GetValue(recordForUpdate);
        var enable = currentArchiveType != EArchiveType.ZipClass;
        _cruder.EnableFieldByName(nameof(ArchiverData.CompressProgramPatch), enable);
        _cruder.EnableFieldByName(nameof(ArchiverData.DecompressProgramPatch), enable);
    }
}