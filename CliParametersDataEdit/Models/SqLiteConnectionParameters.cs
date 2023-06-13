namespace CliParametersDataEdit.Models;

public sealed class SqLiteConnectionParameters : DbConnectionParameters
{
    public string? DatabaseFilePath { get; set; }
    public string? Password { get; set; }

    public override string GetStatus()
    {
        return DatabaseFilePath ?? "(No Database File Path)";
    }
}