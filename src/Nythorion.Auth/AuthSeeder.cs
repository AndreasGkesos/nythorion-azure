using Microsoft.AspNetCore.Identity;
using Nythorion.Auth.Data;
using OpenIddict.Abstractions;

namespace Nythorion.Auth;

public sealed class AuthSeeder(IServiceProvider services, IConfiguration configuration, ILogger<AuthSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        await SeedScopesAsync(sp, cancellationToken);
        await SeedClientsAsync(sp, cancellationToken);
        await SeedUsersAsync(sp, cancellationToken);
    }

    private static async Task SeedScopesAsync(IServiceProvider sp, CancellationToken ct)
    {
        var scopeManager = sp.GetRequiredService<IOpenIddictScopeManager>();
        if (await scopeManager.FindByNameAsync("api", ct) is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "api",
                DisplayName = "Nythorion API",
                Resources = { AuthConstants.ApiResource }
            }, ct);
        }
    }

    private async Task SeedClientsAsync(IServiceProvider sp, CancellationToken ct)
    {
        var spaBaseUrl = configuration["Spa:BaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("Spa:BaseUrl must be set in configuration.");

        var manager = sp.GetRequiredService<IOpenIddictApplicationManager>();

        // Browser client — Authorization Code + PKCE, no secret
        var spaDescriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "nythorion-spa",
            ClientType = OpenIddictConstants.ClientTypes.Public,
            DisplayName = "Nythorion SPA",
            RedirectUris = { new Uri($"{spaBaseUrl}/callback") },
            PostLogoutRedirectUris = { new Uri(spaBaseUrl) },
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.Logout,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.ResponseTypes.Code,
                OpenIddictConstants.Permissions.Prefixes.Scope + "api",
                OpenIddictConstants.Permissions.Prefixes.Scope + "openid",
                OpenIddictConstants.Permissions.Prefixes.Scope + "profile",
            }
        };

        var spaClient = await manager.FindByClientIdAsync("nythorion-spa", ct);
        if (spaClient is null)
            await manager.CreateAsync(spaDescriptor, ct);
        else
            await manager.UpdateAsync(spaClient, spaDescriptor, ct);
    }

    private async Task SeedUsersAsync(IServiceProvider sp, CancellationToken ct)
    {
        var seedUsers = configuration.GetSection("SeedUsers").Get<List<SeedUser>>() ?? [];
        if (seedUsers.Count == 0)
            logger.LogWarning("No SeedUsers configured; only self-registration can create users");

        var userManager = sp.GetRequiredService<UserManager<AppUser>>();

        foreach (var seedUser in seedUsers)
        {
            if (string.IsNullOrWhiteSpace(seedUser.Username) || string.IsNullOrWhiteSpace(seedUser.Password))
                throw new InvalidOperationException("Every SeedUsers entry needs a Username and a Password.");

            if (await userManager.FindByNameAsync(seedUser.Username) is not null)
                continue;

            var user = new AppUser { UserName = seedUser.Username, Email = $"{seedUser.Username}@nythorion.local" };
            var result = await userManager.CreateAsync(user, seedUser.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Seeding user '{seedUser.Username}' failed: {string.Join("; ", result.Errors.Select(e => e.Description))}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
