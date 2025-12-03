using MRP.Handlers;
using MRP.Server;
using MRP.Server.Ext;
using System.Net;
using System.Text.Json.Nodes;

namespace MRP.System;

public sealed class UserHandler : Handler, IHandler
{
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

        string userId = parts[1];
        string action = parts[2];

        switch (action)
        {
            case "profile":
                HandleProtectedUserAction(
                    e,
                    userId,
                    "You are not allowed to access another user's profile.",
                    HandleUserProfile);
                break;

            case "ratings":
                HandleProtectedUserAction(
                    e,
                    userId,
                    "You are not allowed to access another user's ratings.",
                    HandleUserRatings);
                break;

            case "favorites":
                HandleProtectedUserAction(
                    e,
                    userId,
                    "You are not allowed to access another user's favorites.",
                    HandleUserFavorites);
                break;

            case "recommendations":
                HandleProtectedUserAction(
                    e,
                    userId,
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
            string username = e.Content?["username"]?.GetValue<string>() ?? "";
            string fullname = e.Content?["fullname"]?.GetValue<string>() ?? "";
            string email = e.Content?["email"]?.GetValue<string>() ?? "";
            string password = e.Content?["password"]?.GetValue<string>() ?? "";

            var (ok, message, user, session) = UserService.Register(username, fullname, email, password);

            if (!ok)
            {
                e.RespondConflict(message);
                return;
            }

            e.RespondCreated(new JsonObject
            {
                ["success"] = true,
                ["message"] = message,
                ["token"] = session?.Token ?? "",
                ["userId"] = user.Id
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
            string username = e.Content?["username"]?.GetValue<string>() ?? "";
            string password = e.Content?["password"]?.GetValue<string>() ?? "";

            var (ok, message, user, session) = UserService.Login(username, password);

            if (!ok)
            {
                e.RespondUnauthorized();
                return;
            }

            e.RespondOk(new JsonObject
            {
                ["success"] = true,
                ["message"] = "hello " + username,
                ["token"] = session!.Token,
                ["userId"] = user.Id

            });
        }
        catch (Exception ex)
        {
            e.RespondInternalServerError(ex);
        }
    }

    private void HandleProtectedUserAction(
        HttpRestEventArgs e,
        string userId,
        string errorMessage,
        Action<HttpRestEventArgs, string> handler)
    {
        if (!e.EnsureAccess(userId, errorMessage, out var guid)) return;

        handler(e, userId);
    }

    private void HandleUserRecommendations(HttpRestEventArgs e, string userId)
    {
        // Query-Parameter genre: /users/{username}/recommendations?type=genre
        // Query-Parameter content: /users/{username}/recommendations?type=content

        e.Query.TryGetValue("type", out var type);
        type = string.IsNullOrWhiteSpace(type) ? "genre" : type.ToLowerInvariant();

        if (type != "genre" && type != "content")
        {
            e.RespondBadRequest("Invalid recommendation type. Use 'genre' or 'content'.");
            return;
        }

        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["type"] = type,
            ["userId"] = userId,
            ["recommendations"] = $"recommendations based on {type}"
        });
    }

    private void HandleUserProfile(HttpRestEventArgs e, string userId)
    {
        if (e.Method == HttpMethod.Get)
        {
            e.RespondOk(new JsonObject
            {
                ["success"] = true,
                ["userId"] = userId,
                ["profile"] = "here profile data"
            });
        }
        else if (e.Method == HttpMethod.Put)
        {
            e.RespondOk(new JsonObject
            {
                ["success"] = true,
                ["userId"] = userId,
                ["profile"] = "profile updated"
            });
        }
    }

    private void HandleUserRatings(HttpRestEventArgs e, string userId)
    {
        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["userId"] = userId,
            ["ratings"] = "ratings list"
        });
    }

    private void HandleUserFavorites(HttpRestEventArgs e, string userId)
    {
        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["userId"] = userId,
            ["favorites"] = "favorites "
        });
    }
}
