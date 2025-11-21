using MRP.Handlers;
using MRP.Server;
using MRP.Server.Ext;
using System.Net;
using System.Text.Json.Nodes;

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
        var list = MediaRepository.GetAll()
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
        string idPart = e.Path.Trim('/').Split('/')[1];

        if (!Guid.TryParse(idPart, out var id))
        {
            e.Respond(HttpStatusCode.BadRequest, new JsonObject
            {
                ["success"] = false,
                ["reason"] = "Invalid mediaId format."
            });
            return;
        }

        var media = MediaRepository.Get(id);
        if (media is null)
        {
            e.Respond(HttpStatusCode.NotFound, new JsonObject
            {
                ["success"] = false,
                ["reason"] = "Media not found."
            });
            return;
        }

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
