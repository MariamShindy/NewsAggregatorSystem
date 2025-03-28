namespace News.Service.Services
{
    public class ArticleNotificationService  : IHostedService, IDisposable
    {
        private readonly ILogger<ArticleNotificationService> _logger;
        private readonly IServiceProvider _serviceProvider;  
        private Timer _timer;

        public ArticleNotificationService(ILogger<ArticleNotificationService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;  
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ArticleNotificationService starting...");
            _timer = new Timer(ExecuteTask, null, TimeSpan.Zero, TimeSpan.FromHours(3)); 
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
