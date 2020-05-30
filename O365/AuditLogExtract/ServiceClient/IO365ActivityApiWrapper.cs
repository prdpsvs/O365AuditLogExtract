namespace AuditLogExtract.ServiceClient
{
    using AuditLogExtract.Entities;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IO365ActivityApiWrapper
    {
        Task<HttpResponseMessage> ListSubscriptionAsync();
        Task<(List<string>, List<Activity>, List<Activity>, List<Activity>)> RetrieveAuditActivititesFromBlobContentAsync(List<string> contentUris);

        (List<Activity>, List<Activity>, List<Activity>) FilterAndAddObjectToPowerPlatformObjects(JArray activities);
        Task<List<string>> RetrieveBlobUrisAsync(string starttime, string endtime);
        Task<HttpResponseMessage> StartSubscriptionAsync();
    }
}