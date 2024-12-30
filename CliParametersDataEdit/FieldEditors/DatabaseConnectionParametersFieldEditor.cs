using CliParameters.FieldEditors;
using CliParametersDataEdit.Models;
using CliParametersDataEdit.ParametersEditors;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParametersDataEdit.FieldEditors;

public sealed class
    DatabaseConnectionParametersFieldEditor : ParametersFieldEditor<DatabaseConnectionParameters,
    DatabaseConnectionParametersEditor>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public DatabaseConnectionParametersFieldEditor(ILogger logger, string propertyName,
        IParametersManager parametersManager) : base(logger, propertyName, parametersManager)
    {
    }

    protected override DatabaseConnectionParametersEditor CreateEditor(object record,
        DatabaseConnectionParameters currentValue)
    {
        DatabaseConnectionParametersManager memoryParametersManager =
            new(currentValue, ParametersManager, this, record);

        return new DatabaseConnectionParametersEditor(Logger, memoryParametersManager);
    }
}