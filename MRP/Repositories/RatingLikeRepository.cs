using System.Data;
using MRP.Repositories.Ext;
using MRP.System;

namespace MRP.Repositories;

public sealed class RatingLikeRepository
    : Repository<RatingLike>, IRepository<RatingLike>, IRepository
{
    protected override RatingLike _RefreshObject(IDataReader re, RatingLike obj)
    {
        obj.RatingId = re.GetGuid("rating_id");
        obj.UserName = re.GetString("username");
        obj.CreatedAt = re.GetDateTime("created_at");
        return obj;
    }

    public override RatingLike? Get(object id, Session? session = null)
    {
        var key = ((Guid ratingId, string user))id;

        using var cmd = _Cn.CreateCommand();
        cmd.CommandText = """
            SELECT rating_id, username, created_at
            FROM rating_likes
            WHERE rating_id = :r AND username = :u
        """;

        cmd.BindParam(":r", key.ratingId);
        cmd.BindParam(":u", key.user);

        using var re = cmd.ExecuteReader();
        return re.Read() ? _CreateObject(re) : null;
    }

    public override IEnumerable<RatingLike> GetAll(Session? session = null)
    {
        using var cmd = _Cn.CreateCommand();
        cmd.CommandText = "SELECT rating_id, username, created_at FROM rating_likes";

        using var re = cmd.ExecuteReader();
        while (re.Read())
            yield return _CreateObject(re);
    }

    public override void Refresh(RatingLike obj) { }

    public override void Save(RatingLike obj)
    {
        using var cmd = _Cn.CreateCommand();
        cmd.CommandText = """
                              INSERT INTO rating_likes (rating_id, username, created_at)
                              VALUES (:r, :u, :c)
                              ON CONFLICT DO NOTHING
                          """;

        cmd.BindParam(":r", obj.RatingId)
            .BindParam(":u", obj.UserName)
            .BindParam(":c", obj.CreatedAt);

        var affected = cmd.ExecuteNonQuery();

            if (affected == 0)
                Console.WriteLine("RatingLike ignored (already exists or constraint missing)");
    }


    public override void Delete(RatingLike obj)
    {
        using var cmd = _Cn.CreateCommand();
        cmd.CommandText = """
            DELETE FROM rating_likes
            WHERE rating_id = :r AND username = :u
        """;

        cmd.BindParam(":r", obj.RatingId)
           .BindParam(":u", obj.UserName);

        cmd.ExecuteNonQuery();
    }

    protected override RatingLike _CreateObject(IDataReader re)
    {
        var like = new RatingLike();
        return _RefreshObject(re, like);
    }

}
