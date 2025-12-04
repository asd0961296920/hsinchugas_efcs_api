using System.Text.Json.Serialization;

namespace hsinchugas_efcs_api.Model
{
    public class BillerSystemNoticeRq
    {
        public NOTICEHEAD NOTICEHEAD { get; set; }
    }
    public class NOTICEHEAD
    {
        /// <summary>
        /// 公告通知類型
        /// A：業者系統啟動服務
        /// B：業者系統暫停服務
        /// 字串(1)
        /// </summary>
        [JsonPropertyOrder(1)]
        public string NOTICE_TYPE { get; set; }

        /// <summary>
        /// 通知說明
        /// 字串(256)
        /// </summary>
        [JsonPropertyOrder(2)]
        public string? MEMO { get; set; }

        /// <summary>
        /// 開始時間
        /// 格式：YYYYMMDDHHMMSS
        /// 字串(14)
        /// </summary>
        [JsonPropertyOrder(3)]
        public string BEGIN_TIME { get; set; }

        /// <summary>
        /// 結束時間
        /// 格式：YYYYMMDDHHMMSS
        /// 字串(14)
        /// 通知類型為 A 時可為空
        /// </summary>
        [JsonPropertyOrder(4)]
        public string? END_TIME { get; set; }
    }

}
