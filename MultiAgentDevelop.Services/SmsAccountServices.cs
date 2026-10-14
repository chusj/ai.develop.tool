using MultiAgentDevelop.Common.Helper;
using MultiAgentDevelop.IRepository.Base;
using MultiAgentDevelop.IServices;
using MultiAgentDevelop.Model;
using MultiAgentDevelop.Model.Models;
using MultiAgentDevelop.Model.ViewModels;
using MultiAgentDevelop.Services.BASE;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiAgentDevelop.Services
{
    /// <summary>
    /// 短信账号管理服务
    /// </summary>
    public class SmsAccountServices : BaseServices<SmsAccount>, ISmsAccountServices
    {
        /// <summary>
        /// 连通性测试用的 HTTP 客户端工厂（仅 HTTP_CUSTOM 供应商需要）
        /// </summary>
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="baseDal">账号仓储</param>
        /// <param name="httpClientFactory">HTTP 客户端工厂</param>
        public SmsAccountServices(IBaseRepository<SmsAccount> baseDal, IHttpClientFactory httpClientFactory)
            : base(baseDal)
        {
            _httpClientFactory = httpClientFactory;
        }

        #region 查询

        /// <summary>
        /// 分页查询短信账号
        /// </summary>
        public async Task<PageModel<SmsAccountDto>> QueryAccountPage(SmsAccountQueryDto query)
        {
            query ??= new SmsAccountQueryDto();
            var page = query.Page <= 0 ? 1 : query.Page;
            var size = query.IntPageSize <= 0 ? 20 : query.IntPageSize;

            var sql = Db.Queryable<SmsAccount>().Where(a => a.IsDeleted == false);

            if (!string.IsNullOrWhiteSpace(query.AccountName))
            {
                var accountName = query.AccountName.Trim();
                sql = sql.Where(a => a.AccountName.Contains(accountName));
            }

            if (query.ProviderId.HasValue && query.ProviderId.Value > 0)
            {
                var providerId = query.ProviderId.Value;
                sql = sql.Where(a => a.ProviderId == providerId);
            }

            if (query.Status.HasValue)
            {
                var status = query.Status.Value;
                sql = sql.Where(a => a.Status == status);
            }

            var dataCount = await sql.CountAsync();
            var rows = await sql
                .OrderBy(a => a.CreateTime, OrderByType.Desc)
                .OrderBy(a => a.Id, OrderByType.Desc)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            var providers = await QueryProvidersByIds(rows.Select(a => a.ProviderId).Distinct().ToList());
            var data = rows
                .Select(a => ToDto(a, providers.FirstOrDefault(p => p.Id == a.ProviderId)))
                .ToList();

            return new PageModel<SmsAccountDto>(page, dataCount, size, data);
        }

        /// <summary>
        /// 查询账号详情
        /// </summary>
        public async Task<SmsAccountDto> GetAccountDetail(long id)
        {
            var entity = await GetAccountEntity(id);
            if (entity == null)
            {
                return null;
            }

            var providers = await QueryProvidersByIds(new List<long> { entity.ProviderId });
            return ToDto(entity, providers.FirstOrDefault());
        }

        /// <summary>
        /// 查询账号实体
        /// </summary>
        public async Task<SmsAccount> GetAccountEntity(long id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await Db.Queryable<SmsAccount>().Where(a => a.Id == id && a.IsDeleted == false).FirstAsync();
        }

        /// <summary>
        /// 供应商下拉数据
        /// </summary>
        public async Task<List<SmsProviderOptionDto>> GetProviderOptions()
        {
            var providers = await Db.Queryable<SmsProvider>()
                .Where(p => p.IsDeleted == false)
                .OrderBy(p => p.ProviderName)
                .ToListAsync();

            return providers.Select(p => new SmsProviderOptionDto
            {
                Id = p.Id,
                ProviderName = p.ProviderName,
                ProviderType = p.ProviderType,
                Status = p.Status
            }).ToList();
        }

        /// <summary>
        /// 账号名称是否已存在
        /// </summary>
        public async Task<bool> ExistsAccountName(string accountName, long excludeId = 0)
        {
            if (string.IsNullOrWhiteSpace(accountName))
            {
                return false;
            }

            var name = accountName.Trim();
            return await Db.Queryable<SmsAccount>()
                .AnyAsync(a => a.IsDeleted == false && a.AccountName == name && a.Id != excludeId);
        }

        /// <summary>
        /// 是否存在业务引用
        /// </summary>
        /// <remarks>
        /// sms_recharge / sms_send_record 属后续版本（F2 / F3），表不存在时直接跳过检查，
        /// 避免建表前调用报错。表建好后本方法自动生效。
        /// </remarks>
        public async Task<bool> HasBusinessReference(long accountId)
        {
            if (accountId <= 0)
            {
                return false;
            }

            return await HasReferenceInTable("sms_recharge", accountId)
                   || await HasReferenceInTable("sms_send_record", accountId);
        }

        #endregion

        #region 写操作

        /// <summary>
        /// 新增账号
        /// </summary>
        public async Task<SmsActionResult> CreateAccount(SmsAccountSaveDto dto, long operatorId, string operatorName)
        {
            var validate = await ValidateForm(dto, 0);
            if (!validate.Success)
            {
                return validate;
            }

            if (dto.Balance < 0)
            {
                return SmsActionResult.Fail("初始额度不能为负数");
            }

            var entity = new SmsAccount
            {
                AccountName = dto.AccountName.Trim(),
                ProviderId = dto.ProviderId,
                AccessKey = dto.AccessKey?.Trim(),
                AccessSecret = AesHelper.Encrypt(dto.AccessSecret?.Trim()),
                SignName = dto.SignName?.Trim(),
                Balance = dto.Balance,
                WarnThreshold = dto.WarnThreshold < 0 ? 0 : dto.WarnThreshold,
                Status = dto.Status,
                Remark = dto.Remark,
                Enabled = true,
                IsDeleted = false,
                CreateId = operatorId,
                CreateBy = operatorName,
                CreateTime = DateTime.Now
            };

            var id = await base.Add(entity);
            return id > 0
                ? SmsActionResult.Ok("添加成功", id)
                : SmsActionResult.Fail("添加失败");
        }

        /// <summary>
        /// 修改账号
        /// </summary>
        public async Task<SmsActionResult> UpdateAccount(SmsAccountSaveDto dto, long operatorId, string operatorName)
        {
            if (dto.Id <= 0)
            {
                return SmsActionResult.Fail("账号 ID 无效");
            }

            var entity = await GetAccountEntity(dto.Id);
            if (entity == null)
            {
                return SmsActionResult.Fail("账号不存在");
            }

            var validate = await ValidateForm(dto, dto.Id);
            if (!validate.Success)
            {
                return validate;
            }

            if (dto.WarnThreshold < 0)
            {
                return SmsActionResult.Fail("预警阈值不能为负数");
            }

            // 由禁用改为启用时，按 R6 做接入参数完整性校验
            if (!entity.Status && dto.Status)
            {
                var enableCheck = await ValidateEnable(entity.ProviderId, dto.AccessKey, entity.AccessSecret);
                if (!enableCheck.Success)
                {
                    return enableCheck;
                }
            }

            entity.AccountName = dto.AccountName.Trim();
            entity.ProviderId = dto.ProviderId;
            entity.AccessKey = dto.AccessKey?.Trim();
            // 密钥留空表示不修改
            if (!string.IsNullOrWhiteSpace(dto.AccessSecret))
            {
                entity.AccessSecret = AesHelper.Encrypt(dto.AccessSecret.Trim());
            }

            entity.SignName = dto.SignName?.Trim();
            entity.WarnThreshold = dto.WarnThreshold;
            entity.Status = dto.Status;
            entity.Remark = dto.Remark;
            // 余额由充值功能维护，此处不覆盖
            entity.ModifyId = operatorId;
            entity.ModifyBy = operatorName;
            entity.ModifyTime = DateTime.Now;

            var ok = await base.Update(entity);
            return ok ? SmsActionResult.Ok("更新成功") : SmsActionResult.Fail("更新失败");
        }

        /// <summary>
        /// 启用 / 禁用账号
        /// </summary>
        public async Task<SmsActionResult> ChangeStatus(long id, bool status, long operatorId, string operatorName)
        {
            var entity = await GetAccountEntity(id);
            if (entity == null)
            {
                return SmsActionResult.Fail("账号不存在");
            }

            if (status && !entity.Status)
            {
                var enableCheck = await ValidateEnable(entity.ProviderId, entity.AccessKey, entity.AccessSecret);
                if (!enableCheck.Success)
                {
                    return enableCheck;
                }
            }

            entity.Status = status;
            entity.ModifyId = operatorId;
            entity.ModifyBy = operatorName;
            entity.ModifyTime = DateTime.Now;

            var ok = await base.Update(entity);
            if (!ok)
            {
                return SmsActionResult.Fail(status ? "启用失败" : "禁用失败");
            }

            return SmsActionResult.Ok(status ? "已启用" : "已禁用");
        }

        /// <summary>
        /// 逻辑删除账号
        /// </summary>
        public async Task<SmsActionResult> RemoveAccount(long id, long operatorId, string operatorName)
        {
            var entity = await GetAccountEntity(id);
            if (entity == null)
            {
                return SmsActionResult.Fail("账号不存在");
            }

            if (await HasBusinessReference(id))
            {
                return SmsActionResult.Fail("该账号已有充值或发送记录，不可删除");
            }

            entity.IsDeleted = true;
            entity.ModifyId = operatorId;
            entity.ModifyBy = operatorName;
            entity.ModifyTime = DateTime.Now;

            var ok = await base.Update(entity);
            return ok ? SmsActionResult.Ok("删除成功") : SmsActionResult.Fail("删除失败");
        }

        /// <summary>
        /// 连通性测试
        /// </summary>
        public async Task<SmsAccountTestResultDto> TestConnection(long id)
        {
            var account = await GetAccountEntity(id);
            if (account == null)
            {
                return TestFail("账号不存在");
            }

            var providers = await QueryProvidersByIds(new List<long> { account.ProviderId });
            var provider = providers.FirstOrDefault();
            if (provider == null)
            {
                return TestFail("绑定的供应商不存在");
            }

            if (!provider.Status)
            {
                return TestFail($"供应商「{provider.ProviderName}」已禁用，无法测试连通性");
            }

            if (string.IsNullOrWhiteSpace(account.AccessKey) || string.IsNullOrWhiteSpace(account.AccessSecret))
            {
                return TestFail("账号接入参数不完整（AccessKey / AccessSecret），无法测试连通性");
            }

            if (AesHelper.Decrypt(account.AccessSecret) == null)
            {
                return TestFail("AccessSecret 解密失败，请重新保存密钥");
            }

            var providerType = (provider.ProviderType ?? string.Empty).Trim().ToUpperInvariant();
            switch (providerType)
            {
                case "HTTP_CUSTOM":
                    return await TestHttpCustom(provider);

                case "ALIYUN":
                case "TENCENT":
                case "MENGWANG":
                    return TestFail($"供应商类型 {providerType} 的真实通道 SDK 尚未接入（发送链路属后续版本），当前无法返回余额");

                default:
                    return TestFail($"未知的供应商类型：{provider.ProviderType}");
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 表单校验（新增与修改共用）
        /// </summary>
        private async Task<SmsActionResult> ValidateForm(SmsAccountSaveDto dto, long excludeId)
        {
            if (dto == null)
            {
                return SmsActionResult.Fail("请求参数不能为空");
            }

            var accountName = dto.AccountName?.Trim();
            if (string.IsNullOrWhiteSpace(accountName))
            {
                return SmsActionResult.Fail("账号名称不能为空");
            }

            if (accountName.Length > 64)
            {
                return SmsActionResult.Fail("账号名称长度不能超过 64 个字符");
            }

            if (dto.ProviderId <= 0)
            {
                return SmsActionResult.Fail("请选择绑定的供应商");
            }

            var providers = await QueryProvidersByIds(new List<long> { dto.ProviderId });
            if (providers.Count == 0)
            {
                return SmsActionResult.Fail("绑定的供应商不存在");
            }

            if (string.IsNullOrWhiteSpace(dto.AccessKey))
            {
                return SmsActionResult.Fail("AccessKey 不能为空");
            }

            // 新增时密钥必填；修改时留空表示不修改
            if (excludeId <= 0 && string.IsNullOrWhiteSpace(dto.AccessSecret))
            {
                return SmsActionResult.Fail("AccessSecret 不能为空");
            }

            if (await ExistsAccountName(accountName, excludeId))
            {
                return SmsActionResult.Fail("账号名称已存在");
            }

            return SmsActionResult.Ok("校验通过");
        }

        /// <summary>
        /// 启用前的接入参数完整性校验（R6）
        /// </summary>
        private async Task<SmsActionResult> ValidateEnable(long providerId, string accessKey, string accessSecret)
        {
            if (providerId <= 0)
            {
                return SmsActionResult.Fail("接入参数不完整，无法启用：未绑定供应商");
            }

            var providers = await QueryProvidersByIds(new List<long> { providerId });
            if (providers.Count == 0)
            {
                return SmsActionResult.Fail("接入参数不完整，无法启用：绑定的供应商不存在");
            }

            if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(accessSecret))
            {
                return SmsActionResult.Fail("接入参数不完整，无法启用");
            }

            return SmsActionResult.Ok("校验通过");
        }

        /// <summary>
        /// HTTP_CUSTOM 供应商走一次 /health 探活
        /// </summary>
        private async Task<SmsAccountTestResultDto> TestHttpCustom(SmsProvider provider)
        {
            if (string.IsNullOrWhiteSpace(provider.BaseUrl))
            {
                return TestFail("供应商类型 HTTP_CUSTOM 未配置接口地址（baseUrl）");
            }

            if (!Uri.TryCreate(provider.BaseUrl.Trim(), UriKind.Absolute, out var baseUri)
                || (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
            {
                return TestFail("供应商接口地址不是合法的 http(s) 地址");
            }

            var probeUrl = $"{baseUri.GetLeftPart(UriPartial.Path).TrimEnd('/')}/health";

            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var response = await client.GetAsync(probeUrl);
                var code = (int)response.StatusCode;

                return response.IsSuccessStatusCode
                    ? new SmsAccountTestResultDto { Success = true, Message = $"连通成功（HTTP {code}）：{probeUrl}" }
                    : TestFail($"连通失败：HTTP {code}（{probeUrl}）");
            }
            catch (Exception ex)
            {
                return TestFail($"连通失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 按 Id 批量取供应商
        /// </summary>
        private async Task<List<SmsProvider>> QueryProvidersByIds(List<long> providerIds)
        {
            if (providerIds == null || providerIds.Count == 0)
            {
                return new List<SmsProvider>();
            }

            var ids = providerIds.Where(id => id > 0).Distinct().ToList();
            if (ids.Count == 0)
            {
                return new List<SmsProvider>();
            }

            return await Db.Queryable<SmsProvider>()
                .Where(p => p.IsDeleted == false && ids.Contains(p.Id))
                .ToListAsync();
        }

        /// <summary>
        /// 在指定流水表里检查是否引用了该账号
        /// </summary>
        private async Task<bool> HasReferenceInTable(string tableName, long accountId)
        {
            if (!Db.DbMaintenance.IsAnyTable(tableName, false))
            {
                return false;
            }

            try
            {
                var counts = await Db.Ado.SqlQueryAsync<int>(
                    $"SELECT COUNT(1) FROM {tableName} WHERE account_id = :accountId",
                    new SugarParameter(":accountId", accountId));

                return counts.Count > 0 && counts[0] > 0;
            }
            catch (Exception)
            {
                // 表结构尚未按 L1 定义落库时忽略，避免影响删除主流程
                return false;
            }
        }

        /// <summary>
        /// 实体 → DTO（密钥脱敏，计算余额预警标记）
        /// </summary>
        private static SmsAccountDto ToDto(SmsAccount entity, SmsProvider provider)
        {
            return new SmsAccountDto
            {
                Id = entity.Id,
                AccountName = entity.AccountName,
                ProviderId = entity.ProviderId,
                ProviderName = provider?.ProviderName,
                ProviderType = provider?.ProviderType,
                ProviderStatus = provider?.Status ?? false,
                AccessKey = Mask(entity.AccessKey),
                AccessSecret = Mask(entity.AccessSecret),
                SignName = entity.SignName,
                Balance = entity.Balance,
                WarnThreshold = entity.WarnThreshold,
                Warned = entity.WarnThreshold > 0 && entity.Balance <= entity.WarnThreshold,
                Status = entity.Status,
                Remark = entity.Remark,
                CreateTime = entity.CreateTime
            };
        }

        /// <summary>
        /// 密钥脱敏：保留首尾各 4 位，中间固定 6 个星号
        /// </summary>
        private static string Mask(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Length <= 8
                ? "******"
                : $"{value.Substring(0, 4)}******{value.Substring(value.Length - 4)}";
        }

        /// <summary>
        /// 构造连通测试失败结果
        /// </summary>
        private static SmsAccountTestResultDto TestFail(string message)
        {
            return new SmsAccountTestResultDto { Success = false, Message = message };
        }

        #endregion
    }
}
