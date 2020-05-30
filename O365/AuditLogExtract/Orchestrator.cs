namespace AuditLogExtract
{
    using AuditLogExtract.Config;
    using AuditLogExtract.Data;
    using AuditLogExtract.Entities;
    using AuditLogExtract.Extensions;
    using AuditLogExtract.Instrumentation;
    using AuditLogExtract.ServiceClient;
    using log4net;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// This class contains methods to extract Audit Logs 
    /// from Office 365 and Ingest to Azure SQL Database
    /// </summary>
    public class Orchestrator
    {
        private ILog _log;
        private ILogAppender _logAppender;
        private Configuration _configuration;
        private IInitializer _initializer;
        private Tuple<string, string> AuditLogTimeStampsForPowerPlatform;
        private string _lastAuditLogTimeStampForPowerPlatform;
        private string _endTimeStampForPowerPlatform;
        private IAdo _ado;
        private IAuthenticationWrapper _authWrapper;
        private IRetryClient _retryClient;
        private IO365ActivityApiWrapper _o365ActivityApiWrapper;
        private object _syncRoot = new object();
        private Tuple<string, string> _timeStamps;
        private ServiceAuthenticationContract _O365ServiceAuthenticationContract;

        public Orchestrator(IInitializer initializer,
            ILog log, IAdo ado,
            IAuthenticationWrapper authWrapper, IRetryClient retryClient, ILogAppender logAppender,
            IO365ActivityApiWrapper o365ActivityApiWrapper)
        {

            _log = log;           
            _logAppender = logAppender;
            _authWrapper = authWrapper;
            _o365ActivityApiWrapper = o365ActivityApiWrapper;
            _ado = ado;
            _initializer = initializer;
            _configuration = _initializer.Configuration;
            _retryClient = retryClient;

            AuditLogTimeStampsForPowerPlatform = GetLatestTimeStampForPowerPlatform;

            _O365ServiceAuthenticationContract = new ServiceAuthenticationContract
            {
                ClientId = _configuration.AppSettings.AuditLogClientId,
                ClientSecret = _configuration.AppSettings.AuditLogClientSecret,
                LoginUrl = _configuration.AppSettings.LoginUrl,
                ResourceUrl = _configuration.AppSettings.Office365ResourceId,
                TenantId = _configuration.AppSettings.TenantId
            }; 
        }
        public Tuple<string, string> GetLatestTimeStampForPowerPlatform
        {
            get
            {
                lock (_syncRoot)
                {
                    if (_timeStamps == null)
                        Process().GetAwaiter().GetResult();

                    return _timeStamps;
                }
            }
        }

        /// <summary>
        /// This method will create a instantiate a
        /// </summary>
        /// <returns></returns>
        private async Task Process()
        {
            if (_timeStamps == null)
            {
                //Build Configurations
                await _initializer.InitializeConfigurationAsync().ConfigureAwait(false);

                //Initialize Appenders and generate loggers
                _logAppender.Initialize(new List<string>());
                GlobalContext.Properties["BatchId"] = _configuration.AppSettings.ReleaseId;
                _timeStamps = await _ado.GetLastExecutionTimeStampAsync(_configuration.AppSettings.IngestionFrequencyInMinutes).ConfigureAwait(false);
                ;
            }
        }


        /// <summary>
        /// This method is an orchestrator for following activities
        /// 1. Start the subscription
        /// 2. List the subscription
        /// 3. Read content blob urls between start and end date
        /// 4. Read failed requests from previous batches/executions and insert activities to Lists
        /// 5. Read data from current batche and insert activities to the same list
        /// 6. Insert activities to Staging table
        /// 7. Load activities from Staging table to Main table
        /// 8. Clean up & update settings for next execution
        /// </summary>
        /// <returns></returns>
        public async Task ExtractOffice365AuditLogsAsync()
        {
            _lastAuditLogTimeStampForPowerPlatform = AuditLogTimeStampsForPowerPlatform.Item1;
            _endTimeStampForPowerPlatform = AuditLogTimeStampsForPowerPlatform.Item2;

            try
            {
                // Start the subscription
                var startSubscriptionResponse = await _o365ActivityApiWrapper.StartSubscriptionAsync().ConfigureAwait(false);
                if (startSubscriptionResponse.IsSuccessStatusCode)
                {
                    //List the subscription
                    var listSubscriptionResponse = await _o365ActivityApiWrapper.ListSubscriptionAsync().ConfigureAwait(false);
                    if (listSubscriptionResponse.IsSuccessStatusCode)
                    {
                        //Manage date and time
                        if (string.IsNullOrWhiteSpace(_lastAuditLogTimeStampForPowerPlatform))
                        {
                            _lastAuditLogTimeStampForPowerPlatform = DateTime.UtcNow.AddHours(-24).ToString("yyyy-MM-ddTHH:mm");
                            _endTimeStampForPowerPlatform = DateTime.ParseExact(_lastAuditLogTimeStampForPowerPlatform, "yyyy-MM-ddTHH:mm", null)
                                .AddMinutes(_configuration.AppSettings.IngestionFrequencyInMinutes).ToString("yyyy-MM-ddTHH:mm");
                        }
                        else
                        {
                            _lastAuditLogTimeStampForPowerPlatform = Convert.ToDateTime(_lastAuditLogTimeStampForPowerPlatform).ToString("yyyy-MM-ddTHH:mm");
                            _endTimeStampForPowerPlatform = Convert.ToDateTime(_endTimeStampForPowerPlatform).ToString("yyyy-MM-ddTHH:mm");
                        }

                        ValidateStartEndTime(_endTimeStampForPowerPlatform, _lastAuditLogTimeStampForPowerPlatform);

                        //Retrieve content blobs between start and end time.  
                        var contentUris = await _o365ActivityApiWrapper.RetrieveBlobUrisAsync(_lastAuditLogTimeStampForPowerPlatform, _endTimeStampForPowerPlatform).ConfigureAwait(false);
                        _log.Info($"Retrived {contentUris.Count} content blobs between {_lastAuditLogTimeStampForPowerPlatform} and {_endTimeStampForPowerPlatform} time period");

                        //Read Previous failed blob url request and retry those
                        var failedRequests = await _ado.GetFailedRequestsAsync().ConfigureAwait(false);

                        List<int> completedOldRequests = new List<int>();
                        List<Activity> oldPowerAppActivities = new List<Activity>();
                        List<Activity> oldPowerBIActivities = new List<Activity>(); ;
                        List<Activity> oldPowerAutomateActivities = new List<Activity>();

                        if (failedRequests.Count > 0)
                            (completedOldRequests, oldPowerAppActivities, oldPowerBIActivities, oldPowerAutomateActivities)  = await RetryOldRequestsAsync(failedRequests).ConfigureAwait(false);

                        //Read activities from Office 365 and load Power BI, Power App and Power Automate in memory
                        List<string> failedBlobUrls = new List<string>();
                        List<Activity> powerAppActivities = new List<Activity>();
                        List<Activity> powerBIActivities = new List<Activity>(); ;
                        List<Activity> powerAutomateActivities = new List<Activity>();

                        (failedBlobUrls, powerAppActivities, powerBIActivities, powerAutomateActivities) = await _o365ActivityApiWrapper.RetrieveAuditActivititesFromBlobContentAsync(contentUris).ConfigureAwait(false);

                        powerAutomateActivities.AddRange(oldPowerAutomateActivities);
                        powerAppActivities.AddRange(oldPowerAppActivities);
                        powerBIActivities.AddRange(oldPowerBIActivities);

                        oldPowerBIActivities.Clear();
                        oldPowerAppActivities.Clear();
                        oldPowerAutomateActivities.Clear();

                        LogAppender.FlushApplicationInsightsTelemetry();

                        //Bulk insert failed records
                        if (failedBlobUrls.Count > 0)
                            await InsertFailedBlobUrlsToDatabaseAsync(failedBlobUrls).ConfigureAwait(false);

                        //Truncate Staging tables
                        await _ado.TruncateStagingTablesAsync().ConfigureAwait(false);

                        //Load Audit data to staging table
                        await LoadPowerPlatformLogsToStagingTableAsync(powerAppActivities, "stg.PowerAppsAuditLog").ConfigureAwait(false);
                        await LoadPowerPlatformLogsToStagingTableAsync(powerBIActivities, "stg.PowerAutomateAuditLog").ConfigureAwait(false);
                        await LoadPowerPlatformLogsToStagingTableAsync(powerAutomateActivities, "stg.PowerBIAuditLog").ConfigureAwait(false);
                        _log.Info($"Ingested PowerPlatform logs to staging tables..");

                        //Unobserved Operations are marked in data ingestion from staging to main tables
                        //Load Audit data from staging to main tables
                        await _ado.LoadStagingDataToMainTablesAsync().ConfigureAwait(false);
                        _log.Info($"Exploded and ingested audit logs to main tables from staging area..");

                        //Delete completed old requests
                        await _ado.DeleteOldRequestsAsync(completedOldRequests).ConfigureAwait(false);

                        //Update Latesttime stamp
                        bool istimeStampUpdated = await _ado.UpdateLatestTimeStampAsync(_endTimeStampForPowerPlatform + ":00").ConfigureAwait(false);
                        _log.Info("Updated last refresh timestamp for next refresh..");

                        //Truncate Staging tables
                        await _ado.TruncateStagingTablesAsync().ConfigureAwait(false);
                        _log.Info("Cleaned staging tables..");

                        LogAppender.FlushApplicationInsightsTelemetry();
                        //Data Archival
                    }
                    else
                    {
                        var errorMessage = await listSubscriptionResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                        _log.Error($"Unable to list subscriptions to pull the audit logs. Please find the error {errorMessage}.");
                    }
                }
                else
                {
                    var errorMessage = await startSubscriptionResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    _log.Error($"Unable to start the subscription to pull the audit logs. Please find the error {errorMessage}.");
                }
            }
            catch (AggregateException ex)
            {
                var aggregateException = ex.Flatten();
                for (int i = 0; i < aggregateException.InnerExceptions.Count; ++i)
                    _log.Error($"{aggregateException.InnerExceptions[i].GetExceptionFootprints()}");
                throw;
            }
            catch (Exception ex)
            {
                _log.Error($"{ex.GetExceptionFootprints()}");
                throw;
            }
            finally
            {
                LogAppender.FlushApplicationInsightsTelemetry();
            }
        }

        /// <summary>
        /// This method does following validations with start and end timestamp 
        /// 1. Checks if the gap between current time & diff between start and end time is less than 16 hours
        /// 2. Checks if start time is greater than end time
        /// 3. Checks if ingestion frequency is greater than configured value
        /// </summary>
        /// <param name="endTimeStampForPowerPlatform"></param>
        /// <param name="lastAuditLogTimeStampForPowerPlatform"></param>
        private void ValidateStartEndTime(string endTimeStampForPowerPlatform, string lastAuditLogTimeStampForPowerPlatform)
        {
            var e = Convert.ToDateTime(endTimeStampForPowerPlatform);
            var s = Convert.ToDateTime(lastAuditLogTimeStampForPowerPlatform);
            var datecompare = DateTime.Compare(s, e);
            var currentdiff = DateTime.UtcNow.Subtract(e);

            if (currentdiff.TotalHours < 16)
            {
                _log.Error($"Product Group recommends to wait for a day for consistent Audit Logs");
                throw new Exception("Product Group recommends to wait for a day for consistent Audit Logs");
            }

            if (datecompare == 0 || datecompare > 0)
            {
                _log.Error($"Start date should be less than end date");
                throw new Exception("Start date should be less than end date");
            }
        }

        /// <summary>
        /// 1. Retry old requests
        /// 2. Add completed list to a separate list
        /// 3. Add activities to respective Power Platform lists
        /// </summary>
        /// <param name="oldRequests"></param>
        /// <param name="retry"></param>
        /// <returns></returns>
        private async Task<(List<int>, List<Activity>, List<Activity>, List<Activity>)> RetryOldRequestsAsync(List<FailedUrlEntry> oldRequests)
        {
            List<int> completedOldRequests = new List<int>();
            List<Activity> powerAppActivities = new List<Activity>();
            List<Activity> powerBIActivities = new List<Activity>(); ;
            List<Activity> powerAutomateActivities = new List<Activity>();
            try
            {
                _log.Info("#################### OLD REQUESTS  ########################");
                int count = 0;
                foreach (var item in oldRequests)
                {
                    count++;

                    var response = await _retryClient.GetAsyncWithRetryAsync(item.Url, _O365ServiceAuthenticationContract).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        _log.Error($"The request failed with {await response.Content.ReadAsStringAsync()} for {item.Url}. Please try the request later.");
                    }
                    else
                    {
                        completedOldRequests.Add(item.Id);
                        string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        var jarray = JArray.Parse(content);
                        (powerAppActivities, powerBIActivities, powerAutomateActivities) = _o365ActivityApiWrapper.FilterAndAddObjectToPowerPlatformObjects(jarray);

                    }

                    _log.Info($"No of pages read {count} out of {oldRequests.Count} pages");

                }

                _log.Info($"No of activities available for Power BI between {_lastAuditLogTimeStampForPowerPlatform} and {_endTimeStampForPowerPlatform}: {powerBIActivities.Count}");
                _log.Info($"No of activities available for Power Apps between {_lastAuditLogTimeStampForPowerPlatform} and {_endTimeStampForPowerPlatform}:  {powerAppActivities.Count}");
                _log.Info($"No of activities available for Power Automate between {_lastAuditLogTimeStampForPowerPlatform} and {_endTimeStampForPowerPlatform}:  {powerAutomateActivities.Count}");
                _log.Info("#################### OLD REQUESTS ########################");

                return (completedOldRequests, powerAppActivities, powerBIActivities, powerAutomateActivities);
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
        /// 1. Add Data Rows to Data Table
        /// 2. Bulk insert to Staging tables
        /// </summary>
        /// <param name="stageLogs"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private async Task LoadPowerPlatformLogsToStagingTableAsync(List<Activity> stageLogs, string tableName)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("ActivityId", typeof(Guid));
            dataTable.Columns.Add("Activity", typeof(string));
            dataTable.Columns.Add("Operation", typeof(string));

            foreach (var activity in stageLogs)
            {
                DataRow dr = dataTable.NewRow();
                dr["Activity"] = activity.RawActivity;
                dr["ActivityId"] = activity.ActivityId.ToString();
                dr["Operation"] = activity.Operation.ToString();

                dataTable.Rows.Add(dr);
                if (dataTable.Rows.Count > _configuration.AppSettings.BulkInsertPayloadSize)
                {
                    await _ado.SqlBulkInsertAsync(dataTable, tableName).ConfigureAwait(false);
                    dataTable.Rows.Clear();
                }
            }
            if (dataTable.Rows.Count > 0)
                await _ado.SqlBulkInsertAsync(dataTable, tableName).ConfigureAwait(false);

        }

        /// <summary>
        /// Insert failed entries to auditlog.FailedBlobUrls table
        /// </summary>
        /// <returns></returns>
        private async Task InsertFailedBlobUrlsToDatabaseAsync(List<string> failedBlobUrls)
        {
            string tableName = _configuration.AppSettings.FailedBlobUrlTable;
            var dataTable = new DataTable();
            dataTable.Columns.Add("Url", typeof(string));
            dataTable.Columns.Add("InsertedDate", typeof(DateTime));

            foreach (var url in failedBlobUrls)
            {
                DataRow dr = dataTable.NewRow();
                dr["Url"] = url;
                dr["InsertedDate"] = DateTime.UtcNow;

                dataTable.Rows.Add(dr);
                if (dataTable.Rows.Count > _configuration.AppSettings.BulkInsertPayloadSize)
                {
                    await _ado.SqlBulkInsertAsync(dataTable, tableName).ConfigureAwait(false);
                    dataTable.Rows.Clear();
                }
            }
            if (dataTable.Rows.Count > 0)
                await _ado.SqlBulkInsertAsync(dataTable, tableName).ConfigureAwait(false);
        }

    }
}
