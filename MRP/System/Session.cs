using MRP.System;

public sealed class Session
{
    private const int TIMEOUT_MINUTES = 30;
    private static readonly Dictionary<string, Session> _Sessions = new();

    private Session(User user)
    {
        UserId = user.Id;
        UserName = user.UserName;
        IsAdmin = user.IsAdmin;
        Timestamp = DateTime.UtcNow;
        Token = Guid.NewGuid().ToString();

        lock (_Sessions)
        {
            _Sessions[Token] = this;
        }
    }

    public string Token { get; }
    public Guid UserId { get; }
    public string UserName { get; }
    public DateTime Timestamp { get; private set; }
    public bool IsAdmin { get; }

    public bool CanAccessUser(string otherUserName)
    {
        return IsAdmin || UserName == otherUserName;
    }

    public static Session Create(User user)
    {
        return new Session(user);
    }

    public static Session? Get(string token)
    {
        Cleanup();

        lock (_Sessions)
        {
            if (_Sessions.TryGetValue(token, out var s))
            {
                s.Timestamp = DateTime.UtcNow;
                return s;
            }
        }
        return null;
    }

    private static void Cleanup()
    {
        var expired = _Sessions
            .Where(p => (DateTime.UtcNow - p.Value.Timestamp).TotalMinutes > TIMEOUT_MINUTES)
            .Select(p => p.Key)
            .ToList();

        foreach (var key in expired)
            _Sessions.Remove(key);
    }
}