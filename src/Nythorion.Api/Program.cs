using Microsoft.AspNetCore.HttpOverrides;
using Nythorion.Api.Extensions;
using Nythorion.Documents;
using Nythorion.Embeddings;
using Nythorion.Learning;
using Nythorion.Search;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    // The container sits behind the Container Apps ingress, whose IPs are not fixed
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddHealthChecks();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddProcessing();
builder.Services.AddDocumentsModule();
builder.Services.AddAzureOpenAi(builder.Configuration);
builder.Services.AddEmbeddingsModule();
builder.Services.AddSearchModule();
builder.Services.AddLearningModule();
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!)
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapDocumentEndpoints();
app.MapSearchEndpoints();
app.MapLearningEndpoints();

app.MapGet("/config", (IConfiguration config) => Results.Ok(new
{
    llmModel    = config["AzureOpenAI:ChatDeployment"],
    embedModel  = config["AzureOpenAI:EmbeddingDeployment"],
    temperature = config.GetValue<float>("AzureOpenAI:Temperature", 0.1f),
    numPredict  = config.GetValue<int>("AzureOpenAI:MaxOutputTokens", 4096)
})).RequireAuthorization();

app.Run();

public partial class Program { }
