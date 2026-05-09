using Microsoft.EntityFrameworkCore;
using NetSend.Core.Config;
using NetSend.Database.Dbo;

namespace NetSend.Database;

public class ChannelDb(Guid channelId) : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ChannelMessageConfiguration());

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(EnvConfig.LocalDatabaseBasePath, "Channels", $"{channelId}.sqlite");

        optionsBuilder.UseSqlite($"Data Source={dbPath};Version=3;");

        base.OnConfiguring(optionsBuilder);
    }
}