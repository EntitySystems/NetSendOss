using Microsoft.EntityFrameworkCore;

namespace NetSend.Database;

public class CacheDb : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=:memory:;Version=3;New=True;");

        base.OnConfiguring(optionsBuilder);
    }
}