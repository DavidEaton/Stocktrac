namespace Stocktrac.Api.Data;

internal class DatabaseConnectionOptions
{
    public required string Server { get; set; }
    public required string DatabaseName { get; set; }
    public bool IntegratedSecurity { get; set; }
    public required string Password { get; set; }
    public required string UserId { get; set; }
    public bool PersistSecurityInfo = false;
    public bool MultipleActiveResultSets = false;
    public bool Encrypt = true;
    public bool TrustServerCertificate { get; set; }
    public int ConnectTimeout = 30;
}
