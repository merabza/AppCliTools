using CliParameters;
using CliParameters.FieldEditors;
using CliParametersDataEdit.Models;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace CliParametersDataEdit.ParametersEditors;

public sealed class OleDbConnectionParametersEditor : ParametersEditor
{
    public OleDbConnectionParametersEditor(IParametersManager parametersManager) : base(
        nameof(OleDbConnectionParametersEditor).SplitWithSpacesCamelParts(), parametersManager)
    {
        FieldEditors.Add(new FilePathFieldEditor(nameof(OleDbConnectionParameters.DatabaseFilePath)));
        FieldEditors.Add(new TextFieldEditor(nameof(OleDbConnectionParameters.Provider),
            OleDbConnectionParameters.MsAccessOleDbProviderName));
        FieldEditors.Add(new BoolFieldEditor(nameof(OleDbConnectionParameters.PersistSecurityInfo)));
        FieldEditors.Add(new TextFieldEditor(nameof(OleDbConnectionParameters.Password), null, false, PasswordChar));
    }
}
