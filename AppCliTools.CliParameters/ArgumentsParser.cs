using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AppCliTools.LibDataInput;
using Newtonsoft.Json;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters;

public sealed class ArgumentsParser<T> : IArgumentsParser<T> where T : class, IParameters, new()
{
    private readonly string _appName;

    private readonly List<string> _argsList = [];

    //private readonly string? _encKey;
    private readonly string _jsonFileName;
    private readonly ParametersLoader<T> _parLoader;
    private readonly string _pathToContentRoot = Directory.GetCurrentDirectory();
    private readonly string[] _possibleSwitches;

    public ArgumentsParser(IEnumerable<string> args, string appName, params string[] possibleSwitches)
    {
        _appName = appName;
        //_encKey = encKey;
        _possibleSwitches = possibleSwitches;
        _jsonFileName = $"{appName}.json";
        _argsList.AddRange(args);
        _parLoader = new ParametersLoader<T>();
    }

    public T? Par => (T?)_parLoader.Par;
    public string? ParametersFileName => _parLoader.ParametersFileName;
    public List<string> Switches { get; } = [];

    public EParseResult Analysis()
    {
        string? fileName = null;
        if (_argsList.Count > 0)
        {
            int useIndex = Array.FindIndex(_argsList.ToArray(), t => t.Equals("--use", StringComparison.Ordinal));

            if (useIndex + 1 < _argsList.Count)
            {
                fileName = _argsList[useIndex + 1];
                if (!AnalyzeParamFileName(_argsList[useIndex + 1]))
                {
                    return EParseResult.Error;
                }
            }

            var switches = new List<string>();
            if (useIndex > 0)
            {
                switches.AddRange(_argsList.Take(useIndex));
            }

            if (useIndex + 2 < _argsList.Count)
            {
                switches.AddRange(_argsList.Skip(useIndex + 2));
            }

            foreach (string swt in switches.Where(swt => _possibleSwitches.Contains(swt, StringComparer.Ordinal)))
            {
                Switches.Add(swt);
            }
        }

        if (Par != null)
        {
            return EParseResult.Ok;
        }

        if (fileName != null)
        {
            return EParseResult.Error;
        }

        //გამოვიტანოთ ინფორმაცია კონსოლზე
        Console.WriteLine("Usage:");
        Console.WriteLine($"{_appName} --use <file name for use as parameters json>");
        Console.WriteLine();
        return EParseResult.Usage;
    }

    private bool AnalyzeParamFileName(string? startFileName)
    {
        if (startFileName != null)
        {
            //_parLoader
            return TryUseFile(startFileName);
        }

        Console.WriteLine("file name is not specified");

        Console.WriteLine($"Try to use current Directory {_pathToContentRoot}");

        //_parLoader.
        if (TryUseFile(Path.Combine(_pathToContentRoot, _jsonFileName)) && Par != null)
        {
            return true;
        }

        Console.WriteLine("Try to use current Directory");
        // ReSharper disable once using
        using ProcessModule? processModule = Process.GetCurrentProcess().MainModule;

        if (processModule == null)
        {
            return true;
        }

        string pathToExe = processModule.FileName;
        string? pathToExeRoot = Path.GetDirectoryName(pathToExe);
        if (pathToExeRoot != null)
        {
            //_parLoader.
            return TryUseFile(Path.Combine(pathToExeRoot, _jsonFileName));
        }

        Console.WriteLine("Cannot detect executable file path");
        return false;
    }

    private bool TryUseFile(string startFileName)
    {
        _parLoader.ParametersFileName = startFileName;

        if (File.Exists(startFileName))
        {
            if (_parLoader.TryLoadParameters(startFileName))
            {
                return true;
            }

            Console.WriteLine($"File {startFileName} is not valid parameters file");

            return Inputer.InputBool($"File {startFileName} is Invalid, Create, rewrite and use file with this name?",
                false, false) && CreateEmptyParametersFile(startFileName);
        }

        StShared.WriteWarningLine($"File {startFileName} is not exists", true);

        var fileInfo = new FileInfo(startFileName);
        if (fileInfo.Directory == null)
        {
            StShared.WriteErrorLine($"Invalid file name {startFileName} for Parameters", true);
            return false;
        }

        if (!fileInfo.Directory.Exists)
        {
            fileInfo.Directory.Create();
        }

        if (fileInfo.Directory.Exists)
        {
            return Inputer.InputBool($"File {startFileName} is not exists, Create and use file with this name?", true,
                false) && CreateEmptyParametersFile(startFileName);
        }

        StShared.WriteErrorLine($"Cannot create folder {fileInfo.Directory.Name}", true);
        return false;
    }

    private static bool CreateEmptyParametersFile(string startFileName)
    {
        //შევქმნათ ცარელა პარამეტრები
        var sampleParams = new EmptyParameters();

        string sampleParamsJsonText = JsonConvert.SerializeObject(sampleParams);

        //if (_encKey != null)
        //{
        //    sampleParamsJsonText = EncryptDecrypt.EncryptString(sampleParamsJsonText, _encKey);
        //}

        //შევინახოთ ინფორმაცია SampleJsonFileName ფაილში 
        File.WriteAllText(startFileName, sampleParamsJsonText);
        Console.WriteLine($"New parameters saved to file {startFileName}");

        if (File.Exists(startFileName))
        {
            return true;
        }

        StShared.WriteWarningLine($"File {startFileName} steel does not exists", true);
        return false;
    }
}
