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
                        VOLUME,
                        LAMP,
                        SET_USAGE,
                        NEW_POINT,
                        OLD_POINT,
                        CHARGE_DEG,
                        GAS_CHARGE,
                        ADDED_TAX,
                        LAMP_RENT,
                        RENT_TAX,
                        BASE_AMT,
                        BASE_TAX,
                        ADJ_CHARGE,
                        BANK_CODE,
                        BANK_NO,
                        CLEAR_DT,
                        RCV_USERID,
                        PRINT_DT,
                        USER_ID,
                        UPD_DATETIME,
                        SP_NO,
                        STATUS_CODE,
                        PEN_AMT,
                        T_CODE,
                        FILE_DATE,
                        NEW_RECEPT_NO,
                        PAY_KIND,
                        USE_START_DT,
                        SAFE_AMT,
                        ADJ_MEMO1,
                        CARRIERID,
                        INVO_INV,
                        ADJ_TAX,
                        DONATEMARK,
                        NPOBAN,
                        PRINTMARK,
                        CARRIERID_O,
                        CARRIERID1,
                        PEN_TAX,
                        INVO_DATE,
                        CARRIERTYPE,
                        LAST_INVO_DATE,
                        LAST_INVO_NO,
                        MERGETOYMD,
                        ADJ_MEMO,
                        REMARK4R5,
                        SERVICE_TAX,
                        SERVICE_AMT,
                        SERVICE2_TAX,
                        SERVICE2_AMT
                    )
                    VALUES
                    (
                        :CUST_NO,
                        :RECEPT_NO,
                        :RCV_YMD,
                        :VOLUME,
                        :LAMP,
                        :SET_USAGE,
                        :NEW_POINT,
                        :OLD_POINT,
                        :CHARGE_DEG,
                        :GAS_CHARGE,
                        :ADDED_TAX,
                        :LAMP_RENT,
                        :RENT_TAX,
                        :BASE_AMT,
                        :BASE_TAX,
                        :ADJ_CHARGE,
                        :BANK_CODE,
                        :BANK_NO,
                        :CLEAR_DT,
                        :RCV_USERID,
                        :PRINT_DT,
                        :USER_ID,
                        :UPD_DATETIME,
                        :SP_NO,
                        :STATUS_CODE,
                        :PEN_AMT,
                        :T_CODE,
                        :FILE_DATE,
                        :NEW_RECEPT_NO,
                        :PAY_KIND,
                        :USE_START_DT,
                        :SAFE_AMT,
                        :ADJ_MEMO1,
                        :CARRIERID,
                        :INVO_INV,
                        :ADJ_TAX,
                        :DONATEMARK,
                        :NPOBAN,
                        :PRINTMARK,
                        :CARRIERID_O,
                        :CARRIERID1,
                        :PEN_TAX,
                        :INVO_DATE,
                        :CARRIERTYPE,
                        :LAST_INVO_DATE,
                        :LAST_INVO_NO,
                        :MERGETOYMD,
                        :ADJ_MEMO,
                        :REMARK4R5,
                        :SERVICE_TAX,
                        :SERVICE_AMT,
                        :SERVICE2_TAX,
                        :SERVICE2_AMT
                    )";
                        var affectedRows = await conn.ExecuteAsync(sql3, new
                        {
                            CUST_NO = sync.CUST_NO,
                            RECEPT_NO = sync.RECEPT_NO,
                            RCV_YMD = sync.RCV_YMD,
                            VOLUME = sync.VOLUME,
                            LAMP = sync.LAMP,
                            SET_USAGE = sync.SET_USAGE,
                            NEW_POINT = sync.NEW_POINT,
                            OLD_POINT = sync.OLD_POINT,
                            CHARGE_DEG = sync.CHARGE_DEG,
                            GAS_CHARGE = sync.GAS_CHARGE,
                            ADDED_TAX = sync.ADDED_TAX,
                            LAMP_RENT = sync.LAMP_RENT,
                            RENT_TAX = sync.RENT_TAX,
                            BASE_AMT = sync.BASE_AMT,
                            BASE_TAX = sync.BASE_TAX,
                            ADJ_CHARGE = sync.ADJ_CHARGE,
                            BANK_CODE = sync.BANK_CODE,
                            BANK_NO = sync.BANK_NO,
                            CLEAR_DT = sync.CLEAR_DT,
                            RCV_USERID = sync.RCV_USERID,
                            PRINT_DT = sync.PRINT_DT,
                            USER_ID = sync.USER_ID,
                            UPD_DATETIME = sync.UPD_DATETIME,
                            SP_NO = sync.SP_NO,
                            STATUS_CODE = sync.STATUS_CODE,
                            PEN_AMT = sync.PEN_AMT,
                            T_CODE = sync.T_CODE,
                            FILE_DATE = sync.FILE_DATE,
                            NEW_RECEPT_NO = sync.NEW_RECEPT_NO,
                            PAY_KIND = sync.PAY_KIND,
                            USE_START_DT = sync.USE_START_DT,
                            SAFE_AMT = sync.SAFE_AMT,
                            ADJ_MEMO1 = sync.ADJ_MEMO1,
                            CARRIERID = sync.CARRIERID,
                            INVO_INV = sync.INVO_INV,
                            ADJ_TAX = sync.ADJ_TAX,
                            DONATEMARK = sync.DONATEMARK,
                            NPOBAN = sync.NPOBAN,
                            PRINTMARK = sync.PRINTMARK,
                            CARRIERID_O = sync.CARRIERID_O,
                            CARRIERID1 = sync.CARRIERID1,
                            PEN_TAX = sync.PEN_TAX,
                            INVO_DATE = sync.INVO_DATE,
                            CARRIERTYPE = sync.CARRIERTYPE,
                            LAST_INVO_DATE = sync.LAST_INVO_DATE,
                            LAST_INVO_NO = sync.LAST_INVO_NO,
                            MERGETOYMD = sync.MERGETOYMD,
                            ADJ_MEMO = sync.ADJ_MEMO,
                            REMARK4R5 = sync.REMARK4R5,
                            SERVICE_TAX = sync.SERVICE_TAX,
                            SERVICE_AMT = sync.SERVICE_AMT,
                            SERVICE2_TAX = sync.SERVICE2_TAX,
                            SERVICE2_AMT = sync.SERVICE2_AMT
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
                        VOLUME,
                        LAMP,
                        SET_USAGE,
                        NEW_POINT,
                        OLD_POINT,
                        CHARGE_DEG,
                        GAS_CHARGE,
                        ADDED_TAX,
                        LAMP_RENT,
                        RENT_TAX,
                        BASE_AMT,
                        BASE_TAX,
                        ADJ_CHARGE,
                        BANK_CODE,
                        BANK_NO,
                        CLEAR_DT,
                        RCV_USERID,
                        PRINT_DT,
                        USER_ID,
                        UPD_DATETIME,
                        SP_NO,
                        STATUS_CODE,
                        PEN_AMT,
                        T_CODE,
                        FILE_DATE,
                        NEW_RECEPT_NO,
                        PAY_KIND,
                        USE_START_DT,
                        SAFE_AMT,
                        ADJ_MEMO1,
                        CARRIERID,
                        INVO_INV,
                        ADJ_TAX,
                        DONATEMARK,
                        NPOBAN,
                        PRINTMARK,
                        CARRIERID_O,
                        CARRIERID1,
                        PEN_TAX,
                        INVO_DATE,
                        CARRIERTYPE,
                        LAST_INVO_DATE,
                        LAST_INVO_NO,
                        MERGETOYMD,
                        ADJ_MEMO,
                        REMARK4R5,
                        SERVICE_TAX,
                        SERVICE_AMT,
                        SERVICE2_TAX,
                        SERVICE2_AMT
                    )
                    VALUES
                    (
                        :CUST_NO,
                        :RECEPT_NO,
                        :RCV_YMD,
                        :VOLUME,
                        :LAMP,
                        :SET_USAGE,
                        :NEW_POINT,
                        :OLD_POINT,
                        :CHARGE_DEG,
                        :GAS_CHARGE,
                        :ADDED_TAX,
                        :LAMP_RENT,
                        :RENT_TAX,
                        :BASE_AMT,
                        :BASE_TAX,
                        :ADJ_CHARGE,
                        :BANK_CODE,
                        :BANK_NO,
                        :CLEAR_DT,
                        :RCV_USERID,
                        :PRINT_DT,
                        :USER_ID,
                        :UPD_DATETIME,
                        :SP_NO,
                        :STATUS_CODE,
                        :PEN_AMT,
                        :T_CODE,
                        :FILE_DATE,
                        :NEW_RECEPT_NO,
                        :PAY_KIND,
                        :USE_START_DT,
                        :SAFE_AMT,
                        :ADJ_MEMO1,
                        :CARRIERID,
                        :INVO_INV,
                        :ADJ_TAX,
                        :DONATEMARK,
                        :NPOBAN,
                        :PRINTMARK,
                        :CARRIERID_O,
                        :CARRIERID1,
                        :PEN_TAX,
                        :INVO_DATE,
                        :CARRIERTYPE,
                        :LAST_INVO_DATE,
                        :LAST_INVO_NO,
                        :MERGETOYMD,
                        :ADJ_MEMO,
                        :REMARK4R5,
                        :SERVICE_TAX,
                        :SERVICE_AMT,
                        :SERVICE2_TAX,
                        :SERVICE2_AMT
                    )";
                        var affectedRows = await conn.ExecuteAsync(sql3, new
                        {
                            CUST_NO = sync.CUST_NO,
                            RECEPT_NO = sync.RECEPT_NO,
                            RCV_YMD = sync.RCV_YMD,
                            VOLUME = sync.VOLUME,
                            LAMP = sync.LAMP,
                            SET_USAGE = sync.SET_USAGE,
                            NEW_POINT = sync.NEW_POINT,
                            OLD_POINT = sync.OLD_POINT,
                            CHARGE_DEG = sync.CHARGE_DEG,
                            GAS_CHARGE = sync.GAS_CHARGE,
                            ADDED_TAX = sync.ADDED_TAX,
                            LAMP_RENT = sync.LAMP_RENT,
                            RENT_TAX = sync.RENT_TAX,
                            BASE_AMT = sync.BASE_AMT,
                            BASE_TAX = sync.BASE_TAX,
                            ADJ_CHARGE = sync.ADJ_CHARGE,
                            BANK_CODE = sync.BANK_CODE,
                            BANK_NO = sync.BANK_NO,
                            CLEAR_DT = sync.CLEAR_DT,
                            RCV_USERID = sync.RCV_USERID,
                            PRINT_DT = sync.PRINT_DT,
                            USER_ID = sync.USER_ID,
                            UPD_DATETIME = sync.UPD_DATETIME,
                            SP_NO = sync.SP_NO,
                            STATUS_CODE = sync.STATUS_CODE,
                            PEN_AMT = sync.PEN_AMT,
                            T_CODE = sync.T_CODE,
                            FILE_DATE = sync.FILE_DATE,
                            NEW_RECEPT_NO = sync.NEW_RECEPT_NO,
                            PAY_KIND = sync.PAY_KIND,
                            USE_START_DT = sync.USE_START_DT,
                            SAFE_AMT = sync.SAFE_AMT,
                            ADJ_MEMO1 = sync.ADJ_MEMO1,
                            CARRIERID = sync.CARRIERID,
                            INVO_INV = sync.INVO_INV,
                            ADJ_TAX = sync.ADJ_TAX,
                            DONATEMARK = sync.DONATEMARK,
                            NPOBAN = sync.NPOBAN,
                            PRINTMARK = sync.PRINTMARK,
                            CARRIERID_O = sync.CARRIERID_O,
                            CARRIERID1 = sync.CARRIERID1,
                            PEN_TAX = sync.PEN_TAX,
                            INVO_DATE = sync.INVO_DATE,
                            CARRIERTYPE = sync.CARRIERTYPE,
                            LAST_INVO_DATE = sync.LAST_INVO_DATE,
                            LAST_INVO_NO = sync.LAST_INVO_NO,
                            MERGETOYMD = sync.MERGETOYMD,
                            ADJ_MEMO = sync.ADJ_MEMO,
                            REMARK4R5 = sync.REMARK4R5,
                            SERVICE_TAX = sync.SERVICE_TAX,
                            SERVICE_AMT = sync.SERVICE_AMT,
                            SERVICE2_TAX = sync.SERVICE2_TAX,
                            SERVICE2_AMT = sync.SERVICE2_AMT
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
        [HttpGet("api/sync/RCPM005/UpdALL")]
        public async Task<IActionResult> AsyncRCPM005_UpdALL(
       [FromQuery] string startDate,
       [FromQuery] string endDate)
        {
            // ── 1. 驗證日期輸入 ──────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
            {
                return BadRequest(new
                {
                    DOCDATA = new
                    {
                        HEAD = new
                        {
                            ICCHK_CODE = "S400",
                            ICCHK_CODE_DESC = "請輸入日期範圍 (startDate / endDate)，格式 YYYMMDD（例如 1150101）"
                        }
                    }
                });
            }

            if (!int.TryParse(startDate, out int rocStart) ||
                !int.TryParse(endDate, out int rocEnd) ||
                startDate.Length != 7 || endDate.Length != 7)
            {
                return BadRequest(new
                {
                    DOCDATA = new
                    {
                        HEAD = new
                        {
                            ICCHK_CODE = "S400",
                            ICCHK_CODE_DESC = "日期格式錯誤，請使用民國 YYYMMDD（例如 1150101）"
                        }
                    }
                });
            }

            if (rocStart > rocEnd)
            {
                return BadRequest(new
                {
                    DOCDATA = new
                    {
                        HEAD = new
                        {
                            ICCHK_CODE = "S400",
                            ICCHK_CODE_DESC = "startDate 不可大於 endDate"
                        }
                    }
                });
            }

            try
            {
                EfcsService.EFCS_LOG(_db, "修改", "",
                    $"RCPM005修改資料開始，範圍 {rocStart}~{rocEnd}",
                    HttpContext.Connection.RemoteIpAddress?.ToString(), "200");

                using var conn = _db.CreateConnection();
                using var conn_sync = _sync_db.CreateConnection();

                string sql_sync = @"
            SELECT * FROM HCG.RCPM005
            WHERE RCV_YMD >= :rocStart AND RCV_YMD <= :rocEnd";

                var RCPM005_sync = await conn_sync.QueryAsync(
                    sql_sync, new { rocStart, rocEnd });

                int number = 0;
                int number2 = 0;

                foreach (var sync in RCPM005_sync)
                {
                    number2++;

                    string sql_check = @"
                SELECT COUNT(1) FROM RCPM005
                WHERE CUST_NO = :CUST_NO AND RECEPT_NO = :RECEPT_NO";

                    int exists = await conn.ExecuteScalarAsync<int>(sql_check, new
                    {
                        CUST_NO = sync.CUST_NO,
                        RECEPT_NO = sync.RECEPT_NO
                    });

                    if (exists > 0)
                    {
                        string sql_update = @"
UPDATE RCPM005 SET
    RCV_YMD        = :RCV_YMD,
    VOLUME         = :VOLUME,
    LAMP           = :LAMP,
    SET_USAGE      = :SET_USAGE,
    NEW_POINT      = :NEW_POINT,
    OLD_POINT      = :OLD_POINT,
    CHARGE_DEG     = :CHARGE_DEG,
    GAS_CHARGE     = :GAS_CHARGE,
    ADDED_TAX      = :ADDED_TAX,
    LAMP_RENT      = :LAMP_RENT,
    RENT_TAX       = :RENT_TAX,
    BASE_AMT       = :BASE_AMT,
    BASE_TAX       = :BASE_TAX,
    ADJ_CHARGE     = :ADJ_CHARGE,
    BANK_CODE      = :BANK_CODE,
    BANK_NO        = :BANK_NO,
    CLEAR_DT       = :CLEAR_DT,
    RCV_USERID     = :RCV_USERID,
    PRINT_DT       = :PRINT_DT,
    USER_ID        = :USER_ID,
    UPD_DATETIME   = :UPD_DATETIME,
    SP_NO          = :SP_NO,
    STATUS_CODE    = :STATUS_CODE,
    PEN_AMT        = :PEN_AMT,
    T_CODE         = :T_CODE,
    FILE_DATE      = :FILE_DATE,
    NEW_RECEPT_NO  = :NEW_RECEPT_NO,
    PAY_KIND       = :PAY_KIND,
    USE_START_DT   = :USE_START_DT,
    SAFE_AMT       = :SAFE_AMT,
    ADJ_MEMO1      = :ADJ_MEMO1,
    CARRIERID      = :CARRIERID,
    INVO_INV       = :INVO_INV,
    ADJ_TAX        = :ADJ_TAX,
    DONATEMARK     = :DONATEMARK,
    NPOBAN         = :NPOBAN,
    PRINTMARK      = :PRINTMARK,
    CARRIERID_O    = :CARRIERID_O,
    CARRIERID1     = :CARRIERID1,
    PEN_TAX        = :PEN_TAX,
    INVO_DATE      = :INVO_DATE,
    CARRIERTYPE    = :CARRIERTYPE,
    LAST_INVO_DATE = :LAST_INVO_DATE,
    LAST_INVO_NO   = :LAST_INVO_NO,
    MERGETOYMD     = :MERGETOYMD,
    ADJ_MEMO       = :ADJ_MEMO,
    REMARK4R5      = :REMARK4R5,
    SERVICE_TAX    = :SERVICE_TAX,
    SERVICE_AMT    = :SERVICE_AMT
WHERE CUST_NO = :CUST_NO AND RECEPT_NO = :RECEPT_NO";

                        await conn.ExecuteAsync(sql_update, new
                        {
                            CUST_NO = sync.CUST_NO,
                            RECEPT_NO = sync.RECEPT_NO,
                            RCV_YMD = sync.RCV_YMD,
                            VOLUME = sync.VOLUME,
                            LAMP = sync.LAMP,
                            SET_USAGE = sync.SET_USAGE,
                            NEW_POINT = sync.NEW_POINT,
                            OLD_POINT = sync.OLD_POINT,
                            CHARGE_DEG = sync.CHARGE_DEG,
                            GAS_CHARGE = sync.GAS_CHARGE,
                            ADDED_TAX = sync.ADDED_TAX,
                            LAMP_RENT = sync.LAMP_RENT,
                            RENT_TAX = sync.RENT_TAX,
                            BASE_AMT = sync.BASE_AMT,
                            BASE_TAX = sync.BASE_TAX,
                            ADJ_CHARGE = sync.ADJ_CHARGE,
                            BANK_CODE = sync.BANK_CODE,
                            BANK_NO = sync.BANK_NO,
                            CLEAR_DT = sync.CLEAR_DT,
                            RCV_USERID = sync.RCV_USERID,
                            PRINT_DT = sync.PRINT_DT,
                            USER_ID = sync.USER_ID,
                            UPD_DATETIME = sync.UPD_DATETIME,
                            SP_NO = sync.SP_NO,
                            STATUS_CODE = sync.STATUS_CODE,
                            PEN_AMT = sync.PEN_AMT,
                            T_CODE = sync.T_CODE,
                            FILE_DATE = sync.FILE_DATE,
                            NEW_RECEPT_NO = sync.NEW_RECEPT_NO,
                            PAY_KIND = sync.PAY_KIND,
                            USE_START_DT = sync.USE_START_DT,
                            SAFE_AMT = sync.SAFE_AMT,
                            ADJ_MEMO1 = sync.ADJ_MEMO1,
                            CARRIERID = sync.CARRIERID,
                            INVO_INV = sync.INVO_INV,
                            ADJ_TAX = sync.ADJ_TAX,
                            DONATEMARK = sync.DONATEMARK,
                            NPOBAN = sync.NPOBAN,
                            PRINTMARK = sync.PRINTMARK,
                            CARRIERID_O = sync.CARRIERID_O,
                            CARRIERID1 = sync.CARRIERID1,
                            PEN_TAX = sync.PEN_TAX,
                            INVO_DATE = sync.INVO_DATE,
                            CARRIERTYPE = sync.CARRIERTYPE,
                            LAST_INVO_DATE = sync.LAST_INVO_DATE,
                            LAST_INVO_NO = sync.LAST_INVO_NO,
                            MERGETOYMD = sync.MERGETOYMD,
                            ADJ_MEMO = sync.ADJ_MEMO,
                            REMARK4R5 = sync.REMARK4R5,
                            SERVICE_TAX = sync.SERVICE_TAX,
                            SERVICE_AMT = sync.SERVICE_AMT
                        });
                        number++;
                    }
                }

                EfcsService.EFCS_LOG(_db, "修改",
                    $"讀取了 {number2} 筆資料，成功更新 {number} 筆資料",
                    "RCPM005自動抓取資料排程結束",
                    Request.GetDisplayUrl(), "200");

                return Ok();
            }
            catch (Exception ex)
            {
                EfcsService.EFCS_LOG(_db, "修改", ex.Message,
                    "RCPM005自動抓取資料排程失敗",
                    Request.GetDisplayUrl(), "500");

                return Ok(new
                {
                    DOCDATA = new
                    {
                        HEAD = new
                        {
                            ICCHK_CODE = "S999",
                            ICCHK_CODE_DESC = ex.Message
                        }
                    }
                });
            }
        }



    }
}
