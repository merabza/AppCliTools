//using System;
//using CliMenu;
//using SystemToolsShared;
//using LibDataInput;
//using Microsoft.Extensions.Logging;

//namespace CliParameters.Tasks;

//public sealed class TaskRunCommand<TE> : CliMenuCommand where TE : struct, Enum
//{

//    private readonly ILogger _logger;

//    private readonly IParametersManager _parametersManager;

//    private readonly string _taskName;

//    public TaskRunCommand(ILogger logger, IParametersManager parametersManager, string taskName)
//    {
//        _logger = logger;
//        _parametersManager = parametersManager;
//        _taskName = taskName;
//    }

//    public override bool Run()
//    {

//        try
//        {

//            MenuAction = EMenuAction.Reload;
//            ParametersManagerWithTasks<TE> parMan = (ParametersManagerWithTasks<TE>)_parametersManager;
//            IParametersWithTasks parameters = (IParametersWithTasks)_parametersManager.Parameters;
//            TaskModel taskModel = parameters.TasksCol.GetTask(_taskName);

//            if (taskModel == null)
//            {
//                StShared.WriteErrorLine($"Task with name {_taskName} is not found", true);
//                return false;
//            }

//            string taskTypeName = taskModel.TaskTypeName;
//            if (!Enum.TryParse(taskTypeName, out TE tool))
//            {
//                StShared.WriteErrorLine($"Tool type name {taskTypeName} is not valid", true, _logger);
//                return false;
//            }

//            IToolCommand toolCommand = parMan.CreateToolCommand(_logger, tool, _taskName);

//            if (toolCommand.Par == null)
//            {
//                Console.WriteLine("Parameters not loaded. Tool not started.");
//                StShared.Pause();
//                return false;
//            }

//            //დავინიშნოთ დრო
//            DateTime startDateTime = DateTime.Now;
//            Console.WriteLine("Task is running...");
//            Console.WriteLine("---");

//            bool success = toolCommand.Run();

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

