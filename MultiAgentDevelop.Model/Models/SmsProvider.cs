using MultiAgentDevelop.Model.Models.RootTkey;
using SqlSugar;

namespace MultiAgentDevelop.Model.Models
{
    /// <summary>
    /// 短信供应商表
    /// </summary>
    [SugarTable("sms_provider", "短信供应商表")]
    public class SmsProvider : BaseEntity
    {
        /// <summary>
        /// 供应商编码，全局唯一，大写字母 + 数字 + 下划线
        /// </summary>
        [SugarColumn(ColumnName = "provider_code", Length = 50, IsNullable = false)]
        public string ProviderCode { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        [SugarColumn(ColumnName = "provider_name", Length = 100, IsNullable = false)]
        public string ProviderName { get; set; }

        /// <summary>
        /// 供应商类型：ALIYUN / TENCENT / MENGWANG / HTTP_CUSTOM
        /// </summary>
        [SugarColumn(ColumnName = "provider_type", Length = 20, IsNullable = false)]
        public string ProviderType { get; set; }

        /// <summary>
        /// 接口地址，HTTP_CUSTOM 类型必填
        /// </summary>
        [SugarColumn(ColumnName = "base_url", Length = 500, IsNullable = true)]
        public string BaseUrl { get; set; }

        /// <summary>
        /// AppId / 用户名
        /// </summary>
        [SugarColumn(ColumnName = "app_id", Length = 100, IsNullable = true)]
        public string AppId { get; set; }

        /// <summary>
        /// AppSecret / 密码，AES 密文
        /// </summary>
        [SugarColumn(ColumnName = "app_secret", Length = 500, IsNullable = true)]
        public string AppSecret { get; set; }

        /// <summary>
        /// 默认短信签名
        /// </summary>
        [SugarColumn(ColumnName = "sign_name", Length = 100, IsNullable = true)]
        public string SignName { get; set; }

        /// <summary>
        /// 状态回调地址
        /// </summary>
        [SugarColumn(ColumnName = "callback_url", Length = 500, IsNullable = true)]
        public string CallbackUrl { get; set; }

        /// <summary>
        /// 日发送上限，0 表示不限制
        /// </summary>
        [SugarColumn(ColumnName = "daily_limit", IsNullable = false, DefaultValue = "0")]
        public long DailyLimit { get; set; }

        /// <summary>
        /// 状态：true 启用 / false 禁用
        /// </summary>
        [SugarColumn(ColumnName = "status", IsNullable = false, DefaultValue = "1")]
        public bool Status { get; set; } = true;

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(ColumnName = "remark", Length = 500, IsNullable = true)]
        public string Remark { get; set; }
    }
}
