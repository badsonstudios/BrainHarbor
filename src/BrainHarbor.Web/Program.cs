using System.Threading.RateLimiting;
using BrainHarbor.Web.Api;
using BrainHarbor.Web.Content;
using BrainHarbor.Web.Database;
using BrainHarbor.Web.Identity;
using BrainHarbor.Web.Middleware;
using BrainHarbor.Web.Seo;
using BrainHarbor.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Npgsql;

const string AdminPolicy = "AdminOnly";

var builder = WebApplication.CreateBuilder(args);

DapperTypeHandlers.Register();

// Add services to the container. Admin pages require an authenticated admin
// by convention — every page under /Admin except the login flow, which opts
// out with [AllowAnonymous].
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", AdminPolicy);
});

var connectionStringSetting = builder.Configuration.GetConnectionString("BrainHarbor")
    ?? throw new InvalidOperationException(
        "Connection string 'BrainHarbor' not found. Set it via: dotnet user-secrets set \"ConnectionStrings:BrainHarbor\" \"...\" --project src/BrainHarbor.Web (dev) or environment/App Service configuration.");
builder.Services.AddNpgsqlDataSource(connectionStringSetting);
builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
builder.Services.AddSingleton<GlossaryStore>();
builder.Services.AddSingleton<ContentStore>();
builder.Services.AddSingleton<SummaryRenderer>();
builder.Services.AddSingleton<TaxonomyStore>();

// Admin auth (WI-207): one seeded account, TOTP 2FA, NO registration
// endpoint. Identity is the only EF Core usage in the app — hand-rolling
// password hashing and token storage is not a risk worth taking.
builder.Services.AddDbContext<AdminDbContext>(options =>
    options.UseNpgsql(connectionStringSetting,
        npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", AdminDbContext.SchemaName)));

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        // Stated explicitly rather than inherited from defaults: length does
        // the real work for a single human-chosen password, so character-class
        // rules are relaxed in favour of a longer minimum.
        options.Password.RequiredLength = 16;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;

        // A single human account, so lock out hard and for a while: there is
        // no support desk to unlock it and no legitimate burst of failures.
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AdminDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/admin/login";
    options.LogoutPath = "/admin/logout";
    options.AccessDeniedPath = "/admin/login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AdminPolicy, policy => policy.RequireRole(AdminSeeder.AdminRole));

// Sync API (WI-202) — the only write surface.
builder.Services.AddScoped<SyncRepository>();
builder.Services.AddScoped<BrainHarbor.Web.Admin.ReviewRepository>();

// Publish mode (WI-212): Auto by default — summarized items that pass the
// pipeline's automated checks publish themselves; flagged or unsummarized
// items wait in the review queue. Set Publishing:Mode=Review to require a
// person for everything.
builder.Services.Configure<PublishingOptions>(
    builder.Configuration.GetSection(PublishingOptions.SectionName));
builder.Services.AddScoped<BrainHarbor.Web.Feed.FeedRepository>();
builder.Services.AddSingleton<BrainHarbor.Web.Feed.CardImages>();
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

// Identity's own schema + the single admin account (WI-207).
await AdminSeeder.SeedAsync(app.Services,
    app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder"));

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
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapSyncApi();
app.MapSyndication();

app.Run();

// Exposes the implicit Program class to WebApplicationFactory in tests.
public partial class Program;
