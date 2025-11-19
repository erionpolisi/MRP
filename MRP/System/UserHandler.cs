using System.Net;
using System.Text.Json.Nodes;
using MRP.Handlers;
using MRP.Server;

namespace MRP.System;

public sealed class UserHandler : Handler, IHandler
{
    private static readonly Dictionary<string, User> _Users = new();

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

                                _Users.Add(username, user);

                                e.Respond(HttpStatusCode.OK, new JsonObject() { ["success"] = true, ["message"] = "User created." });

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
                case "/users/me" when (e.Method == HttpMethod.Get):
                    try
                    {
                        if (e.Session is null)
                        {
                            e.Respond(HttpStatusCode.Unauthorized,
                                new JsonObject() { ["success"] = false, ["reason"] = "Authentication required." });

                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"[{nameof(VersionHandler)} No user in session. {e.Method.ToString()} {e.Path}.");
                        }
                        else
                        {
                            string user = e.Session.UserName;

                            e.Respond(HttpStatusCode.OK,
                                new JsonObject() { ["success"] = true, ["username"] = user });

                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"[{nameof(VersionHandler)} Handled {e.Method.ToString()} {e.Path}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        e.Respond(HttpStatusCode.InternalServerError, new JsonObject() { ["success"] = false, ["reason"] = ex.Message });
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[{nameof(VersionHandler)} Exception getting user. {e.Method.ToString()} {e.Path}: {ex.Message}");
                    }

                    break;
                default:
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject() { ["success"] = false, ["reason"] = "Invalid user endpoint." });

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{nameof(VersionHandler)} Invalid user endpoint.");
                    break;
            }

            e.Responded = true;
        }
    }
}
