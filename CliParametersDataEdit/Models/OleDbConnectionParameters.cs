namespace CliParametersDataEdit.Models;

public sealed class OleDbConnectionParameters : DbConnectionParameters
{
    public string? DatabaseFilePath { get; set; }
    public string? Provider { get; set; }// = "Microsoft.ACE.OLEDB.12.0";
    public bool PersistSecurityInfo { get; set; } = false;

    public override string GetStatus()
    {
        return DatabaseFilePath ?? "(No Database File Path)";
    }
}