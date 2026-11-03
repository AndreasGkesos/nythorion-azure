using Microsoft.EntityFrameworkCore;

namespace Nythorion.Auth.Data;

internal sealed class DatabaseMigrator(IServiceScopeFactory scopeFactory, ILogger<DatabaseMigrator> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        logger.LogInformation("Applying auth database migrations");
        await db.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
