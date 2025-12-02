using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace hsinchugas_efcs_api
{
    public class OracleDbContext
    {
        private readonly string _connectionString;

        public OracleDbContext(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("OracleConnection");
        }

        public OracleConnection CreateConnection()
        {
            return new OracleConnection(_connectionString);
        }
    }
}
