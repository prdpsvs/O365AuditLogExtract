namespace AuditLogExtract.Entities
{
    /// <summary>
    /// This entity contains attribues needed to complete service authentication
    /// </summary>
    public class ServiceAuthenticationContract
    {
        // Login URl for Azure
        public string LoginUrl { get; set; }

        // Tenant Id of an Organization
        public string TenantId { get; set; }

        // Application Registration Client Id
        public string ClientId { get; set; }

        // Application Registration Secret
        public string ClientSecret { get; set; }

        // Type of resource. Example: Power BI, Azure SQL Database, Office 365 and so on..
        public string ResourceUrl { get; set; }
    }
}
