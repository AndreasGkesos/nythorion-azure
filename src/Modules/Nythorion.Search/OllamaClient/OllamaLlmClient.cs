using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Nythorion.Search.OllamaClient;

internal sealed class OllamaLlmClient(HttpClient httpClient, string model, float temperature = 0.1f, int numPredict = 4096)
{
    public async Task<string> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        var body = new
        {
            model,
            stream = false,
            options = new { temperature, num_predict = numPredict },
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        var response = await httpClient.PostAsJsonAsync("/api/chat", body, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken: cancellationToken);
        return result!.Message.Content;
    }

    public async Task<string> ChatAsync(string systemPrompt, IEnumerable<ChatMessage> history, CancellationToken cancellationToken = default)
    {
        var messages = new List<object> { new { role = "system", content = systemPrompt } };
        messages.AddRange(history.Select(m => new { role = m.Role, content = m.Content } as object));

        var body = new
        {
            model,
            stream = false,
            options = new { temperature, num_predict = numPredict },
            messages
        };

        var response = await httpClient.PostAsJsonAsync("/api/chat", body, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken: cancellationToken);
        return result!.Message.Content;
    }

    private sealed record OllamaChatResponse([property: JsonPropertyName("message")] OllamaMessage Message);
    private sealed record OllamaMessage([property: JsonPropertyName("content")] string Content);
}
