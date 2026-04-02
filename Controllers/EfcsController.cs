using Dapper;
using hsinchugas_efcs_api.Model;
using hsinchugas_efcs_api.Service;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data.SqlTypes;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
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
            try
            {
                var EfcsService = new EfcsService(_config);
                EfcsService.EFCS_LOG(_db, JsonSerializer.Serialize(request), "", "B207輸入", HttpContext.Connection.RemoteIpAddress?.ToString(), "200");
                var check = Verify.CheckCommon(request);
                if (check != null) return Ok(check);

                
                var check2 = Verify.ValidateB207(request.DOCDATA.BODY);
                if (check2 != null) return Ok(check2);


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

                string txnDatetime = request.DOCDATA.HEAD.TXN_DATETIME;

                data.DOCDATA.HEAD.TXN_DATETIME = txnDatetime;
                var BillerDataQueryRs = new BillerDataQueryRs();
                var QUERYHEAD = new QUERYHEAD();
                var QUERYDETAIL = new List<QUERYDETAIL>();


                //查詢邏輯

                if (request.DOCDATA.BODY.QUERY_TYPE == "1" || request.DOCDATA.BODY.QUERY_TYPE == "2")
                {
                    DateTime lastYear = DateTime.Now.AddYears(-1);

                    // 西元轉民國年
                    int rocYear = lastYear.Year - 1911;

                    // 組成民國年月日 (YYYMMDD)
                    int rocDateNumber = int.Parse($"{rocYear:000}{lastYear.Month:00}{lastYear.Day:00}");
                    using var conn = _db.CreateConnection();

                    string sql_u = @"SELECT * FROM RCPM001 WHERE CUST_NO = :QUERY_DATA1 ";
                    var RCPM001 = await conn.QueryFirstOrDefaultAsync(sql_u, new { QUERY_DATA1 = request.DOCDATA.BODY.QUERY_DATA1});

                    string sql_u2 = @"SELECT * FROM RCPM005 WHERE CUST_NO = :QUERY_DATA1 ";
                    var RCPM005_u = await conn.QueryFirstOrDefaultAsync(sql_u2, new { QUERY_DATA1 = request.DOCDATA.BODY.QUERY_DATA1 });





                    string sql = @"SELECT * FROM RCPM005 WHERE CUST_NO = :QUERY_DATA1 AND (FILE_DATE is NULL OR FILE_DATE = 0) AND (EFCS208 is NULL OR EFCS208 = 0)  
        AND RCV_YMD >= :rocDateNumber";
                    var RCPM005 =  await conn.QueryAsync(sql, new { QUERY_DATA1 = request.DOCDATA.BODY.QUERY_DATA1 , rocDateNumber = rocDateNumber });

                    if (RCPM001 == null && RCPM005_u == null) {
                        data.DOCDATA.HEAD.ICCHK_CODE = "6002";
                        data.DOCDATA.HEAD.ICCHK_CODE_DESC = "查尋條件資料不存在";
                    }
                    else { 
                        if (!RCPM005.Any())
                        {
                            // 沒查到資料
                            data.DOCDATA.HEAD.ICCHK_CODE = "6001";
                            data.DOCDATA.HEAD.ICCHK_CODE_DESC = "查無欠費資料";
                        }
                        else
                        {
                            // 有查到資料
                            data.DOCDATA.HEAD.ICCHK_CODE = "0000";
                            data.DOCDATA.HEAD.ICCHK_CODE_DESC = "請求成功";
                        }
                    }

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
                                QUERY_BILLTYPE = "C",
                                QUERY_BILLDATA = EfcsService.BuildBarcodeC(EfcsService.ToTaiwanDate6(EfcsService.GetNext20thDate()) +"311", item.CUST_NO.ToString(), item.RECEPT_NO.ToString(), EfcsService.ToTaiwanDate4(DateTime.Now.ToString("yyyyMM")) + "00" + EfcsService.PadLeftTo9(EfcsService.TotalAmount(item).ToString())), //CUST_NO + RECEPT_NO
                                QUERY_AMOUNT = EfcsService.TotalAmount(item), //總金額
                                QUERY_DATE = EfcsService.GetNext20thDate(), //繳費日期，無期限為99999999
                                QUERY_DATA_NO = 1,
                                QUERY_DISPNAME1 = "用戶號碼",
                                QUERY_DISPDATA1 = item.CUST_NO
                            });
                        TOTAL_COUNT += EfcsService.TotalAmount(item);
                    }
                    QUERYHEAD.TOTAL_AMOUNT = TOTAL_COUNT;
                    QUERYHEAD.TOTAL_COUNT = number;

                    data.DOCDATA.BODY.QUERYDETAIL = QUERYDETAIL;
                    data.DOCDATA.BODY.QUERYHEAD = QUERYHEAD;
                }
                else
                {
                    data.DOCDATA.HEAD.ICCHK_CODE = "I300";
                    data.DOCDATA.HEAD.ICCHK_CODE_DESC = "查詢條件型態值不符合";
                }
                    /*
                    if (request.DOCDATA.BODY.QUERY_TYPE == "2")
                    {
                        using var conn = _db.CreateConnection();

                        string sq2 = "SELECT * FROM RCPM001 WHERE NAME = :QUERY_DATA2";
                        var RCPM001 = await conn.QueryFirstOrDefaultAsync(sq2, new { QUERY_DATA2 = request.DOCDATA.BODY.QUERY_DATA2 });



                        if (RCPM001 != null)
                        {


                            string sql = "SELECT * FROM RCPM005 WHERE CUST_NO = :QUERY_DATA1 AND (CLEAR_DT is NULL OR CLEAR_DT = 0)";
                            var RCPM005 = await conn.QueryAsync(sql, new { QUERY_DATA1 = RCPM001.CUST_NO });
                            int number = 0;
                            int TOTAL_COUNT = 0;
                            foreach (var item in RCPM005)
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
                                    QUERY_BILLDATA = item.CUST_NO.ToString() + item.RCV_YMD.ToString() + item.RECEPT_NO.ToString(), //CUST_NO + RCV_YMD + RECEPT_NO
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

                    }
                    */



                //收尾
                var options = new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                // 正確做法：保持中文，不要 Unicode escape
                string docDataJson = JsonSerializer.Serialize(data.DOCDATA, options);

                // 用正確的字串算 DIG
                data.SEC.DIG = EfcsService.GenerateDIG(docDataJson);
                data.SEC.MAC = EfcsService.ComputeMac(docDataJson, txnDatetime, _config["HEAD:MAC_KEY"]);



                // 正確做法：保持中文，不要 Unicode escape
                string docDataJson2 = JsonSerializer.Serialize(data, options);


                EfcsService.EFCS_LOG(_db, JsonSerializer.Serialize(request), docDataJson2, "B207輸出", Request.GetDisplayUrl(),"200");

                
                
                return Ok(docDataJson2);



            }
            catch (Exception ex)
            {
                var EfcsService = new EfcsService(_config);
                EfcsService.EFCS_LOG(_db, JsonSerializer.Serialize(request), ex.Message, "B207錯誤", HttpContext.Connection.RemoteIpAddress?.ToString(), "500");
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
        #endregion b207

        #region b208
        [HttpPost("api/efcs/b208")]
        public async Task<IActionResult> PostB208([FromBody] ALL<BillerDataPayRq> request)
        {

            try
            {
                var EfcsService = new EfcsService(_config);
                EfcsService.EFCS_LOG(_db, JsonSerializer.Serialize(request), "", "B208輸入", HttpContext.Connection.RemoteIpAddress?.ToString(), "200");
                var check = Verify.CheckCommon(request);
                if (check != null) return Ok(check);

                var check2 = Verify.ValidateB208(request.DOCDATA.BODY);
                if (check2 != null) return Ok(check2);

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
                string txnDatetime = request.DOCDATA.HEAD.TXN_DATETIME;

                data.DOCDATA.HEAD.TXN_DATETIME = txnDatetime;
                data.DOCDATA.BODY.PAYHEAD = new PAYHEAD_RS();
                data.DOCDATA.BODY.PAYHEAD.REGISTERTOKEN = request.DOCDATA.BODY.PAYHEAD.REGISTERTOKEN;

                using var conn = _db.CreateConnection();
                var PAYDETAIL_RS = new List<PAYDETAIL_RS>();
                int number = 0;


                foreach (var item in request.DOCDATA.BODY.PAYDETAIL)
                {
                    string BILLDATA_CUST_NO = EfcsService.ExtractUserCode(item.BILLDATA);
                    string BILLDATA_RECEPT_NO = EfcsService.ExtractBillNumber(item.BILLDATA);
                    string sql3 = "SELECT * FROM RCPM005 WHERE RECEPT_NO = :RECEPT_NO AND CUST_NO = :CUST_NO ";
                    var RCPM005_SELECT = await conn.QueryFirstOrDefaultAsync(
                        sql3,
                        new {
                            RECEPT_NO = BILLDATA_RECEPT_NO,
                            CUST_NO = BILLDATA_CUST_NO,
                        }   // ← 這裡放入你的變數
                    );

                    bool EINV_CARDNO = false;
                    if (RCPM005_SELECT != null)
                    {
                        if(EfcsService.GetCARRIERIDDate(item.EINV_CARDNO) != null)
                        {
                            string sql2 = @"UPDATE RCPM005 SET  CARRIERID = :CARRIERID ,EFCS208 = :EFCS208 WHERE  RECEPT_NO = :RECEPT_NO AND CUST_NO = :CUST_NO";

                            await conn.ExecuteAsync(sql2, new
                            {
                                RECEPT_NO = BILLDATA_RECEPT_NO,
                                CUST_NO = BILLDATA_CUST_NO,
                                CARRIERID = EfcsService.GetCARRIERIDDate(item.EINV_CARDNO),
                                EFCS208 = EfcsService.GetTaiwanDate()
                            });
                            EINV_CARDNO = true;
                        }
                        else
                        {
                            string sql2 = @"UPDATE RCPM005 SET  EFCS208 = :EFCS208 WHERE  RECEPT_NO = :RECEPT_NO AND CUST_NO = :CUST_NO";

                            await conn.ExecuteAsync(sql2, new
                            {
                                RECEPT_NO = BILLDATA_RECEPT_NO,
                                CUST_NO = BILLDATA_CUST_NO,
                                EFCS208 = EfcsService.GetTaiwanDate()
                            });
                            EINV_CARDNO = false;
                        }

                    }
                    


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
                    INSERT INTO EFCS_B208
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
                        EINV_CARDNO,
                        CLEAR_DT,
                        CUST_NO,
                        RECEPT_NO
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
                        :EINV_CARDNO,
                        :CLEAR_DT,
                        :CUST_NO,
                        :RECEPT_NO
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
                            EINV_CARDNO = item.EINV_CARDNO,
                            CLEAR_DT = DateTime.Now,
                            CUST_NO = BILLDATA_CUST_NO,
                            RECEPT_NO = BILLDATA_RECEPT_NO
                        });

                    if (EINV_CARDNO)
                    {
                        PAYDETAIL_RS.Add(new PAYDETAIL_RS
                        {
                            DETAILNO = item.DETAILNO,   // 明細項流水號 01-99
                            TXNAMOUNT = item.TXNAMOUNT,    // 繳費金額
                            BILLDATA = item.BILLDATA,   // 銷帳資料 (50)
                            BILLBATCH = item.BILLBATCH,   // 所屬期別 (10)
                            APRTN_CODE = "0000",   // 處理結果代碼
                            ERRORDESC = null,   // 錯誤敘述
                            PAY_DATA_NO = 3,    // 結果參數數目 (1 digit)
                            PAY_DISPNAME1 = "共通性載具",
                            PAY_DISPDATA1 = item.EINV_CARDNO,
                            PAY_DISPNAME2 = "載具類別",
                            PAY_DISPDATA2 = "",
                            PAY_DISPNAME3 = "載具號碼",
                            PAY_DISPDATA3 = "",
                        });
                    }
                    else
                    {
                        PAYDETAIL_RS.Add(new PAYDETAIL_RS
                        {
                            DETAILNO = item.DETAILNO,   // 明細項流水號 01-99
                            TXNAMOUNT = item.TXNAMOUNT,    // 繳費金額
                            BILLDATA = item.BILLDATA,   // 銷帳資料 (50)
                            BILLBATCH = item.BILLBATCH,   // 所屬期別 (10)
                            APRTN_CODE = "0000",   // 處理結果代碼
                            ERRORDESC = null,   // 錯誤敘述
                            PAY_DATA_NO = 3,    // 結果參數數目 (1 digit)
                            PAY_DISPNAME1 = "共通性載具",
                            PAY_DISPDATA1 = "",
                            PAY_DISPNAME2 = "載具類別",
                            PAY_DISPDATA2 = "ED0014",
                            PAY_DISPNAME3 = "載具號碼",
                            PAY_DISPDATA3 = "",
                        });
                    }





                }

                data.DOCDATA.BODY.PAYHEAD.TOTAL_COUNT = number;
                data.DOCDATA.BODY.PAYHEAD.TOTAL_AMOUNT = request.DOCDATA.BODY.PAYHEAD.TOTAL_AMOUNT;

                data.DOCDATA.BODY.PAYDETAIL = PAYDETAIL_RS;



                data.DOCDATA.HEAD.ICCHK_CODE = "0000";
                data.DOCDATA.HEAD.ICCHK_CODE_DESC = "請求成功";


                //收尾
                var options = new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                // 正確做法：保持中文，不要 Unicode escape
                string docDataJson = JsonSerializer.Serialize(data.DOCDATA, options);

                data.SEC.DIG = EfcsService.GenerateDIG(docDataJson);
                data.SEC.MAC = EfcsService.ComputeMac(docDataJson, txnDatetime, _config["HEAD:MAC_KEY"]);
                var options2 = new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                // 正確做法：保持中文，不要 Unicode escape
                string docDataJson2 = JsonSerializer.Serialize(data, options);
                EfcsService.EFCS_LOG(_db, JsonSerializer.Serialize(request), docDataJson2, "B208輸出", Request.GetDisplayUrl(), "200");


                return Ok(docDataJson2);


            }
            catch (Exception ex)
            {
                var EfcsService = new EfcsService(_config);
                EfcsService.EFCS_LOG(_db, JsonSerializer.Serialize(request), ex.Message, "B208錯誤", HttpContext.Connection.RemoteIpAddress?.ToString(), "500");
                // 一旦進 catch → 立刻終了，不會繼續往下跑
                var error =  new
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
        #endregion b208

        #region b219
        [HttpPost("api/efcs/b219")]
        public async Task<IActionResult> PostB219([FromBody] ALL<BillerQueryNotifyMsgRq> request)
        {
            try
            {
                var check = Verify.CheckCommon(request);
                if (check != null) return Ok(check);

                var EfcsService = new EfcsService(_config);
                EfcsService.EFCS_LOG(_db, JsonSerializer.Serialize(request), "", "B219輸入", HttpContext.Connection.RemoteIpAddress?.ToString(), "200");

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

                string txnDatetime = request.DOCDATA.HEAD.TXN_DATETIME;

                var QUERYDETAIL_B219_RS = new QUERYDETAIL_B219_RS();
                data.DOCDATA.HEAD.TXN_DATETIME = txnDatetime;
                QUERYHEAD_B219_RS QUERYHEAD_B219_RS = new QUERYHEAD_B219_RS();
                QUERYHEAD_B219_RS.TOTAL_COUNT = 1;


                if (request.DOCDATA.BODY.QUERYDETAIL?.Count != 1)
                {
                    throw new Exception("QUERYDETAIL 必須為 1 筆");
                }
                var detail = request.DOCDATA.BODY.QUERYDETAIL[0];

                //查詢邏輯

                if (detail.QUERY_TYPE == "1" || detail.QUERY_TYPE == "2")
                {
                    DateTime lastYear = DateTime.Now.AddYears(-1);

                    // 西元轉民國年
                    int rocYear = lastYear.Year - 1911;

                    // 組成民國年月日 (YYYMMDD)
                    int rocDateNumber = int.Parse($"{rocYear:000}{lastYear.Month:00}{lastYear.Day:00}");

                    using var conn = _db.CreateConnection();
                    string sql = @"SELECT * FROM RCPM005 WHERE CUST_NO = :QUERY_DATA1 AND (FILE_DATE is NULL OR FILE_DATE = 0) AND (EFCS208 is NULL OR EFCS208 = 0)        
        AND RCV_YMD >= :rocDateNumber";
                    var RCPM005 = await conn.QueryAsync(sql, new { QUERY_DATA1 = detail.QUERY_DATA1 , rocDateNumber  = rocDateNumber });
                    string sql2 = "SELECT * FROM EFCS_CONFIG";
                    var config = await conn.QueryFirstOrDefaultAsync(sql2);


                    string sql_u = @"SELECT * FROM RCPM001 WHERE CUST_NO = :QUERY_DATA1 ";
                    var RCPM001 = await conn.QueryFirstOrDefaultAsync(sql_u, new { QUERY_DATA1 = detail.QUERY_DATA1 });

                    string sql_u2 = @"SELECT * FROM RCPM005 WHERE CUST_NO = :QUERY_DATA1 ";
                    var RCPM005_u = await conn.QueryFirstOrDefaultAsync(sql_u2, new { QUERY_DATA1 = detail.QUERY_DATA1 });

                    if (RCPM001 == null && RCPM005_u == null)
                    {
                        data.DOCDATA.HEAD.ICCHK_CODE = "6002";
                        data.DOCDATA.HEAD.ICCHK_CODE_DESC = "查尋條件資料不存在";
                    }
                    else
                    {
                        if (!RCPM005.Any())
                        {
                            // 沒查到資料
                            data.DOCDATA.HEAD.ICCHK_CODE = "6001";
                            data.DOCDATA.HEAD.ICCHK_CODE_DESC = "查無欠費資料";
                        }
                        else
                        {
                            // 有查到資料
                            data.DOCDATA.HEAD.ICCHK_CODE = "0000";
                            data.DOCDATA.HEAD.ICCHK_CODE_DESC = "請求成功";
                        }
                    }

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
                            QUERY_DATA1 = detail.QUERY_DATA1         // 查詢條件 1
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
                            QUERY_DATA1 = detail.QUERY_DATA1         // 查詢條件 1
                        };
                    }


                }
                else
                {
                    data.DOCDATA.HEAD.ICCHK_CODE = "I300";
                    data.DOCDATA.HEAD.ICCHK_CODE_DESC = "查詢條件型態值不符合";
                }
                    /*
                    if (request.DOCDATA.BODY.QUERYDETAIL.QUERY_TYPE == "2")
                    {
                        using var conn = _db.CreateConnection();
                        string sq2 = "SELECT * FROM RCPM001 WHERE NAME = :QUERY_DATA2";
                        var RCPM001 = await conn.QueryFirstOrDefaultAsync(sq2, new { QUERY_DATA2 = request.DOCDATA.BODY.QUERYDETAIL.QUERY_DATA2 });
                        if (RCPM001 != null)
                        {
                            string sql = "SELECT * FROM RCPM005 WHERE CUST_NO = :QUERY_DATA1 AND (CLEAR_DT is NULL OR CLEAR_DT = 0)";
                            var RCPM005 = await conn.QueryAsync(sql, new { QUERY_DATA1 = RCPM001.CUST_NO });
                            string sql2 = "SELECT * FROM EFCS_CONFIG";
                            var config = await conn.QueryFirstOrDefaultAsync(sql2);
                            if (RCPM005.Any())
                            {
                                int allmoney = 0;
                                foreach (var item in RCPM005)
                                {
                                    allmoney += EfcsService.TotalAmount(item);
                                    QUERYHEAD_B219_RS.TOTAL_COUNT += 1;
                                }
                                QUERYHEAD_B219_RS.TOTAL_AMOUNT = allmoney;
                                DateTime tomorrow = DateTime.Now.AddDays(config.B219_Y_NEXT_TIME);



                                QUERYDETAIL_B219_RS = new QUERYDETAIL_B219_RS
                                {
                                    DETAILNO = "01",                 // 流水號 01–99
                                    RTN_CODE = "0000",               // 作業結果
                                    NEXT_FIRE_DATE = tomorrow.ToString("yyyyMMdd"),     // 下次詢問日 YYYYMMDD
                                    NOTIFY_MSG = config.B219_TEXT,    // 通知訊息
                                    TOTAL_AMOUNT = allmoney,             // 應繳總金額
                                    QUERY_TYPE = "1",                // 查詢條件型態
                                    QUERY_DATA1 = request.DOCDATA.BODY.QUERYDETAIL.QUERY_DATA1,         // 查詢條件 1
                                    QUERY_DATA2 = RCPM001.NAME         // 查詢條件 1
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
                                    QUERY_DATA1 = request.DOCDATA.BODY.QUERYDETAIL.QUERY_DATA1,
                                    QUERY_DATA2 = RCPM001.NAME  // 查詢條件 2
                                };
                            }

                        }
                    }
                    */
                data.DOCDATA.BODY.QUERYDETAIL.Add(QUERYDETAIL_B219_RS);
                data.DOCDATA.BODY.QUERYHEAD = QUERYHEAD_B219_RS;




                //收尾
                var options = new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                // 正確做法：保持中文，不要 Unicode escape
                string docDataJson = JsonSerializer.Serialize(data.DOCDATA, options);


                data.SEC.DIG = EfcsService.GenerateDIG(docDataJson);
                data.SEC.MAC = EfcsService.ComputeMac(docDataJson, txnDatetime, _config["HEAD:MAC_KEY"]);
                var options2 = new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                // 正確做法：保持中文，不要 Unicode escape
                string docDataJson2 = JsonSerializer.Serialize(data, options);

                EfcsService.EFCS_LOG(_db, JsonSerializer.Serialize(request), docDataJson2, "B219輸出", Request.GetDisplayUrl(), "200");

                return Ok(docDataJson2);





            }
            catch (Exception ex)
            {
                var EfcsService = new EfcsService(_config);
                EfcsService.EFCS_LOG(_db, JsonSerializer.Serialize(request), ex.Message, "B219錯誤", HttpContext.Connection.RemoteIpAddress?.ToString(), "500");
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

        #endregion b219









        #region b212
        [HttpGet("api/efcs/b212")]
        public async Task<IActionResult> PostB212()
        {

            var EfcsService = new EfcsService(_config);
            var data = new ALL<BillerSystemNoticeRq>()
            {
                FUN = new FUN(),
                DOCDATA = new DOCDATA<BillerSystemNoticeRq>()
                {
                    HEAD = EfcsService.B212HEAD(),
                    BODY = new BillerSystemNoticeRq()  // ★ 你要 new 起來，不然 BODY 也是 null
                },
                SEC = new SEC()
            };
            data.FUN.FUNNAME = "BillerSystemNotice";
            data.FUN.VER = "1.00";
            data.DOCDATA.BODY = new BillerSystemNoticeRq();
            var txnDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");



            using var conn = _db.CreateConnection();
            string sql2 = "SELECT * FROM EFCS_CONFIG";
            var config = await conn.QueryFirstOrDefaultAsync(sql2);
            data.DOCDATA.BODY.NOTICE_TYPE = config.B212_NOTIFY;
            data.DOCDATA.BODY.MEMO = config.B212_TEXT;
            data.DOCDATA.BODY.BEGIN_TIME = config.B212_START.ToString("yyyyMMddHHmmss");
            data.DOCDATA.HEAD.TXN_DATETIME = txnDatetime;
            if (config.B212_NOTIFY != "A")
            {
                data.DOCDATA.BODY.END_TIME = config.B212_END.ToString("yyyyMMddHHmmss");
            }

            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            // 正確做法：保持中文，不要 Unicode escape
            string docDataJson = JsonSerializer.Serialize(data.DOCDATA, options);

            // 用正確的字串算 DIG
            data.SEC.DIG = EfcsService.GenerateDIG(docDataJson);
            data.SEC.MAC = EfcsService.ComputeMac(docDataJson, txnDatetime, _config["HEAD:MAC_KEY"]);



            var options2 = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            // 正確做法：保持中文，不要 Unicode escape
            string docDataJson2 = JsonSerializer.Serialize(data, options);

            //return Ok(docDataJson2);
            var client = new HttpClient();

            var url = _config["HEAD:B212_URL"];


            //return Ok(JsonSerializer.Serialize(data));
            var content = new StringContent(docDataJson2, Encoding.UTF8, "application/json");
            
            // 發送 POST
            var response = await client.PostAsync(url, content);

            // 讀取回應
            var result = await response.Content.ReadAsStringAsync();



            EfcsService.EFCS_LOG(_db, result, docDataJson2, "B212", Request.GetDisplayUrl(), "200");

            return Ok(result);
        }
        #endregion b212








    }
}
