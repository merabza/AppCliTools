using System;
using System.Diagnostics;
using System.IO;
using AppCliTools.LibDataInput;
using Newtonsoft.Json;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters;

public sealed class ParametersService<T> : IParametersService<T> where T : class, IParameters, new()
{
    private readonly Func<string, bool, bool> _inputBool;

    //private readonly string? _encKey;
    private readonly string _jsonFileName;
    private readonly ParametersLoader<T> _parLoader;
    private readonly string _pathToContentRoot = Directory.GetCurrentDirectory();

    public ParametersService(string appName) : this(appName,
        (fieldName, defaultValue) => Inputer.InputBool(fieldName, defaultValue, false))
    {
    }

    //კონსოლიდან შეყვანა პარამეტრებადაა გამოტანილი, რომ ტესტებმა პასუხები თვითონ მიაწოდონ
    // ReSharper disable once ConvertToPrimaryConstructor
    internal ParametersService(string appName, Func<string, bool, bool> inputBool)
    {
        //_encKey = encKey;
        _jsonFileName = $"{appName}.json";
        _parLoader = new ParametersLoader<T>();
        _inputBool = inputBool;
    }

    public T? Par => (T?)_parLoader.Par;
    public string? ParametersFileName => _parLoader.ParametersFileName;

    public EParseResult Analysis(string? parametersFileName)
    {
        //თუ ფაილის სახელი მითითებული არ არის, ჯერ მიმდინარე,
        //შემდეგ კი გამშვები ფაილის ფოლდერში ვეძებთ {appName}.json ფაილს
        if (AnalyzeParamFileName(parametersFileName) && Par != null)
        {
            return EParseResult.Ok;
        }

        return parametersFileName != null ? EParseResult.ParseError : EParseResult.ShowHelp;
    }

    //Analysis-ის მიერ null სახელით გამოძახებისას მიმდინარე და გამშვები ფაილის ფოლდერები მოწმდება
    internal bool AnalyzeParamFileName(string? startFileName)
    {
        if (startFileName != null)
        {
            //_parLoader
            return TryUseFile(startFileName);
        }

        Console.WriteLine("file name is not specified");

        Console.WriteLine($"Try to use current Directory {_pathToContentRoot}");

        //_parLoader.
        if (TryUseFile(Path.Combine(_pathToContentRoot, _jsonFileName), false) && Par != null)
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
            return TryUseFile(Path.Combine(pathToExeRoot, _jsonFileName), false);
        }

        Console.WriteLine("Cannot detect executable file path");
        return false;
    }

    //offerToCreate == false: ავტომატური ძებნისას ფაილი მხოლოდ მოიძებნება, შექმნა არ შემოთავაზდება
    internal bool TryUseFile(string startFileName, bool offerToCreate = true)
    {
        _parLoader.ParametersFileName = startFileName;

        if (File.Exists(startFileName))
        {
            if (_parLoader.TryLoadParameters(startFileName))
            {
                return true;
            }

            Console.WriteLine($"File {startFileName} is not valid parameters file");

            return offerToCreate &&
                   _inputBool($"File {startFileName} is Invalid, Create, rewrite and use file with this name?",
                       false) && CreateEmptyParametersFile(startFileName);
        }

        StShared.WriteWarningLine($"File {startFileName} is not exists", true);

        if (!offerToCreate)
        {
            return false;
        }

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
            return _inputBool($"File {startFileName} is not exists, Create and use file with this name?", true) &&
                   CreateEmptyParametersFile(startFileName);
        }

        StShared.WriteErrorLine($"Cannot create folder {fileInfo.Directory.Name}", true);
        return false;
    }

    internal static bool CreateEmptyParametersFile(string startFileName)
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
