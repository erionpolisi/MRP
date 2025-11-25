namespace MRP.System;

public static class UserService
{
    public static (bool ok, string message, User user, Session? session) Register(
        string username, string fullname, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return (false, "Username is required.", null, null)!;

        if (UserRepository.Exists(username)) // simulate checking in database
            return (false, "Username already exists.", null, null)!;

        var user = new User
        {
            UserName = username,
            FullName = fullname,
            EMail = email
        };

        user.SetPassword(password);
        UserRepository.Add(user); // simulate saving to database

        Session? session = Session.Create(user, password);

        return (true, "User registered.", user, session);
    }

    public static (bool ok, string message, User user, Session? session) Login(
        string username, string password)
    {
        var user = UserRepository.Get(username);// simulate fetching from database

        if (user is null)
            return (false, "Invalid username or password.", null, null)!;

        Session? session = Session.Create(user, password);

        if (session is null)
            return (false, "Invalid username or password.", null, null)!;

        return (true, "Login OK.", user, session);
    }

}

// ----------------------------------------------------------
//         will be replaced by database access later:
// ----------------------------------------------------------

public static class UserRepository
{
    private static readonly Dictionary<string, User> _users = new();

    public static bool Exists(string username)
        => _users.ContainsKey(username);

    public static void Add(User user)
        => _users[user.UserName] = user;

    public static User? Get(string username)
        => _users.TryGetValue(username, out var u) ? u : null;
}

