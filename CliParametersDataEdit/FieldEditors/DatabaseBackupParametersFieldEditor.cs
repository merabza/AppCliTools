using CliMenu;
using CliParameters.FieldEditors;
using LibDatabaseParameters;
using LibParameters;

namespace CliParametersDataEdit.FieldEditors;

public sealed class DatabaseBackupParametersFieldEditor : FieldEditor<DatabaseBackupParametersModel>
{
    private readonly IParametersManager _parametersManager;

    public DatabaseBackupParametersFieldEditor(string propertyName, IParametersManager parametersManager,
        bool enterFieldDataOnCreate = false) : base(propertyName, enterFieldDataOnCreate, null, true)
    {
        _parametersManager = parametersManager;
    }

    public override string GetValueStatus(object? record)
    {
        var val = GetValue(record);
        return val == null ? "(empty)" : "(some parameters)";
    }

    public override CliMenuSet GetSubMenu(object record)
    {
        var currentDatabaseBackupParameters = GetValue(record);
        if (currentDatabaseBackupParameters == null)
        {
            currentDatabaseBackupParameters = new DatabaseBackupParametersModel();
            SetValue(record, currentDatabaseBackupParameters);
        }

        DatabaseBackupParametersEditor databaseBackupParametersEditor =
            new(currentDatabaseBackupParameters, _parametersManager);
        var foldersSet = databaseBackupParametersEditor.GetParametersMainMenu();
        return foldersSet;
    }
}