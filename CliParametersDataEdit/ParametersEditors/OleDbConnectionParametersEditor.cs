using CliParameters;
using CliParameters.FieldEditors;
using CliParametersDataEdit.Models;
using LibParameters;
using SystemToolsShared;

namespace CliParametersDataEdit.ParametersEditors;

public sealed class OleDbConnectionParametersEditor : ParametersEditor
{
    public OleDbConnectionParametersEditor(IParametersManager parametersManager) : base(
        nameof(OleDbConnectionParametersEditor).SplitWithSpacesCamelParts(), parametersManager)
    {
        FieldEditors.Add(new FilePathFieldEditor(nameof(OleDbConnectionParameters.DatabaseFilePath)));
        FieldEditors.Add(new TextFieldEditor(nameof(OleDbConnectionParameters.Provider),
            OleDbConnectionParameters.MsAccessOleDbProviderName));
        FieldEditors.Add(new BoolFieldEditor(nameof(OleDbConnectionParameters.PersistSecurityInfo), false));
        FieldEditors.Add(new TextFieldEditor(nameof(OleDbConnectionParameters.Password), null, PasswordChar));
    }
}