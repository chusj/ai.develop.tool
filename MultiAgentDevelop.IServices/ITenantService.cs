using System.Threading.Tasks;
using MultiAgentDevelop.IServices.BASE;
using MultiAgentDevelop.Model.Models;

namespace MultiAgentDevelop.IServices;

public interface ITenantService : IBaseServices<SysTenant>
{
    public Task SaveTenant(SysTenant tenant);

    public Task InitTenantDb(SysTenant tenant);
}