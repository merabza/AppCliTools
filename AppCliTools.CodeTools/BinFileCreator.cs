using System;
using System.IO;
using Microsoft.Extensions.Logging;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CodeTools;

public /*open*/ class BinFileCreator
{
    private readonly string _placePath;
    protected readonly BinFile BinFile;

    protected readonly ILogger Logger;

    public BinFileCreator(ILogger logger, string placePath, string binFileName)
    {
        Logger = logger;
        _placePath = placePath;
        BinFile = new BinFile(binFileName);
        if (Logger.IsEnabled(LogLevel.Information))
        {
            Logger.LogInformation("create Code file started -> {BinFileName}", binFileName);
        }
    }

    public virtual void CreateFileData()
    {
    }

    public virtual void FinishAndSave()
    {
        CreateFile();
    }

    protected void CreateFile(string? codePath = null)
    {
        //var strCode = CodeFile.Output(-1);
        string placePath = codePath ?? _placePath;
        StShared.CreateFolder(placePath, true);
        string forCreateFileName = Path.Combine(placePath, BinFile.FileName);
        if (string.IsNullOrWhiteSpace(BinFile.Base64String))
        {
            throw new Exception("BinFile.Base64String is empty");
        }

        File.WriteAllBytes(forCreateFileName, Convert.FromBase64String(BinFile.Base64String));
        if (Logger.IsEnabled(LogLevel.Information))
        {
            Logger.LogInformation("Bin file created: {ForCreateFileName}", forCreateFileName);
        }
    }
}
