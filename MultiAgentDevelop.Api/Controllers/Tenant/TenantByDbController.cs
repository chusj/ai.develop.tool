using MultiAgentDevelop.Common.HttpContextUser;
using MultiAgentDevelop.Controllers;
using MultiAgentDevelop.IServices.BASE;
using MultiAgentDevelop.Model;
using MultiAgentDevelop.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MultiAgentDevelop.Api.Controllers.Tenant;

/// <summary>
/// 多租户-多库方案 测试
/// </summary>
[Produces("application/json")]
[Route("api/Tenant/ByDb")]
[Authorize]
public class TenantByDbController : BaseApiController
{
    private readonly IBaseServices<SubLibraryBusinessTable> _services;
    private readonly IUser _user;

    public TenantByDbController(IUser user, IBaseServices<SubLibraryBusinessTable> services)
    {
        _user = user;
        _services = services;
    }

    /// <summary>
    /// 获取租户下全部业务数据 <br/>
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<MessageModel<List<SubLibraryBusinessTable>>> GetAll()
    {
        var data = await _services.Query();
        return Success(data);
    }

    /// <summary>
    /// 新增数据
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<MessageModel> Post(SubLibraryBusinessTable data)
    {
        await _services.Db.Insertable(data).ExecuteReturnSnowflakeIdAsync();

        return Success();
    }
}