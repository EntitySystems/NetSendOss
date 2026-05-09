using Microsoft.EntityFrameworkCore;
using NetSend.Core.Config;

namespace NetSend.Database;

public class MasterDb : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(EnvConfig.LocalDatabaseBasePath, $"master.sqlite");

        optionsBuilder.UseSqlite($"Data Source={dbPath};Version=3;");

        base.OnConfiguring(optionsBuilder);
    }
}