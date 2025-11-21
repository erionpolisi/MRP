using MRP.Handlers;
using MRP.Server;
using MRP.Server.Ext;
using System.Net;
using System.Text.Json.Nodes;

namespace MRP.System;

public sealed class UserHandler : Handler, IHandler
{
    public override void Handle(HttpRestEventArgs e)
    {
        if (!e.Path.StartsWith("/users"))
            return;

        switch (e.Path)
        {
            case "/users/register" when e.Method == HttpMethod.Post:
                HandleRegister(e);
                break;

            case "/users/login" when e.Method == HttpMethod.Post:
                HandleLogin(e);
                break;

            default:
                if (!e.VerifyAuthentication()) return;
                HandleUserSubRoutes(e);
                break;
        }

        e.Responded = true;
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
                e.Respond(HttpStatusCode.Conflict, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = message
                });
                return;
            }

            e.Respond(HttpStatusCode.Created, new JsonObject
            {
                ["success"] = true,
                ["message"] = message,
                ["token"] = session?.Token ?? "",
                ["userId"] = user.Id
            });
        }
        catch (Exception ex)
        {
            e.Respond(HttpStatusCode.InternalServerError, new JsonObject
            {
                ["success"] = false,
                ["reason"] = ex.Message
            });
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
                e.Respond(HttpStatusCode.Unauthorized, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = message
                });
                return;
            }

            e.Respond(HttpStatusCode.OK, new JsonObject
            {
                ["success"] = true,
                ["message"] = "hello " + username,
                ["token"] = session!.Token,
                ["userId"] = user.Id

            });
        }
        catch (Exception ex)
        {
            e.Respond(HttpStatusCode.InternalServerError, new JsonObject
            {
                ["success"] = false,
                ["reason"] = ex.Message
            });
        }
    }

    // ----------------------------------------------------------
    //     /users/{username}/profile|ratings|favorites
    // ----------------------------------------------------------
    private void HandleUserSubRoutes(HttpRestEventArgs e)
    {
        var parts = e.Path.Trim('/').Split('/');

        if (parts.Length < 3)
        {
            e.Respond(HttpStatusCode.BadRequest, new JsonObject
            {
                ["success"] = false,
                ["reason"] = "Invalid user endpoint."
            });
            return;
        }

        string userId = parts[1];
        string action = parts[2];

        switch (action)
        {
            case "profile":
                HandleUserProfile(e, userId);
                break;

            case "ratings":
                HandleUserRatings(e, userId);
                break;

            case "favorites":
                HandleUserFavorites(e, userId);
                break;
            case "recommendations":
                HandleUserRecommendations(e, userId);
                break;

            default:
                e.Respond(HttpStatusCode.NotFound, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Unknown user endpoint."
                });
                break;
        }
    }

    private void HandleUserRecommendations(HttpRestEventArgs e, string userId)
    {
        // Query-Parameter lesen: /users/{username}/recommendations?type=genre

        e.Query.TryGetValue("type", out var type);
        type = string.IsNullOrWhiteSpace(type) ? "genre" : type.ToLowerInvariant();

        if (type != "genre" && type != "content")
        {
            e.Respond(HttpStatusCode.BadRequest, new JsonObject
            {
                ["success"] = false,
                ["reason"] = "Invalid recommendation type. Use 'genre' or 'content'."
            });
            return;
        }

        e.Respond(HttpStatusCode.OK, new JsonObject
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
            e.Respond(HttpStatusCode.OK, new JsonObject
            {
                ["success"] = true,
                ["userId"] = userId,
                ["profile"] = "here profile data"
            });
        }
        else if (e.Method == HttpMethod.Put)
        {
            e.Respond(HttpStatusCode.Accepted, new JsonObject
            {
                ["success"] = true,
                ["userId"] = userId,
                ["profile"] = "profile updated"
            });
        }
    }

    private void HandleUserRatings(HttpRestEventArgs e, string userId)
    {
        e.Respond(HttpStatusCode.OK, new JsonObject
        {
            ["success"] = true,
            ["userId"] = userId,
            ["ratings"] = "ratings list"
        });
    }

    private void HandleUserFavorites(HttpRestEventArgs e, string userId)
    {
        e.Respond(HttpStatusCode.OK, new JsonObject
        {
            ["success"] = true,
            ["userId"] = userId,
            ["favorites"] = "favorites list"
        });
    }
}
