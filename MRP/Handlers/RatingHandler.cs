using System.Text.Json.Nodes;
using MRP.Server;
using MRP.Server.Ext;

namespace MRP.Handlers;

public sealed class RatingHandler : Handler, IHandler
{
    private const int MinimumPathParts = 2;

    public override void Handle(HttpRestEventArgs e)
    {
        if (!e.Path.StartsWith("/ratings"))
            return;

        if (!e.VerifySession())
            return;

        e.SetCurrentHandler(nameof(RatingHandler));

        var parts = e.Path.Trim('/').Split('/');

        if (parts.Length < MinimumPathParts)
        {
            e.RespondInvalidEndpoint();
            e.Responded = true;
            return;
        }

        string idPart = parts[1];

        if (!Guid.TryParse(idPart, out var ratingId))
        {
            e.RespondBadRequest("Invalid ratingId format.");
            e.Responded = true;
            return;
        }

        switch (parts.Length)
        {
            case 2:
                HandleBasicRatingRoutes(e, ratingId);
                break;
            case 3:
                HandleSubRoutes(e, ratingId, parts[2]);
                break;
            default:
                e.RespondInvalidEndpoint();
                break;
        }

        e.Responded = true;
    }

    // ----------------------------------------------------------
    //         /ratings/{id}  GET | PUT | DELETE
    // ----------------------------------------------------------
    private void HandleBasicRatingRoutes(HttpRestEventArgs e, Guid ratingId)
    {
        switch (e.Method.Method)
        {
            case "GET":
                HandleGetRating(e, ratingId);
                break;

            case "PUT":
                HandleUpdateRating(e, ratingId);
                break;

            case "DELETE":
                HandleDeleteRating(e, ratingId);
                break;

            default:
                e.RespondInvalidEndpoint();
                break;
        }
    }

    // ----------------------------------------------------------
    //         /ratings/{id}/like  POST
    //         /ratings/{id}/confirm POST
    // ----------------------------------------------------------
    private void HandleSubRoutes(HttpRestEventArgs e, Guid ratingId, string action)
    {
        switch (action)
        {
            case "like" when e.Method == HttpMethod.Post:
                HandleLikeRating(e, ratingId);
                break;

            case "confirm" when e.Method == HttpMethod.Post:
                HandleConfirmRating(e, ratingId);
                break;

            default:
                e.RespondInvalidEndpoint();
                break;
        }
    }

    private void HandleGetRating(HttpRestEventArgs e, Guid ratingId)
    {
        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["ratingId"] = ratingId.ToString(),
            ["message"] = "Rating details (placeholder)."
        });
    }

    private void HandleUpdateRating(HttpRestEventArgs e, Guid ratingId)
    {
        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["ratingId"] = ratingId.ToString(),
            ["message"] = "Rating updated (placeholder)."
        });
    }

    private void HandleDeleteRating(HttpRestEventArgs e, Guid ratingId)
    {
        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["ratingId"] = ratingId.ToString(),
            ["message"] = "Rating deleted (placeholder)."
        });
    }

    private void HandleLikeRating(HttpRestEventArgs e, Guid ratingId)
    {
        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["ratingId"] = ratingId.ToString(),
            ["message"] = "Rating liked (placeholder)."
        });
    }

    private void HandleConfirmRating(HttpRestEventArgs e, Guid ratingId)
    {
        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["ratingId"] = ratingId.ToString(),
            ["message"] = "Rating confirmed (placeholder)."
        });
    }
}
