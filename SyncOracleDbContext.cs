using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace hsinchugas_efcs_api
{
    public class SyncOracleDbContext
    {
        private readonly string _connectionString;

        public SyncOracleDbContext(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("SyncOracleConnection");
        }

        public OracleConnection CreateConnection()
        {
            return new OracleConnection(_connectionString);
        }
    }
}
