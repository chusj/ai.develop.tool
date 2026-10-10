using MultiAgentDevelop.IServices.BASE;
using MultiAgentDevelop.Model.Models;
using System.Threading.Tasks;

namespace MultiAgentDevelop.IServices
{	
	/// <summary>
	/// RoleServices
	/// </summary>	
    public interface IRoleServices :IBaseServices<Role>
	{
        Task<Role> SaveRole(string roleName);
        Task<string> GetRoleNameByRid(int rid);

    }
}
