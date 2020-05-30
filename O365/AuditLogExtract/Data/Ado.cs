namespace AuditLogExtract.Data
{
    using AuditLogExtract.Config;
    using AuditLogExtract.Data;
    using AuditLogExtract.Entities;
    using AuditLogExtract.ServiceClient;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    public class Ado : IAdo
    {
        protected Configuration _configuration;
        protected ILog _log;
        protected IAuthenticationWrapper _authWrapper;
        protected IMetadataStoreContextFactory _metadataStoreContextFactory;

        public Ado(IInitializer initializer, ILog log,
           IAuthenticationWrapper authWrapper, IMetadataStoreContextFactory metadataStoreContextFactory
          )
        {
            _configuration = initializer.Configuration;
            _log = log;
            _authWrapper = authWrapper;
            _metadataStoreContextFactory = metadataStoreContextFactory;
        }

       

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<FailedUrlEntry>> GetFailedRequestsAsync()
        {
            using (var conn = await _metadataStoreContextFactory.CreateConnectionAsync().ConfigureAwait(false))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "auditlog.uspGetFailedUrls";

                var dataResult = new DataTable();
                dataResult.Locale = CultureInfo.InvariantCulture;

                conn.Open();
                var da = new SqlDataAdapter(cmd);
                await Task.Run(() => da.Fill(dataResult)).ConfigureAwait(false);
                da.Dispose();

                List<FailedUrlEntry> list = dataResult.AsEnumerable().Select(row =>
                    new FailedUrlEntry
                    {
                        Id = row.Field<int>("Id"),
                        Url = row.Field<string>("Url")

                    }).ToList();

                return list;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="completedOldRequests"></param>
        /// <returns></returns>
        public async Task DeleteOldRequestsAsync(List<int> completedOldRequests)
        {
            using (var conn = await _metadataStoreContextFactory.CreateConnectionAsync().ConfigureAwait(false))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "auditlog.uspDeleteExecutedUrls";

                var tableTypeParam = new SqlParameter("@ExecutedUrls", SqlDbType.Structured);
                tableTypeParam.Value = CreateDataTable(completedOldRequests);
                cmd.Parameters.Add(tableTypeParam);

                conn.Open();
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Helper Method
        /// </summary>
        /// <param name="completedOldRequests"></param>
        /// <returns></returns>
        private DataTable CreateDataTable(List<int> completedOldRequests)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            foreach (var item in completedOldRequests)
                table.Rows.Add(item);

            return table;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public async Task SqlBulkInsertAsync(DataTable payload, string tableName)
        {
            using (var conn = await _metadataStoreContextFactory.CreateConnectionAsync().ConfigureAwait(false))
            using (SqlBulkCopy s = new SqlBulkCopy(conn))
            {
                conn.Open();
                s.DestinationTableName = tableName;
                s.BulkCopyTimeout = 0;
                s.BatchSize = _configuration.AppSettings.BulkInsertPayloadSize;

                foreach (var column in payload.Columns)
                {
                    s.ColumnMappings.Add(column.ToString(), column.ToString());
                }

                var reader = payload.CreateDataReader();
                await s.WriteToServerAsync(reader).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<string> LoadStagingDataToMainTablesAsync()
        {
            using (var conn = await _metadataStoreContextFactory.CreateConnectionAsync().ConfigureAwait(false))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "stg.uspLoadAuditLogDataFromStagingToMainTables";

                var outputParameter = new SqlParameter("Status", SqlDbType.NVarChar, 1000);
                outputParameter.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(outputParameter);

                conn.Open();
                var da = new SqlDataAdapter(cmd);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                return Convert.ToString(outputParameter.Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<string, string>> GetLastExecutionTimeStampAsync(int ingestionFrequencyInMinutes)
        {
            using (var conn = await _metadataStoreContextFactory.CreateConnectionAsync().ConfigureAwait(false))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "auditlog.uspGetLatestTimestamp";

                var inputParameter = new SqlParameter("IngestionFrequencyInMinutes", SqlDbType.Int);
                inputParameter.Direction = ParameterDirection.Input;
                inputParameter.Value = ingestionFrequencyInMinutes;
                cmd.Parameters.Add(inputParameter);

                var outputParameter = new SqlParameter("LatestTimestamp", SqlDbType.VarChar, 100);
                outputParameter.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(outputParameter);

                var outputParameter1 = new SqlParameter("EndTimestamp", SqlDbType.VarChar, 100);
                outputParameter1.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(outputParameter1);

                conn.Open();
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                return Tuple.Create(Convert.ToString(outputParameter.Value), Convert.ToString(outputParameter1.Value));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task TruncateStagingTablesAsync()
        {
            using (var conn = await _metadataStoreContextFactory.CreateConnectionAsync().ConfigureAwait(false))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "stg.uspTruncateAuditLogStageTables";

                conn.Open();
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public async Task<bool> UpdateLatestTimeStampAsync(string date)
        {
            using (var conn = await _metadataStoreContextFactory.CreateConnectionAsync().ConfigureAwait(false))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "auditlog.uspUpdateLatestTimestamp";

                var remarksParam = new SqlParameter("Timestamp", SqlDbType.VarChar, 50);
                remarksParam.Value = date;
                cmd.Parameters.Add(remarksParam);

                var outputParameter = new SqlParameter("RowsUpdated", SqlDbType.Int);
                outputParameter.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(outputParameter);

                conn.Open();
                var da = new SqlDataAdapter(cmd);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                return Convert.ToInt32(outputParameter.Value) == 1 ? true : false;
            }
        }

    }
}
