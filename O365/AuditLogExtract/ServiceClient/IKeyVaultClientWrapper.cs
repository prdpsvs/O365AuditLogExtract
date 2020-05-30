namespace AuditLogExtract.ServiceClient
{
    using System.Threading.Tasks;

    /// <summary>
    /// This Interface has contracts to create and get secret from key vault
    /// </summary>
    public interface IKeyVaultClientWrapper
    {
        // Contract to get secret from key vault
        Task<string> GetSecretAsync(string secret);

        // Contract to create secret in key vault
        Task CreateSecretAsync(string secretName, string secretValue);
    }
}
