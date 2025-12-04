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



        //B212 HEAD
        public HEAD B212HEAD()
        {
            var B212HEAD = new HEAD();

            B212HEAD.SRC_ID = _config["HEAD:SRC_ID"];
            B212HEAD.DEST_ID = _config["HEAD:DEST_ID"];

            B212HEAD.TXN_DATETIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            B212HEAD.TXN_NO = "EFCS" +DateTime.Now.ToString("yyyyMMddHHmmss");
            B212HEAD.PRS_CODE = "B207";

            B212HEAD.BILLER_ID = _config["HEAD:BILLER_ID"];

            B212HEAD.BTYPECLASS = "0706";
            B212HEAD.ICCHK_CODE = "0000";
            B212HEAD.ICCHK_CODE_DESC = "成功";
            return B212HEAD;
        }






        public static string ComputeMac(string macdataString, string txnDatetime, string keyHex)
        {
            //string txnDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");

            // 步驟一：macdataString 已經由外部提供（= DOCDATA 原始字串）

            // 步驟二：SHA256 摘要值
            byte[] shaBytes;
            using (SHA256 sha = SHA256.Create())
            {
                shaBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(macdataString));
            }

            // 步驟三：ICV = TXN_DATETIME 補 16 位
            string icvString = txnDatetime.PadRight(16, '0');

            // 3DES CBC IV = 8 Bytes → 取前 8 字元
            string iv8 = icvString.Substring(0, 8);
            byte[] ivBytes = Encoding.ASCII.GetBytes(iv8);

            // 步驟四：3DES CBC 加密
            byte[] keyBytes = HexToBytes(keyHex);
            byte[] macCode = TripleDESCBCEncrypt(shaBytes, keyBytes, ivBytes);


            // 步驟五：取最後一個 Block（8 Bytes）的前 4 Bytes
            byte[] lastBlock = new byte[8];
            Array.Copy(macCode, macCode.Length - 8, lastBlock, 0, 8);

            byte[] mac4 = new byte[4];
            Array.Copy(lastBlock, 0, mac4, 0, 4);

            // 步驟六：轉 HEX 大寫
            return BytesToHex(mac4);
        }

        private static byte[] TripleDESCBCEncrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
            {
                tdes.Key = key;
                tdes.IV = iv;
                tdes.Mode = CipherMode.CBC;
                tdes.Padding = PaddingMode.None; // eFCS 規範：不填充

                // 資料長度必須是 8 的倍數 → 若不是，你要自行補 0
                int mod = data.Length % 8;
                if (mod != 0)
                {
                    int pad = 8 - mod;
                    Array.Resize(ref data, data.Length + pad);
                }

                using (ICryptoTransform encryptor = tdes.CreateEncryptor())
                {
                    return encryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }

        private static byte[] HexToBytes(string hex)
        {
            int len = hex.Length;
            byte[] result = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
                result[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return result;
        }

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
            string rcyYmd = billdata.Substring(7, 7);  // RCV_YMD(7)
            string receiptNo = billdata.Substring(14); // 剩下全部（RECEIPT_NO 5–6）

            return new BillDecodeResult
            {
                CUST_NO = custNo,
                RCV_YMD = rcyYmd,
                RECEPT_NO = receiptNo
            };
        }
        public static string GetTaiwanDate()
        {
            TaiwanCalendar tc = new TaiwanCalendar();
            DateTime now = DateTime.Now;
            return $"{tc.GetYear(now)}{now:MMdd}";
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
              + (item.ADJ_CHARGE ?? 0)
              + (item.ADJ_TAX ?? 0)
              + (item.PEN_AMT ?? 0)
              + (item.PEN_TAX ?? 0)
              + (item.SAFE_AMT ?? 0)
              + (item.SERVICE_AMT ?? 0)
              + (item.SERVICE_TAX ?? 0);
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


     }
}
