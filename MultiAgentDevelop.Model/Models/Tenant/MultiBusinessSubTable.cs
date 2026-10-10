using MultiAgentDevelop.Model.Models.RootTkey;
using MultiAgentDevelop.Model.Tenants;

namespace MultiAgentDevelop.Model.Models;

/// <summary>
/// 多租户-多表方案 业务表 子表 <br/>
/// </summary>
[MultiTenant(TenantTypeEnum.Tables)]
public class MultiBusinessSubTable : BaseEntity
{
    public long MainId { get; set; }
    public string Memo { get; set; }
}