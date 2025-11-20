using MRP.Handlers;
using MRP.Server;
using MRP.Server.Ext;
using System.Net;
using System.Text.Json.Nodes;

namespace MRP.System;

public sealed class UserHandler : Handler, IHandler
{
    private static readonly Dictionary<string, User> _Users = new(); // In-Memory User Store. TODO: Replace with DB.

    public override void Handle(HttpRestEventArgs e)
    {
        if (e.Path.StartsWith("/users"))
        {
            switch (e.Path)
            {
                case "/users/register" when (e.Method == HttpMethod.Post):
                    try
                    {
                        string username = e.Content?["username"]?.GetValue<string>() ?? string.Empty;

                        lock (_Users)
                        {
                            // In-Memory User Store. TODO: Replace with DB Logic.
                            if (_Users.ContainsKey(username))
                            {
                                e.Respond(HttpStatusCode.Conflict,
                                    new JsonObject() { ["success"] = false, ["reason"] = "Username already exists." });
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"[{nameof(VersionHandler)} Username already exists. {e.Method.ToString()} {e.Path}.");
                            }
                            else
                            {
                                User user = new()
                                {
                                    UserName = e.Content?["username"]?.GetValue<string>() ?? string.Empty,
                                    FullName = e.Content?["fullname"]?.GetValue<string>() ?? string.Empty,
                                    EMail = e.Content?["email"]?.GetValue<string>() ?? string.Empty
                                };
                                user.SetPassword(e.Content?["password"]?.GetValue<string>() ?? string.Empty);

                                Session? session = Session.Create(
                                    e.Content["username"]?.GetValue<string>() ?? string.Empty,
                                    e.Content["password"]?.GetValue<string>() ?? string.Empty
                                );
                                if (session is not null)
                                {
                                    _Users.Add(username, user); // In-Memory User Store. TODO: Replace with DB Logic.

                                    e.Respond(HttpStatusCode.OK, new JsonObject()
                                    {
                                        ["success"] = true,
                                        ["token"] = session.Token,
                                        ["message"] = "User created."
                                    });
                                }

                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine($"[{nameof(VersionHandler)} Handled {e.Method.ToString()} {e.Path}.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        e.Respond(HttpStatusCode.InternalServerError, new JsonObject() { ["success"] = false, ["reason"] = ex.Message });
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[{nameof(VersionHandler)} Exception creating user. {e.Method.ToString()} {e.Path}: {ex.Message}");
                    }

                    break;

                case "/users/login" when (e.Method == HttpMethod.Post): //TODO: Implement
                    try
                    {
                        //TODO: Validate Credentials or Token

                        var username = e.Content["username"]?.GetValue<string>() ?? string.Empty;

                        Session? session = Session.Create(
                            username,
                            e.Content["password"]?.GetValue<string>() ?? string.Empty
                        );

                        if (session is not null)
                        {
                            e.Respond(HttpStatusCode.OK, new JsonObject()
                            {
                                ["success"] = true,
                                ["token"] = session.Token,
                                ["message"] = "hello " + username
                            });
                        }

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"[{nameof(VersionHandler)} Handled {e.Method.ToString()} {e.Path}.");
                    }
                    catch (Exception ex)
                    {
                        e.Respond(HttpStatusCode.InternalServerError, new JsonObject() { ["success"] = false, ["reason"] = ex.Message });
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[{nameof(VersionHandler)} Exception authenticating user. {e.Method.ToString()} {e.Path}: {ex.Message}");
                    }

                    break;

                default:

                    if (!e.VerifyAuthentication()) return;

                    if (e.Path.StartsWith("/users/"))
                    {
                        // Teile der URL: /users/john/profile -> ["users", "john", "profile"]
                        string[] parts = e.Path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length >= 2)
                        {
                            string username = parts[1];
                            string action = parts.Length > 2 ? parts[2] : ""; // profile, ratings, favorites

                            //TODO: Check if Username is valid

                            if (e.Method == HttpMethod.Get)
                            {
                                switch (action)
                                {
                                    case "profile":
                                        try
                                        {
                                            // TODO: Profil-Daten von 'username' aus DB holen
                                            e.Respond(HttpStatusCode.OK,
                                                new JsonObject()
                                                {
                                                    ["success"] = true,
                                                    ["username"] = username,
                                                    ["profile"] = "hier Profildaten"
                                                });

                                            Console.ForegroundColor = ConsoleColor.Blue;
                                            Console.WriteLine($"[{nameof(UserHandler)} Handled {e.Method} {e.Path}.");
                                        }
                                        catch (Exception ex)
                                        {
                                            e.Respond(HttpStatusCode.InternalServerError,
                                                new JsonObject() { ["success"] = false, ["reason"] = ex.Message });

                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine($"[{nameof(UserHandler)} Exception getting profile. {e.Method} {e.Path}: {ex.Message}");
                                        }

                                        break;

                                    case "ratings":
                                        try
                                        {
                                            // TODO: Ratings des Users zurückgeben
                                            e.Respond(HttpStatusCode.OK,
                                                new JsonObject()
                                                {
                                                    ["success"] = true,
                                                    ["ratings"] = "ratingsliste"
                                                });

                                            Console.ForegroundColor = ConsoleColor.Blue;
                                            Console.WriteLine($"[{nameof(UserHandler)} Handled {e.Method} {e.Path}.");
                                        }
                                        catch (Exception ex)
                                        {
                                            e.Respond(HttpStatusCode.InternalServerError,
                                                new JsonObject() { ["success"] = false, ["reason"] = ex.Message });

                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine($"[{nameof(UserHandler)} Exception getting ratings. {e.Method} {e.Path}: {ex.Message}");
                                        }

                                        break;

                                    case "favorites":
                                        try
                                        {
                                            // TODO: Favoriten des Users zurückgeben
                                            e.Respond(HttpStatusCode.OK,
                                                new JsonObject()
                                                {
                                                    ["success"] = true,
                                                    ["favorites"] = "favoritenliste"
                                                });

                                            Console.ForegroundColor = ConsoleColor.Blue;
                                            Console.WriteLine($"[{nameof(UserHandler)} Handled {e.Method} {e.Path}.");
                                        }
                                        catch (Exception ex)
                                        {
                                            e.Respond(HttpStatusCode.InternalServerError,
                                                new JsonObject() { ["success"] = false, ["reason"] = ex.Message });

                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine($"[{nameof(UserHandler)} Exception getting favorites. {e.Method} {e.Path}: {ex.Message}");
                                        }

                                        break;

                                    default:
                                        e.Respond(HttpStatusCode.NotFound,
                                            new JsonObject() { ["success"] = false, ["reason"] = "Unknown user endpoint." });

                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"[{nameof(UserHandler)} Unknown user endpoint {e.Method} {e.Path}.");
                                        break;
                                }
                            }

                            if (e.Method == HttpMethod.Put)
                            {
                                switch (action)
                                {
                                    case "profile":
                                        try
                                        {
                                            // TODO: Profil-Daten von 'username' in DB editieren
                                            e.Respond(HttpStatusCode.OK,
                                                new JsonObject()
                                                {
                                                    ["success"] = true,
                                                    ["username"] = username,
                                                    ["profile"] = "hier Profildaten"
                                                });

                                            Console.ForegroundColor = ConsoleColor.Blue;
                                            Console.WriteLine($"[{nameof(UserHandler)} Handled {e.Method} {e.Path}.");
                                        }
                                        catch (Exception ex)
                                        {
                                            e.Respond(HttpStatusCode.InternalServerError,
                                                new JsonObject() { ["success"] = false, ["reason"] = ex.Message });

                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine($"[{nameof(UserHandler)} Exception getting profile. {e.Method} {e.Path}: {ex.Message}");
                                        }

                                        break;

                                    case "ratings":
                                        try
                                        {
                                            // TODO: Ratings des Users bearbeiten
                                            e.Respond(HttpStatusCode.OK,
                                                new JsonObject()
                                                {
                                                    ["success"] = true,
                                                    ["ratings"] = "ratingsliste"
                                                });

                                            Console.ForegroundColor = ConsoleColor.Blue;
                                            Console.WriteLine($"[{nameof(UserHandler)} Handled {e.Method} {e.Path}.");
                                        }
                                        catch (Exception ex)
                                        {
                                            e.Respond(HttpStatusCode.InternalServerError,
                                                new JsonObject() { ["success"] = false, ["reason"] = ex.Message });

                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine($"[{nameof(UserHandler)} Exception getting ratings. {e.Method} {e.Path}: {ex.Message}");
                                        }

                                        break;

                                    default:
                                        e.Respond(HttpStatusCode.NotFound,
                                            new JsonObject() { ["success"] = false, ["reason"] = "Unknown user endpoint." });

                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"[{nameof(UserHandler)} Unknown user endpoint {e.Method} {e.Path}.");
                                        break;
                                }
                            }

                            break;
                        }
                    }

                    e.Respond(HttpStatusCode.BadRequest,
                        new JsonObject() { ["success"] = false, ["reason"] = "Invalid user endpoint." });

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{nameof(UserHandler)} Invalid user endpoint.");
                    break;

            }

            e.Responded = true;
        }
    }
}
