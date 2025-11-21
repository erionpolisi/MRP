namespace MRP.System;

public sealed class Session
{
    private const string _ALPHABET = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int TIMEOUT_MINUTES = 30;

    private static readonly Dictionary<string, Session> _Sessions = new();

    private Session(User user, string password)
    {
        UserId = user.Id;
        UserName = user.UserName;
        IsAdmin = (UserName == "admin");
        Timestamp = DateTime.UtcNow;

        Token = string.Empty;
        Random rnd = new();
        for(int i = 0; i < 24; i++) { Token += _ALPHABET[rnd.Next(0, 62)]; }
    }

    public string Token { get; }
    public string UserName { get; }
    public Guid UserId { get; }

    public DateTime Timestamp
    {
        get; private set;
    }

    public bool Valid
    {
        get
        {
            lock (_Sessions)
            {
                return _Sessions.ContainsKey(Token);
            }
        }
    }

    public bool IsAdmin { get; }

    public static Session? Create(User user, string password)
    {

        Session s = new(user, password);

        lock (_Sessions)
        {
            _Sessions[s.Token] = s;   // Session in der Liste speichern
        }

        return s;
    }


    public static Session? Get(string token)
    {
        Session? rval = null;

        _Cleanup();

        lock(_Sessions)
        {
            if(_Sessions.TryGetValue(token, out var session))
            {
                rval = session;
                rval.Timestamp = DateTime.UtcNow;
            }
        }

        return rval;
    }

    private static void _Cleanup()
    {
        List<string> toRemove = new();

        lock(_Sessions)
        {
            foreach(KeyValuePair<string, Session> pair in _Sessions)
            {
                if((DateTime.UtcNow - pair.Value.Timestamp).TotalMinutes > TIMEOUT_MINUTES) { toRemove.Add(pair.Key); }
            }
            foreach(string key in toRemove) { _Sessions.Remove(key); }
        }
    }

    public void Close()
    {
        lock(_Sessions)
        {
            _Sessions.Remove(Token);
        }
    }
}
