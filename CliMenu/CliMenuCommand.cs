using System;
using LibDataInput;


namespace CliMenu;

public /*open*/ class CliMenuCommand
{
    private readonly bool _askRunAction;
    protected readonly string? ParentMenuName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CliMenuCommand(string? name = null, string? parentMenuName = null, bool askRunAction = false,
        EStatusView statusView = EStatusView.Brackets, bool nameIsStatus = false)
    {
        MenuAction = EMenuAction.Nothing;
        Name = name;
        ParentMenuName = parentMenuName;
        _askRunAction = askRunAction;
        StatusView = statusView;
        NameIsStatus = nameIsStatus;
    }

    public EMenuAction MenuAction { get; protected set; }
    public string? Name { get; }
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
            if (description is not null)
                Console.WriteLine(GetActionDescription());

            if (!Inputer.InputBool("Are you sure, you want to run this action?", true, false))
                return;
        }

        RunAction();
    }


    protected virtual void RunAction()
    {
    }


    protected virtual string? GetActionDescription()
    {
        return null;
    }

    public virtual CliMenuSet? GetSubmenu()
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