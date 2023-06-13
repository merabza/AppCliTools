//using System;
//using CliMenu;
//using SystemToolsShared;
//using CliParameters.FieldEditors;
//using CliParameters.Tasks;
//using LibDataInput;

//namespace CliParameters;

//public sealed class TaskFieldEditorMenuCommand : CliMenuCommand
//{
//    private readonly string _taskName;
//    private readonly FieldEditor _fieldEditor;
//    private readonly ParametersManager _parametersManager;

//    public TaskFieldEditorMenuCommand(string fieldName, string taskName, FieldEditor fieldEditor,
//        ParametersManager parametersManager) : base(fieldName, null, EStatusView.Table)
//    {
//        _taskName = taskName;
//        _fieldEditor = fieldEditor;
//        _parametersManager = parametersManager;
//    }

//    public override bool Run()
//    {
//        try
//        {

//            MenuAction = EMenuAction.Reload;

//            IParametersWithTasks parameters = (IParametersWithTasks) _parametersManager.Parameters;
//            if (parameters == null)
//            {
//                StShared.WriteErrorLine("Parameters with tasks not found", true);
//                return false;
//            }

//            TaskModel task = parameters.TasksCol.GetTask(_taskName);

//            _fieldEditor.UpdateField(null, task);

//            ////პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
//            _parametersManager.Save(_parametersManager.Parameters, "Task Field Saved");

//            return true;
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
//            return false;
//        }

//    }

//    protected override string GetStatus()
//    {
//        IParametersWithTasks parameters = (IParametersWithTasks) _parametersManager.Parameters;
//        if (parameters == null)
//        {
//            StShared.WriteErrorLine("Parameters with tasks not found", true);
//            return "";
//        }

//        TaskModel task = parameters.TasksCol.GetTask(_taskName);


//        return _fieldEditor.GetValueStatus(task);
//    }

//}

