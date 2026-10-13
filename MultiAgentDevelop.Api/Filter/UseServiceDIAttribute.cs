using MultiAgentDevelop.IServices;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MultiAgentDevelop.Filter
{
    public class UseServiceDIAttribute : ActionFilterAttribute
    {

        protected readonly ILogger<UseServiceDIAttribute> _logger;
        private readonly IRoleServices _roleServices;
        private readonly string _name;

        public UseServiceDIAttribute(ILogger<UseServiceDIAttribute> logger, IRoleServices roleServices, string Name = "")
        {
            _logger = logger;
            _roleServices = roleServices;
            _name = Name;
        }


        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var dd = _roleServices.Query().Result;
            _logger.LogInformation("测试自定义服务特性");
            Console.WriteLine(_name);
            base.OnActionExecuted(context);
            DeleteSubscriptionFiles();
        }

        private void DeleteSubscriptionFiles()
        {

        }
    }
}
