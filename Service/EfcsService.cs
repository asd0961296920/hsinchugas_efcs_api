using Dapper;
using hsinchugas_efcs_api.Model;
using System;
using System.Globalization;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;


namespace hsinchugas_efcs_api.Service
{

    public class EfcsService
    {
        private readonly IConfiguration _config;

        public EfcsService(IConfiguration config)
        {
            _config = config;
        }

        // 產生 DIG
        public static string GenerateDIG(string docData)
        {
            // 步驟一：requestString = DOCDATA
            string requestString = docData;

            // 步驟二：SHA256 摘要
            byte[] sha256Bytes;
            using (SHA256 sha = SHA256.Create())
            {
                sha256Bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(requestString));
            }

            // 步驟三：取 SHA256 前 8 Bytes
            byte[] checkCode = new byte[8];
            Array.Copy(sha256Bytes, 0, checkCode, 0, 8);

            // 步驟四：轉 HEX（大寫）
            return BitConverter.ToString(checkCode).Replace("-", "").ToUpper();
        }

        //B207銷帳資料
        public string BuildBarcodeC(string userNo, string billFront, string billEnd, string amount)
        {
            // 第一段：10 碼，右補空白
            string part1 = (userNo ?? "").PadRight(9, ' ');

            // 第二段：20 碼 → 前段 + (0 補滿) + 後段
            billFront = billFront ?? "";
            billEnd = billEnd ?? "";

            // STEP 1：合併後要變成 16 碼，不足在 billEnd 前補 0
            string part2 = billFront + billEnd;

            if (part2.Length < 16)
            {
                int needZero = 16 - part2.Length;
                billEnd = new string('0', needZero) + billEnd;
                part2 = billFront + billEnd;
            }
            if (part2.Length < 20)
                part2 = part2.PadRight(20, ' ');


            // 第三段：15 碼，右補空白
            string part3 = (amount ?? "").PadRight(15, ' ');


            // 最終組合（51 碼）
            return part1 + part2 + part3;
        }
        //取出三段式條碼用戶代號
        public string ExtractUserCode(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length < 16)
                return "";

