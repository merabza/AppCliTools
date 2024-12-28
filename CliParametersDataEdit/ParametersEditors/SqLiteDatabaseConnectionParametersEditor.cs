using CliParameters;
using CliParameters.FieldEditors;
using CliParametersDataEdit.Models;
using LibParameters;
using SystemToolsShared;

namespace CliParametersDataEdit.ParametersEditors;

public sealed class SqLiteDatabaseConnectionParametersEditor : ParametersEditor
{
    public SqLiteDatabaseConnectionParametersEditor(IParametersManager parametersManager) : base(
        nameof(SqLiteDatabaseConnectionParametersEditor).SplitWithSpacesCamelParts(), parametersManager)
    {
        FieldEditors.Add(new FilePathFieldEditor(nameof(SqLiteConnectionParameters.DatabaseFilePath)));
        FieldEditors.Add(new TextFieldEditor(nameof(SqLiteConnectionParameters.Password), default, '*'));
    }
}