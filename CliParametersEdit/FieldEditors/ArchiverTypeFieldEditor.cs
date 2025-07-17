using CliParameters.Cruders;
using CliParameters.FieldEditors;
using LibFileParameters.Models;
using SystemToolsShared;

// ReSharper disable ConvertToPrimaryConstructor

namespace CliParametersEdit.FieldEditors;

public sealed class ArchiverTypeFieldEditor : EnumFieldEditor<EArchiveType>
{
    private readonly Cruder _cruder;

    public ArchiverTypeFieldEditor(string propertyName, Cruder cruder) : base(propertyName, EArchiveType.ZipClass)
    {
        _cruder = cruder;
    }

    public override void UpdateField(string? recordKey, object recordForUpdate) //, object currentRecord
    {
        base.UpdateField(recordKey, recordForUpdate);
        var currentArchiveType = GetValue(recordForUpdate);
        var enable = currentArchiveType != EArchiveType.ZipClass;
        _cruder.EnableFieldByName(nameof(ArchiverData.CompressProgramPatch), enable);
        _cruder.EnableFieldByName(nameof(ArchiverData.DecompressProgramPatch), enable);
    }
}