using System.Data;
using MRP.System;
using MRP.Repositories.Ext;

namespace MRP.Repositories;

public sealed class RatingRepository
    : Repository<Rating>, IRepository<Rating>, IRepository
{
    public IEnumerable<Rating> For(MediaEntry media)
    {
        using IDbCommand cmd = _Cn.CreateCommand();
        cmd.CommandText = """
            SELECT ID, MEDIA_ID, USERNAME, STARS, COMMENT, IS_CONFIRMED, CREATED_AT
            FROM RATINGS
            WHERE MEDIA_ID = :mid
        """;
        cmd.BindParam(":mid", media.Id);

        using IDataReader re = cmd.ExecuteReader();
        while (re.Read())
        {
            yield return _CreateObject(re);
        }
    }

    protected override Rating _RefreshObject(IDataReader re, Rating obj)
    {
        obj.MediaId = Guid.Parse(re.GetString("MEDIA_ID"));
        obj.UserName = re.GetString("USERNAME");
        obj.Stars = re.GetInt("STARS");
        obj.Comment = re.GetString("COMMENT");
        obj.IsConfirmed = re.GetBool("IS_CONFIRMED");
        obj.CreatedAt = re.GetDateTime("CREATED_AT");

        return obj;
    }

    public override Rating? Get(object id, Session? session = null)
    {
        using IDbCommand cmd = _Cn.CreateCommand();
        cmd.CommandText = """
            SELECT ID, MEDIA_ID, USERNAME, STARS, COMMENT, IS_CONFIRMED, CREATED_AT
            FROM RATINGS
            WHERE ID = :id
        """;
        cmd.BindParam(":id", id);

        using IDataReader re = cmd.ExecuteReader();
        return re.Read() ? _CreateObject(re) : null;
    }

    public override IEnumerable<Rating> GetAll(Session? session = null)
    {
        using IDbCommand cmd = _Cn.CreateCommand();
        cmd.CommandText = """
            SELECT ID, MEDIA_ID, USERNAME, STARS, COMMENT, IS_CONFIRMED, CREATED_AT
            FROM RATINGS
        """;

        using IDataReader re = cmd.ExecuteReader();
        while (re.Read())
        {
            yield return _CreateObject(re);
        }
    }

    public override void Refresh(Rating obj)
    {
        using IDbCommand cmd = _Cn.CreateCommand();
        cmd.CommandText = """
            SELECT STARS, COMMENT, IS_CONFIRMED
            FROM RATINGS
            WHERE ID = :id
        """;
        cmd.BindParam(":id", obj.Id);

        using IDataReader re = cmd.ExecuteReader();
        if (re.Read())
        {
            obj.Stars = re.GetInt("STARS");
            obj.Comment = re.GetString("COMMENT");
            obj.IsConfirmed = re.GetBool("IS_CONFIRMED");
        }
    }

    public override void Save(Rating obj)
    {
        using IDbCommand cmd = _Cn.CreateCommand();

        if (((__IVerifiable)obj).__InternalID is null)
        {
            cmd.CommandText = """
                INSERT INTO RATINGS
                (ID, MEDIA_ID, USERNAME, STARS, COMMENT, IS_CONFIRMED, CREATED_AT)
                VALUES (:id, :mid, :u, :s, :c, :conf, :t)
            """;
            cmd.BindParam(":id", obj.Id)
               .BindParam(":mid", obj.MediaId)
               .BindParam(":u", obj.UserName)
               .BindParam(":s", obj.Stars)
               .BindParam(":c", obj.Comment)
               .BindParam(":conf", obj.IsConfirmed)
               .BindParam(":t", obj.CreatedAt);
        }
        else
        {
            cmd.CommandText = """
                UPDATE RATINGS
                SET STARS = :s,
                    COMMENT = :c,
                    IS_CONFIRMED = :conf
                WHERE ID = :id
            """;
            cmd.BindParam(":id", obj.Id)
               .BindParam(":s", obj.Stars)
               .BindParam(":c", obj.Comment)
               .BindParam(":conf", obj.IsConfirmed);
        }

        cmd.ExecuteNonQuery();
    }

    public override void Delete(Rating obj)
    {
        using IDbCommand cmd = _Cn.CreateCommand();
        cmd.CommandText = "DELETE FROM RATINGS WHERE ID = :id";
        cmd.BindParam(":id", obj.Id);
        cmd.ExecuteNonQuery();
    }
}
