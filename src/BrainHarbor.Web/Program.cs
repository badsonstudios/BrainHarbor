using System.Threading.RateLimiting;
using BrainHarbor.Web.Api;
using BrainHarbor.Web.Content;
using BrainHarbor.Web.Database;
using BrainHarbor.Web.Middleware;
using BrainHarbor.Web.Services;
using Microsoft.AspNetCore.RateLimiting;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

DapperTypeHandlers.Register();

// Add services to the container.
builder.Services.AddRazorPages();

var connectionStringSetting = builder.Configuration.GetConnectionString("BrainHarbor")
    ?? throw new InvalidOperationException(
        "Connection string 'BrainHarbor' not found. Set it via: dotnet user-secrets set \"ConnectionStrings:BrainHarbor\" \"...\" --project src/BrainHarbor.Web (dev) or environment/App Service configuration.");
builder.Services.AddNpgsqlDataSource(connectionStringSetting);
builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
builder.Services.AddSingleton<GlossaryStore>();
builder.Services.AddSingleton<ContentStore>();
builder.Services.AddSingleton<TaxonomyStore>();

// Sync API (WI-202) — the only write surface.
builder.Services.AddScoped<SyncRepository>();
// AddEndpointFilter<T> resolves once from the ROOT provider at endpoint build
// time, so a scoped registration would be a captive-dependency trap later.
builder.Services.AddSingleton<SyncApiKeyFilter>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Partitioned by the presented key: the limiter runs BEFORE auth, so a
    // single global bucket would let any unauthenticated scanner burn the
    // budget and lock the daily pipeline out with 429s. The real key gets its
    // own generous partition; everyone else shares a tight one.
    options.AddPolicy(SyncEndpoints.RateLimitPolicy, context =>
    {
        var presented = context.Request.Headers[SyncApiKeyFilter.HeaderName].ToString();
        var configured = context.RequestServices
            .GetRequiredService<IConfiguration>()["SYNC_API_KEY"];
        var authenticated = !string.IsNullOrEmpty(presented) && presented == configured;

        return authenticated
            ? RateLimitPartition.GetFixedWindowLimiter("sync-authenticated", _ =>
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 120,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                })
            : RateLimitPartition.GetFixedWindowLimiter("sync-anonymous", _ =>
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                });
    });
});

var app = builder.Build();

// DbUp runs on startup in dev; in prod (M4) migrations become a CI step.
if (app.Environment.IsDevelopment())
{
    MigrationRunner.Run(connectionStringSetting);
}

// Configure the HTTP request pipeline. Exception handler stays outermost.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Friendly 404/error pages (WI-103) — the helpline band must be present even
// on dead links, in every environment. Scoped to non-API requests: re-executing
// an API 401 into the HTML status page returns markup to a machine client (and
// a POST re-execute has no page handler at all, surfacing as a bogus 400).
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api"),
    branch => branch.UseStatusCodePagesWithReExecute("/status/{0}"));

app.UseHttpsRedirection();

app.UseMiddleware<TextSizeMiddleware>();

app.UseRouting();

app.UseRateLimiter();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapSyncApi();

app.Run();

// Exposes the implicit Program class to WebApplicationFactory in tests.
public partial class Program;
