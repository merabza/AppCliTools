using System.Collections.Generic;
using CliMenu;
using CliParameters;
using CliParametersEdit.CliMenuCommands;
using CliParametersEdit.FieldEditors;
using LibFileParameters.Models;
using LibParameters;

namespace CliParametersEdit.Cruders;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ArchiverCruder : ParCruder<ArchiverData>
{
    //public კონსტრუქტორი საჭიროა. გამოიყენება რეფლექსიით DictionaryFieldEditor-ში
    // ReSharper disable once MemberCanBePrivate.Global
    public ArchiverCruder(IParametersManager parametersManager,
        Dictionary<string, ArchiverData> currentValuesDictionary) : base(parametersManager, currentValuesDictionary,
        "Archiver", "Archivers")
    {
        FieldEditors.Add(new ArchiverTypeFieldEditor(nameof(ArchiverData.Type), this));
        FieldEditors.Add(new ArchiverFileExtensionFieldEditor(nameof(ArchiverData.FileExtension)));
        FieldEditors.Add(new CompressProgramPatchFieldEditor(nameof(ArchiverData.CompressProgramPatch)));
        FieldEditors.Add(new DecompressProgramPatchFieldEditor(nameof(ArchiverData.DecompressProgramPatch)));
    }

    //public static ArchiverCruder Create(IParametersManager parametersManager)
    //{
    //    var parameters = (IParametersWithArchivers)parametersManager.Parameters;
    //    return new ArchiverCruder(parametersManager, parameters.Archivers);
    //}

    protected override void FillListMenuAdditional(CliMenuSet cruderSubMenuSet)
    {
        var generateCommand = new GenerateArchiversCliMenuCommand(ParametersManager);
        cruderSubMenuSet.AddMenuItem(generateCommand);
    }
}