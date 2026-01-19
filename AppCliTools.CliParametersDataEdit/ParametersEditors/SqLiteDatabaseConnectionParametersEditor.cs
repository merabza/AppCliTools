using AppCliTools.CliParameters;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersDataEdit.Models;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace AppCliTools.CliParametersDataEdit.ParametersEditors;

public sealed class SqLiteDatabaseConnectionParametersEditor : ParametersEditor
{
    public SqLiteDatabaseConnectionParametersEditor(IParametersManager parametersManager) : base(
        nameof(SqLiteDatabaseConnectionParametersEditor).SplitWithSpacesCamelParts(), parametersManager)
    {
        FieldEditors.Add(new FilePathFieldEditor(nameof(SqLiteConnectionParameters.DatabaseFilePath)));
        FieldEditors.Add(new TextFieldEditor(nameof(SqLiteConnectionParameters.Password), null, false, PasswordChar));
    }
}
