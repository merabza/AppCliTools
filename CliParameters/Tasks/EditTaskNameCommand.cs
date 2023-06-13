//using System;
//using CliMenu;
//using SystemToolsShared;
//using LibDataInput;

//namespace CliParameters.Tasks;

//public sealed class EditTaskNameCommand : CliMenuCommand
//{
//    private readonly IParametersManager _parametersManager;
//    private readonly string _taskName;

//    public EditTaskNameCommand(IParametersManager parametersManager, string taskName) : base("Edit Task",
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

//            TaskModel task = parameters.TasksCol.GetTask(_taskName);
//            if (task == null)
//            {
//                StShared.WriteErrorLine($"Task with name {_taskName} is not found", true);
//                return false;
//            }

//            //ამოცანის სახელის რედაქტირება
//            TextDataInput nameInput = new TextDataInput("change  Task Name ", _taskName);
//            if (!nameInput.DoInput())
//                return false;
//            string newTaskName = nameInput.Text;

//            if (_taskName == newTaskName)
//                return false; //თუ ცვლილება მართლაც მოითხოვეს

//            if (!parameters.TasksCol.CheckNewTaskNameValid(_taskName, newTaskName))
//            {
//                StShared.WriteErrorLine($"New Name For Task {newTaskName} is not valid", true);
//                return false;
//            }

//            if (!parameters.TasksCol.RemoveTask(_taskName))
//            {
//                StShared.WriteErrorLine(
//                    $"Cannot change  Task with name {_taskName} to {newTaskName}, because cannot remove this  task",
//                    true);
//                return false;
//            }

//            if (!parameters.TasksCol.AddTask(newTaskName, task))
//            {
//                StShared.WriteErrorLine(
//                    $"Cannot change  Task with name {_taskName} to {newTaskName}, because cannot add this  task",
//                    true);
//                return false;
//            }

//            _parametersManager.Save(parameters, $" Task Renamed from {_taskName} To {newTaskName}");

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

