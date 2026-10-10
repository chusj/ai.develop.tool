using MultiAgentDevelop.IRepository.Base;
using MultiAgentDevelop.IServices;
using MultiAgentDevelop.Model.Models;
using MultiAgentDevelop.Services.BASE;
using System.Linq;
using System.Threading.Tasks;

namespace MultiAgentDevelop.FrameWork.Services
{
    /// <summary>
    /// sysUserInfoServices
    /// </summary>	
    public class SplitDemoServices : BaseServices<SplitDemo>, ISplitDemoServices
    {
        private readonly IBaseRepository<SplitDemo> _splitDemoRepository;
        public SplitDemoServices(IBaseRepository<SplitDemo> splitDemoRepository)
        {
            _splitDemoRepository = splitDemoRepository;
        }


    }
}
