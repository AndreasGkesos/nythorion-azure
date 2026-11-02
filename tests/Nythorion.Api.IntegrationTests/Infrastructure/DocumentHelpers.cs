using System.Net.Http.Json;
using Nythorion.Api.IntegrationTests.Infrastructure;

namespace Nythorion.Api.IntegrationTests;

internal static class DocumentHelpers
{
    internal static async Task<Guid> UploadDocumentAsync(HttpClient client)
    {
        var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(ApiFixture.MinimalPdfBytes) { Headers = { ContentType = new("application/pdf") } }, "file", "test.pdf");
        form.Add(new StringContent("Test Doc"), "displayName");

        var response = await client.PostAsync("/documents/upload", form);
        var result = await response.Content.ReadFromJsonAsync<UploadResult>();
        return result!.DocumentId;
    }

    internal record UploadResult(Guid DocumentId, int ChunkCount);
}
