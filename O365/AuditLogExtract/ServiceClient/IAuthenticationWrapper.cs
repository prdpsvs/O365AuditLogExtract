namespace AuditLogExtract.ServiceClient
{
    using AuditLogExtract.Entities;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using System.Threading.Tasks;

    /// <summary>
    /// This Interface has contracts to enable Service Authentication
    /// </summary>
    public interface IAuthenticationWrapper
    {
        // This method use ADAL to fetch JWT service access token against Azure Resource specified in contract
        Task<AuthenticationResult> GetAuthenticationResult(ServiceAuthenticationContract serviceAuthenticationContract);
    }
}
