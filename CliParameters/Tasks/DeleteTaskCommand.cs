//using System;
//using System.Collections.Generic;
//using CliMenu;
//using SystemToolsShared;
//using LibDataInput;

//namespace CliParameters.Tasks;

//public sealed class DeleteTaskCommand : CliMenuCommand
//{
//    private readonly IParametersManager _parametersManager;
//    private readonly string _taskName;

//    public DeleteTaskCommand(IParametersManager parametersManager, string taskName) : base("Delete Task",
//        taskName)
//    {
//        _parametersManager = parametersManager;
//        _taskName = taskName;
//    }

//    public override bool Run()
//    {
//        try
//        {

//            IParametersWithTasks parameters = (IParametersWithTasks)_parametersManager.Parameters;
//            if (parameters == null)
//            {
//                StShared.WriteErrorLine("Usb Copy Parameters not found", true);
//                return false;
//            }

//            if (_taskName == null)
//            {
//                StShared.WriteErrorLine(" Task Name is null", true);
//                return false;
//            }

//            Dictionary<string, TaskModel> tasks = parameters.TasksCol.Tasks;

//            if (tasks == null || !tasks.ContainsKey(_taskName))
//            {
//                StShared.WriteErrorLine($"Task {_taskName} not found", true);
//                return false;
//            }

//            if (!Inputer.InputBool($"This will Delete  Task {_taskName}. are you sure?", false, false))
//                return false;

//            tasks.Remove(_taskName);
//            _parametersManager.Save(parameters, $"Task {_taskName} deleted.");

//            MenuAction = EMenuAction.LevelUp;
//            return true;

//        }
//        catch (DataInputEscapeException)
//        {
//            Console.WriteLine();
//            Console.WriteLine("Escape... ");
//            StShared.Pause();
//        }
//        catch (Exception e)
//        {
//            StShared.WriteException(e, true);
//        }

//        MenuAction = EMenuAction.Reload;
//        return false;
//    }

//}

