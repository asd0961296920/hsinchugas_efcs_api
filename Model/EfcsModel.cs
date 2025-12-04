using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;


namespace hsinchugas_efcs_api.Model
{
    //ALL
    public class ALL<TBody>
    {
        [JsonPropertyOrder(1)]
        public FUN FUN { get; set; }

        [JsonPropertyOrder(2)]
        public DOCDATA<TBody> DOCDATA { get; set; }

        [JsonPropertyOrder(3)]
        public SEC SEC { get; set; }
    }


    //FUN
    public class FUN
    {
        [JsonPropertyOrder(1)]
        public string FUNNAME { get; set; }
        [JsonPropertyOrder(2)]
        public string VER { get; set; }

    }

    //DOCDATA
    public class DOCDATA<TBody>
    {
        [JsonPropertyOrder(1)]
        public HEAD HEAD { get; set; }

        [JsonPropertyOrder(2)]
        public TBody BODY { get; set; }
    }

    //HEAD
    public class HEAD
    {
        [JsonPropertyOrder(1)]
        public string SRC_ID { get; set; }

        [JsonPropertyOrder(2)]
        public string DEST_ID { get; set; }
        [JsonPropertyOrder(3)]
        public string TXN_DATETIME { get; set; }

        [JsonPropertyOrder(4)]
        public string TXN_NO { get; set; }

        [JsonPropertyOrder(5)]
        public string PRS_CODE { get; set; }

        [JsonPropertyOrder(6)]
        public string BILLER_ID { get; set; }

        [JsonPropertyOrder(7)]
        public string BTYPECLASS { get; set; }

        [JsonPropertyOrder(8)]
        public string ICCHK_CODE { get; set; }

        [JsonPropertyOrder(9)]
        public string? ICCHK_CODE_DESC { get; set; }
    }

    //SEC
    public class SEC
    {
        [JsonPropertyOrder(1)]
        public string DIG { get; set; }

        [JsonPropertyOrder(2)]
        public string MAC { get; set; }

    }


    public class BillDecodeResult
    {
        public string CUST_NO { get; set; }
        public string RCV_YMD { get; set; }
        public string RECEPT_NO { get; set; }
    }









}
