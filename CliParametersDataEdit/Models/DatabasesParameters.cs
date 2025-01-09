using LibParameters;

namespace CliParametersDataEdit.Models;

public sealed class DatabasesParameters : ParametersWithStatus
{
    //შემდეგი 2 პარამეტრიდან გამოიყენება ერთერთი
    //ორივეს ერთდროულად შევსება არ შეიძლება.
    //ორივეს ცარელა დატოვება არ შეიძლება
    //მონაცემთა ბაზასთან კავშირის სახელი
    public string? DbConnectionName { get; set; }

    //მონაცემთა ბაზასთან დამაკავშირებელი ვებაგენტის სახელი
    public string? DbWebAgentName { get; set; }

    //ვებაგენტის მხარეს მონაცემთა ბაზასთან დამაკავშირებელი სახელი
    public string? RemoteDbConnectionName { get; set; }

    public string? DbServerFoldersSetName { get; set; }

    public string? DatabaseName { get; set; }

    //ეს პარამეტრები საჭიროა, როცა ბაზა არის ფაილის ტიპის. მაგალითად, SQLite ან აქსესის ბაზებისთვის
    public string? DatabaseFilePath { get; set; }
    public string? DatabasePassword { get; set; }

    //ჭკვიანი სქემის სახელი. გამოიყენება ძველი დასატოვებელი და წასაშლელი ფაილების განსასაზღვრად. (ბაზის სერვერის მხარეს)
    //ბაზის სერვერის მხარეს არსებული ჭკვიანი სქემის სახელებიუს მოტვირთვა შესაძლებელი უნდა იყოს რედაქტირების დროს
    public string? SmartSchemaName { get; set; }

    //სერვერის მხარეს ფაილსაცავის ნიკი
    //ბაზის ბექაპების გაცვლისათვის გამოსადეგი ფაილსაცავების ნიკების მოტვირთვა შესაძლებელი უნდა იყოს რედაქტირების დროს
    public string? FileStorageName { get; set; }
    public int CommandTimeOut { get; set; }
}