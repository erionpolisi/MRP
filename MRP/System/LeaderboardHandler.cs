using MRP.Handlers;
using MRP.Server;
using MRP.Server.Ext;
using System.Text.Json.Nodes;

namespace MRP.System;

public sealed class LeaderboardHandler : Handler, IHandler
{
    public override void Handle(HttpRestEventArgs e)
    {
        if (!e.Path.StartsWith("/leaderboard"))
            return;

        if (!e.VerifySession())
            return;

        e.SetCurrentHandler(nameof(LeaderboardHandler));

        // Only one endpoint needed: GET /leaderboard
        if (e.Path == "/leaderboard" && e.Method == HttpMethod.Get)
        {
            HandleLeaderboard(e);
        }
        else
        {
            e.RespondInvalidEndpoint();
        }

        e.Responded = true;
    }

    // ----------------------------------------------------------
    //                  GET /leaderboard
    // ----------------------------------------------------------
    private void HandleLeaderboard(HttpRestEventArgs e)
    {
        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["leaderboard"] = "Leaderboard data (placeholder)."
        });
    }
}