using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditLogExtract.Entities
{
    public class AppSettings
    {
        public string LoginUrl { get; set; }
        public string AzureManagementUrl { get; set; }
        public string TenantId { get; set; }
        public string AzureSubscriptionId { get; set; }
        public string KeyVaultTokenUrl { get; set; }
        public string KeyVaultUrl { get; set; }
        public string KeyClientId { get; set; }
        public string KeyClientSecret { get; set; }
        public bool LogVerbosity { get; set; }
        public string SqlServerName { get; set; }
        public string SqlDatabaseName { get; set; }
        public string DatabaseResourceId { get; set; }
        public string DatabaseClientId { get; set; }
        public string DatabaseClientSecret { get; set; }
        public string DatabaseClientIdName { get; set; }
        public string DatabaseClientSecretName { get; set; }
        public string Env { get; set; }
        public string DevSqlConnectionString { get; set; }
        public string ReleaseId { get; set; }   
        public string AuditLogClientIdName { get; set; }
        public string AuditLogClientSecretName { get; set; }
        public string AuditLogClientId { get; set; }
        public string AuditLogClientSecret { get; set; }
        public string Office365ResourceId { get; set; }
        public int BulkInsertPayloadSize { get; set; }        
        public string TelemetryInstrumentationKey { get; set; }
        public int IngestionFrequencyInMinutes { get; set; }
        public string FailedBlobUrlTable { get; set; }

    }
}
