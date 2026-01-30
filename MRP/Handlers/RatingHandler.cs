using MRP.Repositories;
using MRP.Server;
using MRP.Server.Ext;
using MRP.System;
using System.Text.Json.Nodes;

namespace MRP.Handlers;

public sealed class RatingHandler : Handler, IHandler
{
    private static readonly RatingRepository _ratingRepo = new();
    private static readonly RatingLikeRepository _likeRepo = new();

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

        if (!Guid.TryParse(parts[1], out var ratingId))
        {
            e.RespondBadRequest("Invalid ratingId format.");
            e.Responded = true;
            return;
        }

        switch (parts.Length)
        {
            case 2:
                HandleBasicRoutes(e, ratingId);
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
    // /ratings/{id}   GET | PUT | DELETE
    // ----------------------------------------------------------
    private void HandleBasicRoutes(HttpRestEventArgs e, Guid ratingId)
    {
        switch (e.Method.Method)
        {
            case "GET":
                HandleGet(e, ratingId);
                break;

            case "PUT":
                HandleUpdate(e, ratingId);
                break;

            case "DELETE":
                HandleDelete(e, ratingId);
                break;

            default:
                e.RespondMethodNotAllowed();
                break;
        }
    }

    // ----------------------------------------------------------
    // /ratings/{id}/like     POST | DELETE
    // /ratings/{id}/confirm  POST
    // ----------------------------------------------------------
    private void HandleSubRoutes(HttpRestEventArgs e, Guid ratingId, string action)
    {
        switch (action)
        {
            case "like" when e.Method == HttpMethod.Post:
                HandleLike(e, ratingId);
                break;

            case "like" when e.Method == HttpMethod.Delete:
                HandleUnlike(e, ratingId);
                break;

            case "confirm" when e.Method == HttpMethod.Post:
                HandleConfirm(e, ratingId);
                break;

            default:
                e.RespondInvalidEndpoint();
                break;
        }
    }

    // ----------------------------------------------------------
    // GET /ratings/{id}
    // ----------------------------------------------------------
    private void HandleGet(HttpRestEventArgs e, Guid ratingId)
    {
        var rating = _ratingRepo.Get(ratingId);
        if (rating == null)
        {
            e.RespondNotFound("Rating not found.");
            return;
        }

        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["id"] = rating.Id.ToString(),
            ["mediaId"] = rating.MediaId.ToString(),
            ["username"] = rating.UserName,
            ["stars"] = rating.Stars,
            ["comment"] = rating.IsConfirmed ? rating.Comment : null,
            ["confirmed"] = rating.IsConfirmed,
            ["createdAt"] = rating.CreatedAt
        });
    }

    // ----------------------------------------------------------
    // PUT /ratings/{id}
    // ----------------------------------------------------------
    private void HandleUpdate(HttpRestEventArgs e, Guid ratingId)
    {
        var rating = _ratingRepo.Get(ratingId, e.Session);
        if (rating == null)
        {
            e.RespondNotFound("Rating not found.");
            return;
        }

        try
        {
            rating.BeginEdit(e.Session!);

            int stars = e.Content["stars"]?.GetValue<int>() ?? rating.Stars;
            string? comment = e.Content["comment"]?.GetValue<string>();

            rating.SetRating(stars, comment);
            rating.Save();

            e.RespondOk(new JsonObject
            {
                ["success"] = true,
                ["message"] = "Rating updated."
            });
        }
        catch (UnauthorizedAccessException)
        {
            e.RespondForbidden("You are not allowed to edit this rating.");
        }
        catch (Exception ex)
        {
            e.RespondInternalServerError(ex);
        }
    }

    // ----------------------------------------------------------
    // DELETE /ratings/{id}
    // ----------------------------------------------------------
    private void HandleDelete(HttpRestEventArgs e, Guid ratingId)
    {
        var rating = _ratingRepo.Get(ratingId, e.Session);
        if (rating == null)
        {
            e.RespondNotFound("Rating not found.");
            return;
        }

        try
        {
            rating.BeginEdit(e.Session!);
            rating.Delete();
            e.RespondNoContent();
        }
        catch (UnauthorizedAccessException)
        {
            e.RespondForbidden("You are not allowed to delete this rating.");
        }
        catch (Exception ex)
        {
            e.RespondInternalServerError(ex);
        }
    }

    // ----------------------------------------------------------
    // POST /ratings/{id}/like
    // ----------------------------------------------------------
    private void HandleLike(HttpRestEventArgs e, Guid ratingId)
    {
        var existing = _likeRepo.Get((ratingId, e.Session!.UserName));
        if (existing != null)
        {
            e.RespondConflict("You already liked this rating.");
            return;
        }

        var like = new RatingLike(e.Session!, ratingId);
        _likeRepo.Save(like);

        e.RespondCreated(new JsonObject
        {
            ["success"] = true,
            ["message"] = "Rating liked."
        });
    }

    // ----------------------------------------------------------
    // DELETE /ratings/{id}/like
    // ----------------------------------------------------------
    private void HandleUnlike(HttpRestEventArgs e, Guid ratingId)
    {
        var like = _likeRepo.Get((ratingId, e.Session!.UserName));
        if (like == null)
        {
            e.RespondNotFound("Like not found.");
            return;
        }

        _likeRepo.Delete(like);
        e.RespondNoContent();
    }

    // ----------------------------------------------------------
    // POST /ratings/{id}/confirm
    // ----------------------------------------------------------
    private void HandleConfirm(HttpRestEventArgs e, Guid ratingId)
    {
        var rating = _ratingRepo.Get(ratingId, e.Session);
        if (rating == null)
        {
            e.RespondNotFound("Rating not found.");
            return;
        }

        try
        {
            rating.BeginEdit(e.Session!);
            rating.Confirm();
            rating.Save();

            e.RespondOk(new JsonObject
            {
                ["success"] = true,
                ["message"] = "Rating confirmed."
            });
        }
        catch (UnauthorizedAccessException)
        {
            e.RespondForbidden("You are not allowed to confirm this rating.");
        }
        catch (Exception ex)
        {
            e.RespondInternalServerError(ex);
        }
    }
}
