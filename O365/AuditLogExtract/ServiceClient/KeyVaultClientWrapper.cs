namespace AuditLogExtract.ServiceClient
{
    using AuditLogExtract.Entities;
    using Microsoft.Azure.KeyVault;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    // <summary>
    /// KeyVault Client Wrapper class is used to authenticate Azure Application credentials against AAD to
    /// access Azure Key Vault and retrieve Secret Values
    /// </summary>

    [ExcludeFromCodeCoverage]
    public class KeyVaultClientWrapper : IKeyVaultClientWrapper
    {
        KeyVaultClient _keyVaultClient;
        AppSettings _appSettings;

        public KeyVaultClientWrapper(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }


        /// <summary>
        /// This method uses the token from AAD, connect to Azure Key Vault and creates a secret
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="secretValue"></param>
        /// <returns></returns>
        public async Task CreateSecretAsync(string secretName, string secretValue)
        {
            if (_keyVaultClient == null)
                _keyVaultClient = new KeyVaultClient(GetAccessToken);

            await _keyVaultClient.SetSecretAsync(_appSettings.KeyVaultUrl, secretName, secretValue);
        }

        /// <summary>
        /// This method uses the token from AAD, connect to Azure Key Vault and get secret value
        /// </summary>
        /// <param name="secret"></param>
        /// <returns>Secret Value</returns>
        public async Task<string> GetSecretAsync(string secret)
        {
            if (_keyVaultClient == null)
                _keyVaultClient = new KeyVaultClient(GetAccessToken);

            var secretValue = await _keyVaultClient.GetSecretAsync(_appSettings.KeyVaultUrl, secret).ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(secretValue.Value) ? string.Empty : secretValue.Value;
        }

        /// <summary>
        /// This method is used to get JWT token by providing client credentials to AAD
        /// </summary>
        /// <param name="azureTenantId"></param>
        /// <param name="azureClientId"></param>
        /// <param name="azureClientSecret"></param>
        /// <returns></returns>
        private async Task<string> GetAccessToken(string azureTenantId, string azureClientId, string azureClientSecret)
        {
            var context = new AuthenticationContext(_appSettings.LoginUrl + _appSettings.TenantId);
            var clientCredential = new ClientCredential(_appSettings.KeyClientId, _appSettings.KeyClientSecret);
            var tokrnResponse = await context.AcquireTokenAsync(_appSettings.KeyVaultTokenUrl, clientCredential).ConfigureAwait(false);
            return tokrnResponse.AccessToken;
        }
    }
}
