using System.Collections.Generic;
using System.Linq;
using CliMenu;
using CliParameters;
using CliParametersEdit.CliMenuCommands;
using CliParametersEdit.FieldEditors;
using LibFileParameters.Interfaces;
using LibFileParameters.Models;
using LibParameters;

namespace CliParametersEdit.Cruders;

public sealed class ArchiverCruder : ParCruder
{
    public ArchiverCruder(IParametersManager parametersManager) : base(parametersManager, "Archiver", "Archivers")
    {
        FieldEditors.Add(new ArchiverTypeFieldEditor(nameof(ArchiverData.Type), this));
        FieldEditors.Add(new ArchiverFileExtensionFieldEditor(nameof(ArchiverData.FileExtension)));
        FieldEditors.Add(new CompressProgramPatchFieldEditor(nameof(ArchiverData.CompressProgramPatch)));
        FieldEditors.Add(new DecompressProgramPatchFieldEditor(nameof(ArchiverData.DecompressProgramPatch)));
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        var parameters = (IParametersWithArchivers)ParametersManager.Parameters;
        return parameters.Archivers.ToDictionary(p => p.Key, p => (ItemData)p.Value);
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        var parameters = (IParametersWithArchivers)ParametersManager.Parameters;
        var archivers = parameters.Archivers;
        return archivers.ContainsKey(recordKey);
    }

    public override void UpdateRecordWithKey(string recordKey, ItemData newRecord)
    {
        var newArchiver = (ArchiverData)newRecord;
        var parameters = (IParametersWithArchivers)ParametersManager.Parameters;
        parameters.Archivers[recordKey] = newArchiver;
    }

    protected override void AddRecordWithKey(string recordKey, ItemData newRecord)
    {
        var newArchiver = (ArchiverData)newRecord;
        var parameters = (IParametersWithArchivers)ParametersManager.Parameters;
        parameters.Archivers.Add(recordKey, newArchiver);
    }

    protected override void RemoveRecordWithKey(string recordKey)
    {
        var parameters = (IParametersWithArchivers)ParametersManager.Parameters;
        var archivers = parameters.Archivers;
        archivers.Remove(recordKey);
    }

    protected override ItemData CreateNewItem(ItemData? defaultItemData)
    {
        return new ArchiverData();
    }

    protected override void FillListMenuAdditional(CliMenuSet cruderSubMenuSet)
    {
        GenerateArchiversCliMenuCommand generateCommand = new(ParametersManager);
        cruderSubMenuSet.AddMenuItem(generateCommand);
    }
}