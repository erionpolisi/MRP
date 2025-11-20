using System.Net;
using System.Text.Json.Nodes;
using MRP.Handlers;
using MRP.Server;

namespace MRP.System;

public sealed class SessionHandler : Handler, IHandler
{
    public override void Handle(HttpRestEventArgs e)
    {
        if (e.Path.StartsWith("/sessions"))
        {
            switch (e.Path)
            {
                case "/sessions" when e.Method == HttpMethod.Post:
                    try
                    {
                        Session? session = Session.Create(
                            e.Content["username"]?.GetValue<string>() ?? string.Empty,
                            e.Content["password"]?.GetValue<string>() ?? string.Empty
                        );

                        if (session is null)
                        {
                            e.Respond(HttpStatusCode.Unauthorized, new JsonObject()
                            {
                                ["success"] = false,
                                ["reason"] = "Invalid username or password."
                            });

                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"[{nameof(SessionHandler)} Invalid login attempt. {e.Method} {e.Path}.");
                        }
                        else
                        {
                            e.Respond(HttpStatusCode.OK, new JsonObject()
                            {
                                ["success"] = true,
                                ["token"] = session.Token
                            });

                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"[{nameof(SessionHandler)} Handled {e.Method} {e.Path}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        e.Respond(HttpStatusCode.InternalServerError, new JsonObject()
                        {
                            ["success"] = false,
                            ["reason"] = ex.Message
                        });

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[{nameof(SessionHandler)} Exception creating session. {e.Method} {e.Path}: {ex.Message}");
                    }
                    break;

                default:
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject()
                    {
                        ["success"] = false,
                        ["reason"] = "Invalid session endpoint."
                    });

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{nameof(SessionHandler)} Invalid session endpoint.");
                    break;
            }

            e.Responded = true;
        }
    }
}
