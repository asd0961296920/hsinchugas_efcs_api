using Oracle.ManagedDataAccess.Client;
using Dapper;



namespace hsinchugas_efcs_api
{
    public class RCPM005Repository
    {
        private readonly OracleDbContext _db;

        public RCPM005Repository(OracleDbContext db)
        {
            _db = db;
        }

        // 查詢全部
        public async Task<IEnumerable<dynamic>> GetAllAsync()
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT * FROM RCPM005";
            return await conn.QueryAsync(sql);
        }

        // 查一筆
        public async Task<dynamic> GetByIdAsync(string custNo)
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT * FROM RCPM005 WHERE CUST_NO = :custNo";
            return await conn.QueryFirstOrDefaultAsync(sql, new { custNo });
        }

        // 新增
        public async Task<int> InsertAsync(string custNo, int receptNo)
        {
            using var conn = _db.CreateConnection();
            string sql = @"INSERT INTO RCPM005 (CUST_NO, RECEPT_NO)
                       VALUES (:custNo, :receptNo)";
            return await conn.ExecuteAsync(sql, new { custNo, receptNo });
        }

        // 更新
        public async Task<int> UpdateAsync(string custNo, int newPoint)
        {
            using var conn = _db.CreateConnection();
            string sql = @"UPDATE RCPM005 SET NEW_POINT = :newPoint 
                       WHERE CUST_NO = :custNo";
            return await conn.ExecuteAsync(sql, new { newPoint, custNo });
        }

        // 刪除
        public async Task<int> DeleteAsync(string custNo)
        {
            using var conn = _db.CreateConnection();
            string sql = "DELETE FROM RCPM005 WHERE CUST_NO = :custNo";
            return await conn.ExecuteAsync(sql, new { custNo });
        }
    }
}
