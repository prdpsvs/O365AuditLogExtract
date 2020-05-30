namespace AuditLogExtract.ServiceClient
{
    using AuditLogExtract.Entities;
    using AuditLogExtract.Extensions;
    using log4net;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Polly;
    using Polly.Retry;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class RetryClient : IRetryClient
    {
        public RetryClient(ILog log, IAuthenticationWrapper authWrapper)
        {
            _log = log;
            _authWrapper = authWrapper;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(1);
            _asyncRetryPolicy = Policy.Handle<HttpRequestException>()
                            .Or<TaskCanceledException>()
                            .Or<Exception>()
                            .OrResult<HttpResponseMessage>(r => r.StatusCode != System.Net.HttpStatusCode.OK)
                            .RetryAsync(3, (exception, retryCount, context) =>
                            {
                                _log.Error($"RetryCount: {retryCount}, {exception.Exception.GetExceptionFootprints()}");
                            });

        }
        private HttpClient _httpClient;
        private AsyncRetryPolicy<HttpResponseMessage> _asyncRetryPolicy;
        private ILog _log;
        private IAuthenticationWrapper _authWrapper;
        private AuthenticationResult _authenticationResult;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="serviceAuthenticationContract"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetAsyncWithRetryAsync(string url, ServiceAuthenticationContract serviceAuthenticationContract)
        {
            try
            {
                var response = await _asyncRetryPolicy.ExecuteAsync(async () =>
                {
                    await ApplyAccessTokenAsync(serviceAuthenticationContract).ConfigureAwait(false);
                    return await _httpClient.GetAsync(url).ConfigureAwait(false);
                });
                return response;
            }
            catch (Exception e)
            {
                _log.Error(e.GetExceptionFootprints());
                return new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Content = new StringContent($"Request Failed {e.GetExceptionFootprints()}")
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <param name="serviceAuthenticationContract"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostAsyncWithRetryAsync(string url, StringContent content, ServiceAuthenticationContract serviceAuthenticationContract)
        {
            try
            {
                var response = await _asyncRetryPolicy.ExecuteAsync(async () =>
                {
                    await ApplyAccessTokenAsync(serviceAuthenticationContract).ConfigureAwait(false);
                    return await _httpClient.PostAsync(url, null).ConfigureAwait(false);
                });
                return response;
            }
            catch (Exception e)
            {
                _log.Error(e.GetExceptionFootprints());
                return new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Content = new StringContent($"Request Failed {e.GetExceptionFootprints()}")
                };
            }
        }

        /// <summary>
        /// Fetches Office 365 access token. If Token is expired, 
        /// </summary>
        /// <returns></returns>
        private async Task ApplyAccessTokenAsync(ServiceAuthenticationContract serviceAuthenticationContract)
        {
            if (_authenticationResult == null || DateTime.UtcNow >= _authenticationResult.ExpiresOn)
            {
                _authenticationResult = await _authWrapper.GetAuthenticationResult(serviceAuthenticationContract);
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _authenticationResult.AccessToken);
            }
        }
    }
}
