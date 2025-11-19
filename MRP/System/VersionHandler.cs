using System.Net;
using System.Reflection;
using System.Text.Json.Nodes;

using MRP.Handlers;
using MRP.Server;

namespace MRP.System;

public sealed class VersionHandler: Handler, IHandler
{

    public override void Handle(HttpRestEventArgs e)
    {
        if(e.Path.StartsWith("/version"))
        {
            if((e.Path == "/version") && (e.Method == HttpMethod.Get))
            {
                e.Respond(HttpStatusCode.OK, new JsonObject() 
                          { ["success"] = true, ["version"] = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown" });

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"[{nameof(VersionHandler)} Handled {e.Method.ToString()} {e.Path}.");
            }
            else
            {
                e.Respond(HttpStatusCode.BadRequest, new JsonObject(){ ["success"] = false, ["reason"] = "Invalid version endpoint." });

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{nameof(VersionHandler)} Invalid session endpoint.");
            }

            e.Responded = true;
        }
    }
}
