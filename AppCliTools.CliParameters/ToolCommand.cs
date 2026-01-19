using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;
using ToolsManagement.LibToolActions;

namespace AppCliTools.CliParameters;

public /*open*/ class ToolCommand : ToolAction, IToolCommand
{
    protected readonly IParametersManager? ParametersManager;

    protected ToolCommand(ILogger logger, string actionName, ParametersManager parametersManager, string description,
        bool useConsole = false) : base(logger, actionName, null, null, useConsole)
    {
        ParametersManager = parametersManager;
        Par = parametersManager.Parameters;
        Description = description;
    }

    protected ToolCommand(ILogger logger, string actionName, IParameters par, IParametersManager? parametersManager,
        string description, bool useConsole = false) : base(logger, actionName, null, null, useConsole)
    {
        Par = par;
        ParametersManager = parametersManager;
        Description = description;
    }

    public IParameters Par { get; }
    public string Description { get; }
}
