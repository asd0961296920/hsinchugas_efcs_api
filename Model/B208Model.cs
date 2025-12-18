using System.Text.Json.Serialization;

namespace hsinchugas_efcs_api.Model
{
    public class BillerDataPayRq
    {
        [JsonPropertyOrder(1)]
        public PAYHEAD PAYHEAD { get; set; } = new PAYHEAD();
        [JsonPropertyOrder(2)]
        public List<PAYDETAIL> PAYDETAIL { get; set; } = new List<PAYDETAIL>();

    }

    // PAYHEAD 物件
    public class PAYHEAD
    {

        /// <summary>
        /// 跨系統介接戳記 (字串36) - 可為 null
        /// </summary>
        [JsonPropertyOrder(1)]
        public string? REGISTERTOKEN { get; set; }

        /// <summary>
        /// 總筆數 (數字2) *
        /// </summary>
        [JsonPropertyOrder(2)]
        public int TOTAL_COUNT { get; set; }

        /// <summary>
        /// 總金額 (數字10) * 單位：元
        /// </summary>
        [JsonPropertyOrder(3)]
        public decimal TOTAL_AMOUNT { get; set; }
    }

    // PAYDETAIL 陣列物件
    public class PAYDETAIL
    {
        /// <summary>
        /// 明細項流水號 (字串2, 01-99) *
        /// </summary>
        [JsonPropertyOrder(2)]
        public string DETAILNO { get; set; }

        /// <summary>
        /// 扣款單位代號 (字串7) *
        /// </summary>
        [JsonPropertyOrder(3)]
        public string OUTBANKID { get; set; }

        /// <summary>
        /// 扣款者身分證字號 (字串10) - 繳費類別為人壽類時才會有值
        /// </summary>
        [JsonPropertyOrder(4)]
        public string? MEMBERID { get; set; }

        /// <summary>
        /// 扣款帳號 (特定字串16，右靠左補0) *
        /// </summary>
        [JsonPropertyOrder(5)]
        public string OUTACCOUNTNUM { get; set; }

        /// <summary>
        /// 繳費金額 (數字10) *
        /// </summary>
        [JsonPropertyOrder(6)]
        public decimal TXNAMOUNT { get; set; }

        /// <summary>
        /// 繳費日期 (字串8, YYYYMMDD) *
        /// </summary>
        [JsonPropertyOrder(7)]
        public string PAY_DATE { get; set; }

        /// <summary>
        /// 繳費時間 (字串6, HHMMSS) *
        /// </summary>
        [JsonPropertyOrder(8)]
        public string PAY_TIME { get; set; }

        /// <summary>
        /// 清算日期 (字串8, YYYYMMDD) *
        /// </summary>
        [JsonPropertyOrder(9)]
        public string PAY_CLRDATE { get; set; }

        /// <summary>
        /// 繳費方式 (字串1) *
        /// 1:櫃檯 2:網路 3:ATM 4:語音 6:信用卡 8:行動 T:智慧繳費 Z:其他
        /// </summary>
        [JsonPropertyOrder(10)]
        public string CHECKTYPE { get; set; }

        /// <summary>
        /// 銷帳方式 (字串1) * B:銷帳編號 C:三段條碼
        /// </summary>
        [JsonPropertyOrder(11)]
        public string BILLTYPE { get; set; }

        /// <summary>
        /// 銷帳資料 (字串50) *
        /// </summary>
        [JsonPropertyOrder(12)]
        public string BILLDATA { get; set; }

        /// <summary>
        /// 銷帳資料所屬期別 (字串10)
        /// </summary>
        [JsonPropertyOrder(13)]
        public string? BILLBATCH { get; set; }

        /// <summary>
        /// 平台交易序號 (字串7) *
        /// </summary>
        [JsonPropertyOrder(14)]
        public string EFCSSEQNO { get; set; }

        /// <summary>
        /// 電子發票手機條碼 (字串30)
        /// </summary>
        [JsonPropertyOrder(15)]
        public string EINV_CARDNO { get; set; }
    }




    //B208輸出
    public class BillerDataPayRs
    {
        [JsonPropertyOrder(1)]
        public PAYHEAD_RS PAYHEAD { get; set; }

        [JsonPropertyOrder(2)]
        public List<PAYDETAIL_RS> PAYDETAIL { get; set; } = new List<PAYDETAIL_RS>();
    }

    public class PAYHEAD_RS
    {
        /// <summary>跨系統介接戳記 (字串36)</summary>
        [JsonPropertyOrder(1)]
        public string? REGISTERTOKEN { get; set; }

        /// <summary>總筆數 (數字2)</summary>
        [JsonPropertyOrder(2)]
        public int TOTAL_COUNT { get; set; }

        /// <summary>總金額 (數字10)</summary>
        [JsonPropertyOrder(3)]
        public decimal TOTAL_AMOUNT { get; set; }
    }

    public class PAYDETAIL_RS
    {
        /// <summary>明細項流水號 01-99</summary>
        [JsonPropertyOrder(1)]
        public string DETAILNO { get; set; }

        /// <summary>繳費金額 數字10</summary>
        [JsonPropertyOrder(2)]
        public decimal TXNAMOUNT { get; set; }

        /// <summary>銷帳資料 字串50</summary>
        [JsonPropertyOrder(3)]
        public string BILLDATA { get; set; }

        /// <summary>銷帳資料所屬期別 字串10</summary>
        [JsonPropertyOrder(4)]
        public string? BILLBATCH { get; set; }

        /// <summary>處理結果代碼 (原值帶回)</summary>
        [JsonPropertyOrder(5)]
        public string APRTN_CODE { get; set; }

        /// <summary>處理描述（錯誤訊息或業者描述）</summary>
        [JsonPropertyOrder(6)]
        public string? ERRORDESC { get; set; }

        /// <summary>結果參數數目 數字1</summary>
        [JsonPropertyOrder(7)]
        public int PAY_DATA_NO { get; set; }

        /// <summary>繳費結果名稱1 字串20</summary>
        [JsonPropertyOrder(8)]
        public string? PAY_DISPNAME1 { get; set; }

        /// <summary>繳費結果值1 字串120</summary>
        [JsonPropertyOrder(9)]
        public string? PAY_DISPDATA1 { get; set; }

        /// <summary>繳費結果名稱2 字串20</summary>
        [JsonPropertyOrder(10)]
        public string? PAY_DISPNAME2 { get; set; }

        /// <summary>繳費結果值2 字串120</summary>
        [JsonPropertyOrder(11)]
        public string? PAY_DISPDATA2 { get; set; }

        /// <summary>繳費結果名稱3 字串20</summary>
        [JsonPropertyOrder(12)]
        public string? PAY_DISPNAME3 { get; set; }

        /// <summary>繳費結果值3 字串120</summary>
        [JsonPropertyOrder(13)]
        public string? PAY_DISPDATA3 { get; set; }

        /// <summary>繳費結果名稱4 字串20</summary>
        [JsonPropertyOrder(14)]
        public string? PAY_DISPNAME4 { get; set; }

        /// <summary>繳費結果值4 字串120</summary>
        [JsonPropertyOrder(15)]
        public string? PAY_DISPDATA4 { get; set; }

        /// <summary>繳費結果名稱5 字串20</summary>
        [JsonPropertyOrder(16)]
        public string? PAY_DISPNAME5 { get; set; }

        /// <summary>繳費結果值5 字串120</summary>
        [JsonPropertyOrder(17)]
        public string? PAY_DISPDATA5 { get; set; }
    }




}
