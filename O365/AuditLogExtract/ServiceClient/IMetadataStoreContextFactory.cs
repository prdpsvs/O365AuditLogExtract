namespace AuditLogExtract.ServiceClient
{
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    public interface IMetadataStoreContextFactory
    {
        Task<SqlConnection> CreateConnectionAsync();
    }
}
