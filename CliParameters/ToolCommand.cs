using LibParameters;
using LibToolActions;
using Microsoft.Extensions.Logging;

namespace CliParameters;

public /*open*/ class ToolCommand : ToolAction, IToolCommand
{
    protected readonly IParametersManager? ParametersManager;


    protected ToolCommand(ILogger logger, bool useConsole, string actionName, ParametersManager parametersManager,
        string? description = null) : base(logger, useConsole, actionName)
    {
        ParametersManager = parametersManager;
        Par = parametersManager.Parameters;
        Description = description;
    }

    protected ToolCommand(ILogger logger, bool useConsole, string actionName, IParameters par,
        IParametersManager? parametersManager, string? description = null) : base(logger, useConsole, actionName)
    {
        Par = par;
        ParametersManager = parametersManager;
        Description = description;
    }

    public IParameters Par { get; }
    public string? Description { get; }
}