using Microsoft.AspNetCore.HttpOverrides;
using Nythorion.Auth.Endpoints;
using Nythorion.Auth.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthServer(builder.Configuration);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    // The container sits behind the Container Apps ingress, whose IPs are not fixed
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapRazorPages();
app.MapPost("/connect/token", TokenEndpoint.Handle);
app.MapGet("/connect/authorize", AuthorizeEndpoint.Handle);
app.MapPost("/connect/authorize", AuthorizeEndpoint.Handle);
app.MapGet("/connect/logout", LogoutEndpoint.Handle);
app.MapPost("/connect/logout", LogoutEndpoint.Handle);

app.Run();
