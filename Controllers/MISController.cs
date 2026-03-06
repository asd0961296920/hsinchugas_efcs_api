using Dapper;
using hsinchugas_efcs_api.Model;
using hsinchugas_efcs_api.Service;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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


        [HttpGet("api/mis/payment/status")]
        public async Task<IActionResult> Payment_status([FromQuery] PaymentStatusRq request)
        {
            try
            {
                using var conn = _db.CreateConnection();
            string sql2 = "SELECT * FROM RCPM005 WHERE  RECEPT_NO = :RECEPT_NO AND CUST_NO = :CUST_NO";
            var data = await conn.QueryFirstOrDefaultAsync(sql2, new
            {
                RECEPT_NO = request.RECEPT_NO,
                CUST_NO = request.CUST_NO
            });

            PaymentStatusRs PaymentStatusRs = new PaymentStatusRs();

            if (data != null)
            {
                PaymentStatusRs.CUST_NO = data.CUST_NO;
                PaymentStatusRs.RECEPT_NO = data.RECEPT_NO;
                if (data.FILE_DATE != null || data.EFCS208 != null)
                {
                    PaymentStatusRs.status = true;
                }
                else
                {
                    PaymentStatusRs.status = false;
                }
            }
            else
            {
                PaymentStatusRs.CUST_NO = "查無資料";
                PaymentStatusRs.RECEPT_NO = 0;
                PaymentStatusRs.status = false;
            }



                //string docData = JsonSerializer.Serialize(PaymentStatusRs);


                return Ok(PaymentStatusRs);

            }
            catch (Exception ex)
            {
                EfcsService.EFCS_LOG(_db, "", ex.Message, "新竹瓦斯查詢帳單是否繳費失敗", Request.GetDisplayUrl(), "500");
                // 一旦進 catch → 立刻終了，不會繼續往下跑
                var error = new
                {
                    DOCDATA = new
                    {
                        HEAD = new
                        {
                            ICCHK_CODE = "S999",
                            ICCHK_CODE_DESC = ex.Message
                        }
                    }
                };
                return Ok(error);
            }
        }



    }
}
