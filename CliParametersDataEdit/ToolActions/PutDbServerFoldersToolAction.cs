using LibToolActions;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace CliParametersDataEdit.ToolActions;

public class PutDbServerFoldersToolAction : ToolAction
{
    protected PutDbServerFoldersToolAction(ILogger logger, string actionName, IMessagesDataManager? messagesDataManager,
        string? userName, bool useConsole = false) : base(logger, actionName, messagesDataManager, userName, useConsole)
    {
    }
}