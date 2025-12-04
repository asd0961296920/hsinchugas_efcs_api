using hsinchugas_efcs_api.Model;
using hsinchugas_efcs_api.Service;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Dapper;

namespace hsinchugas_efcs_api.Controllers
{
    [ApiController]
    [Route("")]
    public class MISController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly OracleDbContext _db;

        public MISController(IConfiguration config, OracleDbContext db)
        {
            _config = config;
            _db = db;
        }

        [HttpGet("api/mis/config/get")]
        public async Task<IActionResult> mis_config_get()
        {
            using var conn = _db.CreateConnection();
            string sql2 = "SELECT * FROM EFCS_CONFIG";
            var config = await conn.QueryFirstOrDefaultAsync(sql2);

            string docData = JsonSerializer.Serialize(config);


            return Ok(config);
        }

        [HttpPost("api/mis/config/upd_212")]
        public async Task<IActionResult> mis_config_upd_B212([FromBody] EfcsConfig request)
        {



            using var conn = _db.CreateConnection();
            string sql2 = @"
            UPDATE EFCS_CONFIG
            SET 
                B212_NOTIFY = :B212_NOTIFY,
                B212_TEXT = :B212_TEXT,
                B212_START = :B212_START,
                B212_END = :B212_END
            ";

            var affectedRows = await conn.ExecuteAsync(sql2, new
            {
                B212_NOTIFY = request.B212_NOTIFY,
                B212_TEXT = request.B212_TEXT,
                B212_START = request.B212_START,
                B212_END = request.B212_END,
            });



            return Ok(affectedRows);
        }

        [HttpPost("api/mis/config/upd_219")]
        public async Task<IActionResult> mis_config_upd_B219([FromBody] EfcsConfig request)
        {

            using var conn = _db.CreateConnection();
            string sql2 = @"
            UPDATE EFCS_CONFIG
            SET 
                B219_Y_NEXT_TIME = :B219_Y_NEXT_TIME,
                B219_N_NEXT_TIME = :B219_N_NEXT_TIME,
                B219_TEXT = :B219_TEXT
            ";

            var affectedRows = await conn.ExecuteAsync(sql2, new
            {
                B219_Y_NEXT_TIME = request.B219_Y_NEXT_TIME,
                B219_N_NEXT_TIME = request.B219_N_NEXT_TIME,
                B219_TEXT = request.B219_TEXT
            });



            return Ok(affectedRows);
        }






    }
}
