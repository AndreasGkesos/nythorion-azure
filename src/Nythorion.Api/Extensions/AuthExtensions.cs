using OpenIddict.Validation.AspNetCore;

namespace Nythorion.Api.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenIddict()
            .AddValidation(options =>
            {
                options.SetIssuer(configuration["Auth:Issuer"]!);
                options.AddAudiences("nythorion-api");
                options.UseSystemNetHttp();
                options.UseAspNetCore();
            });

        services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        services.AddAuthorization();

        return services;
    }
}
