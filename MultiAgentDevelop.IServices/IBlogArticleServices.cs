using MultiAgentDevelop.IServices.BASE;
using MultiAgentDevelop.Model.Models;
using MultiAgentDevelop.Model.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiAgentDevelop.IServices
{
    public interface IBlogArticleServices :IBaseServices<BlogArticle>
    {
        Task<List<BlogArticle>> GetBlogs();
        Task<BlogViewModels> GetBlogDetails(long id);

    }

}
