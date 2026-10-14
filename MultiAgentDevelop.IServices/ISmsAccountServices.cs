using MultiAgentDevelop.IServices.BASE;
using MultiAgentDevelop.Model;
using MultiAgentDevelop.Model.Models;
using MultiAgentDevelop.Model.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiAgentDevelop.IServices
{
    /// <summary>
    /// 短信账号管理服务
    /// </summary>
    public interface ISmsAccountServices : IBaseServices<SmsAccount>
    {
        /// <summary>
        /// 分页查询短信账号（含供应商名称、脱敏密钥、余额预警标记）
        /// </summary>
        Task<PageModel<SmsAccountDto>> QueryAccountPage(SmsAccountQueryDto query);

        /// <summary>
        /// 查询账号详情，不存在或已删除返回 null
        /// </summary>
        Task<SmsAccountDto> GetAccountDetail(long id);

        /// <summary>
        /// 查询账号实体，不存在或已删除返回 null
        /// </summary>
        Task<SmsAccount> GetAccountEntity(long id);

        /// <summary>
        /// 供应商下拉数据（全部未删除的供应商，含禁用标记）
        /// </summary>
        Task<List<SmsProviderOptionDto>> GetProviderOptions();

        /// <summary>
        /// 账号名称是否已存在（不含已逻辑删除数据）
        /// </summary>
        /// <param name="accountName">账号名称</param>
        /// <param name="excludeId">排除的账号 Id（编辑时传自身 Id）</param>
        Task<bool> ExistsAccountName(string accountName, long excludeId = 0);

        /// <summary>
        /// 是否存在业务引用（充值流水 / 发送记录），删除保护用
        /// </summary>
        Task<bool> HasBusinessReference(long accountId);

        /// <summary>
        /// 新增账号
        /// </summary>
        Task<SmsActionResult> CreateAccount(SmsAccountSaveDto dto, long operatorId, string operatorName);

        /// <summary>
        /// 修改账号
        /// </summary>
        Task<SmsActionResult> UpdateAccount(SmsAccountSaveDto dto, long operatorId, string operatorName);

        /// <summary>
        /// 启用 / 禁用账号
        /// </summary>
        Task<SmsActionResult> ChangeStatus(long id, bool status, long operatorId, string operatorName);

        /// <summary>
        /// 逻辑删除账号（受业务引用保护）
        /// </summary>
        Task<SmsActionResult> RemoveAccount(long id, long operatorId, string operatorName);

        /// <summary>
        /// 连通性测试，不改变任何数据
        /// </summary>
        Task<SmsAccountTestResultDto> TestConnection(long id);
    }
}
