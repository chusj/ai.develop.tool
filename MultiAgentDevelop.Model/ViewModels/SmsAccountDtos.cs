using System;

namespace MultiAgentDevelop.Model.ViewModels
{
    /// <summary>
    /// 短信账号 - 列表 / 详情 DTO
    /// </summary>
    public class SmsAccountDto
    {
        /// <summary>
        /// 账号 ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 账号名称
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 绑定的供应商 ID
        /// </summary>
        public long ProviderId { get; set; }

        /// <summary>
        /// 绑定供应商名称
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// 绑定供应商类型
        /// </summary>
        public string ProviderType { get; set; }

        /// <summary>
        /// 绑定供应商是否启用
        /// </summary>
        public bool ProviderStatus { get; set; }

        /// <summary>
        /// AccessKey，脱敏展示
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// AccessSecret，一律返回掩码
        /// </summary>
        public string AccessSecret { get; set; }

        /// <summary>
        /// 默认签名
        /// </summary>
        public string SignName { get; set; }

        /// <summary>
        /// 剩余额度
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// 余额预警阈值
        /// </summary>
        public decimal WarnThreshold { get; set; }

        /// <summary>
        /// 是否触发余额预警（阈值 &gt; 0 且 余额 &lt;= 阈值）
        /// </summary>
        public bool Warned { get; set; }

        /// <summary>
        /// 状态：true 启用 / false 禁用
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 短信账号 - 新增 / 修改入参
    /// </summary>
    public class SmsAccountSaveDto
    {
        /// <summary>
        /// 账号 ID，新增时为 0
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 账号名称
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 绑定的供应商 ID
        /// </summary>
        public long ProviderId { get; set; }

        /// <summary>
        /// AccessKey
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// AccessSecret，编辑时留空表示不修改
        /// </summary>
        public string AccessSecret { get; set; }

        /// <summary>
        /// 默认签名
        /// </summary>
        public string SignName { get; set; }

        /// <summary>
        /// 初始额度，仅新增时生效
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// 余额预警阈值
        /// </summary>
        public decimal WarnThreshold { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public bool Status { get; set; } = true;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }

    /// <summary>
    /// 短信账号 - 分页查询入参
    /// </summary>
    public class SmsAccountQueryDto
    {
        /// <summary>
        /// 当前页，从 1 开始
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每页条数
        /// </summary>
        public int IntPageSize { get; set; } = 20;

        /// <summary>
        /// 账号名称，模糊匹配
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 供应商 ID，null 表示全部
        /// </summary>
        public long? ProviderId { get; set; }

        /// <summary>
        /// 状态，null 表示全部
        /// </summary>
        public bool? Status { get; set; }
    }

    /// <summary>
    /// 供应商下拉项
    /// </summary>
    public class SmsProviderOptionDto
    {
        /// <summary>
        /// 供应商 ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// 供应商类型
        /// </summary>
        public string ProviderType { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Status { get; set; }
    }

    /// <summary>
    /// 短信账号 - 连通性测试结果
    /// </summary>
    public class SmsAccountTestResultDto
    {
        /// <summary>
        /// 是否连通
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 通道返回的余额，未知时为 null
        /// </summary>
        public decimal? Balance { get; set; }

        /// <summary>
        /// 成功说明或失败原因
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// 短信模块通用操作结果（服务层返回，由控制器转成统一返回结构）
    /// </summary>
    public class SmsActionResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 新增成功时返回的新 Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 构造成功结果
        /// </summary>
        public static SmsActionResult Ok(string message, long id = 0)
        {
            return new SmsActionResult { Success = true, Message = message, Id = id };
        }

        /// <summary>
        /// 构造失败结果
        /// </summary>
        public static SmsActionResult Fail(string message)
        {
            return new SmsActionResult { Success = false, Message = message };
        }
    }
}
