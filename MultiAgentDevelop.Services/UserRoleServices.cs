using MultiAgentDevelop.Common;
using MultiAgentDevelop.IRepository.Base;
using MultiAgentDevelop.IServices;
using MultiAgentDevelop.Model.Models;
using MultiAgentDevelop.Services.BASE;
using System.Linq;
using System.Threading.Tasks;

namespace MultiAgentDevelop.Services
{
    /// <summary>
    /// UserRoleServices
    /// </summary>	
    public class UserRoleServices : BaseServices<UserRole>, IUserRoleServices
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="rid"></param>
        /// <returns></returns>
        public async Task<UserRole> SaveUserRole(long uid, long rid)
        {
            UserRole userRole = new UserRole(uid, rid);

            UserRole model = new UserRole();
            var userList = await base.Query(a => a.UserId == userRole.UserId && a.RoleId == userRole.RoleId);
            if (userList.Count > 0)
            {
                model = userList.FirstOrDefault();
            }
            else
            {
                var id = await base.Add(userRole);
                model = await base.QueryById(id);
            }

            return model;

        }



        [Caching(AbsoluteExpiration = 30)]
        public async Task<int> GetRoleIdByUid(long uid)
        {
            return ((await base.Query(d => d.UserId == uid)).OrderByDescending(d => d.Id).LastOrDefault()?.RoleId).ObjToInt();
        }
    }
}
