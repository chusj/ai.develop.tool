using MultiAgentDevelop.Common.HttpContextUser;
using MultiAgentDevelop.Controllers;
using MultiAgentDevelop.IServices;
using MultiAgentDevelop.Model;
using MultiAgentDevelop.Model.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiAgentDevelop.Api.Controllers
{
    /// <summary>
    /// 短信账号管理
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class SmsAccountController : BaseApiController
    {
        private readonly ISmsAccountServices _smsAccountServices;
        private readonly IUser _user;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="smsAccountServices">短信账号服务</param>
        /// <param name="user">当前登录用户</param>
        public SmsAccountController(ISmsAccountServices smsAccountServices, IUser user)
        {
            _smsAccountServices = smsAccountServices;
            _user = user;
        }

        /// <summary>
        /// 分页查询短信账号
        /// </summary>
        /// <param name="query">查询条件（账号名称模糊、供应商、状态、分页）</param>
        /// <returns>分页列表</returns>
        [HttpGet("page")]
        public async Task<MessageModel<PageModel<SmsAccountDto>>> Page([FromQuery] SmsAccountQueryDto query)
        {
            var data = await _smsAccountServices.QueryAccountPage(query);
            return SuccessPage(data);
        }

        /// <summary>
        /// 查询短信账号详情
        /// </summary>
        /// <param name="id">账号 Id</param>
        /// <returns>账号详情，AccessSecret 为掩码</returns>
        [HttpGet("detail/{id}")]
        public async Task<MessageModel<SmsAccountDto>> Detail(long id)
        {
            var data = await _smsAccountServices.GetAccountDetail(id);
            return data == null ? Failed<SmsAccountDto>("账号不存在") : Success(data);
        }

        /// <summary>
        /// 供应商下拉数据
        /// </summary>
        /// <returns>供应商列表（含禁用标记）</returns>
        [HttpGet("providerOptions")]
        public async Task<MessageModel<List<SmsProviderOptionDto>>> ProviderOptions()
        {
            return Success(await _smsAccountServices.GetProviderOptions());
        }

        /// <summary>
        /// 新增短信账号
        /// </summary>
        /// <param name="dto">账号表单</param>
        /// <returns>新账号 Id</returns>
        [HttpPost("create")]
        public async Task<MessageModel<string>> Create([FromBody] SmsAccountSaveDto dto)
        {
            var result = await _smsAccountServices.CreateAccount(dto, _user.ID, _user.Name);
            return result.Success
                ? Success(result.Id.ToString(), result.Message)
                : Failed(result.Message);
        }

        /// <summary>
        /// 修改短信账号
        /// </summary>
        /// <param name="dto">账号表单，AccessSecret 留空表示不修改</param>
        /// <returns>操作结果</returns>
        [HttpPut("update")]
        public async Task<MessageModel<string>> Update([FromBody] SmsAccountSaveDto dto)
        {
            var result = await _smsAccountServices.UpdateAccount(dto, _user.ID, _user.Name);
            return result.Success
                ? Success<string>(null, result.Message)
                : Failed(result.Message);
        }

        /// <summary>
        /// 删除短信账号（逻辑删除，被充值或发送记录引用时禁止删除）
        /// </summary>
        /// <param name="id">账号 Id</param>
        /// <returns>操作结果</returns>
        [HttpDelete("remove/{id}")]
        public async Task<MessageModel<string>> Remove(long id)
        {
            var result = await _smsAccountServices.RemoveAccount(id, _user.ID, _user.Name);
            return result.Success
                ? Success<string>(null, result.Message)
                : Failed(result.Message);
        }

        /// <summary>
        /// 启用或禁用短信账号
        /// </summary>
        /// <param name="id">账号 Id</param>
        /// <param name="status">true 启用 / false 禁用</param>
        /// <returns>操作结果</returns>
        [HttpPatch("status/{id}/{status}")]
        public async Task<MessageModel<string>> ChangeStatus(long id, bool status)
        {
            var result = await _smsAccountServices.ChangeStatus(id, status, _user.ID, _user.Name);
            return result.Success
                ? Success<string>(null, result.Message)
                : Failed(result.Message);
        }

        /// <summary>
        /// 短信账号连通性测试（不改变任何数据）
        /// </summary>
        /// <param name="id">账号 Id</param>
        /// <returns>是否连通及失败原因</returns>
        [HttpPost("test/{id}")]
        public async Task<MessageModel<SmsAccountTestResultDto>> Test(long id)
        {
            var data = await _smsAccountServices.TestConnection(id);
            // 接口本身调用成功即视为成功，连通结果放在 response 里由前端区分展示
            return Success(data, data.Success ? "连通测试通过" : "连通测试未通过");
        }
    }
}
