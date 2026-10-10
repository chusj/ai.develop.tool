    

using MultiAgentDevelop.IServices.BASE;
using MultiAgentDevelop.Model.Models;
using System.Threading.Tasks;

namespace MultiAgentDevelop.IServices
{	
	/// <summary>
	/// sysUserInfoServices
	/// </summary>	
    public interface ISysUserInfoServices :IBaseServices<SysUserInfo>
	{
        Task<SysUserInfo> SaveUserInfo(string loginName, string loginPwd);
        Task<string> GetUserRoleNameStr(string loginName, string loginPwd);
    }
}
