using System.Threading.Tasks;
using MultiAgentDevelop.IServices.BASE;
using MultiAgentDevelop.Model.IDS4DbModels;

namespace MultiAgentDevelop.IServices
{
    public partial interface IApplicationUserServices : IBaseServices<ApplicationUser>
    {
        bool IsEnable();
    }
}