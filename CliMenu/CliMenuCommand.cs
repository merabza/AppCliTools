using System;
using LibDataInput;
using SystemToolsShared;

namespace CliMenu;

public /*open*/ class CliMenuCommand
{
    private readonly bool _askRunAction;
    public readonly string? ParentMenuName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CliMenuCommand(string name, EMenuAction menuActionOnBodySuccess = EMenuAction.Nothing,
        EMenuAction menuActionOnBodyFail = EMenuAction.Reload, string? parentMenuName = null, bool askRunAction = false,
        EStatusView statusView = EStatusView.Brackets, bool nameIsStatus = false)
    {
        Name = name;
        MenuAction = EMenuAction.Nothing;
        MenuActionOnBodySuccess = menuActionOnBodySuccess;
        MenuActionOnBodyFail = menuActionOnBodyFail;
        ParentMenuName = parentMenuName;
        _askRunAction = askRunAction;
        StatusView = statusView;
        NameIsStatus = nameIsStatus;
    }

    public EMenuAction MenuActionOnBodyFail { get; protected set; }
    public EMenuAction MenuActionOnBodySuccess { get; protected set; }

    public EMenuAction MenuAction { get; protected set; }
    public string Name { get; }
    public EStatusView StatusView { get; }
    public string? Status { get; private set; }
    public bool NameIsStatus { get; }

    public void CountStatus()
    {
        Status = GetStatus();
    }

    public void Run()
    {
        if (_askRunAction)
        {
            var description = GetActionDescription();
            if (description is null)
            {
                StShared.WriteErrorLine("description is null", true, null, false);
                MenuAction = EMenuAction.Reload;
                return;
            }

            Console.WriteLine(description);

            if (!Inputer.InputBool("Are you sure, you want to run this action?", true, false))
            {
                MenuAction = EMenuAction.Reload;
                return;
            }
        }

        try
        {
            MenuAction = RunBody() ? MenuActionOnBodySuccess : MenuActionOnBodyFail;
        }
        catch (DataInputEscapeException)
        {
            Console.WriteLine();
            Console.WriteLine("Escape... ");
            MenuAction = EMenuAction.Reload;
        }
        catch (ListIsEmptyException lie)
        {
            Console.WriteLine();
            Console.WriteLine(lie.Message);
            MenuAction = EMenuAction.Reload;
        }
        catch (Exception e)
        {
            StShared.WriteException(e, true);
        }
    }

    //protected virtual void RunAction()
    //{
    //    MenuAction = RunBody() ? _menuActionOnBodySuccess : _menuActionOnBodyFail;
    //}

    protected virtual bool RunBody()
    {
        return true;
    }

    //virtual string? საჭიროა SupportTools პროექტში
    // ReSharper disable once ReturnTypeCanBeNotNullable
    // ReSharper disable once VirtualMemberNeverOverridden.Global
    protected virtual string? GetActionDescription()
    {
        return Name;
    }

    public virtual CliMenuSet? GetSubMenu()
    {
        return null;
    }

    protected virtual string? GetStatus()
    {
        return null;
    }

    public virtual string? GetMenuLinkToGo()
    {
        return null;
    }
}