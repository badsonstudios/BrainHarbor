using System.Net;
using System.Net.Http.Json;
using BrainHarbor.Web.Api;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-210: staleness must be visible, never silent (architecture.md §6). A
/// source that quietly stopped working otherwise looks exactly like a quiet
/// week — which is how a feed dies without anyone noticing.
/// </summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class SourceHealthTests
    : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const string TestKey = "health-test-key-0123456789";
    private const string TestSource = "test_sync";

    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _database;

    public SourceHealthTests(WebApplicationFactory<Program> factory, DatabaseFixture database)
    {
        _database = database;
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:BrainHarbor", database.ConnectionString);
            builder.UseSetting("SYNC_API_KEY", TestKey);
        });
    }

    public Task InitializeAsync() => CleanupAsync();

    public Task DisposeAsync() => CleanupAsync();

    private async Task CleanupAsync()
    {
        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        await connection.ExecuteAsync(
            "DELETE FROM source_sync_state WHERE source = @TestSource", new { TestSource });
    }

    private HttpClient AuthedClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(SyncApiKeyFilter.HeaderName, TestKey);
        return client;
    }

    [Fact]
    public async Task AFailureIsRecordedAndVisibleInState()
    {
        var client = AuthedClient();

        var response = await client.PostAsJsonAsync("/api/sync/failure",
            new FailureRequest(TestSource, "feed returned 404"));
        response.EnsureSuccessStatusCode();

        var state = await client.GetFromJsonAsync<SyncStateResponse>("/api/sync/state");
        var source = Assert.Single(state!.Sources.Where(s => s.Source == TestSource));

        Assert.Equal("feed returned 404", source.LastError);
    }

    [Fact]
    public async Task AFailureDoesNotAdvanceTheCursor()
    {
        // A failed run must retry the same window, not skip it.
        var client = AuthedClient();
        await client.PostAsJsonAsync("/api/sync/cursor", new CursorRequest(TestSource, "window-1"));

        await client.PostAsJsonAsync("/api/sync/failure", new FailureRequest(TestSource, "boom"));

        var state = await client.GetFromJsonAsync<SyncStateResponse>("/api/sync/state");
        var source = Assert.Single(state!.Sources.Where(s => s.Source == TestSource));

        Assert.Equal("window-1", source.Cursor);
    }

    [Fact]
    public async Task ASubsequentSuccessClearsTheError()
    {
        var client = AuthedClient();
        await client.PostAsJsonAsync("/api/sync/failure", new FailureRequest(TestSource, "boom"));

        await client.PostAsJsonAsync("/api/sync/cursor", new CursorRequest(TestSource, "window-2"));

        var state = await client.GetFromJsonAsync<SyncStateResponse>("/api/sync/state");
        var source = Assert.Single(state!.Sources.Where(s => s.Source == TestSource));

        Assert.Null(source.LastError);
        Assert.NotNull(source.LastSuccessAt);
    }

    [Fact]
    public async Task AnUnknownSourceCannotCreateAPhantomHealthRow()
    {
        var response = await AuthedClient().PostAsJsonAsync("/api/sync/failure",
            new FailureRequest("not_a_source", "boom"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReportingAFailureRequiresTheApiKey()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/sync/failure",
            new FailureRequest(TestSource, "boom"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TheHealthPageRequiresAdminAuth()
    {
        var client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/admin/health");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("/admin/login", response.Headers.Location?.ToString());
    }

    [Theory]
    [InlineData(0, "today")]
    [InlineData(1, "yesterday")]
    [InlineData(5, "5 days ago")]
    public void StalenessIsDescribedInPlainWords(int daysAgo, string expected)
    {
        var health = new BrainHarbor.Web.Pages.Admin.HealthModel.SourceHealth(
            "pubmed", DateTimeOffset.UtcNow.AddDays(-daysAgo).AddMinutes(-1), null, null);

        Assert.Equal(expected, health.LastSuccessText);
    }

    [Fact]
    public void ASourceThatHasNeverSucceededIsStale()
    {
        var health = new BrainHarbor.Web.Pages.Admin.HealthModel.SourceHealth(
            "pubmed", null, null, null);

        Assert.True(health.IsStale);
        Assert.Equal("never", health.LastSuccessText);
    }

    [Fact]
    public void ADailyPipelineIsStaleAfterTwoDays()
    {
        var fresh = new BrainHarbor.Web.Pages.Admin.HealthModel.SourceHealth(
            "pubmed", DateTimeOffset.UtcNow.AddHours(-12), null, null);
        var stale = new BrainHarbor.Web.Pages.Admin.HealthModel.SourceHealth(
            "pubmed", DateTimeOffset.UtcNow.AddDays(-3), null, null);

        Assert.False(fresh.IsStale);
        Assert.True(stale.IsStale);
    }
}
