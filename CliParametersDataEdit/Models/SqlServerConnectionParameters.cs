namespace CliParametersDataEdit.Models;

public sealed class SqlServerConnectionParameters : DbConnectionParameters
{
    public string? ServerAddress { get; set; }
    public bool WindowsNtIntegratedSecurity { get; set; }
    public string? ServerUser { get; set; }
    public string? ServerPass { get; set; }
    public string? DatabaseName { get; set; }
    public int ConnectionTimeOut { get; set; }
    public bool Encrypt { get; set; }
    public bool TrustServerCertificate { get; set; }

    public override string GetStatus()
    {
        return $"{ServerAddress}.{DatabaseName}";
    }
}