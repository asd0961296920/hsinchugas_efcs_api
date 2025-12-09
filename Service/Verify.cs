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



        public static object? ValidateB207(BillerDataQueryRq body)
        {
            // ---- 必填欄位檢核 ----
            if (string.IsNullOrWhiteSpace(body.QUERY_TYPE))
                return Error("I300", "查詢條件型態不得為空");

            if (string.IsNullOrWhiteSpace(body.QUERY_DATA1))
                return Error("I301", "查詢條件參數 1 不得為空");

            // ---- 長度檢核（規格：字串長度不能超過定義） ----
            if (body.QUERY_TYPE?.Length > 1)
                return Error("I300", "查詢條件型態長度不符（需為 1）");

            if (body.QUERY_DATA1?.Length > 20)
                return Error("I301", "查詢條件參數 1 長度不可超過 20");

            if (body.QUERY_DATA2?.Length > 20)
                return Error("I302", "查詢條件參數 2 長度不可超過 20");

            if (body.QUERY_DATA3?.Length > 20)
                return Error("I303", "查詢條件參數 3 長度不可超過 20");

            if (body.QUERY_DATA4?.Length > 20)
                return Error("I304", "查詢條件參數 4 長度不可超過 20");

            if (body.QUERY_DATA5?.Length > 20)
                return Error("I305", "查詢條件參數 5 長度不可超過 20");

            // 通過檢核
            return null;
        }



        public static object? ValidateB208(BillerDataPayRq body)
        {
            // ========= 1. 總筆數檢核 =========
            if (body.PAYHEAD == null)
                return Error("I401", "PAYHEAD 不得為空");

            if (body.PAYHEAD.TOTAL_COUNT <= 0)
                return Error("I401", "總筆數屬性檢核不符");

            if (body.PAYDEATIL == null || body.PAYDEATIL.Count == 0)
                return Error("I402", "無明細資料");

            if (body.PAYHEAD.TOTAL_COUNT != body.PAYDEATIL.Count)
                return Error("I402", "總筆數與明細筆數不符");

            // ========= 2. 總金額檢核 =========
            decimal TOTAL_AMOUNT = 0;
            /*
            if (body.PAYHEAD.TOTAL_AMOUNT <= 0)
                return Error("1403", "總金額屬性檢核不符");
            */
            var sumDetailAmount = body.PAYDEATIL.Sum(x => x.TXNAMOUNT);

            if (sumDetailAmount != body.PAYHEAD.TOTAL_AMOUNT)
                return Error("I404", "總金額與明細加總不符");

            // ========= 3. 明細逐筆檢核 =========
            foreach (var d in body.PAYDEATIL)
            {
                TOTAL_AMOUNT += d.TXNAMOUNT;
                // 扣款單位代號檢核
                if (string.IsNullOrWhiteSpace(d.OUTBANKID))
                    return Error("I405", "扣款單位代號檢核失敗（不得為空）");

                // 扣款帳號檢核
                if (string.IsNullOrWhiteSpace(d.OUTACCOUNTNUM))
                    return Error("I406", "扣款帳號檢核失敗（不得為空）");

                // 繳費金額檢核
                if (d.TXNAMOUNT <= 0)
                    return Error("I407", "繳費金額不得小於等於 0");

                // 繳費日期檢核（格式 YYYYMMDD）
                if (!Regex.IsMatch(d.PAY_DATE ?? "", @"^\d{8}$"))
                    return Error("I408", "繳費日期格式不符（需 YYYYMMDD）");

                // 繳費時間檢核（格式 HHMMSS）
                if (!Regex.IsMatch(d.PAY_TIME ?? "", @"^\d{6}$"))
                    return Error("I409", "繳費時間格式不符（需 HHMMSS）");

                // 清算日期檢核（格式 YYYYMMDD）
                if (!Regex.IsMatch(d.PAY_CLRDATE ?? "", @"^\d{8}$"))
                    return Error("I410", "清算日期格式不符（需 YYYYMMDD）");
                /*
                // 繳費方式檢核（文件規定 1/2/3/4/6/8/T/Z）
                var validPayTypes = new[] { "1", "2", "3", "4", "6", "8", "T", "Z" };
                if (!validPayTypes.Contains(d.CHECKTYPE))
                    return Error("1411", "繳費方式不符規範");
                */
                // 銷帳方式檢核（B 或 C）
                if (d.BILLTYPE != "B" && d.BILLTYPE != "C")
                    return Error("I412", "銷帳方式須為 B 或 C");

                // 銷帳資料檢核
                if (string.IsNullOrWhiteSpace(d.BILLDATA))
                    return Error("I413", "銷帳資料不得為空");
            }
            // ========= 2. 總金額檢核 =========
            if (TOTAL_AMOUNT != body.PAYHEAD.TOTAL_AMOUNT)
                return Error("I403", "總金額屬性檢核不符");



            // 全部通過
            return null;
        }









    }
}
