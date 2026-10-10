using System.Threading.Tasks;
using MultiAgentDevelop.Common.DB;
using MultiAgentDevelop.Common.DB.Extension;
using MultiAgentDevelop.IRepository.Base;
using MultiAgentDevelop.Model.IDS4DbModels;
using MultiAgentDevelop.Services.BASE;

namespace MultiAgentDevelop.IServices
{
    public class ApplicationUserServices : BaseServices<ApplicationUser>, IApplicationUserServices
    {
        public bool IsEnable()
        {
            var configId = typeof(ApplicationUser).GetEntityTenant();
            return Db.AsTenant().IsAnyConnection(configId);
        }
    }
}