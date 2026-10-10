using MultiAgentDevelop.Common;
using MultiAgentDevelop.Common.Helper;
using MultiAgentDevelop.IRepository.Base;
using MultiAgentDevelop.IServices;
using MultiAgentDevelop.Model;
using MultiAgentDevelop.Model.Models;
using MultiAgentDevelop.Model.ViewModels;
using MultiAgentDevelop.Services.BASE;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MultiAgentDevelop.Repository.UnitOfWorks;

namespace MultiAgentDevelop.Services
{
    /// <summary>
	/// WeChatCompanyServices
	/// </summary>
    public class WeChatCompanyServices : BaseServices<WeChatCompany>, IWeChatCompanyServices
    {
        readonly IUnitOfWorkManage _unitOfWorkManage;
        readonly ILogger<WeChatCompanyServices> _logger;
        public WeChatCompanyServices(IUnitOfWorkManage unitOfWorkManage, ILogger<WeChatCompanyServices> logger)
        {
            this._unitOfWorkManage = unitOfWorkManage;
            this._logger = logger;
        }  
        
    }
}