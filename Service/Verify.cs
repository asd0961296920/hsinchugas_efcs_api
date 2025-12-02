using hsinchugas_efcs_api.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace hsinchugas_efcs_api.Service
{
    public class Verify
    {
        
        //====== 通用檢核 ======//
        public static object? CheckCommon<T>(ALL<T> request)
        {
            var head = request.DOCDATA.HEAD;

            // 欄位必填檢查
            if (string.IsNullOrWhiteSpace(head.SRC_ID))
                return Error("I100", "來源端代碼不可空白");

            if (string.IsNullOrWhiteSpace(head.DEST_ID))
                return Error("I101", "目的端代碼不可空白");

            if (string.IsNullOrWhiteSpace(head.TXN_DATETIME))
                return Error("I102", "交易日期時間不可空白");

            if (!Regex.IsMatch(head.TXN_DATETIME, @"^\d{14}$"))
                return Error("I103", "TXN_DATETIME 格式錯誤 (YYYYMMDDHHMMSS)");

            if (string.IsNullOrWhiteSpace(head.TXN_NO))
                return Error("I104", "交易序號不可空白");

            if (string.IsNullOrWhiteSpace(head.PRS_CODE))
                return Error("I105", "PRS_CODE 不可空白");

            if (string.IsNullOrWhiteSpace(head.BILLER_ID))
                return Error("I106", "BILLER_ID 不可空白");

            if (string.IsNullOrWhiteSpace(head.BTYPECLASS))
                return Error("I107", "BTYPECLASS 不可空白");

            // 通用格式檢查成功 → 回傳 null
            return null;
        }


        private static object Error(string code, string message)
        {
            var errorResponse = new
            {
                DOCDATA = new
                {
                    HEAD = new
                    {
                        ICCHK_CODE = code,
                        ICCHK_CODE_DESC = message
                    }
                }
            };

            return errorResponse;
        }



    }
}
