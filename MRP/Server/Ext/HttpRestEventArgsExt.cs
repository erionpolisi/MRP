using System.Net;
using System.Text.Json.Nodes;
using MRP.System;

namespace MRP.Server.Ext
{
    public static class HttpRestEventArgsExt
    {
        public static bool VerifyAuthentication(this HttpRestEventArgs e)
        {
            if (e.Session is not null) return true;

            e.Respond(HttpStatusCode.Unauthorized,
                new JsonObject() { ["success"] = false, ["reason"] = "Authentication required." });

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{nameof(UserHandler)} No valid session. {e.Method} {e.Path}.");
            return false;
        }
    }
}