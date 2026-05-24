using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersEdit.Cruders;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParametersEdit.FieldEditors;

public sealed class RetryStrategyParametersNameFieldEditor : FieldEditor<string>
{
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RetryStrategyParametersNameFieldEditor(string propertyName, IParametersManager parametersManager,
        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate)
    {
        _parametersManager = parametersManager;
    }

    public override async ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        var retryStrategyParametersCruder = RetryStrategyParametersCruder.Create(_parametersManager);
        SetValue(recordForUpdate,
            await retryStrategyParametersCruder.GetNameWithPossibleNewName(FieldName, GetValue(recordForUpdate), null,
                true, cancellationToken));
    }
}
