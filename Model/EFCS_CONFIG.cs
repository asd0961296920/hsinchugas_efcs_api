namespace hsinchugas_efcs_api.Model
{
    public class EfcsConfig
    {
        /// <summary>
        /// 是 → 下次查詢時間（分鐘/小時等）
        /// Oracle: NUMBER(2)
        /// </summary>
        public int? B219_Y_NEXT_TIME { get; set; }

        /// <summary>
        /// 否 → 下次查詢時間
        /// Oracle: NUMBER(7)
        /// </summary>
        public int? B219_N_NEXT_TIME { get; set; }

        /// <summary>
        /// 文字
        /// Oracle: VARCHAR2(50)
        /// </summary>
        public string? B219_TEXT { get; set; }

        /// <summary>
        /// 通知文字
        /// Oracle: VARCHAR2(50)
        /// </summary>
        public string? B212_NOTIFY { get; set; }

        /// <summary>
        /// 文字
        /// Oracle: VARCHAR2(50)
        /// </summary>
        public string? B212_TEXT { get; set; }

        /// <summary>
        /// 啟用開始時間
        /// Oracle: DATE
        /// </summary>
        public DateTime? B212_START { get; set; }

        /// <summary>
        /// 啟用結束時間
        /// Oracle: DATE
        /// </summary>
        public DateTime? B212_END { get; set; }

        /// <summary>
        /// API 或通知用 URL
        /// Oracle: VARCHAR2(200)
        /// </summary>
        public string? B212_URL { get; set; }
    }

}
