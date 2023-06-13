//using CliMenu;
//using CliParameters.FieldEditors;
//using CliParameters.MenuCommands;

//namespace CliParameters.Tasks;

//public sealed class AloneTaskParametersEditor : ParametersEditor
//{

//    protected AloneTaskParametersEditor(string name, IParametersManager parametersManager) : base(name,
//        parametersManager)
//    {
//    }


//    public void UseParametersEditCommands(CliMenuSet taskSubMenuSet, TaskModel taskModel)
//    {
//        //მენიუს ჩანაწერი, რომელიც საშუალებას გვაძლევს პარამეტრები დავარედაქტიროთ ყველა თანმიმდევრობით
//        EditParametersInSequenceCommand editCommand = new EditParametersInSequenceCommand(this);
//        taskSubMenuSet.AddMenuItem(editCommand, "Edit All Parameters in sequence");

//        foreach (FieldEditor fieldEditor in FieldEditors) //.Where(w=>w.Enabled)
//            fieldEditor.AddParameterEditMenuItem(taskSubMenuSet, this);
//    }

//}

