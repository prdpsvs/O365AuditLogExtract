namespace AuditLogExtract.Data
{
    using AuditLogExtract.Entities;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    public interface IAdo
    {
        Task DeleteOldRequestsAsync(List<int> completedOldRequests);
        Task<List<FailedUrlEntry>> GetFailedRequestsAsync();
        Task<Tuple<string, string>> GetLastExecutionTimeStampAsync(int ingestionFrequencyInMinutes);
        Task<string> LoadStagingDataToMainTablesAsync();
        Task SqlBulkInsertAsync(DataTable payload, string tableName);
        Task TruncateStagingTablesAsync();
        Task<bool> UpdateLatestTimeStampAsync(string date);

    }
}