using System.Net;
using System.Text.Json.Nodes;
using MRP.Handlers;
using MRP.Server;

namespace MRP.System;

public sealed class SessionHandler : Handler, IHandler
{
    public override void Handle(HttpRestEventArgs e)
    {
        if (!e.Path.StartsWith("/sessions"))
            return;

        switch (e.Path)
        {
            case "/sessions" when e.Method == HttpMethod.Post:
                HandleCreateSession(e);
                break;

            default:
                RespondInvalidEndpoint(e);
                break;
        }

        e.Responded = true;
    }

    // ----------------------------------------------------------
    //               POST /sessions
    // ----------------------------------------------------------
    private void HandleCreateSession(HttpRestEventArgs e)
    {
        try
        {
            string username = e.Content?["username"]?.GetValue<string>() ?? "";
            string password = e.Content?["password"]?.GetValue<string>() ?? "";

            Session? session = Session.Create(username, password);

            if (session is null)
            {
                e.Respond(HttpStatusCode.Unauthorized, new JsonObject
                {
                    ["success"] = false,
                    ["reason"] = "Invalid username or password."
                });
                return;
            }

            e.Respond(HttpStatusCode.OK, new JsonObject
            {
                ["success"] = true,
                ["token"] = session.Token
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

    private void RespondInvalidEndpoint(HttpRestEventArgs e)
    {
        e.Respond(HttpStatusCode.BadRequest, new JsonObject
        {
            ["success"] = false,
            ["reason"] = "Invalid session endpoint."
        });
    }
}
