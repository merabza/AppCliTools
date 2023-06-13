using CliTools.ArchiverTools;
using LibFileParameters.Interfaces;
using LibFileParameters.Models;
using LibParameters;
using SystemToolsShared;

namespace CliParametersEdit.Generators;

public sealed class StandardArchiversGenerator
{
    private readonly IParametersManager _parametersManager;
    private readonly bool _useConsole;


    public StandardArchiversGenerator(bool useConsole, IParametersManager parametersManager)
    {
        _useConsole = useConsole;
        _parametersManager = parametersManager;
    }

    public string? ArchiverZipClassName { get; private set; } //ZipClass
    public string? ArchiverZipName { get; private set; } //Zip
    public string? ArchiverRarName { get; private set; } //Rar

    public void Generate()
    {
        var parameters = (IParametersWithArchivers)_parametersManager.Parameters;

        //თუ არ არსებობს შეიქმნას zipClass არქივატორი
        ArchiverZipClassName = CreateArchiverZip(_useConsole, parameters, EArchiveType.ZipClass); //ZipClass

        //თუ არ არსებობს შეიქმნას zip არქივატორი
        ArchiverZipName = CreateArchiverZip(_useConsole, parameters, EArchiveType.Zip); //Zip

        //თუ არ არსებობს შეიქმნას zip არქივატორი
        ArchiverRarName = CreateArchiverZip(_useConsole, parameters, EArchiveType.Rar); //Rar
    }

    private static string? CreateArchiverZip(bool useConsole, IParametersWithArchivers parameters,
        EArchiveType archiveType)
    {
        var archiverName = archiveType.ToString();
        if (parameters.Archivers.ContainsKey(archiverName))
            return archiverName;

        ArchiverData archiver;
        if (archiveType == EArchiveType.ZipClass)
        {
            archiver = new ArchiverData { Type = EArchiveType.ZipClass, FileExtension = ".zip" };
            parameters.Archivers.Add(archiverName, archiver);
            return archiverName;
        }

        var archiverDetector = ArchiverDetectorFabric.Create(useConsole, archiverName);
        var archiverDetectorResults = archiverDetector?.Run();

        var decompressProgramPatch = archiverDetectorResults?.DecompressProgramPatch;
        var compressProgramPatch = archiverDetectorResults?.CompressProgramPatch;

        if (decompressProgramPatch == null || compressProgramPatch == null)
            return null;

        archiver = new ArchiverData
        {
            Type = archiveType, FileExtension = ".zip", DecompressProgramPatch = decompressProgramPatch,
            CompressProgramPatch = compressProgramPatch
        };
        parameters.Archivers.Add(archiverName, archiver);
        return archiverName;
    }
}