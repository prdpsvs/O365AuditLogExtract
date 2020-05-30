using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditLogExtract.ServiceClient
{
    public class SqlConnectionProperties
    {
        private SqlConnectionStringBuilder _sqlConnectionString;

        public void SetSqlConnectionString(string sqlconnection)
        {
            _sqlConnectionString = new SqlConnectionStringBuilder(sqlconnection);
        }

        public SqlConnectionStringBuilder GetConnectionProperties()
        {
            return _sqlConnectionString;
        }
    }
}
