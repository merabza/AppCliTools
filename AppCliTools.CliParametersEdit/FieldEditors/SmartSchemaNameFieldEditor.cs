using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersEdit.Cruders;
using ParametersManagement.LibParameters;

namespace AppCliTools.CliParametersEdit.FieldEditors;

public sealed class SmartSchemaNameFieldEditor : FieldEditor<string>
{
    private readonly IParametersManager _parametersManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SmartSchemaNameFieldEditor(string propertyName, IParametersManager parametersManager,
        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate)
    {
        _parametersManager = parametersManager;
    }

    public override async ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        var smartSchemaCruder = SmartSchemaCruder.Create(_parametersManager);
        SetValue(recordForUpdate,
            await smartSchemaCruder.GetNameWithPossibleNewName(FieldName, GetValue(recordForUpdate), null, false,
                cancellationToken));
    }
}
