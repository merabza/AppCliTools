using CliParameters;
using CliParameters.FieldEditors;
using DbTools;
using LibDatabaseParameters;
using LibParameters;

namespace CliParametersDataEdit.ParametersEditors;

public sealed class DatabaseBackupParametersEditor : ParametersEditor
{
    public DatabaseBackupParametersEditor(IParameters parameters, IParametersManager parametersManager) : base(
        "Database Backup ParametersEditor", parameters, parametersManager)
    {
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseBackupParametersModel.BackupNamePrefix)));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseBackupParametersModel.DateMask), "yyyyMMddHHmmss"));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseBackupParametersModel.BackupFileExtension), ".bak"));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseBackupParametersModel.BackupNameMiddlePart), "_FullDb_"));
        //FIXME აქ სასურველია დადგინდეს შეუძლია თუ არა სერვერს კომპრესია და ისე განისაზღვროს ძირითადი მნიშვნელობა
        FieldEditors.Add(new BoolFieldEditor(nameof(DatabaseBackupParametersModel.Compress), true));
        FieldEditors.Add(new BoolFieldEditor(nameof(DatabaseBackupParametersModel.Verify), true));
        FieldEditors.Add(new EnumFieldEditor<EBackupType>(nameof(DatabaseBackupParametersModel.BackupType),
            EBackupType.Full));
    }
}