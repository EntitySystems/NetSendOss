using Microsoft.EntityFrameworkCore;
using NetSend.Core.Config;

namespace NetSend.Database;

public class NsdpUserDb(Guid userId) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(EnvConfig.LocalDatabaseBasePath, "UsersNsdp", $"{userId}.sqlite");

        optionsBuilder.UseSqlite($"Data Source={dbPath};Version=3;");

        base.OnConfiguring(optionsBuilder);
    }
}