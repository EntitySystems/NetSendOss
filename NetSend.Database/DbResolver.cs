using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace NetSend.Database;

public class DbResolver
{
    private readonly ConcurrentDictionary<Guid, bool> _channelDbMigrations = [];
    private readonly ConcurrentDictionary<Guid, bool> _userDbDbMigrations = [];

    public async Task<ChannelDb> GetChannelDbAsync(Guid channelId)
    {
        var dbContext = new ChannelDb(channelId);
        return dbContext;
    }

    public async Task<NsdpUserDb> GetNsdpUserDbAsync(Guid userId)
    {
        var dbContext = new NsdpUserDb(userId);
        return dbContext;
    }

    private async Task _ensureNsdpUserDbMigrated(Guid userId, NsdpUserDb dbContext)
    {
        _userDbDbMigrations.GetOrAdd(userId, id =>
        {
            dbContext.Database.Migrate();
            return true;
        });
    }

    private async Task _ensureChannelDbMigrated(Guid channelId, ChannelDb dbContext)
    {
        _channelDbMigrations.GetOrAdd(channelId, id =>
        {
            dbContext.Database.Migrate();
            return true;
        });
    }
}