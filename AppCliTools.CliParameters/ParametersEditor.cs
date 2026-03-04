using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.LibDataInput;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParameters;

public /*open*/ class ParametersEditor : IFieldEditors
{
    public const char PasswordChar = '*';
    public const string SaveMessage = "Parameters Saved"; // S3400 fix: use constant instead of method
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
        FieldEditor? fieldEditor = FieldEditors.SingleOrDefault(s => s.PropertyName == fieldName);
        fieldEditor?.Enabled = enable;
    }

    public static string? GetStatus()
    {
        return null;
    }

    public async ValueTask Save(string message, string? saveAsFilePath = null,
        CancellationToken cancellationToken = default)
    {
        if (_parametersManager.Parameters is null)
        {
            throw new Exception("Invalid parameters for save");
        }

        await _parametersManager.Save(_parametersManager.Parameters, message, saveAsFilePath, cancellationToken);
    }

    private string GetMainMenuCaption()
    {
        return Name;
    }

    public CliMenuSet GetParametersMainMenu()
    {
        var parametersEditorMenuSet = new CliMenuSet(GetMainMenuCaption());

        FillDetailsSubMenu(parametersEditorMenuSet);

        parametersEditorMenuSet.AddEscapeCommand();

        return parametersEditorMenuSet;
    }

    public void FillDetailsSubMenu(CliMenuSet parametersEditorMenuSet)
    {
        //მენიუს ჩანაწერი, რომელიც საშუალებას გვაძლევს პარამეტრები დავარედაქტიროთ ყველა თანმიმდევრობით
        var editCommand = new EditParametersInSequenceCliMenuCommand(this);
        parametersEditorMenuSet.AddMenuItem(editCommand);

        foreach (FieldEditor fieldEditor in FieldEditors.Where(w => w.Enabled))
        {
            fieldEditor.AddParameterEditMenuItem(parametersEditorMenuSet, this);
        }
    }

    //ყველა პარამეტრის რედაქტირება თანმიმდევრობით
    internal async ValueTask<bool> EditParametersInSequence(CancellationToken cancellationToken = default)
    {
        //პარამეტრების შეცვლის პროცესი დაიწყო
        Console.WriteLine($"Edit {Name} parameters started");

        //შეიცვალოს პარამეტრები თანმიმდევრობით
        if (!await InputParametersData(cancellationToken))
        {
            return false;
        }

        //პარამეტრების შენახვა (ცვლილებების გათვალისწინებით)
        await Save($"{Name} parameters Updated", null, cancellationToken);

        //ყველაფერი კარგად დასრულდა
        return true;
    }

    private async ValueTask<bool> InputParametersData(CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (FieldEditor fieldEditor in FieldEditors.Where(fieldUpdater => fieldUpdater.Enabled))
            {
                await fieldEditor.UpdateField(null, _parametersManager.Parameters, cancellationToken);
            }

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

    public virtual void CheckFieldsEnables(object record)
    {
    }
}
