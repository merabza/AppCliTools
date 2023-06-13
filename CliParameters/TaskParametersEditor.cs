//using System;
//using CliParameters.Models;

//namespace CliParameters;

//public sealed class TaskParametersEditor : ParametersEditor
//{
//    private readonly ParametersTaskInfo? _parametersTaskInfo;

//    protected TaskParametersEditor(string name, IParameters? parameters,
//        ParametersTaskInfo? parametersTaskInfo) : base(name,
//        new ParametersManager(parametersTaskInfo?.ParametersFileName, parameters))
//    {
//        _parametersTaskInfo = parametersTaskInfo;
//    }

//    internal bool DeleteTask()
//    {
//        return false;
//    }

//    protected override string GetMainMenuCaption()
//    {
//        return
//            $"{Name} {Environment.NewLine} Project: {_parametersTaskInfo?.AppName} {Environment.NewLine} Task: {_parametersTaskInfo?.TaskName} {Environment.NewLine} Parameters File Name: {_parametersTaskInfo?.ParametersFileName}";
//    }

//    //ყველა პარამეტრის რედაქტირება თანმიმდევრობით
//    internal override bool EditParametersInSequence(string parentMenuName)
//    {

//        //პარამეტრების შეცვლის პროცესი დაიწყო
//        Console.WriteLine(
//            $"Edit parameters for project {_parametersTaskInfo?.AppName} and for task {_parametersTaskInfo?.TaskName} started");

//        //თუ შესაძლებელია დარედაქტირდეს ამოცანის სახელი
//        //_editTaskNameCommand?.Run();

//        //შეიცვალოს პარამეტრები თანმიმდევრობით
//        InputParametersData();

//        //პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
//        Save($"Project => {_parametersTaskInfo?.AppName} Task => {_parametersTaskInfo?.TaskName} Updated");

//        //ყველაფერი კარგად დასრულდა
//        return true;
//    }

//    public override string GetSaveMessage()
//    {
//        return
//            $"Project: {_parametersTaskInfo?.AppName} {Environment.NewLine} Task: {_parametersTaskInfo?.TaskName} {Environment.NewLine} saved To File: {_parametersTaskInfo?.ParametersFileName} ";
//    }


//}

