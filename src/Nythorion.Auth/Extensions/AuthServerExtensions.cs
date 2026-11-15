using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nythorion.Auth.Data;

namespace Nythorion.Auth.Extensions;

public static class AuthServerExtensions
{
    private const string IssuerKey = "Auth:Issuer";

    public static IServiceCollection AddAuthServer(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Default"));
            options.UseOpenIddict();
        });

        services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Connect/Login";
        });

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<AuthDbContext>();
            })
            .AddServer(options =>
            {
                options.SetAuthorizationEndpointUris("/connect/authorize")
                       .SetTokenEndpointUris("/connect/token");

                options.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();

                options.RegisterScopes(
                    OpenIddict.Abstractions.OpenIddictConstants.Scopes.OpenId,
                    OpenIddict.Abstractions.OpenIddictConstants.Scopes.Profile,
                    "api");

                if (environment.IsDevelopment())
                {
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();
                }
                else
                {
                    // In memory and regenerated on restart: users sign in again, nothing to store or rotate
                    options.AddEphemeralEncryptionKey()
                           .AddEphemeralSigningKey();
                }

                options.DisableAccessTokenEncryption();

                // Must equal the Auth:Issuer the Api validates against, and cannot be inferred behind an ingress
                var issuer = configuration[IssuerKey];
                if (!string.IsNullOrWhiteSpace(issuer))
                    options.SetIssuer(issuer);

                options.UseAspNetCore()
                       .EnableTokenEndpointPassthrough()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableLogoutEndpointPassthrough();

                options.SetLogoutEndpointUris("/connect/logout");
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
        });
        services.AddAuthorization();
        services.Configure<RegistrationOptions>(configuration.GetSection(RegistrationOptions.SectionName));
        services.AddRazorPages();
        // Order matters: hosted services start one after another, so the schema exists before seeding
        services.AddHostedService<DatabaseMigrator>();
        services.AddHostedService<AuthSeeder>();

        return services;
    }
}
