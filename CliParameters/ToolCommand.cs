using LibParameters;
using LibToolActions;
using Microsoft.Extensions.Logging;

namespace CliParameters;

public /*open*/ class ToolCommand : ToolAction, IToolCommand
{
    protected readonly IParametersManager? ParametersManager;


    protected ToolCommand(ILogger logger, string actionName, ParametersManager parametersManager,
        string? description = null) : base(logger, actionName, null, null)
    {
        ParametersManager = parametersManager;
        Par = parametersManager.Parameters;
        Description = description;
    }

    protected ToolCommand(ILogger logger, string actionName, IParameters par, IParametersManager? parametersManager,
        string? description = null) : base(logger, actionName, null, null)
    {
        Par = par;
        ParametersManager = parametersManager;
        Description = description;
    }

    public IParameters Par { get; }
    public string? Description { get; }
}