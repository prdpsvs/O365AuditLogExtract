namespace AuditLogExtract.ServiceClient
{
    using AuditLogExtract.Entities;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using System;
    using System.Threading.Tasks;

    public class AuthenticationWrapper : IAuthenticationWrapper
    {
        /// <summary>
        /// This method authenticates app registration against resource and returns JWT access token. 
        /// The access token can be used to access Azure Resource 
        /// </summary>
        /// <param name="serviceAuthenticationContract"></param>
        /// <returns> Authentication Result</returns>
        public async Task<AuthenticationResult> GetAuthenticationResult(ServiceAuthenticationContract serviceAuthenticationContract)
        {
            var authContext = new AuthenticationContext(string.Format("{0}/{1}", serviceAuthenticationContract.LoginUrl, serviceAuthenticationContract.TenantId));
            var clientCredential = new ClientCredential(serviceAuthenticationContract.ClientId, serviceAuthenticationContract.ClientSecret);
            var result = await authContext.AcquireTokenAsync(serviceAuthenticationContract.ResourceUrl, clientCredential);

            if (result == null)
                throw new InvalidOperationException("Could not get token");

            return result;
        }
    }
}
