using MultiAgentDevelop.IServices.BASE;
using MultiAgentDevelop.Model;
using MultiAgentDevelop.Model.Models;
using System.Threading.Tasks;

namespace MultiAgentDevelop.IServices
{
    public partial interface IGuestbookServices : IBaseServices<Guestbook>
    {
        Task<MessageModel<string>> TestTranInRepository();
        Task<bool> TestTranInRepositoryAOP();

        Task<bool> TestTranPropagation();

        Task<bool> TestTranPropagationNoTran();

        Task<bool> TestTranPropagationTran();
        Task TestTranPropagationTran2();
        Task TestTranPropagationTran3();
    }
}