using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters;
using ParametersManagement.LibDatabaseParameters;

namespace AppCliTools.CliParametersDataEdit.CliMenuCommands;

//ბაზის პარამეტრების რედაქტორის ჩანაწერის მთლიანად წაშლა: ველი null ხდება და ცარიელი ობიექტი არ რჩება, რადგან
//ცარიელ ობიექტს პროგრამები მითითებულ პარამეტრებად თვლიან
public sealed class DeleteDatabaseParametersCliMenuCommand : CliMenuCommand
{
    private readonly SubParametersManager<DatabaseParameters> _parametersManager;
    private readonly string _propertyName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DeleteDatabaseParametersCliMenuCommand(SubParametersManager<DatabaseParameters> parametersManager,
        string propertyName) : base("Delete", EMenuAction.LevelUp, EMenuAction.Reload, null, true)
    {
        _parametersManager = parametersManager;
        _propertyName = propertyName;
    }

    protected override ValueTask<string?> GetActionDescription(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult<string?>($"This process will delete {_propertyName}");
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        return _parametersManager.Delete($"{_propertyName} deleted", cancellationToken);
    }
}
