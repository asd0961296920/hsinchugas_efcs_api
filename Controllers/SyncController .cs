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
    public class SyncController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly OracleDbContext _db;
        private readonly SyncOracleDbContext _sync_db;

        public SyncController(IConfiguration config, OracleDbContext db, SyncOracleDbContext sync_db)
        {
            _config = config;
            _db = db;
            _sync_db = sync_db;
        }



        [HttpGet("api/sync/RCPM005")]
        public async Task<IActionResult> AsyncRCPM005()
        {
            try
            {
                EfcsService.EFCS_LOG(_db, "抓取", "", "RCPM005自動抓取資料排程開始", HttpContext.Connection.RemoteIpAddress?.ToString(), "200");
                DateTime lastYear = DateTime.Now.AddYears(-1);

            // 西元轉民國年
            int rocYear = lastYear.Year - 1911;

            // 組成民國年月日 (YYYMMDD)
            int rocDateNumber = int.Parse($"{rocYear:000}{lastYear.Month:00}{lastYear.Day:00}");


            using var conn = _db.CreateConnection();
            using var conn_sync = _sync_db.CreateConnection();

            string sql_u2 = @"SELECT * FROM RCPM005 WHERE RCV_YMD >= :rocDateNumber";
            var RCPM005 = await conn.QueryAsync(sql_u2, new { rocDateNumber = rocDateNumber });

            string sql_sync = @"SELECT * FROM HCG.RCPM005 WHERE RCV_YMD >= :rocDateNumber";
            var RCPM005_sync = await conn_sync.QueryAsync(sql_sync, new { rocDateNumber = rocDateNumber });
            //List<RCPM005> List = new List<RCPM005>();
            int number = 0;
            int number2 = 0;
            foreach (var sync in RCPM005_sync)
            {
                bool type = true;
                number2++;
                foreach (var item in RCPM005)
                {
                    if ((sync.CUST_NO == item.CUST_NO) && (sync.RECEPT_NO == item.RECEPT_NO))
                    {
                        type = false;
                    }
                }
                if (type)
                {



                    string sql3 = @"
                    INSERT INTO RCPM005
                    (
                        CUST_NO,
                        RECEPT_NO,
                        RCV_YMD,
                        FILE_DATE,
                        CARRIERID,
                        GAS_CHARGE,
                        ADDED_TAX,
                        LAMP_RENT,
                        RENT_TAX,
                        BASE_AMT,
                        BASE_TAX,
                        ADJ_CHARGE,
                        CARRIERID_O
                    )
                    VALUES
                    (
                        :CUST_NO,
                        :RECEPT_NO,
                        :RCV_YMD,
                        :FILE_DATE,
                        :CARRIERID,
                        :GAS_CHARGE,
                        :ADDED_TAX,
                        :LAMP_RENT,
                        :RENT_TAX,
                        :BASE_AMT,
                        :BASE_TAX,
                        :ADJ_CHARGE,
                        :CARRIERID_O
                    )";

                    var affectedRows = await conn.ExecuteAsync(sql3, new
                    {
                        CUST_NO = sync.CUST_NO,
                        RECEPT_NO = sync.RECEPT_NO,
                        RCV_YMD = sync.RCV_YMD,
                        FILE_DATE = sync.FILE_DATE,
                        CARRIERID = sync.CARRIERID,
                        GAS_CHARGE = sync.GAS_CHARGE,
                        ADDED_TAX = sync.ADDED_TAX,
                        LAMP_RENT = sync.LAMP_RENT,
                        RENT_TAX = sync.RENT_TAX,
                        BASE_AMT = sync.BASE_AMT,
                        BASE_TAX = sync.BASE_TAX,
                        ADJ_CHARGE = sync.ADJ_CHARGE,
                        CARRIERID_O = sync.CARRIERID_O,
                    });
                    number++;
                }

            }
                EfcsService.EFCS_LOG(_db, "抓取", "讀取了" + number2 + "筆資料,成功匯入" + number+"筆資料", "RCPM005自動抓取資料排程結束", Request.GetDisplayUrl(), "200");
                return Ok();
            }
            catch (Exception ex)
            {
                EfcsService.EFCS_LOG(_db, "抓取", ex.Message, "RCPM005自動抓取資料排程失敗", Request.GetDisplayUrl(), "500");
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
        [HttpGet("api/sync/RCPM005/day")]
        public async Task<IActionResult> AsyncRCPM005day(int day=1)
        {
            try
            {
                EfcsService.EFCS_LOG(_db, "抓取", "", "RCPM005自動抓取資料排程開始", HttpContext.Connection.RemoteIpAddress?.ToString(), "200");
                DateTime lastYear = DateTime.Now.AddDays(-(day));
                // 西元轉民國年
                int rocYear = lastYear.Year - 1911;

                // 組成民國年月日 (YYYMMDD)
                int rocDateNumber = int.Parse($"{rocYear:000}{lastYear.Month:00}{lastYear.Day:00}");


                using var conn = _db.CreateConnection();
                using var conn_sync = _sync_db.CreateConnection();

                string sql_u2 = @"SELECT * FROM RCPM005 WHERE RCV_YMD >= :rocDateNumber";
                var RCPM005 = await conn.QueryAsync(sql_u2, new { rocDateNumber = rocDateNumber });

                string sql_sync = @"SELECT * FROM HCG.RCPM005 WHERE RCV_YMD >= :rocDateNumber";
                var RCPM005_sync = await conn_sync.QueryAsync(sql_sync, new { rocDateNumber = rocDateNumber });
                //List<RCPM005> List = new List<RCPM005>();
                int number = 0;
                int number2 = 0;
                foreach (var sync in RCPM005_sync)
                {
                    bool type = true;
                    number2++;
                    foreach (var item in RCPM005)
                    {
                        if ((sync.CUST_NO == item.CUST_NO) && (sync.RECEPT_NO == item.RECEPT_NO))
                        {
                            type = false;
                        }
                    }
                    if (type)
                    {



                        string sql3 = @"
                    INSERT INTO RCPM005
                    (
                        CUST_NO,
                        RECEPT_NO,
                        RCV_YMD,
                        FILE_DATE,
                        CARRIERID,
                        GAS_CHARGE,
                        ADDED_TAX,
                        LAMP_RENT,
                        RENT_TAX,
                        BASE_AMT,
                        BASE_TAX,
                        ADJ_CHARGE,
                        CARRIERID_O
                    )
                    VALUES
                    (
                        :CUST_NO,
                        :RECEPT_NO,
                        :RCV_YMD,
                        :FILE_DATE,
                        :CARRIERID,
                        :GAS_CHARGE,
                        :ADDED_TAX,
                        :LAMP_RENT,
                        :RENT_TAX,
                        :BASE_AMT,
                        :BASE_TAX,
                        :ADJ_CHARGE,
                        :CARRIERID_O
                    )";

                        var affectedRows = await conn.ExecuteAsync(sql3, new
                        {
                            CUST_NO = sync.CUST_NO,
                            RECEPT_NO = sync.RECEPT_NO,
                            RCV_YMD = sync.RCV_YMD,
                            FILE_DATE = sync.FILE_DATE,
                            CARRIERID = sync.CARRIERID,
                            GAS_CHARGE = sync.GAS_CHARGE,
                            ADDED_TAX = sync.ADDED_TAX,
                            LAMP_RENT = sync.LAMP_RENT,
                            RENT_TAX = sync.RENT_TAX,
                            BASE_AMT = sync.BASE_AMT,
                            BASE_TAX = sync.BASE_TAX,
                            ADJ_CHARGE = sync.ADJ_CHARGE,
                            CARRIERID_O = sync.CARRIERID_O,
                        });
                        number++;
                    }

                }
                EfcsService.EFCS_LOG(_db, "抓取", "讀取了"+ number2 + "筆資料,成功匯入" + number + "筆資料", "RCPM005自動抓取資料排程結束", Request.GetDisplayUrl(), "200");
                return Ok();
            }
            catch (Exception ex)
            {
                EfcsService.EFCS_LOG(_db, "抓取", ex.Message, "RCPM005自動抓取資料排程失敗", Request.GetDisplayUrl(), "500");
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


        [HttpGet("api/sync/RCPM005/upd")]
        public async Task<IActionResult> AsyncRCPM005upd()
        {
            try
            {
                EfcsService.EFCS_LOG(_db, "更新", "", "RCPM005自動更新資料開始", HttpContext.Connection.RemoteIpAddress?.ToString(), "200");
                DateTime lastYear = DateTime.Now.AddYears(-1);

                // 西元轉民國年
                int rocYear = lastYear.Year - 1911;

                // 組成民國年月日 (YYYMMDD)
                int rocDateNumber = int.Parse($"{rocYear:000}{lastYear.Month:00}{lastYear.Day:00}");


                using var conn = _db.CreateConnection();
                using var conn_sync = _sync_db.CreateConnection();

                string sql_u2 = @"SELECT * FROM RCPM005 WHERE RCV_YMD >= :rocDateNumber";
                var RCPM005 = await conn.QueryAsync(sql_u2, new { rocDateNumber = rocDateNumber });

                string sql_sync = @"SELECT * FROM HCG.RCPM005 WHERE RCV_YMD >= :rocDateNumber";
                var RCPM005_sync = await conn_sync.QueryAsync(sql_sync, new { rocDateNumber = rocDateNumber });
                //List<RCPM005> List = new List<RCPM005>();
                int number = 0;
                int number2 = 0;
                foreach (var sync in RCPM005_sync)
                {
                    number2++;
                    var RCPM005One = RCPM005.FirstOrDefault(x => x.CUST_NO == sync.CUST_NO && x.RECEPT_NO == sync.RECEPT_NO);

                    if (RCPM005One.FILE_DATE != sync.FILE_DATE && RCPM005One != null)
                    {
                        string sql2 = @"UPDATE RCPM005 SET  FILE_DATE = :FILE_DATE WHERE  RECEPT_NO = :RECEPT_NO AND CUST_NO = :CUST_NO";

                        await conn.ExecuteAsync(sql2, new
                        {
                            RECEPT_NO = RCPM005One.RECEPT_NO,
                            CUST_NO = RCPM005One.CUST_NO,
                            FILE_DATE = sync.FILE_DATE
                        });
                        number++;
                    }


                }
                EfcsService.EFCS_LOG(_db, "更新", "讀取了" + number2 + "筆資料,成功更新" + number + "筆資料", "RCPM005自動更新資料結束", Request.GetDisplayUrl(), "200");
                return Ok();
            }
            catch (Exception ex)
            {
                EfcsService.EFCS_LOG(_db, "", ex.Message, "RCPM005自動更新資料失敗", Request.GetDisplayUrl(), "500");
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
        [HttpGet("api/sync/RCPM001")]
        public async Task<IActionResult> AsyncRCPM001()
        {
            try
            {
                EfcsService.EFCS_LOG(_db, "抓取", "", "RCPM001自動抓取資料排程開始", HttpContext.Connection.RemoteIpAddress?.ToString(), "200");
                using var conn = _db.CreateConnection();
            using var conn_sync = _sync_db.CreateConnection();

            string sql_u2 = @"SELECT * FROM RCPM001";
            var RCPM001 = await conn.QueryAsync(sql_u2);

            string sql_sync = @"SELECT * FROM HCG.RCPM001";
            var RCPM001_sync = await conn_sync.QueryAsync(sql_sync);
            int number = 0;
            int number2 = 0;
            foreach (var sync in RCPM001_sync)
            {
                bool type = true;
                number2++;

                foreach (var item in RCPM001)
                {
                    if (sync.CUST_NO == item.CUST_NO)
                    {
                        type = false;
                    }
                }
                if (type)
                {
                    string sql3 = @"
                    INSERT INTO RCPM001
                    (
                        CUST_NO
                    )
                    VALUES
                    (
                        :CUST_NO
                    )";

                    var affectedRows = await conn.ExecuteAsync(sql3, new
                    {
                        CUST_NO = sync.CUST_NO
                    });
                    number++;
                }

            }
                EfcsService.EFCS_LOG(_db, "抓取", "讀取了"+ number2 + "筆資料,成功匯入" + number + "筆資料", "RCPM001自動抓取資料排程結束", Request.GetDisplayUrl(), "200");
                return Ok();
            }
            catch (Exception ex)
            {
                EfcsService.EFCS_LOG(_db, "抓取", ex.Message, "RCPM001自動抓取資料排程失敗", Request.GetDisplayUrl(), "500");
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
