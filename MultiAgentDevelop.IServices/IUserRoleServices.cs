using MultiAgentDevelop.IServices.BASE;
using MultiAgentDevelop.Model.Models;
using System.Threading.Tasks;

namespace MultiAgentDevelop.IServices
{	
	/// <summary>
	/// UserRoleServices
	/// </summary>	
    public interface IUserRoleServices :IBaseServices<UserRole>
	{

        Task<UserRole> SaveUserRole(long uid, long rid);
        Task<int> GetRoleIdByUid(long uid);
    }
}

