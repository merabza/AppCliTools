using AppCliTools.CliTools.ArchiverTools;
using ParametersManagement.LibFileParameters.Interfaces;
using ParametersManagement.LibFileParameters.Models;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersEdit.Generators;

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
        string archiverName = archiveType.ToString();
        if (parameters.Archivers.ContainsKey(archiverName))
        {
            return;
        }

        ArchiverData archiver;
        if (archiveType == EArchiveType.ZipClass)
        {
            archiver = new ArchiverData { Type = EArchiveType.ZipClass, FileExtension = ".zip" };
            parameters.Archivers.Add(archiverName, archiver);
            return;
        }

        ArchiverDetector? archiverDetector = ArchiverDetectorFactory.Create(useConsole, archiverName);
        ArchiverDetectorResults? archiverDetectorResults = archiverDetector?.Run();

        string? decompressProgramPatch = archiverDetectorResults?.DecompressProgramPatch;
        string? compressProgramPatch = archiverDetectorResults?.CompressProgramPatch;

        if (decompressProgramPatch == null || compressProgramPatch == null)
        {
            return;
        }

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
