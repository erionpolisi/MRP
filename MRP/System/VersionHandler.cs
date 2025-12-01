using System.Net;
using System.Reflection;
using System.Text.Json.Nodes;

using MRP.Handlers;
using MRP.Server;
using MRP.Server.Ext;

namespace MRP.System;

public sealed class VersionHandler : Handler, IHandler
{
    public override void Handle(HttpRestEventArgs e)
    {
        if (e.Path.StartsWith("/version"))
        {
            e.SetCurrentHandler(nameof(VersionHandler));
            switch (e.Path)
            {
                case "/version" when e.Method == HttpMethod.Get:
                    GetVersion(e);
                    break;

                default:
                    e.RespondInvalidEndpoint();
                    break;
            }

            e.Responded = true;
        }
    }

    private static void GetVersion(HttpRestEventArgs e)
    {
        try
        {
            e.RespondOk(new JsonObject()
            {
                ["success"] = true,
                ["name"] = Assembly.GetExecutingAssembly().GetName().Name,
                ["version"] = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown"
            });

        }
        catch (Exception ex)
        {
            e.RespondInternalServerError(ex);
            e.ConsoleResponse(false, "Exception handling version.", ex);
        }
    }
}
