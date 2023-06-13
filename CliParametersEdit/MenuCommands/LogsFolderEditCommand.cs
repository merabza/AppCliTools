//using System.IO;
//using CliMenu;
//using CliParameters;
//using LibDataInput;

//namespace CliParametersEdit.MenuCommands;

//public sealed class LogsFolderEditCommand : CliMenuCommand
//{
//    private readonly ParametersManager _parametersManager;

//    public LogsFolderEditCommand(ParametersManager parametersManager) : base("Logs Folder")
//    {
//        _parametersManager = parametersManager;
//    }


//    public override bool Run()
//    {
//        FolderPathInput folderInput = new FolderPathInput(Name, GetLogsFolder());
//        if (!folderInput.DoInput())
//            return false;
//        IParameters parameters = _parametersManager.Parameters;
//        if (parameters is not IParametersWithLog parametersWithLog)
//            return false;
//        parametersWithLog.LogFolder = folderInput.FolderPath;
//        _parametersManager.Save(_parametersManager.Parameters, "Log Folder Updated");

//        MenuAction = EMenuAction.Reload;
//        return true;
//    }

//    public override CliMenuSet GetSubmenu()
//    {
//        return null;
//    }


//    private string GetLogsFolder()
//    {
//        IParameters parameters = _parametersManager.Parameters;
//        FileInfo pf = new FileInfo(_parametersManager.ParametersFileName);
//        if (parameters is IParametersWithLog parametersWithLog)
//            return parametersWithLog.LogFolder ?? pf.Directory?.FullName;
//        return null;
//    }


//    protected override string GetStatus()
//    {
//        return GetLogsFolder();
//    }

//}

