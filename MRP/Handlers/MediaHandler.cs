using MRP.Server;
using MRP.Server.Ext;
using System.Text.Json.Nodes;
using MRP.Repositories;
using MRP.System;

namespace MRP.Handlers;

public sealed class MediaHandler : Handler, IHandler
{
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
    private void HandleList(HttpRestEventArgs e)
    {
        var titleQuery =
            (e.Query.TryGetValue("title", out var t) ? t : "")
            ?.ToLowerInvariant() ?? "";

        var list = MediaEntry.All
            .Where(m => m.Title.ToLowerInvariant().Contains(titleQuery))
            .Select(m => new JsonObject
            {
                ["id"] = m.Id.ToString(),
                ["title"] = m.Title,
                ["type"] = m.Type.ToString(),
                ["score"] = m.AverageScore
            })
            .ToArray();

        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["media"] = new JsonArray(list)
        });
    }


    private void HandleRatingsMedia(HttpRestEventArgs e, MediaEntry media)
    {
            e.RespondOk(new JsonObject()
            {
                ["success"] = true,
                ["ratings"] = "ratings from media."
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
            if (fav != null)
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
        media.Title = e.Content["title"]?.GetValue<string>() ?? media.Title;
        media.Description = e.Content["description"]?.GetValue<string>() ?? media.Description;

        if (Enum.TryParse<MediaEntry.MediaType>(
                e.Content["mediaType"]?.GetValue<string>() ?? "",
                true, out var mt))
        {
            media.Type = mt;
        }

        media.ReleaseYear = e.Content["releaseYear"]?.GetValue<int>() ?? media.ReleaseYear;

        if (e.Content.TryGetPropertyValue("genres", out var gnode))
        {
            media.Genres = gnode!.AsArray().Select(n => n!.GetValue<string>()).ToList();
        }

        media.AgeRestriction = e.Content["ageRestriction"]?.GetValue<int>() ?? media.AgeRestriction;

        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["message"] = "Media updated."
        });
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
