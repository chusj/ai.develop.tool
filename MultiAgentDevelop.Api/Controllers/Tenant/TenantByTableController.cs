using MultiAgentDevelop.Common.HttpContextUser;
using MultiAgentDevelop.Controllers;
using MultiAgentDevelop.IServices.BASE;
using MultiAgentDevelop.Model;
using MultiAgentDevelop.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MultiAgentDevelop.Api.Controllers.Tenant;

/// <summary>
/// 多租户-多表方案 测试
/// </summary>
[Produces("application/json")]
[Route("api/Tenant/ByTable")]
[Authorize]
public class TenantByTableController : BaseApiController
{
    private readonly IBaseServices<MultiBusinessTable> _services;
    private readonly IUser _user;

    public TenantByTableController(IUser user, IBaseServices<MultiBusinessTable> services)
    {
        _user = user;
        _services = services;
    }

    /// <summary>
    /// 获取租户下全部业务数据 <br/>
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<MessageModel<List<MultiBusinessTable>>> GetAll()
    {
        //查询
        // var data = await _services.Query();

        //关联查询
        var data = await _services.Db
            .Queryable<MultiBusinessTable>()
            .Includes(s => s.Child)
            .ToListAsync();
        return Success(data);
    }

    /// <summary>
    /// 新增数据
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<MessageModel> Post(MultiBusinessTable data)
    {
        await _services.Db.Insertable(data).ExecuteReturnSnowflakeIdAsync();

        return Success();
    }
}