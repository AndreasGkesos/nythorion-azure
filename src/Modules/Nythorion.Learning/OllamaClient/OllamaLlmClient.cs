using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Nythorion.Learning.OllamaClient;

public sealed class OllamaLlmClient(HttpClient httpClient, string model, float temperature = 0.1f, int numPredict = 4096)
{
    public async Task<string> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default, bool jsonFormat = true, string? assistantPrefill = null, int? numCtx = null)
    {
        var options = numCtx.HasValue
            ? (object)new { temperature, num_predict = numPredict, num_ctx = numCtx.Value }
            : new { temperature, num_predict = numPredict };

        var messageList = new List<object>
        {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = userPrompt }
        };
        if (assistantPrefill is not null)
            messageList.Add(new { role = "assistant", content = assistantPrefill });

        var messages = messageList.ToArray();

        object body = jsonFormat
            ? new { model, stream = false, format = "json", options, messages }
            : new { model, stream = false, options, messages };

        var response = await httpClient.PostAsJsonAsync("/api/chat", body, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken: cancellationToken);
        var content = result!.Message.Content;
        return assistantPrefill is not null ? assistantPrefill + content : content;
    }

    private sealed record OllamaChatResponse([property: JsonPropertyName("message")] OllamaMessage Message);
    private sealed record OllamaMessage([property: JsonPropertyName("content")] string Content);
}
