using System.Text.Json.Serialization;

namespace hsinchugas_efcs_api.Model
{
    public class BillerQueryNotifyMsgRq
    {
        [JsonPropertyOrder(1)]
        public QUERYHEAD_B219 QUERYHEAD { get; set; }

        [JsonPropertyOrder(2)]
        public QUERYDETAIL_B219 QUERYDETAIL { get; set; }
    }

    public class QUERYHEAD_B219
    {
        /// <summary>查詢筆數 數字(2)</summary>
        [JsonPropertyOrder(1)]
        public int TOTAL_COUNT { get; set; }
    }

    public class QUERYDETAIL_B219
    {
        /// <summary>流水號 01–99</summary>
        [JsonPropertyOrder(1)]
        public string DETAILNO { get; set; }

        /// <summary>查詢條件型態 字串(1) ※由業者申請定義</summary>
        [JsonPropertyOrder(2)]
        public string QUERY_TYPE { get; set; }

        /// <summary>查詢條件參數 1 字串(20)</summary>
        [JsonPropertyOrder(3)]
        public string QUERY_DATA1 { get; set; }

        /// <summary>查詢條件參數 2 字串(20)</summary>
        [JsonPropertyOrder(4)]
        public string QUERY_DATA2 { get; set; }

        /// <summary>查詢條件參數 3 字串(20)</summary>
        [JsonPropertyOrder(5)]
        public string QUERY_DATA3 { get; set; }

        /// <summary>查詢條件參數 4 字串(20)</summary>
        [JsonPropertyOrder(6)]
        public string QUERY_DATA4 { get; set; }

        /// <summary>查詢條件參數 5 字串(20)</summary>
        [JsonPropertyOrder(7)]
        public string QUERY_DATA5 { get; set; }
    }


    //B219輸出
    public class BillerQueryNotifyMsgRs
    {
        [JsonPropertyOrder(1)]
        public QUERYHEAD_B219_RS QUERYHEAD { get; set; }

        [JsonPropertyOrder(2)]
        public QUERYDETAIL_B219_RS QUERYDETAIL { get; set; }
    }

    public class QUERYHEAD_B219_RS
    {
        /// <summary>總筆數 數字(2)</summary>
        [JsonPropertyOrder(1)]
        public int TOTAL_COUNT { get; set; }
    }

    public class QUERYDETAIL_B219_RS
    {
        /// <summary>流水號 01–99</summary>
        [JsonPropertyOrder(1)]
        public string DETAILNO { get; set; }

        /// <summary>作業結果 字串(4)（0000成功 / 6002查無資料 等）</summary>
        [JsonPropertyOrder(2)]
        public string RTN_CODE { get; set; }

        /// <summary>下次詢問日期 YYYYMMDD</summary>
        [JsonPropertyOrder(3)]
        public string NEXT_FIRE_DATE { get; set; }

        /// <summary>通知訊息（提供給支付端推播）</summary>
        [JsonPropertyOrder(4)]
        public string NOTIFY_MSG { get; set; }

        /// <summary>應繳總金額 數字(10)</summary>
        [JsonPropertyOrder(5)]
        public decimal TOTAL_AMOUNT { get; set; }

        /// <summary>查詢條件型態 字串(1)</summary>
        [JsonPropertyOrder(6)]
        public string QUERY_TYPE { get; set; }

        /// <summary>查詢條件參數 1 字串(20)</summary>
        [JsonPropertyOrder(7)]
        public string QUERY_DATA1 { get; set; }

        /// <summary>查詢條件參數 2 字串(20)</summary>
        [JsonPropertyOrder(8)]
        public string QUERY_DATA2 { get; set; }

        /// <summary>查詢條件參數 3 字串(20)</summary>
        [JsonPropertyOrder(9)]
        public string QUERY_DATA3 { get; set; }

        /// <summary>查詢條件參數 4 字串(20)</summary>
        [JsonPropertyOrder(10)]
        public string QUERY_DATA4 { get; set; }

        /// <summary>查詢條件參數 5 字串(20)</summary>
        [JsonPropertyOrder(11)]
        public string QUERY_DATA5 { get; set; }
    }


}
