using BrainHarbor.Web.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// DbUp runs on startup in dev; in prod (M4) migrations become a CI step.
if (app.Environment.IsDevelopment())
{
    var connectionString = app.Configuration.GetConnectionString("BrainHarbor")
        ?? throw new InvalidOperationException(
            "Connection string 'BrainHarbor' not found. Set it via: dotnet user-secrets set \"ConnectionStrings:BrainHarbor\" \"...\" --project src/BrainHarbor.Web");
    MigrationRunner.Run(connectionString);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
