using System.Data;
using MRP.Repositories.Ext;
using MRP.System;

namespace MRP.Repositories;

public sealed class MediaEntryRepository
    : Repository<MediaEntry>, IRepository<MediaEntry>, IRepository
{
    protected override MediaEntry _RefreshObject(IDataReader re, MediaEntry obj)
    {
        obj.Title = re.GetString("title");
        obj.Description = re.GetString("description");
        obj.Type = (MediaEntry.MediaType)re.GetInt("type");
        obj.ReleaseYear = re.GetInt("release_year");
        obj.AgeRestriction = re.GetInt("age_restriction");

        obj.CreatorId = Guid.Parse(re.GetString("creator_id"));
        obj.CreatorUserName = re.GetString("creator_username");

        obj.AverageScore = re.GetDouble("avg_score");
        obj.CreatedAt = re.GetDateTime("created_at");

        // text[] → List<string>
        var genres = (string[])re["genres"];
        obj.Genres = genres?.ToList() ?? new();

        return obj;
    }


    public override MediaEntry? Get(object id, Session? session = null)
    {
        using var cmd = _Cn.CreateCommand();
        cmd.CommandText = """
                              SELECT
                                  m.id, m.title, m.description, m.type,
                                  m.release_year, m.age_restriction,
                                  m.genres, m.created_at,
                                  m.creator_id,
                                  u.username AS creator_username,
                                  COALESCE(AVG(r.stars), 0) AS avg_score
                              FROM media_entries m
                              JOIN users u ON u.id = m.creator_id
                              LEFT JOIN ratings r ON r.media_id = m.id
                              WHERE m.id = :id
                              GROUP BY m.id, u.username
                          """;

        cmd.BindParam(":id", id);

        using var re = cmd.ExecuteReader();
        return re.Read() ? _CreateObject(re) : null;
    }


    public override IEnumerable<MediaEntry> GetAll(Session? session = null)
    {
        using var cmd = _Cn.CreateCommand();
        cmd.CommandText = """
                              SELECT
                                  m.id, m.title, m.description, m.type,
                                  m.release_year, m.age_restriction,
                                  m.genres, m.created_at,
                                  m.creator_id,
                                  u.username AS creator_username,
                                  COALESCE(AVG(r.stars), 0) AS avg_score
                              FROM media_entries m
                              JOIN users u ON u.id = m.creator_id
                              LEFT JOIN ratings r ON r.media_id = m.id
                              GROUP BY m.id, u.username
                          """;

        using var re = cmd.ExecuteReader();
        while (re.Read())
            yield return _CreateObject(re);
    }


    public override void Refresh(MediaEntry obj)
    {
        using IDbCommand cmd = _Cn.CreateCommand();
        cmd.CommandText = """
            SELECT TITLE, DESCRIPTION, TYPE, RELEASE_YEAR, AGE_RESTRICTION, GENRES, CREATOR
            FROM MEDIA_ENTRIES
            WHERE ID = :id
        """;
        cmd.BindParam(":id", obj.Id);

        using IDataReader re = cmd.ExecuteReader();
        if (re.Read())
        {
            _RefreshObject(re, obj);
        }
    }

    public override void Save(MediaEntry obj)
    {
        using var cmd = _Cn.CreateCommand();
        cmd.CommandText = """
                              INSERT INTO media_entries
                              (id, title, description, type, release_year, age_restriction, genres, creator_id, created_at)
                              VALUES (:id, :t, :d, :ty, :y, :a, :g, :c, :ca)
                              ON CONFLICT (id) DO UPDATE SET
                                  title = EXCLUDED.title,
                                  description = EXCLUDED.description,
                                  type = EXCLUDED.type,
                                  release_year = EXCLUDED.release_year,
                                  age_restriction = EXCLUDED.age_restriction,
                                  genres = EXCLUDED.genres
                          """;

        cmd.BindParam(":id", obj.Id)
            .BindParam(":t", obj.Title)
            .BindParam(":d", obj.Description)
            .BindParam(":ty", (int)obj.Type)
            .BindParam(":y", obj.ReleaseYear)
            .BindParam(":a", obj.AgeRestriction)
            .BindParam(":g", obj.Genres.ToArray())
            .BindParam(":c", obj.CreatorId)
            .BindParam(":ca", obj.CreatedAt);

        cmd.ExecuteNonQuery();
    }



    public override void Delete(MediaEntry obj)
    {
        using var cmd = _Cn.CreateCommand();
        cmd.CommandText = "DELETE FROM media_entries WHERE id = :id";
        cmd.BindParam(":id", obj.Id);
        cmd.ExecuteNonQuery();
    }

}
