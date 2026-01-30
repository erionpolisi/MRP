using MRP.Server;
using MRP.Server.Ext;
using MRP.System;
using System.Net;
using System.Text.Json.Nodes;
using MRP.Repositories;

namespace MRP.Handlers;

public sealed class UserHandler : Handler, IHandler
{
    private static readonly MediaEntryRepository _mediaRepo = new();

    private const int MinimumPathPartsWithId = 3;

    public override void Handle(HttpRestEventArgs e)
    {
        if (!e.Path.StartsWith("/users"))
            return;

        e.SetCurrentHandler(nameof(UserHandler));

        switch (e.Path)
        {
            case "/users/register" when e.Method == HttpMethod.Post:
                HandleRegister(e);
                break;

            case "/users/login" when e.Method == HttpMethod.Post:
                HandleLogin(e);
                break;

            default:
                if (!e.VerifySession()) return;
                HandleUserSubRoutes(e);
                break;
        }

        e.Responded = true;
    }

    // ----------------------------------------------------------
    //     /users/{username}/profile|ratings|favorites
    // ----------------------------------------------------------
    private void HandleUserSubRoutes(HttpRestEventArgs e)
    {
        var parts = e.Path.Trim('/').Split('/');

        if (parts.Length < MinimumPathPartsWithId)
        {
            e.RespondInvalidEndpoint();
            return;
        }

        string username = parts[1];
        string action = parts[2];

        switch (action)
        {
            case "profile":
                HandleProtectedUserAction(
                    e,
                    username,
                    "You are not allowed to access another user's profile.",
                    HandleUserProfile);
                break;

            case "ratings":
                HandleProtectedUserAction(
                    e,
                    username,
                    "You are not allowed to access another user's ratings.",
                    HandleUserRatings);
                break;

            case "favorites":
                HandleProtectedUserAction(
                    e,
                    username,
                    "You are not allowed to access another user's favorites.",
                    HandleUserFavorites);
                break;

            case "recommendations":
                HandleProtectedUserAction(
                    e,
                    username,
                    "You are not allowed to access another user's recommendations.",
                    HandleUserRecommendations);
                break;

            default:
                e.RespondInvalidEndpoint();
                break;
        }
    }

    // ----------------------------------------------------------
    //               /users/register
    // ----------------------------------------------------------
    private void HandleRegister(HttpRestEventArgs e)
    {
        try
        {
            string username = e.Content["username"]?.GetValue<string>() ?? "";
            string fullname = e.Content["fullname"]?.GetValue<string>() ?? "";
            string email = e.Content["email"]?.GetValue<string>() ?? "";
            string password = e.Content["password"]?.GetValue<string>() ?? "";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                e.RespondBadRequest("Username and password are required.");
                return;
            }

            if (User.Get(username) != null)
            {
                e.RespondConflict("Username already exists.");
                return;
            }

            var user = new User()
            {
                UserName = username,
                FullName = fullname,
                EMail = email
            };

            user.SetPassword(password);
            user.Save();

            var session = Session.Create(user);

            e.RespondCreated(new JsonObject
            {
                ["success"] = true,
                ["token"] = session.Token,
                ["userName"] = username
            });
        }
        catch (Exception ex)
        {
            e.RespondInternalServerError(ex);
        }
    }


    // ----------------------------------------------------------
    //               /users/login
    // ----------------------------------------------------------
    private void HandleLogin(HttpRestEventArgs e)
    {
        try
        {
            string username = e.Content["username"]?.GetValue<string>() ?? "";
            string password = e.Content["password"]?.GetValue<string>() ?? "";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                e.RespondBadRequest("Username and password are required.");
                return;
            }

            var user = User.Get(username);
            if (user == null)
            {
                e.RespondUnauthorized();
                return;
            }

            var hash = User._HashPassword(username, password);
            if (hash != ((__IAuthentificable)user).__PasswordHash)
            {
                e.RespondUnauthorized();
                return;
            }

            var session = Session.Create(user);

            e.RespondOk(new JsonObject
            {
                ["success"] = true,
                ["message"] = "hello " + user.UserName,
                ["token"] = session.Token,
                ["userName"] = user.UserName
            });
        }
        catch (Exception ex)
        {
            e.RespondInternalServerError(ex);
        }
    }


    private void HandleProtectedUserAction(
        HttpRestEventArgs e,
        string username,
        string errorMessage,
        Action<HttpRestEventArgs, string> handler)
    {
        if (!e.EnsureAccess(username, errorMessage)) return;

        handler(e, username);
    }

    private void HandleUserRecommendations(HttpRestEventArgs e, string username)
    {
        if (e.Method != HttpMethod.Get)
        {
            e.RespondMethodNotAllowed();
            return;
        }

        e.Query.TryGetValue("type", out var type);
        type = string.IsNullOrWhiteSpace(type) ? "genre" : type.ToLowerInvariant();

        IEnumerable<MediaEntry> recs = type switch
        {
            "content" => _mediaRepo.RecommendByContent(username),
            _ => _mediaRepo.RecommendByGenre(username)
        };

        var json = recs.Select(m => new JsonObject
        {
            ["id"] = m.Id.ToString(),
            ["title"] = m.Title,
            ["type"] = m.Type.ToString(),
            ["avgScore"] = m.AverageScore
        }).ToArray();

        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["recommendationType"] = type,
            ["results"] = new JsonArray(json)
        });
    }

    private void HandleUserProfile(HttpRestEventArgs e, string username)
    {
        if (e.Method != HttpMethod.Get && e.Method != HttpMethod.Put)
        {
            e.RespondMethodNotAllowed();
            return;
        }

        var user = User.Get(username);
        if (user == null)
        {
            e.RespondNotFound("User not found.");
            return;
        }

        if (e.Method == HttpMethod.Get)
        {
            e.RespondOk(new JsonObject
            {
                ["success"] = true,
                ["username"] = user.UserName,
                ["fullname"] = user.FullName,
                ["email"] = user.EMail,
                ["isAdmin"] = user.IsAdmin
            });
            return;
        }

        if (e.Method == HttpMethod.Put)
        {
            user.FullName = e.Content["fullname"]?.GetValue<string>() ?? user.FullName;
            user.EMail = e.Content["email"]?.GetValue<string>() ?? user.EMail;

            user.Save();

            e.RespondOk(new JsonObject
            {
                ["success"] = true,
                ["message"] = "Profile updated."
            });
            return;
        }
    }


    private void HandleUserRatings(HttpRestEventArgs e, string username)
    {
        if (e.Method != HttpMethod.Get)
        {
            e.RespondMethodNotAllowed();
            return;
        }

        var ratings = Rating.ForUser(username);

        var json = new JsonArray(
            ratings.Select(r => new JsonObject
            {
                ["id"] = r.Id.ToString(),
                ["mediaId"] = r.MediaId.ToString(),
                ["stars"] = r.Stars,
                ["comment"] = r.IsConfirmed ? r.Comment : null,
                ["createdAt"] = r.CreatedAt
            }).ToArray<JsonNode?>()
        );

        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["ratings"] = json
        });
    }

    private void HandleUserFavorites(HttpRestEventArgs e, string username)
    {
        if (e.Method != HttpMethod.Get)
        {
            e.RespondMethodNotAllowed();
            return;
        }

        var favorites = MediaFavorite.ForUser(username);

        var json = new JsonArray(
            favorites.Select(f => new JsonObject
            {
                ["mediaId"] = f.MediaId.ToString(),
                ["title"] = f.Media.Title,
                ["type"] = f.Media.Type.ToString()
            }).ToArray()
        );

        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["favorites"] = json
        });
    }

}
