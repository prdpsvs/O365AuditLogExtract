namespace AuditLogExtract.ServiceClient
{
    using AuditLogExtract.Config;
    using AuditLogExtract.Entities;
    using log4net;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class O365ActivityApiWrapper : IO365ActivityApiWrapper
    {
        private ILog _log;
        private IRetryClient _retryClient;
        private Configuration _configuration;
        private ServiceAuthenticationContract _O365ServiceAuthenticationContract;

        public O365ActivityApiWrapper(ILog log, IInitializer initializer, IRetryClient retryClient)
        {
            _log = log;
            _retryClient = retryClient;
            _configuration = initializer.Configuration;

            _O365ServiceAuthenticationContract = new ServiceAuthenticationContract
            {
                ClientId = _configuration.AppSettings.AuditLogClientId,
                ClientSecret = _configuration.AppSettings.AuditLogClientSecret,
                LoginUrl = _configuration.AppSettings.LoginUrl,
                ResourceUrl = _configuration.AppSettings.Office365ResourceId,
                TenantId = _configuration.AppSettings.TenantId
            };
        }

        /// <summary>
        /// Start Office 365 Management Activity Api subscription
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponseMessage> StartSubscriptionAsync()
        {
            try
            {
                string endpoint = $"{_configuration.AppSettings.Office365ResourceId }/api/v1.0/{_configuration.AppSettings.TenantId}/activity/feed/subscriptions/start?contentType=Audit.General";
                var response = await _retryClient.PostAsyncWithRetryAsync(endpoint, null, _O365ServiceAuthenticationContract).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    _log.Error($"Unable to start subscription - Audit.General  {await response.Content.ReadAsStringAsync()}");
                    throw new Exception($"Unable to start the subscription");
                }
                else
                {
                    _log.Info($"Started subscription for Audit.General {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                    return response;
                }

            }
            catch (Exception ex)
            {
                _log.Error($"Unable to start the subscription {ex.Message}\n {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// List Office 365 Management Activity Api subscription
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponseMessage> ListSubscriptionAsync()
        {
            try
            {
                string endpoint = $"https://manage.office.com/api/v1.0/{_configuration.AppSettings.TenantId}/activity/feed/subscriptions/list";
                var response = await _retryClient.GetAsyncWithRetryAsync(endpoint, _O365ServiceAuthenticationContract).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    _log.Error($"Unable to list subscription - Audit.General  {await response.Content.ReadAsStringAsync()}");
                    throw new Exception($"Unable to list subscription - Audit.General"); ;
                }
                else
                {
                    _log.Info($"Listing subscriptions for Audit.General {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                    return response;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Unable to list subscriptions {ex.Message}\n {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// 1. Retrieve blob URLS and prepare URL list
        /// </summary>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        /// <returns></returns>
        public async Task<List<string>> RetrieveBlobUrisAsync(string starttime, string endtime)
        {
            List<string> contentUris = new List<string>();
            string nextpageUri = $"https://manage.office.com/api/v1.0/{_configuration.AppSettings.TenantId}/activity/feed/subscriptions/content?contentType=Audit.General&PublisherIdentifier={_configuration.AppSettings.TenantId}&startTime={starttime}&endTime={endtime}";

            try
            {
                do
                {
                    JArray root = null;
                    var response = await _retryClient.GetAsyncWithRetryAsync(nextpageUri, _O365ServiceAuthenticationContract).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        _log.Error($"Unable to retrieve blob content for Audit.General {await response.Content.ReadAsStringAsync()}");
                        throw new Exception($"{await response.Content.ReadAsStringAsync()}");
                    }
                    else
                    {
                        var contentList = response.Content.ReadAsStringAsync().Result;
                        root = JArray.Parse(contentList);

                        foreach (var item in root.Children())
                        {
                            var itemProperties = item.Children<JProperty>();
                            //you could do a foreach or a linq here depending on what you need to do exactly with the value
                            var myElement = itemProperties.FirstOrDefault(x => x.Name == "contentUri");
                            var myElementValue = myElement.Value.ToString(); ////This is a JValue type
                            contentUris.Add(myElement.Value.ToString());

                        }
                        IEnumerable<string> values = null;
                        nextpageUri = string.Empty;
                        if (response.Headers.TryGetValues("NextPageUri", out values))
                        {
                            nextpageUri = values.First();
                        }

                    }


                } while (!string.IsNullOrWhiteSpace(nextpageUri));

                return contentUris;
            }
            catch (AggregateException ex)
            {
                var aggregateException = ex.Flatten();
                for (int i = 0; i < aggregateException.InnerExceptions.Count; ++i)
                {
                    var canceledException = aggregateException.InnerExceptions[i];
                    _log.Error($"Error in retrieving blob content urls {canceledException.Message}\n {canceledException.StackTrace}");
                }
                throw;
            }
            catch (Exception ex)
            {
                _log.Error($"Error in retrieving blob content urls {ex.Message}\n {ex.StackTrace}");
                throw;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentUris"></param>
        /// <returns> powerAppActivities, powerBIActivities, powerAutomateActivities </returns>
        public async Task<(List<string>, List<Activity>, List<Activity>, List<Activity>)> RetrieveAuditActivititesFromBlobContentAsync(List<string> contentUris)
        {
            List<string> failedBlobUrls = new List<string>();
            List<Activity> powerAppActivities = new List<Activity>();
            List<Activity> powerBIActivities = new List<Activity>(); ;
            List<Activity> powerAutomateActivities = new List<Activity>();
            try
            {
                int count = 0;
                foreach (var item in contentUris)
                {
                    count++;

                    var response = await _retryClient.GetAsyncWithRetryAsync(item, _O365ServiceAuthenticationContract).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        failedBlobUrls.Add(item);
                        _log.Error($"The request failed with {await response.Content.ReadAsStringAsync()} for {item}. Please try the request later.");
                    }
                    else
                    {
                        string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var jarray = JArray.Parse(content);
                        (powerAppActivities, powerBIActivities, powerAutomateActivities) = FilterAndAddObjectToPowerPlatformObjects(jarray);
                    }

                    _log.Info($"No of pages read {count} out of {contentUris.Count} pages");

                }
                //_log.Info($"No of activities available for Power BI between {_lastAuditLogTimeStampForPowerPlatform} and {_endTimeStampForPowerPlatform}: {largerPowerBiObject.Count}");
                //_log.Info($"No of activities available for Power Apps between {_lastAuditLogTimeStampForPowerPlatform} and {_endTimeStampForPowerPlatform}:  {largerPowerAppsObject.Count}");
                //_log.Info($"No of activities available for Power Automate between {_lastAuditLogTimeStampForPowerPlatform} and {_endTimeStampForPowerPlatform}:  {largerPowerAutomateObject.Count}");

                return (failedBlobUrls, powerAppActivities, powerBIActivities, powerAutomateActivities);

            }
            catch (AggregateException ex)
            {
                var aggregateException = ex.Flatten();
                for (int i = 0; i < aggregateException.InnerExceptions.Count; ++i)
                {
                    var canceledException = aggregateException.InnerExceptions[i];
                    _log.Error($"Error in retrieving content url {canceledException.Message}\n {canceledException.StackTrace}");
                }
                throw;
            }
            catch (TaskCanceledException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                _log.Error($"Error in retrieving content url {ex.Message}\n {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jarray"></param>
        /// <returns> powerAppActivities, powerBIActivities, powerAutomateActivities </returns>
        public (List<Activity>, List<Activity>, List<Activity>) FilterAndAddObjectToPowerPlatformObjects(JArray jarray)
        {
            List<Activity> powerAppActivities = new List<Activity>();
            List<Activity> powerBIActivities = new List<Activity>(); ;
            List<Activity> powerAutomateActivities = new List<Activity>();
            for (int i = 0; i < jarray.Count; i++)
            {
                if (jarray[i]["Workload"].ToString().ToLower() == "powerapps")
                    powerAppActivities.Add(new Activity { ActivityId = Guid.Parse(jarray[i]["Id"].ToString()), RawActivity = jarray[i].ToString(), Operation = jarray[i]["Operation"].ToString() });

                if (jarray[i]["Workload"].ToString().ToLower() == "powerbi")
                    powerBIActivities.Add(new Activity { ActivityId = Guid.Parse(jarray[i]["Id"].ToString()), RawActivity = jarray[i].ToString(), Operation = jarray[i]["Operation"].ToString() });

                if (jarray[i]["Workload"].ToString().ToLower() == "microsoftflow")
                    powerAutomateActivities.Add(new Activity { ActivityId = Guid.Parse(jarray[i]["Id"].ToString()), RawActivity = jarray[i].ToString(), Operation = jarray[i]["Operation"].ToString() });
            }

            return (powerAppActivities, powerBIActivities, powerAutomateActivities);

        }

    }
}
