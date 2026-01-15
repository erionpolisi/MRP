using System.Data;
using MRP.Repositories.Ext;
using MRP.System;

namespace MRP.Repositories;

public sealed class MediaEntryRepository
    : Repository<MediaEntry>, IRepository<MediaEntry>, IRepository
{
    protected override MediaEntry _RefreshObject(IDataReader re, MediaEntry obj)
    {
        obj.Title = re.GetString("TITLE");
        obj.Description = re.GetString("DESCRIPTION");
        obj.Type = (MediaEntry.MediaType)re.GetInt("TYPE");
        obj.ReleaseYear = re.GetInt("RELEASE_YEAR");
        obj.AgeRestriction = re.GetInt("AGE_RESTRICTION");
        obj.Creator = re.GetString("CREATOR");
        obj.AverageScore = re.GetDouble("AVG_SCORE");

        var genres = re.GetString("GENRES");
        obj.Genres = string.IsNullOrWhiteSpace(genres)
            ? new List<string>()
            : genres.Split(',').ToList();

        return obj;
    }

    public override MediaEntry? Get(object id, Session? session = null)
    {
        using IDbCommand cmd = _Cn.CreateCommand();
        cmd.CommandText = """
            SELECT ID, TITLE, DESCRIPTION, TYPE, RELEASE_YEAR, AGE_RESTRICTION, GENRES, CREATOR
            FROM MEDIA
            WHERE ID = :id
        """;
        cmd.BindParam(":id", id);

        using IDataReader re = cmd.ExecuteReader();
        return re.Read() ? _CreateObject(re) : null;
    }

    public override IEnumerable<MediaEntry> GetAll(Session? session = null)
    {
        using IDbCommand cmd = _Cn.CreateCommand();
        cmd.CommandText = """
            SELECT 
            m.ID,
            m.TITLE,
            m.DESCRIPTION,
            m.TYPE,
            m.RELEASE_YEAR,
            m.AGE_RESTRICTION,
            m.GENRES,
            m.CREATOR,
            COALESCE(AVG(r.STARS), 0) AS AVG_SCORE
        FROM MEDIA m
        LEFT JOIN RATINGS r ON r.MEDIA_ID = m.ID
        GROUP BY m.ID
        """;

        using IDataReader re = cmd.ExecuteReader();
        while (re.Read())
        {
            yield return _CreateObject(re);
        }
    }

    public override void Refresh(MediaEntry obj)
    {
        using IDbCommand cmd = _Cn.CreateCommand();
        cmd.CommandText = """
            SELECT TITLE, DESCRIPTION, TYPE, RELEASE_YEAR, AGE_RESTRICTION, GENRES, CREATOR
            FROM MEDIA
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
        using IDbCommand cmd = _Cn.CreateCommand();

        if (((__IVerifiable)obj).__InternalID is null)
        {
            cmd.CommandText = """
                INSERT INTO MEDIA
                (ID, TITLE, DESCRIPTION, TYPE, RELEASE_YEAR, AGE_RESTRICTION, GENRES, CREATOR)
                VALUES (:id, :t, :d, :ty, :y, :a, :g, :c)
            """;
            cmd.BindParam(":id", obj.Id);
        }
        else
        {
            cmd.CommandText = """
                UPDATE MEDIA
                SET TITLE = :t,
                    DESCRIPTION = :d,
                    TYPE = :ty,
                    RELEASE_YEAR = :y,
                    AGE_RESTRICTION = :a,
                    GENRES = :g,
                    CREATOR = :c
                WHERE ID = :id
            """;
            cmd.BindParam(":id", obj.Id);
        }

        cmd.BindParam(":t", obj.Title)
           .BindParam(":d", obj.Description)
           .BindParam(":ty", (int)obj.Type)
           .BindParam(":y", obj.ReleaseYear)
           .BindParam(":a", obj.AgeRestriction)
           .BindParam(":g", string.Join(",", obj.Genres))
           .BindParam(":c", obj.Creator);

        cmd.ExecuteNonQuery();
    }

    public override void Delete(MediaEntry obj)
    {
        using IDbCommand cmd = _Cn.CreateCommand();
        cmd.CommandText = "DELETE FROM MEDIA WHERE ID = :id";
        cmd.BindParam(":id", obj.Id);
        cmd.ExecuteNonQuery();
    }
}
