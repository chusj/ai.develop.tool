
using System;
using System.Threading.Tasks;
using MultiAgentDevelop.IServices.BASE;
using MultiAgentDevelop.Model;
using MultiAgentDevelop.Model.Models;

namespace MultiAgentDevelop.IServices
{	
	/// <summary>
	/// ITasksLogServices
	/// </summary>	
    public interface ITasksLogServices :IBaseServices<TasksLog>
	{
		public Task<PageModel<TasksLog>> GetTaskLogs(long jobId, int page, int intPageSize,DateTime? runTime,DateTime? endTime);
        public Task<object> GetTaskOverview(long jobId, DateTime? runTime, DateTime? endTime, string type);
    }
}
                    