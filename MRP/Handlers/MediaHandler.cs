using MRP.Server;
using MRP.Server.Ext;
using System.Text.Json.Nodes;
using MRP.Repositories;
using MRP.System;

namespace MRP.Handlers;

public sealed class MediaHandler : Handler, IHandler
{
    private static readonly MediaEntryRepository _mediaRepo = new();

    public override void Handle(HttpRestEventArgs e)
    {
        if (!e.Path.StartsWith("/media"))
            return;

        if (!e.VerifySession())
            return;

        e.SetCurrentHandler(nameof(MediaHandler));

        if (e.Path == "/media" && e.Method == HttpMethod.Post)
        {
            HandleCreate(e);
        }
        else if (e.Path == "/media" && e.Method == HttpMethod.Get)
        {
            HandleList(e);
        }
        else if (e.Path.StartsWith("/media/"))
        {
            HandleMediaById(e);
        }
        else
        {
            e.RespondInvalidEndpoint();
        }

        e.Responded = true;
    }

    private void HandleMediaById(HttpRestEventArgs e)
    {
        var parts = e.Path.Trim('/').Split('/');

        switch (parts.Length)
        {
            case 1:
                e.RespondInvalidEndpoint();
                break;
            case 2:
                HandleBasicRoute(e, parts); // GET / PUT / DELETE — /media/{id}
                break;
            case 3:
                HandleSubRoutes(e, parts); // /media/{id}/rate or /media/{id}/favorite
                break;
            default:
                e.RespondInvalidEndpoint();
                break;
        }
    }
    private void HandleBasicRoute(HttpRestEventArgs e, string[] parts)
    {
        var idPart = parts[1];

        var media = GetMedia(e, idPart, out var id);

        if (media is null)
            return;

        switch (e.Method.Method)
        {
            case "GET":
                HandleGet(e, media);
                break;
            case "PUT":
                HandleUpdate(e, media);
                break;
            case "DELETE":
                HandleDelete(e, id);
                break;
            default:
                e.RespondInvalidEndpoint();
                break;
        }
    }

    private void HandleSubRoutes(HttpRestEventArgs e, string[] parts)
    {
        var idPart = parts[1];
        var action = parts[2];

        var media = GetMedia(e, idPart, out var id);
        if (media is null)
            return;

        switch (action)
        {
            case "rate" when e.Method == HttpMethod.Post:
                HandleRateMedia(e, media);
                break;
            case "ratings" when e.Method == HttpMethod.Get:
                HandleRatingsMedia(e, media);
                break;
            case "favorite" when e.Method == HttpMethod.Post || e.Method == HttpMethod.Delete:
                HandleFavoriseMedia(e, media);
                break;
            default:
                e.RespondInvalidEndpoint();
                break;
        }
    }

    // ----------------------------------------------------------
    //                  POST /media
    // ----------------------------------------------------------
    private void HandleCreate(HttpRestEventArgs e)
    {
        try
        {
            if (e.Session == null)
            {
                e.RespondUnauthorized();
                return;
            }

            var media = new MediaEntry(e.Session)
            {
                Title = e.Content["title"]?.GetValue<string>() ?? "",
                Description = e.Content["description"]?.GetValue<string>() ?? "",
                Type = Enum.TryParse<MediaEntry.MediaType>(
                    e.Content["mediaType"]?.GetValue<string>() ?? "",
                    true,
                    out var mt
                ) ? mt : MediaEntry.MediaType.Unknown,

                ReleaseYear = e.Content["releaseYear"]?.GetValue<int>() ?? 0,
                AgeRestriction = e.Content["ageRestriction"]?.GetValue<int>() ?? 0,
                Genres = e.Content["genres"]?.AsArray()
                             ?.Select(g => g!.GetValue<string>())
                             .ToList()
                         ?? new List<string>()
            };

            media.Save();

            e.RespondCreated(new JsonObject
            {
                ["success"] = true,
                ["id"] = media.Id.ToString()
            });
        }
        catch (Exception ex)
        {
            e.RespondInternalServerError(ex);
        }
    }


