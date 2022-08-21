using Leaderboard.Data;
using Leaderboard.EntityFrameworkExtensions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var dbConnectionStringBuilder = new SqliteConnectionStringBuilder
{
    DataSource = "demo.db"
};

// Add services to the container.
builder.Services.AddEntityFrameworkSqlite();
//builder.Services.AddSqlite<LeaderboardContext>(dbConnectionStringBuilder.ConnectionString);
builder.Services.AddRelationalDbFunctions();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddDbContext<LeaderboardContext>();
builder.Services.AddTransient<LeaderboardService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

using (var leaderboardContext = new LeaderboardContext())
{
    _ = leaderboardContext.Database.EnsureCreated();

    var initialMigration = leaderboardContext.Database.GetMigrations().First();
    var pendingMigrations = leaderboardContext.Database.GetPendingMigrations();
    if (!pendingMigrations.Contains(initialMigration))
    {
        leaderboardContext.Database.Migrate();
    }
}

app.Run();
