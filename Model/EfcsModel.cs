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











    //B207Q
    public class BillerDataQueryRq
    {
        public string QUERY_TYPE { get; set; }
        public string QUERY_DATA1 { get; set; }
        public string? QUERY_DATA2 { get; set; }
        public string? QUERY_DATA3 { get; set; }
        public string? QUERY_DATA4 { get; set; }
        public string? QUERY_DATA5 { get; set; }

    }

    //B207S
    public class BillerDataQueryRs
    {
        [JsonPropertyOrder(1)]
        public QUERYHEAD QUERYHEAD { get; set; }
        [JsonPropertyOrder(2)]
        public List<QUERYDETAIL> QUERYDETAIL { get; set; } = new List<QUERYDETAIL>();

    }

    public class QUERYHEAD
    {
        [JsonPropertyOrder(1)]
        // 1. 總筆數 數字(2)
        public int TOTAL_COUNT { get; set; }

        [JsonPropertyOrder(2)]
        // 2. 總金額 數字(10) 單位：元
        public decimal TOTAL_AMOUNT { get; set; }
    }

    public class QUERYDETAIL
    {
        [JsonPropertyOrder(1)]
        // 1. 流水號 特定字串(2)
        public string QUERY_DETAILNO { get; set; }

        [JsonPropertyOrder(2)]
        // 2. 是否可繳費 字串(1)  Y/N
        public string QUERY_ISPAY { get; set; }

        [JsonPropertyOrder(3)]
        // 3. 銷帳方式 字串(1) B / C
        public string QUERY_BILLTYPE { get; set; }

        [JsonPropertyOrder(4)]
        // 4. 銷帳資料 字串(50)
        public string QUERY_BILLDATA { get; set; }

        [JsonPropertyOrder(5)]
        // 5. 銷帳資料所屬期別 字串(10)
        public string QUERY_BILLBATCH { get; set; }


        [JsonPropertyOrder(6)]
        // 6. 應繳金額 數字(10)
        public decimal QUERY_AMOUNT { get; set; }

        [JsonPropertyOrder(7)]
        // 7. 限繳日期 字串(8)
        public string? QUERY_DATE { get; set; }

        [JsonPropertyOrder(8)]
        // 8. 查詢結果參數數目 數字(2)
        public int QUERY_DATA_NO { get; set; }

        [JsonPropertyOrder(9)]
        // 9. 資料名稱1 字串(20)
        public string? QUERY_DISPNAME1 { get; set; }

        [JsonPropertyOrder(10)]
        // 10. 資料值1 字串(50)
        public string? QUERY_DISPDATA1 { get; set; }

        [JsonPropertyOrder(11)]
        // 11. 資料名稱2 字串(20)
        public string? QUERY_DISPNAME2 { get; set; }

        [JsonPropertyOrder(12)]
        // 12. 資料值2 字串(50)
        public string? QUERY_DISPDATA2 { get; set; }

        [JsonPropertyOrder(13)]
        // 13. 資料名稱3 字串(20)
        public string? QUERY_DISPNAME3 { get; set; }

        [JsonPropertyOrder(14)]
        // 14. 資料值3 字串(50)
        public string? QUERY_DISPDATA3 { get; set; }

        [JsonPropertyOrder(15)]
        // 15. 資料名稱4 字串(20)
        public string? QUERY_DISPNAME4 { get; set; }

        [JsonPropertyOrder(16)]
        // 16. 資料值4 字串(50)
        public string? QUERY_DISPDATA4 { get; set; }

        [JsonPropertyOrder(17)]
        // 17. 資料名稱5 字串(20)
        public string? QUERY_DISPNAME5 { get; set; }

        [JsonPropertyOrder(18)]
        // 18. 資料值5 字串(50)
        public string? QUERY_DISPDATA5 { get; set; }

        [JsonPropertyOrder(19)]
        // 19. 資料名稱6 字串(20)
        public string? QUERY_DISPNAME6 { get; set; }

        [JsonPropertyOrder(20)]
        // 20. 資料值6 字串(50)
        public string? QUERY_DISPDATA6 { get; set; }

        [JsonPropertyOrder(21)]
        // 21. 資料名稱7 字串(20)
        public string? QUERY_DISPNAME7 { get; set; }

        [JsonPropertyOrder(22)]
        // 22. 資料值7 字串(50)
        public string? QUERY_DISPDATA7 { get; set; }

        [JsonPropertyOrder(23)]
        // 23. 資料名稱8 字串(20)
        public string? QUERY_DISPNAME8 { get; set; }

        [JsonPropertyOrder(24)]
        // 24. 資料值8 字串(50)
        public string? QUERY_DISPDATA8 { get; set; }

        [JsonPropertyOrder(25)]
        // 25. 資料名稱9 字串(20)
        public string? QUERY_DISPNAME9 { get; set; }

        [JsonPropertyOrder(26)]
        // 26. 資料值9 字串(50)
        public string? QUERY_DISPDATA9 { get; set; }

        [JsonPropertyOrder(27)]
        // 27. 資料名稱10 字串(20)
        public string? QUERY_DISPNAME10 { get; set; }

        [JsonPropertyOrder(28)]
        // 28. 資料值10 字串(50)
        public string? QUERY_DISPDATA10 { get; set; }
    }



}
