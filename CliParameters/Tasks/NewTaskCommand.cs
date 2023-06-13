//using System;
//using System.Linq;
//using CliMenu;
//using SystemToolsShared;
//using LibDataInput;
//using Microsoft.Extensions.Logging;

//namespace CliParameters.Tasks;

//public sealed class NewTaskCommand<TE> : CliMenuCommand where TE : struct, Enum
//{
//    private readonly ILogger _logger;
//    private readonly IParametersManager _parametersManager;

//    //ახალი აპლიკაციის ამოცანის შექმნა
//    public NewTaskCommand(ILogger logger, IParametersManager parametersManager) :
//        base("New Task")
//    {
//        _logger = logger;
//        _parametersManager = parametersManager;
//    }


//    public override bool Run()
//    {
//        MenuAction = EMenuAction.Reload;
//        try
//        {

//            ParametersManagerWithTasks<TE> parMan = (ParametersManagerWithTasks<TE>)_parametersManager;
//            IParametersWithTasks parameters = (IParametersWithTasks)_parametersManager.Parameters;

//            //ამოცანის შექმნის პროცესი დაიწყო
//            Console.WriteLine("Create new Task started");

//            //ამოცანის ტიპის არჩევა
//            SelectFromListInput appNameInput = new SelectFromListInput("Task Type Name",
//                parMan.GetStandAloneTools().Select(x => x.ToString()).OrderBy(s => s).ToList());

//            if (!appNameInput.DoInput())
//                return false;
//            string taskTypeName = appNameInput.Text;

//            if (!Enum.TryParse(taskTypeName, out TE _))
//            {
//                Console.WriteLine($"Invalid Task Type Name {taskTypeName}");
//                return false;
//            }

//            //ახალი ამოცანის სახელის შეტანა პროგრამაში
//            TextDataInput newAppTaskNameInput = new("New Task Name");
//            if (!newAppTaskNameInput.DoInput())
//                return false;
//            string newTaskName = newAppTaskNameInput.Text;

//            //გადავამოწმოთ ხომ არ არსებობს იგივე სახელით სხვა ამოცანა.

//            if (parameters.TasksCol != null &&
//                parameters.TasksCol.Tasks.Keys.Any(a => a == newTaskName))
//            {
//                StShared.WriteErrorLine(
//                    $"Task with Name {newTaskName} is already exists. cannot create task with this name. ", true);
//                return false;
//            }

//            return CreateTask(taskTypeName, newTaskName);

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

//        return false;
//    }

//    private bool CreateTask(string taskTypeName, string taskName)
//    {
//        ParametersManagerWithTasks<TE> parMan = (ParametersManagerWithTasks<TE>)_parametersManager;
//        try
//        {

//            if (!Enum.TryParse(taskTypeName, out TE tool))
//            {
//                StShared.WriteErrorLine($"Tool type name {taskTypeName} is not valid", true, _logger);
//                return false;
//            }

//            IToolCommand toolCommand = parMan.CreateToolCommand(_logger, tool, taskName);
//            ParametersEditor parametersEditor = toolCommand.GetParametersEditor();

//            return parametersEditor.EditParametersInSequence("");
//        }
//        catch (Exception e)
//        {
//            Console.WriteLine(e);
//        }

//        return false;
//    }

//}

