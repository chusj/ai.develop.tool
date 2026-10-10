using System.Threading.Tasks;
using MultiAgentDevelop.IServices.BASE;
using MultiAgentDevelop.Model.Models;

namespace MultiAgentDevelop.IServices
{
    public partial interface IPasswordLibServices :IBaseServices<PasswordLib>
    {
        Task<bool> TestTranPropagation2();
        Task<bool> TestTranPropagationNoTranError();
        Task<bool> TestTranPropagationTran2();
        Task<bool> TestTranPropagationTran3();
    }
}
