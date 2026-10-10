using MultiAgentDevelop.IServices.BASE;
using MultiAgentDevelop.Model.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiAgentDevelop.IServices
{
    public interface ITopicDetailServices : IBaseServices<TopicDetail>
    {
        Task<List<TopicDetail>> GetTopicDetails();
    }
}
