using System;
using LibDataInput;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace CliMenu;

public /*open*/ class CliMenuCommand
{
    private readonly bool _askRunAction;
    private readonly bool _countRunDuration;
    private readonly ILogger? _logger;
    protected readonly string? ParentMenuName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CliMenuCommand(string? name = null, string? parentMenuName = null, bool askRunAction = false,
        EStatusView statusView = EStatusView.Brackets, bool nameIsStatus = false, bool countRunDuration = false,
        ILogger? logger = null)
    {
        MenuAction = EMenuAction.Nothing;
        Name = name;
        ParentMenuName = parentMenuName;
        _askRunAction = askRunAction;
        _countRunDuration = countRunDuration;
        _logger = logger;
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

        //დავინიშნოთ დრო პროცესისათვის
        var startDateTime = DateTime.Now;

        RunAction();

        if (!_countRunDuration)
            return;
        var timeTakenMessage = StShared.TimeTakenMessage(startDateTime);
        if (_logger is null)
            Console.WriteLine("{0} Finished. {1}", Name, timeTakenMessage);
        else
            _logger.LogInformation("{Name} Finished. {timeTakenMessage}", Name, timeTakenMessage);
        StShared.Pause();

        //if (MessagesDataManager is not null)
        //    await MessagesDataManager.SendMessage(UserName, $"{_actionName} Finished. {timeTakenMessage}",
        //        cancellationToken);
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