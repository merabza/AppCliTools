using System.Net.Http;
using CliParameters;
using CliParameters.FieldEditors;
using CliParametersDataEdit.FieldEditors;
using CliParametersEdit.FieldEditors;
using LibDatabaseParameters;
using LibParameters;
using Microsoft.Extensions.Logging;

namespace CliParametersDataEdit.ParametersEditors;

public sealed class DatabaseParametersEditor : ParametersEditor
{
    public DatabaseParametersEditor(ILogger logger, IHttpClientFactory httpClientFactory,
        IParametersManager parametersManager, IParametersManager listsParametersManager) : base("Database Parameters",
        parametersManager)
    {
        //FieldEditors.Add(
        //    new EnumFieldEditor<EDataProvider>(nameof(DatabaseParameters.DataProvider), EDataProvider.Sql));

        //შემდეგი 2 პარამეტრიდან გამოიყენება ერთერთი
        //ორივეს ერთდროულად შევსება არ შეიძლება.
        //ორივეს ცარელა დატოვება არ შეიძლება
        //მონაცემთა ბაზასთან კავშირის სახელი
        FieldEditors.Add(new DatabaseServerConnectionNameFieldEditor(logger, httpClientFactory,
            nameof(DatabaseParameters.DbConnectionName), listsParametersManager, true));

        ////მონაცემთა ბაზასთან დამაკავშირებელი ვებაგენტის სახელი
        //FieldEditors.Add(new ApiClientNameFieldEditor(logger, httpClientFactory,
        //    nameof(DatabaseParameters.DbWebAgentName), listsParametersManager, true));

        ////მონაცემთა ბაზასთან დამაკავშირებელი ვებაგენტის სახელი
        //FieldEditors.Add(new RemoteDbConnectionNameFieldEditor(logger, httpClientFactory,
        //    nameof(DatabaseParameters.RemoteDbConnectionName), listsParametersManager,
        //    nameof(DatabaseParameters.DbWebAgentName)));

        //FieldEditors.Add(new EnumFieldEditor<EDataProvider>(nameof(DatabaseConnectionParameters.DataProvider),
        //    EDataProvider.Sql));

        ////ბექაპირების პარამეტრები  სერვერის მხარეს
        //FieldEditors.Add(new DatabaseBackupParametersFieldEditor(logger, nameof(DatabaseParameters.DbBackupParameters),
        //    listsParametersManager));

        //ფოლდერი სერვერის მხარეს, რომელშიც უნდა მოხდეს ბექაპის შენახვა
        //FieldEditors.Add(new DbServerSideBackupPathFieldEditor(nameof(DatabaseParameters.DbServerSideBackupPath),
        //    listsParametersManager, nameof(DatabaseParameters.DbWebAgentName),
        //    nameof(DatabaseParameters.DbConnectionName)));


        //public string? DbServerFoldersSetName { get; set; }
        FieldEditors.Add(new DbServerFoldersSetNameFieldEditor(logger, httpClientFactory,
            nameof(DatabaseParameters.DbServerFoldersSetName), listsParametersManager,
            nameof(DatabaseParameters.DbConnectionName)));

        ////ფოლდერი სერვერის მხარეს, რომელშიც უნდა მოხდეს ბაზის მონაცემების ფაილის აღდგენა
        //FieldEditors.Add(new TextFieldEditor(nameof(DatabaseParameters.DbServerSideDataFolderPath)));

        ////ფოლდერი სერვერის მხარეს, რომელშიც უნდა მოხდეს ბაზის ლოგების ფაილის აღდგენა
        //FieldEditors.Add(new TextFieldEditor(nameof(DatabaseParameters.DbServerSideLogFolderPath)));

        ////ბაზის სახელის არჩევა ხდება სერვერზე არსებული ბაზების სახელებიდან.
        ////შესაძლებელია ახალი სახელის მითითება, თუ ბაზა ჯერ არ არსებობს
        //FieldEditors.Add(new DatabaseNameFieldEditor(logger, httpClientFactory,
        //    nameof(DatabaseParameters.CurrentBaseName), listsParametersManager,
        //    nameof(DatabaseParameters.DbConnectionName), nameof(DatabaseParameters.DbWebAgentName), true));

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
        FieldEditors.Add(new BoolFieldEditor(nameof(DatabaseParameters.SkipBackupBeforeRestore), false));
        FieldEditors.Add(new DatabaseBackupParametersFieldEditor(logger, nameof(DatabaseParameters.DatabaseBackupParameters), parametersManager));
        
        
    }


    //public override void CheckFieldsEnables(object record)
    //{
    //    var dataProvider = FieldEditor.GetValue<EDatabaseProvider>(record, nameof(DatabaseParameters.DataProvider));

    //    switch (dataProvider)
    //    {
    //        case EDatabaseProvider.None:
    //            EnableOffAllFieldButList([nameof(DatabaseParameters.DataProvider)]);
    //            break;
    //        case EDatabaseProvider.SqlServer:
    //            EnableOffAllFieldButList([
    //                nameof(DatabaseParameters.DataProvider),
    //                nameof(DatabaseParameters.DbConnectionName),
    //                nameof(DatabaseParameters.DbServerFoldersSetName),
    //                nameof(DatabaseParameters.DatabaseName),
    //                nameof(DatabaseParameters.SmartSchemaName),
    //                nameof(DatabaseParameters.FileStorageName),
    //                nameof(DatabaseParameters.CommandTimeOut)
    //            ]);
    //            break;
    //        case EDatabaseProvider.SqLite:
    //        case EDatabaseProvider.OleDb:
    //            EnableOffAllFieldButList([
    //                nameof(DatabaseParameters.DataProvider),
    //                nameof(DatabaseParameters.DatabaseFilePath),
    //                nameof(DatabaseParameters.DatabasePassword),
    //                nameof(DatabaseParameters.SmartSchemaName),
    //                nameof(DatabaseParameters.FileStorageName),
    //                nameof(DatabaseParameters.CommandTimeOut)
    //            ]);
    //            break;
    //        case EDatabaseProvider.WebAgent:
    //            EnableOffAllFieldButList([
    //                nameof(DatabaseParameters.DataProvider),
    //                nameof(DatabaseParameters.DbWebAgentName),
    //                nameof(DatabaseParameters.RemoteDbConnectionName),
    //                nameof(DatabaseParameters.DbServerFoldersSetName),
    //                nameof(DatabaseParameters.DatabaseName),
    //                nameof(DatabaseParameters.SmartSchemaName),
    //                nameof(DatabaseParameters.FileStorageName),
    //                nameof(DatabaseParameters.CommandTimeOut)
    //            ]);
    //            break;
    //        default:
    //            throw new ArgumentOutOfRangeException();
    //    }
    //}
}