// ReSharper disable ConvertToPrimaryConstructor

using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParameters.FieldEditors;
using ParametersManagement.LibFileParameters.Models;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersEdit.FieldEditors;

public sealed class ArchiverTypeFieldEditor : EnumFieldEditor<EArchiveType>
{
    private readonly Cruder _cruder;

    public ArchiverTypeFieldEditor(string propertyName, Cruder cruder) : base(propertyName, EArchiveType.ZipClass)
    {
        _cruder = cruder;
    }

    public override async ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        await base.UpdateField(recordKey, recordForUpdate, cancellationToken);
        EArchiveType currentArchiveType = GetValue(recordForUpdate);
        bool enable = currentArchiveType != EArchiveType.ZipClass;
        _cruder.EnableFieldByName(nameof(ArchiverData.CompressProgramPatch), enable);
        _cruder.EnableFieldByName(nameof(ArchiverData.DecompressProgramPatch), enable);
    }
}
