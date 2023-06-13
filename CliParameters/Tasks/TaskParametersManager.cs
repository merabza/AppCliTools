//using System;
//using System.Collections.Generic;
//using Newtonsoft.Json;

//namespace CliParameters.Tasks;

//public sealed class TaskParametersManager<TE, T> : ParametersManager where T : IParameters, new() where TE : Enum
//{
//    private readonly ParametersManagerWithTasks<TE> _parentParametersManager;
//    private readonly TE _tool;
//    private readonly string _taskName;

//    public ParametersManager MainParametersManager => _parentParametersManager;

//    public static TaskParametersManager<TE, T> Create(ParametersManagerWithTasks<TE> parentParametersManager, TE tool,
//        string taskName)
//    {
//        return new(parentParametersManager, tool, taskName, GetParameters(parentParametersManager, taskName));
//    }

//    public TaskParametersManager(ParametersManagerWithTasks<TE> parentParametersManager, TE tool, string taskName,
//        T parameters) : base(parameters)
//    {
//        _parentParametersManager = parentParametersManager;
//        _tool = tool;
//        _taskName = taskName;
//    }

//    public override void Save(IParameters parameters, string message, bool pauseAfterMessage = true,
//        string saveAsFilePath = null)
//    {
//        IParametersWithTasks parentParameters = (IParametersWithTasks)_parentParametersManager.Parameters;

//        //არსებული ინფორმაციის გამოყენებით ახალი ამოცანის შექმნა დაიწყო
//        parentParameters.TasksCol ??= new TasksCollection();
//        parentParameters.TasksCol.Tasks ??= new Dictionary<string, TaskModel>();

//        if (parentParameters.TasksCol.Tasks.ContainsKey(_taskName))
//            parentParameters.TasksCol.Tasks[_taskName].TaskParametersData =
//                JsonConvert.SerializeObject(parameters, Formatting.Indented);
//        else
//            parentParameters.TasksCol.Tasks.Add(_taskName,
//                new TaskModel
//                {
//                    TaskTypeName = _tool.ToString(),
//                    TaskParametersData = JsonConvert.SerializeObject(parameters, Formatting.Indented)
//                });
//        _parentParametersManager.Save(parentParameters, message);
//    }

//    private static T GetParameters(ParametersManagerWithTasks<TE> parentParametersManager, string taskName)
//    {
//        IParametersWithTasks parentParameters = (IParametersWithTasks)parentParametersManager.Parameters;
//        TaskModel taskModel = parentParameters.TasksCol?.GetTask(taskName);
//        return taskModel?.TaskParametersData != null
//            ? JsonConvert.DeserializeObject<T>(taskModel.TaskParametersData)
//            : new T();
//    }

//}

