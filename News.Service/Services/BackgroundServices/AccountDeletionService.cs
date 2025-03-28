namespace News.Service.Services.BackgroundServices
{
    public class AccountDeletionService : BackgroundService
    {
        private readonly ILogger<AccountDeletionService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public AccountDeletionService(ILogger<AccountDeletionService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await DeleteExpiredAccountsAsync();
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task DeleteExpiredAccountsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var expiredUsers = userManager.Users
                .Where(u => u.IsPendingDeletion && u.DeletionRequestedAt.HasValue)
                .AsEnumerable() 
                .Where(u => (DateTime.UtcNow - u.DeletionRequestedAt.Value).TotalDays >= 14)
                .ToList();

            foreach (var user in expiredUsers)
            {
                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                    _logger.LogInformation($"Deleted user {user.UserName} after 14 days.");
                else
                    _logger.LogError($"Failed to delete user {user.UserName}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

    }
}
