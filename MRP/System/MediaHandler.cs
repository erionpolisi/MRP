using MRP.Handlers;
using MRP.Server;
using MRP.Server.Ext;
using System.Text.Json.Nodes;
using MRP.Services;

namespace MRP.System;

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
            var entry = new MediaEntry
            {
                Id = Guid.NewGuid(),
                Creator = UserRepository.Get(e.Session!.UserName)!,
                Title = e.Content["title"]?.GetValue<string>() ?? "",
                Description = e.Content["description"]?.GetValue<string>() ?? "",
                Type = Enum.TryParse<MediaEntry.MediaType>(
                    e.Content["mediaType"]?.GetValue<string>() ?? "",
                    true,
                    out var mt
                ) ? mt : MediaEntry.MediaType.Unknown,

                ReleaseYear = e.Content["releaseYear"]?.GetValue<int>() ?? 0,
                AgeRestriction = e.Content["ageRestriction"]?.GetValue<int>() ?? 0,
                Genres = e.Content["genres"]?.AsArray()?.Select(g => g!.GetValue<string>()).ToList()
                         ?? new List<string>()
            };

            MediaRepository.Add(entry);

            e.RespondCreated(new JsonObject
            {
                ["success"] = true,
                ["id"] = entry.Id.ToString()
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
        // Filter by title if provided
        var titleQuery = (e.Query.TryGetValue("title", out var t) ? t : "")
            ?.ToLowerInvariant() ?? "";

        var list = MediaRepository.GetAll()
            .Where(x => x.Title.ToLowerInvariant().Contains(titleQuery))
            .Select(m => new JsonObject
            {
                ["id"] = m.Id.ToString(),
                ["title"] = m.Title,
                ["type"] = m.Type.ToString(),
                ["score"] = m.AverageScore
            });

        var json = new JsonArray(list.ToArray());

        e.RespondOk(new JsonObject
        {
            ["success"] = true,
            ["media"] = json
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

    private void HandleFavoriseMedia(HttpRestEventArgs e, MediaEntry? media)
    {
        var user = UserRepository.Get(e.Session!.UserName)!;
        if (e.Method == HttpMethod.Post)
        {
            media.FavoritedBy.Add(user);
            user.FavoritedMediaIds.Add(media.Id);

            e.RespondOk(new JsonObject()
            {
                ["success"] = true,
                ["message"] = "Media set as favorite."
            });
        }
        else if (e.Method == HttpMethod.Delete)
        {
            media.FavoritedBy.Remove(user);
            user.FavoritedMediaIds.Remove(media.Id);

            e.RespondOk(new JsonObject()
            {
                ["success"] = true,
                ["message"] = "Media unfavorited."
            });
        }
    }

    private void HandleRateMedia(HttpRestEventArgs e, MediaEntry? media)
    {
            var user = UserRepository.Get(e.Session!.UserName)!;
            int stars = e.Content["stars"]?.GetValue<int>() ?? 0;
            string? comment = e.Content["comment"]?.GetValue<string>() ?? "";

            var rating = new Rating(user, media, stars, comment);
            media.Ratings.Add(rating);

            e.RespondOk(new JsonObject()
            {
                ["success"] = true,
                ["ratingId"] = rating.Id.ToString(),
                ["stars"] = rating.Stars,
                ["comment"] = rating.Comment,
                ["message"] = "Media rated."
            });
    }

    private static MediaEntry? GetMedia(HttpRestEventArgs e, string idPart, out Guid id)
    {
        if (!Guid.TryParse(idPart, out id))
        {
            e.RespondBadRequest("Invalid mediaId format.");
            return null;
        }

        var media = MediaRepository.Get(id);
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
        MediaRepository.Delete(id);
        e.RespondNoContent();
    }
}