    // ----------------------------------------------------------
    //                  GET /media
    // ----------------------------------------------------------
    // ----------------------------------------------------------
    //                  GET /media  (Search / Filter / Sort)
    // ----------------------------------------------------------
    private void HandleList(HttpRestEventArgs e)
    {
        var q = e.Query;

        q.TryGetValue("search", out var search);
        q.TryGetValue("genre", out var genre);
        q.TryGetValue("sort", out var sort);

        MediaEntry.MediaType? type = null;
        if (q.TryGetValue("type", out var t) &&
            Enum.TryParse<MediaEntry.MediaType>(t, true, out var parsed))
        {
            type = parsed;
        }

        int? year = q.TryGetValue("year", out var y) && int.TryParse(y, out var yi)
            ? yi : null;

        int? age = q.TryGetValue("age", out var a) && int.TryParse(a, out var ai)
            ? ai : null;

        double? minRating = q.TryGetValue("minRating", out var r) && double.TryParse(r, out var ri)
            ? ri : null;

        var result = _mediaRepo.Search(
                search,
                genre,
                type,
                year,
                age,
                minRating,
                sort
            )
            .Select(m => new JsonObject
            {
                ["id"] = m.Id.ToString(),
                ["title"] = m.Title,
                ["type"] = m.Type.ToString(),
                ["year"] = m.ReleaseYear,
                ["avgScore"] = m.AverageScore
            })
            .ToArray();

        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["count"] = result.Length,
            ["media"] = new JsonArray(result)
        });
    }


    private void HandleRatingsMedia(HttpRestEventArgs e, MediaEntry media)
    {
        if (e.Method != HttpMethod.Get)
        {
            e.RespondMethodNotAllowed();
            return;
        }

        var ratings = Rating.ForMedia(media.Id);

        var json = new JsonArray(
            ratings.Select(r => new JsonObject
            {
                ["id"] = r.Id.ToString(),
                ["username"] = r.UserName,
                ["stars"] = r.Stars,
                ["comment"] = r.IsConfirmed ? r.Comment : null,
                ["createdAt"] = r.CreatedAt
            }).ToArray<JsonNode?>()
        );

        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["mediaId"] = media.Id.ToString(),
            ["ratings"] = json
        });
    }


    private void HandleFavoriseMedia(HttpRestEventArgs e, MediaEntry media)
    {
        if (e.Session == null)
        {
            e.RespondUnauthorized();
            return;
        }

        if (e.Method == HttpMethod.Post)
        {
            var fav = new MediaFavorite(e.Session, media);
            fav.Save();

            e.RespondOk(new JsonObject
            {
                ["success"] = true,
                ["message"] = "Media set as favorite."
            });
        }
        else if (e.Method == HttpMethod.Delete)
        {
            var fav = MediaFavorite.Get(e.Session.UserName, media.Id);

            if (fav == null)
            {
                e.RespondNotFound("Media not found");
                return;
            }

            fav.BeginEdit(e.Session);
            fav.Delete();
            

            e.RespondOk(new JsonObject
            {
                ["success"] = true,
                ["message"] = "Media unfavorited."
            });
        }
    }


    private void HandleRateMedia(HttpRestEventArgs e, MediaEntry media)
    {
        try
        {
            int stars = e.Content["stars"]?.GetValue<int>() ?? 0;
            string? comment = e.Content["comment"]?.GetValue<string>();

            if (stars < 1 || stars > 5)
            {
                e.RespondBadRequest("Stars must be between 1 and 5.");
                return;
            }

            var rating = new Rating(e.Session!, media);
            rating.SetRating(stars, comment);
            rating.Save();

            e.RespondCreated(new JsonObject
            {
                ["success"] = true,
                ["mediaId"] = media.Id,
                ["ratingId"] = rating.Id,
                ["stars"] = rating.Stars,
                ["comment"] = rating.Comment
            });
        }
        catch (Exception ex)
        {
            e.RespondInternalServerError(ex);
        }
    }


    private static MediaEntry? GetMedia(HttpRestEventArgs e, string idPart, out Guid id)
    {
        if (!Guid.TryParse(idPart, out id))
        {
            e.RespondBadRequest("Invalid mediaId format.");
            return null;
        }

        var media = MediaEntry.Get(id, e.Session);
        if (media is null)
        {
            e.RespondNotFound("Media not found.");
            return null;
        }

        return media;
    }

    private void HandleGet(HttpRestEventArgs e, MediaEntry media)
    {
        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["id"] = media.Id.ToString(),
            ["title"] = media.Title,
            ["description"] = media.Description,
            ["type"] = media.Type.ToString(),
            ["year"] = media.ReleaseYear,
            ["genres"] = new JsonArray(media.Genres.Select(g => (JsonNode)g).ToArray()),
            ["age"] = media.AgeRestriction,
            ["avgScore"] = media.AverageScore
        });
    }

    private void HandleUpdate(HttpRestEventArgs e, MediaEntry media)
    {
        try
        {
            media.BeginEdit(e.Session!);

            media.Title = e.Content["title"]?.GetValue<string>() ?? media.Title;
            media.Description = e.Content["description"]?.GetValue<string>() ?? media.Description;

            if (Enum.TryParse<MediaEntry.MediaType>(
                    e.Content["mediaType"]?.GetValue<string>(),
                    true,
                    out var mt))
            {
                media.Type = mt;
            }

            media.ReleaseYear =
                e.Content["releaseYear"]?.GetValue<int>() ?? media.ReleaseYear;

            if (e.Content.TryGetPropertyValue("genres", out var gnode))
            {
                media.Genres = gnode!
                    .AsArray()
                    .Select(n => n!.GetValue<string>())
                    .ToList();
            }

            media.AgeRestriction =
                e.Content["ageRestriction"]?.GetValue<int>() ?? media.AgeRestriction;

            media.Save();

            e.RespondOk(new JsonObject
            {
                ["success"] = true,
                ["message"] = "Media updated."
            });
        }
        catch (UnauthorizedAccessException)
        {
            e.RespondForbidden("You are not allowed to edit this media entry.");
        }
        catch (Exception ex)
        {
            e.RespondInternalServerError(ex);
        }
    }


    private void HandleDelete(HttpRestEventArgs e, Guid id)
    {
        try
        {
            var media = MediaEntry.Get(id, e.Session);
            if (media is null)
            {
                e.RespondNotFound("Media not found.");
                return;
            }

            media.BeginEdit(e.Session!);
            media.Delete();

            e.RespondNoContent();
        }
        catch (UnauthorizedAccessException)
        {
            e.RespondForbidden("You are not allowed to delete this media.");
        }
        catch (Exception ex)
        {
            e.RespondInternalServerError(ex);
        }
    }
}
