using System;
using System.Collections.Generic;
using System.Linq;
using CliMenu;
using CliParameters.CliMenuCommands;
using CliParameters.FieldEditors;
using LibDataInput;
using LibParameters;
using SystemToolsShared;

namespace CliParameters;

public /*open*/ class ParametersEditor : IFieldEditors
{
    private readonly IParametersManager _parametersManager;
    protected readonly List<FieldEditor> FieldEditors = [];

    protected ParametersEditor(string name, IParameters parameters, IParametersManager parametersManager)
    {
        Name = name;
        Parameters = parameters;
        _parametersManager = parametersManager;
    }

    protected ParametersEditor(string name, IParametersManager parametersManager)
    {
        Name = name;
        Parameters = parametersManager.Parameters;
        _parametersManager = parametersManager;
    }

    public string Name { get; }
    public IParameters Parameters { get; }

    public void EnableFieldByName(string fieldName, bool enable = true)
    {
        var fieldEditor = FieldEditors.SingleOrDefault(s => s.PropertyName == fieldName);
        if (fieldEditor != null)
            fieldEditor.Enabled = enable;
    }

    public static string? GetStatus()
    {
        return null;
    }

    public void Save(string message, string? saveAsFilePath = null)
    {
        if (_parametersManager.Parameters is null)
            throw new Exception("Invalid parameters for save");
        _parametersManager.Save(_parametersManager.Parameters, message, saveAsFilePath);
    }

    private string GetMainMenuCaption()
    {
        return Name;
    }

    public CliMenuSet GetParametersMainMenu()
    {
        CliMenuSet parametersEditorMenuSet = new(GetMainMenuCaption());

        FillDetailsSubMenu(parametersEditorMenuSet);

        var key = ConsoleKey.Escape.Value().ToLower();
        parametersEditorMenuSet.AddMenuItem(key, "Exit to Main menu", new ExitToMainMenuCliMenuCommand(null, null),
            key.Length);


        return parametersEditorMenuSet;
    }

    private void FillDetailsSubMenu(CliMenuSet parametersEditorMenuSet)
    {
        //მენიუს ჩანაწერი, რომელიც საშუალებას გვაძლევს პარამეტრები დავარედაქტიროთ ყველა თანმიმდევრობით
        EditParametersInSequenceCliMenuCommand editCommand = new(this);
        parametersEditorMenuSet.AddMenuItem(editCommand, "Edit All Parameters in sequence");

        foreach (var fieldEditor in FieldEditors) //.Where(w=>w.Enabled)
            fieldEditor.AddParameterEditMenuItem(parametersEditorMenuSet, this);
    }

    //ყველა პარამეტრის რედაქტირება თანმიმდევრობით
    internal bool EditParametersInSequence()
    {
        //პარამეტრების შეცვლის პროცესი დაიწყო
        Console.WriteLine($"Edit {Name} parameters started");

        //შეიცვალოს პარამეტრები თანმიმდევრობით
        if (!InputParametersData())
            return false;

        //პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
        Save($"{Name} parameters Updated");

        //ყველაფერი კარგად დასრულდა
        return true;
    }

    private bool InputParametersData()
    {
        try
        {
            foreach (var fieldEditor in FieldEditors.Where(fieldUpdater => fieldUpdater.Enabled))
                fieldEditor.UpdateField(null, _parametersManager.Parameters);
            //FIXME IF NEED ვალიდაცია პარამეტრების რედაქტირებისას გვჭირდება თუ არა ჯერ არ ვიცი, ალბათ გვჭირდება და უნდა გაკეთდეს
            //CheckFieldsEnables(newItem);

            //if (CheckValidation(newItem)) 
            //  return newItem;

            //return !Inputer.InputBool($"{recordKey} is not valid {CrudName}, continue input data?", false, false)
            //  ? null
            //  : newItem;
            return true;
        }
        catch (DataInputEscapeException)
        {
            Console.WriteLine();
            Console.WriteLine("Escape... ");
            StShared.Pause();
            return false;
        }
        catch (Exception e)
        {
            StShared.WriteException(e, true);
            return false;
        }
    }

    public static string GetSaveMessage()
    {
        return "Parameters Saved";
    }
}