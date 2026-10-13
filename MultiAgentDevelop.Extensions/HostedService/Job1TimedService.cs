using MultiAgentDevelop.Common;
using MultiAgentDevelop.IServices;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiAgentDevelop.Extensions
{
    public class Job1TimedService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly ISysUserInfoServices _sysUserInfoServices;

        // 这里可以注入
        public Job1TimedService(ISysUserInfoServices sysUserInfoServices)
        {
            _sysUserInfoServices = sysUserInfoServices;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Job 1 is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(60 * 60));//一个小时

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            try
            {
                var model = _sysUserInfoServices.QueryById(1).Result;
                Console.WriteLine($"Job 1 启动成功，获取id=1的用户登录名为:{model?.LoginName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error:{ex.Message}");
            }

            ConsoleHelper.WriteSuccessLine($"Job 1： {DateTime.Now}");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Job 1 is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
