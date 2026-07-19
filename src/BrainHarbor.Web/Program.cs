using BrainHarbor.Web.Content;
using BrainHarbor.Web.Database;
using BrainHarbor.Web.Middleware;
using BrainHarbor.Web.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

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
// on dead links, in every environment.
app.UseStatusCodePagesWithReExecute("/status/{0}");

app.UseHttpsRedirection();

app.UseMiddleware<TextSizeMiddleware>();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

// Exposes the implicit Program class to WebApplicationFactory in tests.
public partial class Program;