            // 用戶代號：從 index 9 開始，長度 7
            return input.Substring(9, 7);
        }
        //取出三段式條碼帳單編號
        public  string ExtractBillNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length < 20)
                return "";

            // 帳單編號從 index 16 開始，直到遇到空白
            int start = 16;

            // 找帳單編號的結束點（遇到空白就停止）
            int end = start;
            while (end < input.Length && input[end] != ' ')
            {
                end++;
            }

            // 取得帳單欄位
            string billRaw = input.Substring(start, end - start);

            // 去除左側 0
            return billRaw.TrimStart('0');
        }


        //B212 HEAD
        public HEAD B212HEAD()
        {
            var B212HEAD = new HEAD();

            B212HEAD.SRC_ID = _config["HEAD:SRC_ID"];
            B212HEAD.DEST_ID = _config["HEAD:DEST_ID"];

            B212HEAD.TXN_DATETIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            B212HEAD.TXN_NO = "EFCS" +DateTime.Now.ToString("yyyyMMddHHmmss");
            B212HEAD.PRS_CODE = "B212";

            B212HEAD.BILLER_ID = _config["HEAD:BILLER_ID"];

            B212HEAD.BTYPECLASS = "0111";
            B212HEAD.ICCHK_CODE = "9999";
            B212HEAD.ICCHK_CODE_DESC = "成功";
            return B212HEAD;
        }






        // ----------------------------------------------------------------------------------------
        // ★ 最終正確版：可跑出 MAC = 3D051450（範例 PRS_CODE = B207）
        // ----------------------------------------------------------------------------------------
        public static string ComputeMac(string macdataString, string txnDatetime, string keyHex)
        {
            // ---------------------------------------------------
            // 步驟 1：macdataString = DOCDATA 原文（不可有換行）
            // ---------------------------------------------------
            macdataString = macdataString.Replace("\r", "").Replace("\n", "");

            // ---------------------------------------------------
            // 步驟 2：SHA256，取得 32 bytes 原始值
            // ---------------------------------------------------
            byte[] shaBytes;
            using (SHA256 sha = SHA256.Create())
            {
                shaBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(macdataString));
            }

            // ---------------------------------------------------
            // 步驟 3：ICV = TXN_DATETIME 補滿 16 位（右邊補 '0'）
            // 例如：20180716140509 → 2018071614050900
            // ---------------------------------------------------
            string icvString = txnDatetime.PadRight(16, '0');
            byte[] ivBytes = PackICV(icvString); // 正確轉 8 bytes

            // ---------------------------------------------------
            // 步驟 4：3DES CBC（無 Padding）
            // ---------------------------------------------------
            byte[] keyBytes = HexToBytes(keyHex);
            FixParityBits(keyBytes); // ★ 依 FIPS 46-3 規範修正 parity（Java 與 C# 才會一致）

            byte[] macCode = TripleDESCBCEncrypt(shaBytes, keyBytes, ivBytes);

            // ---------------------------------------------------
            // 步驟 5：取最後一個 BLOCK（8 bytes）前 4 bytes
            // ---------------------------------------------------
            byte[] last8 = macCode.Skip(macCode.Length - 8).Take(8).ToArray();
            byte[] mac4 = last8.Take(4).ToArray();

            // ---------------------------------------------------
            // 步驟 6：轉 HEX（大寫）
            // ---------------------------------------------------
            return BytesToHex(mac4);
        }

        // ----------------------------------------------------------------------------------------
        // 依規格：ICV = 16 個十六進位字元 → 每 2 個字元 pack 成 1 byte（共 8 bytes）
        // ----------------------------------------------------------------------------------------
        private static byte[] PackICV(string icv16)
        {
            byte[] buffer = new byte[8];
            for (int i = 0; i < 16; i += 2)
            {
                buffer[i / 2] = Convert.ToByte(icv16.Substring(i, 2), 16);
            }
            return buffer;
        }

        // ----------------------------------------------------------------------------------------
        // ★ 修正 3DES Parity bit（依 FIPS 46-3，每 7 bits 需加入 parity）
        // ----------------------------------------------------------------------------------------
        private static void FixParityBits(byte[] key)
        {
            for (int i = 0; i < key.Length; i++)
            {
                int b = key[i];
                int bitCount = 0;
                for (int j = 1; j < 8; j++)
                {
                    if (((b >> j) & 1) == 1) bitCount++;
                }

                // 奇偶校驗（LSB 為校驗 bit）
                if ((bitCount % 2) == 0)
                    key[i] = (byte)(b | 0x01); // 設為奇數
                else
                    key[i] = (byte)(b & 0xFE); // 設為偶數
            }
        }

        // ----------------------------------------------------------------------------------------
        // Triple DES CBC Encrypt（無 Padding，自行補 0）
        // ----------------------------------------------------------------------------------------
        private static byte[] TripleDESCBCEncrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
            {
                tdes.Key = key;
                tdes.IV = iv;
                tdes.Mode = CipherMode.CBC;
                tdes.Padding = PaddingMode.None;

                // 8 byte 對齊
                int mod = data.Length % 8;
                if (mod != 0)
                {
                    int pad = 8 - mod;
                    Array.Resize(ref data, data.Length + pad);
                }

                using (ICryptoTransform enc = tdes.CreateEncryptor())
                {
                    return enc.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }

        // HEX → bytes
        private static byte[] HexToBytes(string hex)
        {
            byte[] buf = new byte[hex.Length / 2];
            for (int i = 0; i < buf.Length; i++)
                buf[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return buf;
        }

        // bytes → HEX 字串
        private static string BytesToHex(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var b in data)
                sb.AppendFormat("{0:X2}", b);
            return sb.ToString();
        }



        public BillDecodeResult DecodeBillData(string billdata)
        {
            if (string.IsNullOrWhiteSpace(billdata))
                throw new ArgumentException("BILLDATA 不可為空");

            if (billdata.Length < 19) // 7 + 7 + 至少 5
                throw new ArgumentException("BILLDATA 長度不符格式");

            // 固定長度切割
            string custNo = billdata.Substring(0, 7);  // CUST_NO(7)
            string rcyYmd = billdata.Substring(7, 6);  // RCV_YMD(7)
            string receiptNo = billdata.Substring(13); // 剩下全部（RECEIPT_NO 5–6）

            return new BillDecodeResult
            {
                CUST_NO = custNo,
                RCV_YMD = rcyYmd,
                RECEPT_NO = receiptNo
            };
        }

        public BillDecodeResult2 DecodeBillData2(string billdata)
        {
            if (string.IsNullOrWhiteSpace(billdata))
                throw new ArgumentException("BILLDATA 不可為空");

            if (billdata.Length < 7) // 7 + 7 + 至少 5
                throw new ArgumentException("BILLDATA 長度不符格式");

            // 固定長度切割
            string custNo = billdata.Substring(0, 7);  // CUST_NO(7)
            //string rcyYmd = billdata.Substring(7, 6);  // RCV_YMD(7)
            string receiptNo = billdata.Substring(7); // 剩下全部（RECEIPT_NO 5–6）

            return new BillDecodeResult2
            {
                CUST_NO = custNo,
                RECEPT_NO = receiptNo
            };
        }
        public static string GetTaiwanDate()
        {
            TaiwanCalendar tc = new TaiwanCalendar();
            DateTime now = DateTime.Now;
            return $"{tc.GetYear(now)}{now:MMdd}";
        }

        public static string GetCARRIERIDDate(string CARRIERID)
        {
            if (string.IsNullOrEmpty(CARRIERID))
                return null; // 你也可以改成 return ""; 依需求

            int index = CARRIERID.LastIndexOf('/');
            if (index == -1)
                return CARRIERID;  // 沒有 "/" → 整串回傳

            return CARRIERID.Substring(index + 1); // 切 "/" 後的字串
        }
        

        public decimal TotalAmount(dynamic item)
        {
            return
                (item.GAS_CHARGE ?? 0)
              + (item.ADDED_TAX ?? 0)
              + (item.LAMP_RENT ?? 0)
              + (item.RENT_TAX ?? 0)
              + (item.BASE_AMT ?? 0)
              + (item.BASE_TAX ?? 0)
              + (item.ADJ_CHARGE ?? 0);
        }

        public static async void  EFCS_LOG(OracleDbContext db,string Log,string return_log,string rmark,string url,string code)
        {
            using var conn = db.CreateConnection();
            string sql = @"
INSERT INTO EFCS_LOG 
    (Add_Date, Json_Log, url, rmark, return_code,Return_Log)
VALUES 
    (:Add_Date, :Json_Log, :url, :rmark, :return_code, :Return_Log)";

            await conn.ExecuteAsync(sql, new
            {
                Add_Date = DateTime.Now,
                Json_Log = Log,
                url = url,
                rmark = rmark,
                return_code = code,
                Return_Log = return_log
            });

        }

        public static string GetNext20thDate()
        {
            DateTime today = DateTime.Now;

            // 本月 20 號
            DateTime thisMonth20 = new DateTime(today.Year, today.Month, 20);

            DateTime result;

            result = thisMonth20.AddMonths(1);

            return result.ToString("yyyyMMdd");
        }
        //西元轉民國
        public string ToTaiwanDate(string yyyyMMdd)
        {
            if (string.IsNullOrWhiteSpace(yyyyMMdd) || yyyyMMdd.Length != 8)
                return "";

            // 解析西元年月日
            string y = yyyyMMdd.Substring(0, 4);
            string m = yyyyMMdd.Substring(4, 2);
            string d = yyyyMMdd.Substring(6, 2);

            if (!int.TryParse(y, out int year))
                return "";

            // 民國年 = 西元年 - 1911
            int twYear = year - 1911;

            return twYear.ToString("000") + m + d;
        }
        //去頭的民國日期
        public string ToTaiwanDate6(string yyyyMMdd)
        {
            string tw = this.ToTaiwanDate(yyyyMMdd); // 例如: 1130201

            if (tw.Length == 7)
                return tw.Substring(1); // 去掉第一碼 → 130201

            return tw;
        }
        //4碼的民國日期
        public string ToTaiwanDate4(string yyyyMM)
        {
            // 取年份與月份
            int year = int.Parse(yyyyMM.Substring(0, 4));
            string mm = yyyyMM.Substring(4, 2);

            // 民國年 +1（你的需求），並取後兩碼
            string tw2 = (year - 1911).ToString("000").Substring(1);

            // 組出結果
            return tw2 + mm;
        }
        //向前補0到9碼
        public string PadLeftTo9(string input)
        {
            return (input ?? "").PadLeft(9, '0');
        }

    }
}
