using System.Data;
using MRP.Repositories.Ext;
using MRP.System;

namespace MRP.Repositories;

public sealed class MediaFavoriteRepository
    : Repository<MediaFavorite>, IRepository<MediaFavorite>, IRepository
{
    protected override MediaFavorite _RefreshObject(IDataReader re, MediaFavorite obj)
    {
        obj.CreatorUserName = re.GetString("USER_NAME");
        obj.MediaId = re.GetGuid("MEDIA_ID");
        obj.CreatedAt = re.GetDateTime("CREATED_AT");
        return obj;
    }

    public override MediaFavorite? Get(object id, Session? session = null)
    {
        using IDbCommand cmd = _Cn.CreateCommand();
        cmd.CommandText = """
            SELECT USER_NAME, MEDIA_ID, CREATED_AT
            FROM MEDIA_FAVORITES
            WHERE USER_NAME = :u AND MEDIA_ID = :m
        """;

        var key = ((string user, Guid media))id;
        cmd.BindParam(":u", key.user);
        cmd.BindParam(":m", key.media);

        using IDataReader re = cmd.ExecuteReader();
        return re.Read() ? _CreateObject(re) : null;
    }

    public override IEnumerable<MediaFavorite> GetAll(Session? session = null)
    {
        using IDbCommand cmd = _Cn.CreateCommand();
        cmd.CommandText = """
            SELECT USER_NAME, MEDIA_ID, CREATED_AT
            FROM MEDIA_FAVORITES
        """;

        using IDataReader re = cmd.ExecuteReader();
        while (re.Read())
            yield return _CreateObject(re);
    }

    public override void Refresh(MediaFavorite obj)
    {
        using IDbCommand cmd = _Cn.CreateCommand();
        cmd.CommandText = """
            SELECT CREATED_AT
            FROM MEDIA_FAVORITES
            WHERE USER_NAME = :u AND MEDIA_ID = :m
        """;

        cmd.BindParam(":u", obj.CreatorUserName);
        cmd.BindParam(":m", obj.MediaId);

        using IDataReader re = cmd.ExecuteReader();
        if (re.Read())
            obj.CreatedAt = re.GetDateTime("CREATED_AT");
    }

    public override void Save(MediaFavorite obj)
    {
        using IDbCommand cmd = _Cn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO MEDIA_FAVORITES (USER_NAME, MEDIA_ID, CREATED_AT)
            VALUES (:u, :m, :c)
            ON CONFLICT DO NOTHING
        """;

        cmd.BindParam(":u", obj.CreatorUserName);
        cmd.BindParam(":m", obj.MediaId);
        cmd.BindParam(":c", obj.CreatedAt);

        cmd.ExecuteNonQuery();
    }

    public override void Delete(MediaFavorite obj)
    {
        using IDbCommand cmd = _Cn.CreateCommand();
        cmd.CommandText = """
            DELETE FROM MEDIA_FAVORITES
            WHERE USER_NAME = :u AND MEDIA_ID = :m
        """;

        cmd.BindParam(":u", obj.CreatorUserName);
        cmd.BindParam(":m", obj.MediaId);

        cmd.ExecuteNonQuery();
    }

    public IEnumerable<MediaFavorite> ForUser(string userName)
    {
        using IDbCommand cmd = _Cn.CreateCommand();
        cmd.CommandText = """
                              SELECT USER_NAME, MEDIA_ID, CREATED_AT
                              FROM MEDIA_FAVORITES
                              WHERE USER_NAME = :u
                              ORDER BY CREATED_AT DESC
                          """;

        cmd.BindParam(":u", userName);

        using IDataReader re = cmd.ExecuteReader();
        while (re.Read())
        {
            yield return _CreateObject(re);
        }
    }

    protected override MediaFavorite _CreateObject(IDataReader re)
    {
        var fav = new MediaFavorite();
        return _RefreshObject(re, fav);
    }

}
