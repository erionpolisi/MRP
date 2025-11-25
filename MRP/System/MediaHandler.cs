using MRP.Handlers;
using MRP.Server;
using MRP.Server.Ext;
using System.Net;
using System.Text.Json.Nodes;
using MRP.Services;

namespace MRP.System;

public sealed class MediaHandler : Handler, IHandler
{
    public override void Handle(HttpRestEventArgs e)
    {
        if (!e.Path.StartsWith("/media"))
            return;

        if (!e.VerifyAuthentication())
            return;

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
            RespondInvalidEndpoint(e);
        }

        e.Responded = true;
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

            e.Respond(HttpStatusCode.Created, new JsonObject
            {
                ["success"] = true,
                ["id"] = entry.Id.ToString()
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

        e.Respond(HttpStatusCode.OK, new JsonObject
        {
            ["success"] = true,
            ["media"] = json
        });
    }

    // ----------------------------------------------------------
    //      GET / PUT / DELETE — /media/{id}
    // ----------------------------------------------------------
    private void HandleMediaById(HttpRestEventArgs e)
    {
        var parts = e.Path.Trim('/').Split('/');



        switch (parts.Length)
        {
            case 1:
                RespondInvalidEndpoint(e);
                break;
            case 2:
                HandleBasicRoute(e, parts);
                break;
            case 3:
                HandleSubRoutes(e, parts);
                break;
            default:
                RespondInvalidEndpoint(e);
                break;
        }
    }

    private void HandleSubRoutes(HttpRestEventArgs e, string[] parts)
    {
        var idPart = parts[1];
        var action = parts[2];

        var media = GetMedia(e, idPart, out var id);

        switch (action)
        {
            case "rate":
                HandleRateMedia(e, media);
                break;
            case "favorite":
                HandleFavoriseMedia(e, media);
                break;
            default:
                RespondInvalidEndpoint(e);
                break;
        }
    }

    private void HandleFavoriseMedia(HttpRestEventArgs e, MediaEntry? media)
    {
        if (e.Method == HttpMethod.Post)
        {
            e.Respond(HttpStatusCode.OK, new JsonObject()
            {
                ["success"] = true,
                ["message"] = "Media favorited."
            });
        }
        else if (e.Method == HttpMethod.Delete)
        {
            e.Respond(HttpStatusCode.OK, new JsonObject()
            {
                ["success"] = true,
                ["message"] = "Media unfavorited."
            });
        }
        else
        {
            RespondInvalidEndpoint(e);
        }
 
    }

    private void HandleRateMedia(HttpRestEventArgs e, MediaEntry? media)
    {
        e.Respond(HttpStatusCode.OK, new JsonObject()
        {
            ["success"] = true,
            ["message"] = "Media rated."
        });
    }

    private void HandleBasicRoute(HttpRestEventArgs e, string[] parts)
    {
        var idPart = parts[1];

        var media = GetMedia(e, idPart, out var id);

        if (e.Method == HttpMethod.Get)
        {
            HandleGet(e, media);
        }
        else if (e.Method == HttpMethod.Put)
        {
            HandleUpdate(e, media);
        }
        else if (e.Method == HttpMethod.Delete)
        {
            HandleDelete(e, id);
        }
        else
        {
            RespondInvalidEndpoint(e);
        }
    }

    private static MediaEntry? GetMedia(HttpRestEventArgs e, string idPart, out Guid id)
    {
        if (!Guid.TryParse(idPart, out id))
        {
            e.Respond(HttpStatusCode.BadRequest, new JsonObject
            {
                ["success"] = false,
                ["reason"] = "Invalid mediaId format."
            });
            return null;
        }

        var media = MediaRepository.Get(id);
        if (media is null)
        {
            e.Respond(HttpStatusCode.NotFound, new JsonObject
            {
                ["success"] = false,
                ["reason"] = "Media not found."
            });
            return media;
        }

        return media;
    }

    private void HandleGet(HttpRestEventArgs e, MediaEntry media)
    {
        e.Respond(HttpStatusCode.OK, new JsonObject
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

        e.Respond(HttpStatusCode.OK, new JsonObject
        {
            ["success"] = true,
            ["message"] = "Media updated."
        });
    }

    private void HandleDelete(HttpRestEventArgs e, Guid id)
    {
        MediaRepository.Delete(id);
        e.Respond(HttpStatusCode.NoContent, new JsonObject());
    }

    private void RespondInvalidEndpoint(HttpRestEventArgs e)
    {
        e.Respond(HttpStatusCode.BadRequest, new JsonObject
        {
            ["success"] = false,
            ["reason"] = "Invalid media endpoint."
        });
    }
}
