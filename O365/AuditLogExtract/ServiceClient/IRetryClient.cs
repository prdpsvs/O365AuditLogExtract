namespace AuditLogExtract.ServiceClient
{
    using AuditLogExtract.Entities;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IRetryClient
    {
        Task<HttpResponseMessage> GetAsyncWithRetryAsync(string url, ServiceAuthenticationContract serviceAuthenticationContract);
        Task<HttpResponseMessage> PostAsyncWithRetryAsync(string url, StringContent content, ServiceAuthenticationContract serviceAuthenticationContract);
    }
}
