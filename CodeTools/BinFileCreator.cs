using System;
using System.IO;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace CodeTools;

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
        Logger.LogInformation($"create Code file started -> {binFileName}");
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
        var placePath = codePath ?? _placePath;
        StShared.CreateFolder(placePath, true);
        var forCreateFileName = Path.Combine(placePath, BinFile.FileName);
        if (string.IsNullOrWhiteSpace(BinFile.Base64String))
            throw new Exception("BinFile.Base64String is empty");
        File.WriteAllBytes(forCreateFileName, Convert.FromBase64String(BinFile.Base64String));
        Logger.LogInformation($"Bin file created: {forCreateFileName}");
    }
}