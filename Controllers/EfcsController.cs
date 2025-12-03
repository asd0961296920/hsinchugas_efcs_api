using Dapper;
using hsinchugas_efcs_api.Model;
using hsinchugas_efcs_api.Service;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlTypes;
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

        #region b207
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
                string sql = "SELECT * FROM RCPM005 WHERE CUST_NO = :QUERY_DATA1 AND (CLEAR_DT is NULL OR CLEAR_DT = 0)";
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
                            QUERY_BILLDATA = item.CUST_NO.ToString() + item.RCY_YMD.ToString() + item.RECEIPT_NO.ToString(), //CUST_NO + RCY_YMD + RECEIPT_NO
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



            data.DOCDATA.HEAD.ICCHK_CODE = "0000";
            data.DOCDATA.HEAD.ICCHK_CODE_DESC = "請求成功";
            data.SEC.DIG = EfcsService.GenerateDIG(JsonSerializer.Serialize(data));
            data.SEC.MAC = EfcsService.ComputeMac(JsonSerializer.Serialize(data), txnDatetime, _config["HEAD:MAC_KEY"]);


            EfcsService.EFCS_LOG(_db, JsonSerializer.Serialize(request), JsonSerializer.Serialize(data),"B207", Request.GetDisplayUrl(),"200");
            return Ok(data);
        }
        #endregion b207

        #region b208
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
            var EfcsService = new EfcsService(_config);
            var txnDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");

            data.DOCDATA.HEAD.TXN_DATETIME = txnDatetime;
            data.DOCDATA.BODY.PAYHEAD.REGISTERTOKEN = request.DOCDATA.BODY.PAYHEAD.REGISTERTOKEN;

            using var conn = _db.CreateConnection();
            var PAYDETAIL_RS = new List<PAYDETAIL_RS>();
            int number = 0;


            foreach (var item in request.DOCDATA.BODY.PAYDETAIL)
            {
                number++;
                string DETAILNO = "";

                if (number < 10)
                {
                    DETAILNO = "0" + number.ToString();
                }
                else
                {
                    DETAILNO = number.ToString();
                }


                string sql = @"
                INSERT INTO SA.EFCS_B208
                (
                    DETAILNO,
                    OUTBANKID,
                    MEMBERID,
                    OUTACCOUNTNUM,
                    TXNAMOUNT,
                    PAY_DATE,
                    PAY_TIME,
                    PAY_CLRDATE,
                    CHECKTYPE,
                    BILLTYPE,
                    BILLDATA,
                    BILLBATCH,
                    EFCSSEQNO,
                    EINV_CARDNO
                )
                VALUES
                (
                    :DETAILNO,
                    :OUTBANKID,
                    :MEMBERID,
                    :OUTACCOUNTNUM,
                    :TXNAMOUNT,
                    :PAY_DATE,
                    :PAY_TIME,
                    :PAY_CLRDATE,
                    :CHECKTYPE,
                    :BILLTYPE,
                    :BILLDATA,
                    :BILLBATCH,
                    :EFCSSEQNO,
                    :EINV_CARDNO
                )";

                await conn.ExecuteAsync(sql, new
                {
                    DETAILNO = item.DETAILNO,
                    OUTBANKID = item.OUTBANKID,
                    MEMBERID = item.MEMBERID,
                    OUTACCOUNTNUM = item.OUTACCOUNTNUM,
                    TXNAMOUNT = item.TXNAMOUNT,
                    PAY_DATE = item.PAY_DATE,
                    PAY_TIME = item.PAY_TIME,
                    PAY_CLRDATE = item.PAY_CLRDATE,
                    CHECKTYPE = item.CHECKTYPE,
                    BILLTYPE = item.BILLTYPE,
                    BILLDATA = item.BILLDATA,
                    BILLBATCH = item.BILLBATCH,
                    EFCSSEQNO = item.EFCSSEQNO,
                    EINV_CARDNO = item.EINV_CARDNO
                });
                var key = EfcsService.DecodeBillData(item.BILLDATA);
                string sql2 = @"UPDATE RCPM005 SET CLEAR_DT = :DATA WHERE CUST_NO = :CUST_NO AND RCY_YMD = :RCY_YMD AND RECEIPT_NO = :RECEIPT_NO";

                await conn.ExecuteAsync(sql, new
                {
                    CUST_NO = key.CUST_NO,
                    RCY_YMD = key.RCY_YMD,
                    RECEIPT_NO = key.RECEIPT_NO,
                    DATA = EfcsService.GetTaiwanDate()
                });
                


                PAYDETAIL_RS.Add(new PAYDETAIL_RS
                {
                    DETAILNO = DETAILNO,   // 明細項流水號 01-99
                    TXNAMOUNT = item.TXNAMOUNT,    // 繳費金額
                    BILLDATA = item.BILLDATA,   // 銷帳資料 (50)
                    BILLBATCH = item.BILLBATCH,   // 所屬期別 (10)
                    APRTN_CODE = "200",   // 處理結果代碼
                    ERRORDESC = null,   // 錯誤敘述
                    PAY_DATA_NO = 1,    // 結果參數數目 (1 digit)

                    PAY_DISPNAME1 = "繳費結果",   // 結果名稱 1 (20)
                    PAY_DISPDATA1 = "成功",   // 結果值 1 (120)
                });




            }

            data.DOCDATA.BODY.PAYHEAD.TOTAL_COUNT = number;
            data.DOCDATA.BODY.PAYHEAD.TOTAL_AMOUNT = request.DOCDATA.BODY.PAYHEAD.TOTAL_AMOUNT;

            data.DOCDATA.BODY.PAYDETAIL = PAYDETAIL_RS;



            data.DOCDATA.HEAD.ICCHK_CODE = "0000";
            data.DOCDATA.HEAD.ICCHK_CODE_DESC = "請求成功";
            data.SEC.DIG = EfcsService.GenerateDIG(JsonSerializer.Serialize(data));
            data.SEC.MAC = EfcsService.ComputeMac(JsonSerializer.Serialize(data), txnDatetime, _config["HEAD:MAC_KEY"]);
            EfcsService.EFCS_LOG(_db, JsonSerializer.Serialize(request), JsonSerializer.Serialize(data), "B208", Request.GetDisplayUrl(), "200");
            return Ok(data);


        }
        #endregion b208

        #region b219
        [HttpPost("api/efcs/b219")]
        public async Task<IActionResult> PostB219([FromBody] ALL<BillerQueryNotifyMsgRq> request)
        {
            var check = Verify.CheckCommon(request);
            if (check != null) return Ok(check);


            var data = new ALL<BillerQueryNotifyMsgRs>()
            {
                FUN = request.FUN,
                DOCDATA = new DOCDATA<BillerQueryNotifyMsgRs>()
                {
                    HEAD = request.DOCDATA.HEAD,
                    BODY = new BillerQueryNotifyMsgRs()  // ★ 你要 new 起來，不然 BODY 也是 null
                },
                SEC = new SEC()
            };

            var EfcsService = new EfcsService(_config);
            var txnDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");

            var QUERYDETAIL_B219_RS = new QUERYDETAIL_B219_RS();

            //查詢邏輯

            if (request.DOCDATA.BODY.QUERYDETAIL.QUERY_TYPE == "1")
            {
                using var conn = _db.CreateConnection();
                string sql = "SELECT * FROM RCPM005 WHERE CUST_NO = :QUERY_DATA1 AND (CLEAR_DT is NULL OR CLEAR_DT = 0)";
                var RCPM005 = await conn.QueryAsync(sql, new { QUERY_DATA1 = request.DOCDATA.BODY.QUERYDETAIL.QUERY_DATA1 });
                string sql2 = "SELECT * FROM EFCS_CONFIG";
                var config = await conn.QueryFirstOrDefaultAsync(sql2);
                if (RCPM005.Any())
                {
                    int allmoney = 0;

                    foreach (var item in RCPM005)
                    {
                        allmoney += EfcsService.TotalAmount(item);
                    }

                        DateTime tomorrow = DateTime.Now.AddDays(config.B219_Y_NEXT_TIME);

                    QUERYDETAIL_B219_RS = new QUERYDETAIL_B219_RS
                    {
                        DETAILNO = "01",                 // 流水號 01–99
                        RTN_CODE = "0000",               // 作業結果
                        NEXT_FIRE_DATE = tomorrow.ToString("yyyyMMdd"),     // 下次詢問日 YYYYMMDD
                        NOTIFY_MSG = config.B219_TEXT,    // 通知訊息
                        TOTAL_AMOUNT = allmoney,             // 應繳總金額
                        QUERY_TYPE = "1",                // 查詢條件型態
                        QUERY_DATA1 = request.DOCDATA.BODY.QUERYDETAIL.QUERY_DATA1         // 查詢條件 1
                    };
                }
                else
                {

                    DateTime tomorrow = DateTime.Now.AddDays(config.B219_N_NEXT_TIME);
                    QUERYDETAIL_B219_RS = new QUERYDETAIL_B219_RS
                    {
                        DETAILNO = "01",                 // 流水號 01–99
                        RTN_CODE = "6002",               // 作業結果
                        NEXT_FIRE_DATE = tomorrow.ToString("yyyyMMdd"),     // 下次詢問日 YYYYMMDD
                        QUERY_TYPE = "1",                // 查詢條件型態
                        QUERY_DATA1 = request.DOCDATA.BODY.QUERYDETAIL.QUERY_DATA1         // 查詢條件 1
                    };
                }


            }




            data.DOCDATA.BODY.QUERYDETAIL = QUERYDETAIL_B219_RS;
            data.DOCDATA.HEAD.ICCHK_CODE = "0000";
            data.DOCDATA.HEAD.ICCHK_CODE_DESC = "請求成功";
            data.SEC.DIG = EfcsService.GenerateDIG(JsonSerializer.Serialize(data));
            data.SEC.MAC = EfcsService.ComputeMac(JsonSerializer.Serialize(data), txnDatetime, _config["HEAD:MAC_KEY"]);


            EfcsService.EFCS_LOG(_db, JsonSerializer.Serialize(request), JsonSerializer.Serialize(data), "B207", Request.GetDisplayUrl(), "200");
            return Ok(data);
        }

        #endregion b219









        #region b212
        [HttpGet("api/efcs/b212")]
        public async Task<IActionResult> PostB212()
        {
            var data = new
            {
                msg = "ok"
            };
            return Ok(data);
        }
        #endregion b212








    }
}
