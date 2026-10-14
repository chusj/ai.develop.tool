using MultiAgentDevelop.Model.Models.RootTkey;
using SqlSugar;

namespace MultiAgentDevelop.Model.Models
{
    /// <summary>
    /// 短信账号表
    /// </summary>
    [SugarTable("sms_account", "短信账号表")]
    public class SmsAccount : BaseEntity
    {
        /// <summary>
        /// 账号名称，全局唯一
        /// </summary>
        [SugarColumn(ColumnName = "account_name", Length = 64, IsNullable = false)]
        public string AccountName { get; set; }

        /// <summary>
        /// 绑定的供应商 ID（sms_provider.id）
        /// </summary>
        [SugarColumn(ColumnName = "provider_id", IsNullable = false)]
        public long ProviderId { get; set; }

        /// <summary>
        /// AccessKey / 密钥 ID
        /// </summary>
        [SugarColumn(ColumnName = "access_key", Length = 128, IsNullable = true)]
        public string AccessKey { get; set; }

        /// <summary>
        /// AccessSecret / 密钥，AES 密文存储
        /// </summary>
        [SugarColumn(ColumnName = "access_secret", Length = 500, IsNullable = true)]
        public string AccessSecret { get; set; }

        /// <summary>
        /// 默认签名，为空时取供应商签名
        /// </summary>
        [SugarColumn(ColumnName = "sign_name", Length = 64, IsNullable = true)]
        public string SignName { get; set; }

        /// <summary>
        /// 剩余额度（条）
        /// </summary>
        [SugarColumn(ColumnName = "balance", Length = 12, DecimalDigits = 2, IsNullable = false, DefaultValue = "0")]
        public decimal Balance { get; set; }

        /// <summary>
        /// 余额预警阈值，小于等于该值时提示；0 表示不预警
        /// </summary>
        [SugarColumn(ColumnName = "warn_threshold", Length = 12, DecimalDigits = 2, IsNullable = false, DefaultValue = "0")]
        public decimal WarnThreshold { get; set; }

        /// <summary>
        /// 状态：true 启用 / false 禁用
        /// </summary>
        [SugarColumn(ColumnName = "status", IsNullable = false, DefaultValue = "1")]
        public bool Status { get; set; } = true;

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(ColumnName = "remark", Length = 255, IsNullable = true)]
        public string Remark { get; set; }
    }
}
