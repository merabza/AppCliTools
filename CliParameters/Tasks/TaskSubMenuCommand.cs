//using System;
//using CliMenu;
//using SystemToolsShared;
//using CliParameters.MenuCommands;
//using LibDataInput;
//using Microsoft.Extensions.Logging;

//namespace CliParameters.Tasks;

//public sealed class TaskSubMenuCommand<TE> : CliMenuCommand where TE : struct, Enum
//{
//    private readonly ILogger _logger;
//    private readonly ParametersManager _parametersManager;

//    public TaskSubMenuCommand(ILogger logger, ParametersManager parametersManager,
//        string taskName) : base(taskName)
//    {
//        _logger = logger;
//        _parametersManager = parametersManager;
//    }

//    public override bool Run()
//    {
//        MenuAction = EMenuAction.LoadSubMenu;
//        return true;
//    }

//    public override CliMenuSet GetSubmenu()
//    {
//        CliMenuSet taskSubMenuSet = new($" Task => {Name}");

//        DeleteTaskCommand deleteTaskCommand = new(_parametersManager, Name);
//        taskSubMenuSet.AddMenuItem(deleteTaskCommand);


//        taskSubMenuSet.AddMenuItem(new EditTaskNameCommand(_parametersManager, Name), "Edit task Name");

//        ParametersManagerWithTasks<TE> parMan = (ParametersManagerWithTasks<TE>)_parametersManager;
//        IParametersWithTasks parameters = (IParametersWithTasks)_parametersManager.Parameters;

//        TaskModel taskModel = parameters.TasksCol.GetTask(Name);
//        string taskTypeName = taskModel.TaskTypeName;

//        if (!Enum.TryParse(taskTypeName, out TE tool))
//        {
//            StShared.WriteErrorLine($"Tool type name {taskTypeName} is not valid", true, _logger);
//            return taskSubMenuSet;
//        }

//        IToolCommand toolCommand = parMan.CreateToolCommand(_logger, tool, Name);

//        AloneTaskParametersEditor parametersEditor = toolCommand.GetParametersEditor() as AloneTaskParametersEditor;
//        if (parametersEditor == null)

//        {
//            StShared.WriteErrorLine(
//                $"Parameters editor must be TaskParametersEditor for Tool type with name {taskTypeName}", true,
//                _logger);
//            return taskSubMenuSet;
//        }

//        parametersEditor.UseParametersEditCommands(taskSubMenuSet, taskModel);

//        taskSubMenuSet.AddMenuItem(new TaskRunCommand<TE>(_logger, _parametersManager, Name), "Run this app task");


//        string key = ConsoleKey.Escape.Value().ToLower();
//        taskSubMenuSet.AddMenuItem(key, "Exit to Main menu", new ExitToMainMenuCommand(null, null), key.Length);

//        return taskSubMenuSet;
//    }

//}

