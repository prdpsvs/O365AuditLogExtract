namespace AuditLogExtract.ServiceClient
{
    using AuditLogExtract.Config;
    using AuditLogExtract.Entities;
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Environment = Entities.Environment;

    public class MetadataStoreContextFactory : IMetadataStoreContextFactory
    {
        private Configuration _configuration;
        private IAuthenticationWrapper _authWrapper; 
        private ServiceAuthenticationContract _sqlServiceAuthenticationContract;


        public MetadataStoreContextFactory(IInitializer initializer, IAuthenticationWrapper authWrapper)
        {
            _configuration = initializer.Configuration;
            _authWrapper = authWrapper;

            _sqlServiceAuthenticationContract = new ServiceAuthenticationContract
            {
                ClientId = _configuration.AppSettings.DatabaseClientId,
                ClientSecret = _configuration.AppSettings.DatabaseClientSecret,
                LoginUrl = _configuration.AppSettings.LoginUrl,
                ResourceUrl = _configuration.AppSettings.DatabaseResourceId,
                TenantId = _configuration.AppSettings.TenantId
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<SqlConnection> CreateConnectionAsync()
        {
            var result = await _authWrapper.GetAuthenticationResult(_sqlServiceAuthenticationContract).ConfigureAwait(false);

            SqlConnection sqlConnection;
            if (_configuration.AppSettings.Env == Enum.GetName(typeof(Environment.Env), 0))
                sqlConnection = new SqlConnection(_configuration.AppSettings.DevSqlConnectionString);
            else
                sqlConnection = GetSqlConnection(result.AccessToken);
            return sqlConnection;
        }

        /// <summary>
        /// Build SQL Connection with AAD Token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private SqlConnection GetSqlConnection(string token)
        {
            var builder = new SqlConnectionStringBuilder();
            builder["Data Source"] = _configuration.AppSettings.SqlServerName;
            builder["Initial Catalog"] = _configuration.AppSettings.SqlDatabaseName;
            builder["Connect Timeout"] = 30;
            builder["Persist Security Info"] = false;
            builder["TrustServerCertificate"] = false;
            builder["Encrypt"] = true;
            builder["MultipleActiveResultSets"] = false;
            var con = new SqlConnection(builder.ToString());
            con.AccessToken = token;
            return con;
        }
    }
}
