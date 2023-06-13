//using System;
//using CliMenu;
//using SystemToolsShared;
//using LibDataInput;
//using Microsoft.Extensions.Logging;

//namespace CliParameters.Tasks;

//public sealed class TaskCommand : CliMenuCommand
//{

//    private readonly ILogger _logger;

//    private readonly IParametersManager _parametersManager;

//    private readonly TaskRunner _taskRunner;

//    public TaskCommand(ILogger logger, IParametersManager parametersManager, TaskRunner taskRunner)
//    {
//        _logger = logger;
//        _parametersManager = parametersManager;
//        _taskRunner = taskRunner;
//    }

//    public override bool Run()
//    {

//        try
//        {

//            MenuAction = EMenuAction.Reload;

//            //დავინიშნოთ დრო
//            DateTime startDateTime = DateTime.Now;
//            Console.WriteLine("Task is running...");
//            Console.WriteLine("---");

//            bool success = _taskRunner.Run();

//            Console.WriteLine("---");

//            Console.WriteLine($"Task Finished. {StShared.TimeTakenMessage(startDateTime)}");
//            StShared.Pause();
//            return success;
//        }
//        catch (DataInputEscapeException)
//        {
//            Console.WriteLine();
//            Console.WriteLine("Escape... ");
//            StShared.Pause();
//            return false;
//        }
//        catch (Exception e)
//        {
//            StShared.WriteException(e, true);
//            StShared.Pause();
//            return false;
//        }
//    }

//}

