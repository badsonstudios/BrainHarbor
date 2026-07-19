using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-207: the admin area is the human review gate — the thing standing
/// between a generated summary and a patient reading it. These tests are
/// about the boundary, not the UI.
/// </summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public class AdminAuthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string AdminEmail = "admin-test@example.org";
    private const string AdminPassword = "test-admin-password-1234";

    private readonly WebApplicationFactory<Program> _factory;

    public AdminAuthTests(WebApplicationFactory<Program> factory, DatabaseFixture database)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:BrainHarbor", database.ConnectionString);
            builder.UseSetting("Admin:Email", AdminEmail);
            builder.UseSetting("Admin:Password", AdminPassword);
        });
    }

    private HttpClient NoRedirectClient() => _factory.CreateClient(
        new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Theory]
    [InlineData("/admin")]
    [InlineData("/admin/two-factor")]
    public async Task AdminPagesRedirectAnonymousUsersToLogin(string url)
    {
        var response = await NoRedirectClient().GetAsync(url);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("/admin/login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task TheLoginPageItselfIsReachableAnonymously()
    {
        var response = await _factory.CreateClient().GetAsync("/admin/login");

        response.EnsureSuccessStatusCode();
        Assert.Contains("Admin sign in", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ThereIsNoRegistrationEndpoint()
    {
        // Security reference: single seeded account, no self-service signup.
        var client = _factory.CreateClient();

        foreach (var url in new[]
                 {
                     "/admin/register", "/Identity/Account/Register",
                     "/admin/forgot-password", "/Identity/Account/ForgotPassword",
                 })
        {
            var response = await client.GetAsync(url);
            Assert.True(
                response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Found,
                $"{url} unexpectedly returned {response.StatusCode}");
        }
    }

    [Fact]
    public async Task WrongPasswordDoesNotSignYouIn()
    {
        var client = NoRedirectClient();
        var response = await PostLoginAsync(client, AdminEmail, "not-the-password");

        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);   // re-renders the form
        Assert.Contains("did not match", html);
        Assert.DoesNotContain("Set-Cookie: .AspNetCore.Identity.Application",
            string.Join("\n", response.Headers.Select(h => $"{h.Key}: {string.Join(",", h.Value)}")));
    }

    [Fact]
    public async Task TheErrorMessageDoesNotRevealWhetherTheAccountExists()
    {
        var client = NoRedirectClient();

        var unknownUser = await (await PostLoginAsync(client, "nobody@example.org", "whatever"))
            .Content.ReadAsStringAsync();
        var knownUser = await (await PostLoginAsync(client, AdminEmail, "wrong-password"))
            .Content.ReadAsStringAsync();

        Assert.Contains("did not match", unknownUser);
        Assert.Contains("did not match", knownUser);
    }

    [Fact]
    public async Task CorrectCredentialsSignYouIn()
    {
        var client = NoRedirectClient();

        var response = await PostLoginAsync(client, AdminEmail, AdminPassword);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.True(response.Headers.Contains("Set-Cookie"),
            "a successful sign-in must issue the auth cookie");
    }

    [Fact]
    public async Task LoginPostsCarryAnAntiForgeryToken()
    {
        // Razor Pages validates it automatically; this proves the form emits
        // it, so the protection is actually in play.
        var html = await _factory.CreateClient().GetStringAsync("/admin/login");

        Assert.Contains("__RequestVerificationToken", html);
    }

    [Fact]
    public async Task ARequestWithoutTheAntiForgeryTokenIsRejected()
    {
        var client = NoRedirectClient();

        var response = await client.PostAsync("/admin/login", new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("Input.Email", AdminEmail),
            new KeyValuePair<string, string>("Input.Password", AdminPassword),
        ]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LogoutIsPostOnly()
    {
        // A GET logout can be triggered by any embedded image or link.
        var response = await NoRedirectClient().GetAsync("/admin/logout");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("/admin/login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task TwoFactorLoginIsNotReachableWithoutPassingThePasswordStep()
    {
        var response = await NoRedirectClient().GetAsync("/admin/two-factor-login");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("/admin/login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task AdminPagesAreNotIndexedOrLinkedFromThePublicSite()
    {
        var home = await _factory.CreateClient().GetStringAsync("/");

        Assert.DoesNotContain("/admin", home);
    }

    private async Task<HttpResponseMessage> PostLoginAsync(
        HttpClient client, string email, string password)
    {
        var page = await client.GetAsync("/admin/login");
        var html = await page.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(html);
        var cookies = page.Headers.TryGetValues("Set-Cookie", out var values)
            ? string.Join("; ", values.Select(v => v.Split(';')[0]))
            : "";

        var request = new HttpRequestMessage(HttpMethod.Post, "/admin/login")
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("Input.Email", email),
                new KeyValuePair<string, string>("Input.Password", password),
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
            ]),
        };
        if (cookies.Length > 0)
        {
            request.Headers.Add("Cookie", cookies);
        }

        return await client.SendAsync(request);
    }

    private static string ExtractAntiForgeryToken(string html)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            html, """name="__RequestVerificationToken"[^>]*value="([^"]+)""");
        return match.Success ? match.Groups[1].Value : "";
    }
}
