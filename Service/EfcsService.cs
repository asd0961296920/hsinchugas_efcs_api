using hsinchugas_efcs_api.Model;
using System;
using System.Security.Cryptography;
using System.Text;


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





















        }
}
