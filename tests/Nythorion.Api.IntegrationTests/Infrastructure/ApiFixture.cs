using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Nythorion.Api.Data;
using Nythorion.Api.Processing;
using Testcontainers.PostgreSql;

namespace Nythorion.Api.IntegrationTests.Infrastructure;

public class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg16")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Settings that normally come from the gitignored appsettings.Development.json
        builder.UseSetting("Auth:Issuer", "https://localhost");
        builder.UseSetting("ConnectionStrings:Default", _postgres.GetConnectionString());

        builder.ConfigureServices(services =>
        {
            // Replace the real DbContext with one pointing at the test container
            services.RemoveAll<DbContextOptions<NythorionDbContext>>();
            services.AddDbContext<NythorionDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString(), o => o.UseVector()));

            // Replace OpenIddict auth with a simple JWT bearer scheme using a known test key
            services.RemoveAll<IAuthenticationSchemeProvider>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        IssuerSigningKey = TokenHelper.SecurityKey
                    };
                });

            // Remove background worker — no Ollama available in tests
            services.RemoveAll<IHostedService>();

            // Apply migrations
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NythorionDbContext>();
            db.Database.Migrate();
        });
    }

    // Minimal valid PDF with one page of text that PdfPig can parse
    public static readonly byte[] MinimalPdfBytes = Convert.FromBase64String(
        "JVBERi0xLjQKMSAwIG9iago8PC9UeXBlIC9DYXRhbG9nIC9QYWdlcyAyIDAgUj4+CmVuZG9iagoy" +
        "IDAgb2JqCjw8L1R5cGUgL1BhZ2VzIC9LaWRzIFszIDAgUl0gL0NvdW50IDE+PgplbmRvYmoKMyAw" +
        "IG9iago8PC9UeXBlIC9QYWdlIC9QYXJlbnQgMiAwIFIgL01lZGlhQm94IFswIDAgNjEyIDc5Ml0g" +
        "L0NvbnRlbnRzIDQgMCBSIC9SZXNvdXJjZXMgPDwvRm9udCA8PC9GMSA1IDAgUj4+Pj4+PgplbmRv" +
        "YmoKNCAwIG9iago8PC9MZW5ndGggNDQ+PgpzdHJlYW0KQlQKL0YxIDEyIFRmCjcyIDcyMCBUZAoo" +
        "SGVsbG8gV29ybGQpIFRqCkVUCmVuZHN0cmVhbQplbmRvYmoKNSAwIG9iago8PC9UeXBlIC9Gb250" +
        "IC9TdWJ0eXBlIC9UeXBlMSAvQmFzZUZvbnQgL0hlbHZldGljYT4+CmVuZG9iagp4cmVmCjAgNgow" +
        "MDAwMDAwMDAwIDY1NTM1IGYgCjAwMDAwMDAwMDkgMDAwMDAgbiAKMDAwMDAwMDA1OCAwMDAwMCBu" +
        "IAowMDAwMDAwMTE1IDAwMDAwIG4gCjAwMDAwMDAyNjYgMDAwMDAgbiAKMDAwMDAwMDM2MCAwMDAwMCBuIAp0cmFpbGVyCjw8L1NpemUgNiAvUm9vdCAxIDAgUj4+CnN0YXJ0eHJlZgo0NDUKJSVFT0YK");

    public HttpClient CreateClientForUser(string userId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenHelper.GenerateToken(userId));
        return client;
    }
}
