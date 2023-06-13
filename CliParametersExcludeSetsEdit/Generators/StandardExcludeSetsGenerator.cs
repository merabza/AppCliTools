using System.Collections.Generic;
using System.IO;
using LibFileParameters.Interfaces;
using LibFileParameters.Models;
using LibParameters;

namespace CliParametersExcludeSetsEdit.Generators;

public sealed class StandardExcludeSetsGenerator
{
    private readonly IParametersManager _parametersManager;

    public StandardExcludeSetsGenerator(IParametersManager parametersManager)
    {
        _parametersManager = parametersManager;
    }

    public void Generate()
    {
        var parameters = (IParametersWithExcludeSets)_parametersManager.Parameters;

        var gitBinObjExcludeSet = new ExcludeSet
        {
            FolderFileMasks = new List<string>
            {
                $"*{Path.DirectorySeparatorChar}.git{Path.DirectorySeparatorChar}*",
                $"*{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}*",
                $"*{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}*",
                $"*{Path.DirectorySeparatorChar}Debug{Path.DirectorySeparatorChar}*",
                $"*{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}*",
                $"*{Path.DirectorySeparatorChar}packages{Path.DirectorySeparatorChar}*",
                $"*{Path.DirectorySeparatorChar}.vs{Path.DirectorySeparatorChar}*",
                $"*{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}*",
                $"*{Path.DirectorySeparatorChar}Temp{Path.DirectorySeparatorChar}*",
                $"*{Path.DirectorySeparatorChar}temp{Path.DirectorySeparatorChar}*"
            }
        };

        //თუ არ არსებობს შეიქმნას GitBinObj სტანდარტული სიმრავლე
        CreateExcludeSet(parameters, "GitBinObj", gitBinObjExcludeSet); //GitBinObj

        var upDownTempExcludeSet = new ExcludeSet
        {
            FolderFileMasks = new List<string>
            {
                "*.up!",
                "*.down!",
                "*.go!",
                "*.crdownload"
            }
        };

        //თუ არ არსებობს შეიქმნას UpDownTemp სტანდარტული სიმრავლე
        CreateExcludeSet(parameters, "UpDownTemp", upDownTempExcludeSet); //UpDownTemp

        var upDownTempZipExcludeSet = new ExcludeSet
        {
            FolderFileMasks = new List<string>
            {
                "*.up!",
                "*.down!",
                "*.go!",
                "*.crdownload",
                "*.zip"
            }
        };

        //თუ არ არსებობს შეიქმნას UpDownTemp სტანდარტული სიმრავლე
        CreateExcludeSet(parameters, "UpDownTempZip", upDownTempZipExcludeSet); //UpDownTemp
    }

    private static void CreateExcludeSet(IParametersWithExcludeSets parameters, string excludeSetName,
        ExcludeSet excludeSet)
    {
        if (parameters.ExcludeSets.ContainsKey(excludeSetName))
            return;

        if (!parameters.ExcludeSets.ContainsKey(excludeSetName))
            parameters.ExcludeSets.Add(excludeSetName, excludeSet);
    }
}