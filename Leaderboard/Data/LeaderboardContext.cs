using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using System.Diagnostics;

namespace Leaderboard.Data;

public partial class LeaderboardContext : DbContext
{
    public DbSet<LeaderboardEntry> LeaderboardEntries { get; set; } = default!;

    public DbSet<Person> People { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        var dbConnectionStringBuilder = new SqliteConnectionStringBuilder()
        {
            DataSource = "demo.db"
        };

        _ = optionsBuilder
            .UseSqlite(dbConnectionStringBuilder.ConnectionString);

        _ = optionsBuilder
            .EnableSensitiveDataLogging(true)
            .UseLoggerFactory(
                LoggerFactory.Create(
                    builder => builder.AddSimpleConsole()
                )
            );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //_ = modelBuilder.Entity<LeaderboardEntry>()
        //    .HasMany(entry => entry.Members)
        //    .WithMany(person => person.LeaderboardEntries);

        //_ = modelBuilder.Entity<Person>()
        //    .HasMany(person => person.LeaderboardEntries)
        //    .WithMany(entry => entry.Members);

        if (Debugger.IsAttached)
        {
            Seed(modelBuilder);
        }
    }
}
