using Dapper;
using hsinchugas_efcs_api.Model;
using hsinchugas_efcs_api.Service;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace hsinchugas_efcs_api.Controllers
{
    [ApiController]
    [Route("")]
    public class EfcsController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly OracleDbContext _db;

        public EfcsController(IConfiguration config, OracleDbContext db)
        {
            _config = config;
            _db = db;
        }


        [HttpPost("api/efcs/b207")]
        public async Task<IActionResult> PostB207([FromBody] ALL<BillerDataQueryRq> request)
        {
            var check = Verify.CheckCommon(request);
            if (check != null) return Ok(check);


            var data = new ALL<BillerDataQueryRs>()
            {
                FUN = request.FUN,
                DOCDATA = new DOCDATA<BillerDataQueryRs>()
                {
                    HEAD = request.DOCDATA.HEAD,
                    BODY = new BillerDataQueryRs()  // ★ 你要 new 起來，不然 BODY 也是 null
                },
                SEC = new SEC()
            };
            var EfcsService = new EfcsService(_config);
            var txnDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");

            data.DOCDATA.HEAD.TXN_DATETIME = txnDatetime;
            var BillerDataQueryRs = new BillerDataQueryRs();
            var QUERYHEAD = new QUERYHEAD();
            var QUERYDETAIL = new List<QUERYDETAIL>();


            //查詢邏輯

            if (request.DOCDATA.BODY.QUERY_TYPE == "1")
            {
                using var conn = _db.CreateConnection();
                string sql = "SELECT * FROM RCPM005 WHERE CUST_NO = :QUERY_DATA1 AND CLEAR_DT is NULL";
                var RCPM005 =  await conn.QueryAsync(sql, new { QUERY_DATA1 = request.DOCDATA.BODY.QUERY_DATA1 });
                int number = 0;
                int TOTAL_COUNT = 0;
                foreach ( var item in RCPM005)
                {
                    number++;
                    string QUERY_DETAILNO = "";
                    if (number < 10)
                    {
                        QUERY_DETAILNO = "0" + number.ToString();
                    }
                    else
                    {
                         QUERY_DETAILNO = number.ToString();
                    }

                        QUERYDETAIL.Add(new QUERYDETAIL
                        {
                            QUERY_DETAILNO = QUERY_DETAILNO, 
                            QUERY_ISPAY = "Y",
                            QUERY_BILLTYPE = "B",
                            QUERY_BILLDATA = item.RECEPT_NO.ToString(), //收據號碼
                            QUERY_AMOUNT = EfcsService.TotalAmount(item), //總金額
                            QUERY_DATE = "99999999", //繳費日期，無期限為99999999
                            QUERY_DATA_NO = 1,
                            QUERY_DISPNAME1 = "用戶號碼",
                            QUERY_DISPDATA1 = item.CUST_NO
                        });
                    TOTAL_COUNT += EfcsService.TotalAmount(item);
                }
                QUERYHEAD.TOTAL_AMOUNT = number;
                QUERYHEAD.TOTAL_COUNT = TOTAL_COUNT;

                data.DOCDATA.BODY.QUERYDETAIL = QUERYDETAIL;
                data.DOCDATA.BODY.QUERYHEAD = QUERYHEAD;
            }




            data.SEC.DIG = EfcsService.GenerateDIG(JsonSerializer.Serialize(data));
            data.SEC.MAC = EfcsService.ComputeMac(JsonSerializer.Serialize(data), txnDatetime, _config["HEAD:MAC_KEY"]);



            EfcsService.EFCS_LOG(_db, JsonSerializer.Serialize(request), JsonSerializer.Serialize(data),"B207", Request.GetDisplayUrl(),"200");
            return Ok(data);
        }

        [HttpPost("api/efcs/b208")]
        public async Task<IActionResult> PostB208([FromBody] ALL<BillerDataPayRq> request)
        {

            var check = Verify.CheckCommon(request);
            if (check != null) return Ok(check);



            var data = new ALL<BillerDataPayRs>()
            {
                FUN = request.FUN,
                DOCDATA = new DOCDATA<BillerDataPayRs>()
                {
                    HEAD = request.DOCDATA.HEAD,
                    BODY = new BillerDataPayRs()  // ★ 你要 new 起來，不然 BODY 也是 null
                },
                SEC = new SEC()
            };










            return Ok(data);


        }

        [HttpPost("api/efcs/b219")]
        public async Task<IActionResult> PostB219()
        {
            var data = new
            {
                msg = "ok"
            };
            return Ok(data);
        }



        [HttpGet("api/efcs/b212")]
        public async Task<IActionResult> PostB212()
        {
            var data = new
            {
                msg = "ok"
            };
            return Ok(data);
        }









    }
}
