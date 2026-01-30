using MRP.System;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRP.Repositories.Ext;

namespace MRP.Repositories
{
    public sealed class UserRepository : Repository<User>, IRepository<User>, IRepository
    {
        protected override User _CreateObject(IDataReader re)
        {
            var user = new User();
            ((__IVerifiable)user).__InternalID = re.GetGuid(re.GetOrdinal("id"));
            return _RefreshObject(re, user);
        }

        protected override User _RefreshObject(IDataReader re, User obj)
        {
            obj.FullName = re.GetString("fullname");
            obj.EMail = re.GetString("email");
            obj.IsAdmin = re.GetBool("hadmin");

            ((__IAuthentificable)obj).__Username = re.GetString("username");
            ((__IAuthentificable)obj).__PasswordHash = re.GetString("passwd");

            return obj;
        }

        public override User? Get(object id, Session? session = null)
        {
            using IDbCommand cmd = _Cn.CreateCommand();
            cmd.CommandText = """
                                  SELECT USERNAME, FULLNAME, EMAIL, HADMIN, PASSWD, ID
                                  FROM USERS
                                  WHERE USERNAME = :u
                              """;
            cmd.BindParam(":u", id);

            using IDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                return _CreateObject(re);
            }

            return null;
        }

        public override IEnumerable<User> GetAll(Session? session = null)
        {
            using IDbCommand cmd = _Cn.CreateCommand();
            cmd.CommandText = "SELECT USERNAME, FULLNAME, EMAIL, HADMIN FROM USERS";

            List<User> rval = new List<User>();

            using IDataReader re = cmd.ExecuteReader();
            while (re.Read())
            {
                rval.Add(_CreateObject(re));
            }

            return rval;
        }

        public override void Refresh(User obj)
        {
            using IDbCommand cmd = _Cn.CreateCommand();
            cmd.CommandText = """
                                  SELECT USERNAME, FULLNAME, EMAIL, HADMIN, PASSWD
                                  FROM USERS
                                  WHERE USERNAME = :u
                              """;
            cmd.BindParam(":u", obj.UserName);

            using IDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                _RefreshObject(re, obj);
            }
        }


        public override void Delete(User obj)
        {
            using IDbCommand cmd = _Cn.CreateCommand();
            cmd.CommandText = "DELETE FROM USERS WHERE USERNAME = :u";
            cmd.BindParam(":u", obj.UserName);
            cmd.ExecuteNonQuery();
        }


        public override void Save(User obj)
        {
            if (((__IVerifiable)obj).__InternalID is null)
            {
                if (string.IsNullOrWhiteSpace(((__IAuthentificable)obj).__Username))
                {
                    throw new InvalidOperationException("User name must not be empty.");
                }
                if (string.IsNullOrWhiteSpace(((__IAuthentificable)obj).__PasswordHash))
                {
                    throw new InvalidOperationException("Password must not be empty.");
                }

                using IDbCommand cmd = _Cn.CreateCommand();
                cmd.CommandText = """
                                  INSERT INTO users (id, username, fullname, passwd, email, hadmin)
                                  VALUES (:id, :u, :n, :p, :e, :a)
                                  """;

                cmd.BindParam(":id", obj.Id = Guid.NewGuid());
                cmd.BindParam(":u", ((__IAuthentificable)obj).__Username)
                   .BindParam(":n", obj.FullName)
                   .BindParam(":p", ((__IAuthentificable)obj).__PasswordHash)
                   .BindParam(":e", obj.EMail)
                   .BindParam(":a", obj.IsAdmin);
                cmd.ExecuteNonQuery();
            }
            else
            {
                string pwd = string.IsNullOrWhiteSpace(((__IAuthentificable)obj).__PasswordHash) ?
                             string.Empty : "PASSWD = :p, ";
                using IDbCommand cmd = _Cn.CreateCommand();
                cmd.CommandText = $"UPDATE USERS SET FULLNAME = :n, {pwd}EMAIL = :e, HADMIN = :a " +
                                  "WHERE USERNAME = :u";
                cmd.BindParam(":n", obj.FullName);
                if (!string.IsNullOrWhiteSpace(pwd)) { cmd.BindParam(":p", ((__IAuthentificable)obj).__PasswordHash); }
                cmd.BindParam(":e", obj.EMail).BindParam(":a", obj.IsAdmin).BindParam(":u", obj.UserName);
                cmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<(string UserName, int RatingCount)> GetLeaderboard()
        {
            using IDbCommand cmd = _Cn.CreateCommand();
            cmd.CommandText = """
                                  SELECT u.username, COUNT(r.id) AS rating_count
                                  FROM users u
                                  LEFT JOIN ratings r ON r.username = u.username
                                  GROUP BY u.username
                                  ORDER BY rating_count DESC
                              """;

            using IDataReader re = cmd.ExecuteReader();
            while (re.Read())
            {
                yield return (
                    re.GetString("username"),
                    re.GetInt("rating_count")
                );
            }
        }

        public (int TotalRatings, double AverageRating, string? FavoriteGenre)
            GetUserStatistics(string username)
        {
            int totalRatings = 0;
            double avgRating = 0;
            string? favGenre = null;

            // ---------------------------------------
            // totalRatings + avgRating
            // ---------------------------------------
            using (IDbCommand cmd = _Cn.CreateCommand())
            {
                cmd.CommandText = """
                                      SELECT COUNT(*) AS cnt, COALESCE(AVG(stars),0) AS avg
                                      FROM ratings
                                      WHERE username = :u
                                  """;
                cmd.BindParam(":u", username);

                using IDataReader re = cmd.ExecuteReader();
                if (re.Read())
                {
                    totalRatings = re.GetInt("cnt");
                    avgRating = re.GetDouble("avg");
                }
            }

            // ---------------------------------------
            // favoriteGenre (simple + sufficient)
            // ---------------------------------------
            using (IDbCommand cmd = _Cn.CreateCommand())
            {
                cmd.CommandText = """
                                      SELECT g, COUNT(*) AS cnt
                                      FROM ratings r
                                      JOIN media_entries m ON m.id = r.media_id,
                                           unnest(m.genres) g
                                      WHERE r.username = :u
                                      GROUP BY g
                                      ORDER BY cnt DESC
                                      LIMIT 1
                                  """;
                cmd.BindParam(":u", username);

                using IDataReader re = cmd.ExecuteReader();
                if (re.Read())
                {
                    favGenre = re.GetString("g");
                }
            }

            return (totalRatings, avgRating, favGenre);
        }


    }

}
