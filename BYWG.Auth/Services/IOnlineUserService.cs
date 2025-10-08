using System.Collections.Concurrent;

namespace BYWG.Auth.Services;

public interface IOnlineUserService
{
    void UpdateHeartbeat(int userId);
    void RemoveUser(int userId);
    bool IsUserOnline(int userId, TimeSpan? onlineWindow = null);
    int GetOnlineCount(TimeSpan? onlineWindow = null);
    IReadOnlyCollection<int> GetOnlineUserIds(TimeSpan? onlineWindow = null);
}

public class OnlineUserService : IOnlineUserService
{
    private readonly ConcurrentDictionary<int, DateTime> _heartbeats = new();
    private static readonly TimeSpan DefaultOnlineWindow = TimeSpan.FromMinutes(5);

    public void UpdateHeartbeat(int userId)
    {
        _heartbeats[userId] = DateTime.UtcNow;
    }

    public void RemoveUser(int userId)
    {
        _heartbeats.TryRemove(userId, out _);
    }

    public bool IsUserOnline(int userId, TimeSpan? onlineWindow = null)
    {
        if (!_heartbeats.TryGetValue(userId, out var last)) return false;
        var window = onlineWindow ?? DefaultOnlineWindow;
        return DateTime.UtcNow - last <= window;
    }

    public int GetOnlineCount(TimeSpan? onlineWindow = null)
    {
        var window = onlineWindow ?? DefaultOnlineWindow;
        var threshold = DateTime.UtcNow - window;
        return _heartbeats.Values.Count(t => t >= threshold);
    }

    public IReadOnlyCollection<int> GetOnlineUserIds(TimeSpan? onlineWindow = null)
    {
        var window = onlineWindow ?? DefaultOnlineWindow;
        var threshold = DateTime.UtcNow - window;
        return _heartbeats.Where(kv => kv.Value >= threshold).Select(kv => kv.Key).ToArray();
    }
}


