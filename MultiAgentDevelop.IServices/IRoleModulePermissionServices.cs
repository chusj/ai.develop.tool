using MultiAgentDevelop.IServices.BASE;
using MultiAgentDevelop.Model.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiAgentDevelop.IServices
{
    /// <summary>
    /// RoleModulePermissionServices
    /// </summary>	
    public interface IRoleModulePermissionServices :IBaseServices<RoleModulePermission>
	{

        Task<List<RoleModulePermission>> GetRoleModule();
        Task<List<RoleModulePermission>> RoleModuleMaps();
        Task<List<RoleModulePermission>> GetRMPMaps();
        /// <summary>
        /// �������²˵���ӿڵĹ�ϵ
        /// </summary>
        /// <param name="permissionId">�˵�����</param>
        /// <param name="moduleId">�ӿ�����</param>
        /// <returns></returns>
        Task UpdateModuleId(long permissionId, long moduleId);
    }
}
