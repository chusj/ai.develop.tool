using MultiAgentDevelop.Common;
using MultiAgentDevelop.EventBus.EventHandling;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MultiAgentDevelop.EventBus
{
    public class BlogQueryIntegrationEventHandler : IIntegrationEventHandler<BlogQueryIntegrationEvent>
    {
        private readonly ILogger<BlogQueryIntegrationEventHandler> _logger;

        public BlogQueryIntegrationEventHandler(
            ILogger<BlogQueryIntegrationEventHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(BlogQueryIntegrationEvent @event)
        {
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, "MultiAgentDevelop", @event);

            ConsoleHelper.WriteSuccessLine($"----- Handling integration event: {@event.Id} at MultiAgentDevelop - ({@event})");

            await Task.CompletedTask;
        }

    }
}
