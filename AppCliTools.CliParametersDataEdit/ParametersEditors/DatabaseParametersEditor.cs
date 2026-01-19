using System;
using System.Net.Http;
using CliParameters;
using CliParameters.FieldEditors;
using CliParametersDataEdit.FieldEditors;
using CliParametersEdit.FieldEditors;
using DatabaseTools.DbTools;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;

namespace CliParametersDataEdit.ParametersEditors;

public sealed class DatabaseParametersEditor : ParametersEditor
{
    public DatabaseParametersEditor(ILogger logger, IHttpClientFactory httpClientFactory,
        IParametersManager parametersManager, IParametersManager listsParametersManager, string propertyName) : base(
        $"{propertyName} {nameof(DatabaseParametersEditor)}", parametersManager)
    {
        //შემდეგი 2 პარამეტრიდან გამოიყენება ერთერთი
        //ორივეს ერთდროულად შევსება არ შეიძლება.
        //ორივეს ცარელა დატოვება არ შეიძლება
        //მონაცემთა ბაზასთან კავშირის სახელი
        FieldEditors.Add(new DatabaseServerConnectionNameFieldEditor(logger, httpClientFactory,
            nameof(DatabaseParameters.DbConnectionName), listsParametersManager, true));

        FieldEditors.Add(new DbServerFoldersSetNameFieldEditor(logger, httpClientFactory,
            nameof(DatabaseParameters.DbServerFoldersSetName), listsParametersManager,
            nameof(DatabaseParameters.DbConnectionName)));

        FieldEditors.Add(new EnumNullableFieldEditor<EDatabaseRecoveryModel>(
            nameof(DatabaseParameters.DatabaseRecoveryModel), DatabaseParameters.DefaultDatabaseRecoveryModel, true));

        //ბაზის სახელის არჩევა ხდება სერვერზე არსებული ბაზების სახელებიდან.
        //შესაძლებელია ახალი სახელის მითითება, თუ ბაზა ჯერ არ არსებობს
        //NewBaseName არის ბაზის სახელი, რომელიც უნდა შეიქმნას განახლების დროს
        //თუ განსხვავდება მიმდინარე სახელისგან, ეს ნიშნავს, რომ გვჭირდება მიმდინარე ბაზის შენარჩუნება
        //ხოლო ახალი ბაზისათვის მზად არის ახალი პროგრამა.
        //მას მერე, რაც საჭიროება აღარ იქნება, CurrentBaseName და BaseName სახელები ერთმანეთს უნდა დაემთხვას.
        FieldEditors.Add(new DatabaseNameFieldEditor(logger, httpClientFactory, nameof(DatabaseParameters.DatabaseName),
            listsParametersManager, nameof(DatabaseParameters.DbConnectionName), true));

        //ჭკვიანი სქემის სახელი. გამოიყენება ძველი დასატოვებელი და წასაშლელი ფაილების განსასაზღვრად. (ეს ბაზის სერვერის მხარეს)
        FieldEditors.Add(new SmartSchemaNameFieldEditor(nameof(DatabaseParameters.SmartSchemaName),
            listsParametersManager));
        //სერვერის მხარეს ფაილსაცავის სახელი
        FieldEditors.Add(new FileStorageNameFieldEditor(logger, nameof(DatabaseParameters.FileStorageName),
            listsParametersManager));

        FieldEditors.Add(new IntFieldEditor(nameof(DatabaseParameters.CommandTimeOut), 10000));
        FieldEditors.Add(new BoolFieldEditor(nameof(DatabaseParameters.SkipBackupBeforeRestore)));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseParameters.BackupNamePrefix), $"{Environment.MachineName}_",
            true));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseParameters.DateMask), DatabaseParameters.DefaultDateMask,
            true));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseParameters.BackupFileExtension),
            DatabaseParameters.DefaultBackupFileExtension, true));
        FieldEditors.Add(new TextFieldEditor(nameof(DatabaseParameters.BackupNameMiddlePart),
            DatabaseParameters.DefaultBackupNameMiddlePart, true));

        FieldEditors.Add(new BoolNullableFieldEditor(nameof(DatabaseParameters.Compress),
            DatabaseParameters.DefaultCompress, true));
        FieldEditors.Add(new BoolNullableFieldEditor(nameof(DatabaseParameters.Verify),
            DatabaseParameters.DefaultVerify, true));
        FieldEditors.Add(new EnumNullableFieldEditor<EBackupType>(nameof(DatabaseParameters.BackupType),
            DatabaseParameters.DefaultBackupType, true));
    }
}
