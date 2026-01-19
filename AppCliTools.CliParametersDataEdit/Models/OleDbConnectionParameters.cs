namespace AppCliTools.CliParametersDataEdit.Models;

public sealed class OleDbConnectionParameters : DbConnectionParameters
{
    public const string MsAccessOleDbProviderName = "Microsoft.ACE.OLEDB.12.0";

    public string? DatabaseFilePath { get; set; }
    public string? Provider { get; set; } = MsAccessOleDbProviderName;
    public bool PersistSecurityInfo { get; set; }
    public string? Password { get; set; }

    public override string GetStatus()
    {
        return DatabaseFilePath ?? "(No Database File Path)";
    }
}
