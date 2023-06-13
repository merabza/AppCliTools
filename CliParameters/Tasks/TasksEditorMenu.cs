//using System;
//using System.Linq;
//using CliMenu;
//using Microsoft.Extensions.Logging;

//namespace CliParameters.Tasks;

//public sealed class TasksEditorMenu<TE> where TE : struct, Enum
//{
//    private readonly ILogger _logger;
//    private readonly ParametersManager _parametersManager;

//    public TasksEditorMenu(ILogger logger, ParametersManager parametersManager)
//    {
//        _logger = logger;
//        _parametersManager = parametersManager;
//    }

//    public void TaskMenuElements(CliMenuSet mainMenuSet)
//    {
//        IParametersWithTasks parameters = (IParametersWithTasks)_parametersManager.Parameters;
//        //ამოცანების დამატება
//        NewTaskCommand<TE> newAppTaskCommand = new(_logger, _parametersManager);
//        mainMenuSet.AddMenuItem(newAppTaskCommand);

//        //ამოცანების რედაქტირება და გაშვება
//        if (parameters.TasksCol == null)
//            return;
//        foreach (string key in parameters.TasksCol.Tasks.Keys.OrderBy(o => o))
//            mainMenuSet.AddMenuItem(new TaskSubMenuCommand<TE>(_logger, _parametersManager, key));
//    }

//}

