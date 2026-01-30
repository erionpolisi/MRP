using System.Text.Json.Nodes;
using MRP.Server;
using MRP.Server.Ext;
using MRP.Repositories;

namespace MRP.Handlers;

public sealed class LeaderboardHandler : Handler, IHandler
{
    private readonly UserRepository _repo = new();

    public override void Handle(HttpRestEventArgs e)
    {
        if (!e.Path.StartsWith("/leaderboard"))
            return;

        if (!e.VerifySession())
            return;

        e.SetCurrentHandler(nameof(LeaderboardHandler));

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

    private void HandleLeaderboard(HttpRestEventArgs e)
    {
        var list = _repo.GetLeaderboard()
            .Select(x => new JsonObject
            {
                ["username"] = x.UserName,
                ["ratingCount"] = x.RatingCount
            })
            .ToArray();

        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["leaderboard"] = new JsonArray(list)
        });
    }

}