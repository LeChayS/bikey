using Microsoft.Extensions.Caching.Memory;

namespace bikey.Services;

public class OnlineUserService : IOnlineUserService
{
    private readonly IMemoryCache _cache;
    private const string OnlineUsersKey = "OnlineUsers";
    private readonly TimeSpan _onlineTimeout = TimeSpan.FromMinutes(30); // User được coi là online trong 30 phút sau lần cuối active

    public OnlineUserService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void UserLoggedIn(int userId)
    {
        var onlineUsers = GetOnlineUsersDictionary();
        onlineUsers[userId] = DateTime.UtcNow;
        _cache.Set(OnlineUsersKey, onlineUsers, _onlineTimeout);
    }

    public void UserLoggedOut(int userId)
    {
        var onlineUsers = GetOnlineUsersDictionary();
        onlineUsers.Remove(userId);
        _cache.Set(OnlineUsersKey, onlineUsers, _onlineTimeout);
    }

    public bool IsUserOnline(int userId)
    {
        var onlineUsers = GetOnlineUsersDictionary();
        if (onlineUsers.TryGetValue(userId, out var lastActivity))
        {
            // Kiểm tra xem user có active trong thời gian timeout không
            return (DateTime.UtcNow - lastActivity) < _onlineTimeout;
        }
        return false;
    }

    public IEnumerable<int> GetOnlineUserIds()
    {
        var onlineUsers = GetOnlineUsersDictionary();
        var currentTime = DateTime.UtcNow;

        // Lọc ra những users vẫn còn active
        var activeUsers = onlineUsers
            .Where(kvp => (currentTime - kvp.Value) < _onlineTimeout)
            .Select(kvp => kvp.Key)
            .ToList();

        // Cập nhật cache với danh sách active users
        var activeUsersDict = activeUsers.ToDictionary(id => id, id => onlineUsers[id]);
        _cache.Set(OnlineUsersKey, activeUsersDict, _onlineTimeout);

        return activeUsers;
    }

    private Dictionary<int, DateTime> GetOnlineUsersDictionary()
    {
        if (!_cache.TryGetValue(OnlineUsersKey, out Dictionary<int, DateTime> onlineUsers))
        {
            onlineUsers = new Dictionary<int, DateTime>();
        }
        return onlineUsers;
    }
}