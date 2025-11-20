using System.Net;
using System.Reflection;
using System.Text.Json.Nodes;

using MRP.Handlers;
using MRP.Server;

namespace MRP.System;

public sealed class VersionHandler : Handler, IHandler
{
    public override void Handle(HttpRestEventArgs e)
    {
        if (e.Path.StartsWith("/version"))
        {
            switch (e.Path)
            {
                case "/version" when e.Method == HttpMethod.Get:
                    try
                    {
                        e.Respond(HttpStatusCode.OK, new JsonObject()
                        {
                            ["success"] = true,
                            ["version"] = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown"
                        });

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"[{nameof(VersionHandler)} Handled {e.Method} {e.Path}.");
                    }
                    catch (Exception ex)
                    {
                        e.Respond(HttpStatusCode.InternalServerError, new JsonObject()
                        {
                            ["success"] = false,
                            ["reason"] = ex.Message
                        });

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[{nameof(VersionHandler)} Exception handling version. {e.Method} {e.Path}: {ex.Message}");
                    }
                    break;

                default:
                    e.Respond(HttpStatusCode.BadRequest, new JsonObject()
                    {
                        ["success"] = false,
                        ["reason"] = "Invalid version endpoint."
                    });

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{nameof(VersionHandler)} Invalid version endpoint.");
                    break;
            }

            e.Responded = true;
        }
    }
}
