using CliTools.ArchiverTools;
using LibFileParameters.Interfaces;
using LibFileParameters.Models;
using LibParameters;
using SystemToolsShared;

namespace CliParametersEdit.Generators;

public static class StandardArchiversGenerator
{
    public static void Generate(bool useConsole, IParametersManager parametersManager)
    {
        var parameters = (IParametersWithArchivers)parametersManager.Parameters;

        //თუ არ არსებობს შეიქმნას zipClass არქივატორი
        CreateArchiverZip(useConsole, parameters, EArchiveType.ZipClass); //ZipClass

        //თუ არ არსებობს შეიქმნას zip არქივატორი
        CreateArchiverZip(useConsole, parameters, EArchiveType.Zip); //Zip

        //თუ არ არსებობს შეიქმნას zip არქივატორი
        CreateArchiverZip(useConsole, parameters, EArchiveType.Rar); //Rar
    }

    private static void CreateArchiverZip(bool useConsole, IParametersWithArchivers parameters,
        EArchiveType archiveType)
    {
        var archiverName = archiveType.ToString();
        if (parameters.Archivers.ContainsKey(archiverName))
            return;

        ArchiverData archiver;
        if (archiveType == EArchiveType.ZipClass)
        {
            archiver = new ArchiverData { Type = EArchiveType.ZipClass, FileExtension = ".zip" };
            parameters.Archivers.Add(archiverName, archiver);
            return;
        }

        var archiverDetector = ArchiverDetectorFactory.Create(useConsole, archiverName);
        var archiverDetectorResults = archiverDetector?.Run();

        var decompressProgramPatch = archiverDetectorResults?.DecompressProgramPatch;
        var compressProgramPatch = archiverDetectorResults?.CompressProgramPatch;

        if (decompressProgramPatch == null || compressProgramPatch == null)
            return;

        archiver = new ArchiverData
        {
            Type = archiveType,
            FileExtension = ".zip",
            DecompressProgramPatch = decompressProgramPatch,
            CompressProgramPatch = compressProgramPatch
        };
        parameters.Archivers.Add(archiverName, archiver);
    }
}