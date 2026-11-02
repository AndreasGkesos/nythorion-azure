using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Nythorion.Api.IntegrationTests.Infrastructure;

namespace Nythorion.Api.IntegrationTests;

public class ConfigEndpointTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    [Fact]
    public async Task GetConfig_WithoutToken_Returns401()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/config");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetConfig_WithToken_Returns200_WithExpectedFields()
    {
        var client = fixture.CreateClientForUser("user-config");

        var response = await client.GetAsync("/config");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var config = await response.Content.ReadFromJsonAsync<ConfigDto>();
        config.Should().NotBeNull();
        config!.LlmModel.Should().NotBeNullOrEmpty();
        config.EmbedModel.Should().NotBeNullOrEmpty();
        config.Temperature.Should().BeGreaterThan(0);
        config.NumPredict.Should().BeGreaterThan(0);
    }

    private record ConfigDto(string LlmModel, string EmbedModel, float Temperature, int NumPredict);
}
