using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using News.Core.Contracts;
using static System.Formats.Asn1.AsnWriter;

namespace News.Service.Services.NewsCatcher
{
    public class ArticleNotificationTwoService  : IHostedService, IDisposable
    {
        private readonly ILogger<ArticleNotificationTwoService> _logger;
        private readonly IServiceProvider _serviceProvider;  
        private Timer _timer;

        public ArticleNotificationTwoService(ILogger<ArticleNotificationTwoService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;  
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ArticleNotificationService starting...");
            _timer = new Timer(ExecuteTask, null, TimeSpan.Zero, TimeSpan.FromHours(2)); 
            return Task.CompletedTask;
        }

        private async void ExecuteTask(object state)
        {
            _logger.LogInformation("Executing task at: " + DateTime.UtcNow);
            using (var scope = _serviceProvider.CreateScope())
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                await notificationService.SendNotificationsAsync();
                if (scope.ServiceProvider is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
