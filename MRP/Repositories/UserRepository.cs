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
            User rval = new();
            ((__IVerifiable)rval).__InternalID = re.GetString("USERNAME");
            return _RefreshObject(re, rval);
        }


        protected override User _RefreshObject(IDataReader re, User obj)
        {
            obj.FullName = re.GetString("NAME");
            obj.EMail = re.GetString("EMAIL");
            obj.IsAdmin = re.GetBool("HADMIN");

            return obj;
        }


        public override User? Get(object id, Session? session = null)
        {
            using IDbCommand cmd = _Cn.CreateCommand();
            cmd.CommandText = "SELECT USERNAME, NAME, EMAIL, HADMIN FROM USERS WHERE USERNAME = :u";
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
            cmd.CommandText = "SELECT USERNAME, NAME, EMAIL, HADMIN FROM USERS";

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
            cmd.CommandText = "SELECT NAME, EMAIL, HADMIN FROM USERS WHERE USERNAME = :u";
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
                if (string.IsNullOrWhiteSpace(((_IAuthentificable)obj).__Username))
                {
                    throw new InvalidOperationException("User name must not be empty.");
                }
                if (string.IsNullOrWhiteSpace(((_IAuthentificable)obj).__PasswordHash))
                {
                    throw new InvalidOperationException("Password must not be empty.");
                }

                using IDbCommand cmd = _Cn.CreateCommand();
                cmd.CommandText = "INSERT INTO USERS (USERNAME, NAME, PASSWD, EMAIL, HADMIN) " +
                                  "VALUES (:u, :n, :p, :e, :a)";
                cmd.BindParam(":u", ((_IAuthentificable)obj).__Username)
                   .BindParam(":n", obj.FullName)
                   .BindParam(":p", ((_IAuthentificable)obj).__PasswordHash)
                   .BindParam(":e", obj.EMail)
                   .BindParam(":a", obj.IsAdmin);
                cmd.ExecuteNonQuery();
            }
            else
            {
                string pwd = string.IsNullOrWhiteSpace(((_IAuthentificable)obj).__PasswordHash) ?
                             string.Empty : "PASSWD = :p, ";
                using IDbCommand cmd = _Cn.CreateCommand();
                cmd.CommandText = $"UPDATE USERS SET NAME ? :n, {pwd}EMAIL = :e, HADMIN = :a " +
                                  "WHERE USERNAME = :u";
                cmd.BindParam(":n", obj.FullName);
                if (!string.IsNullOrWhiteSpace(pwd)) { cmd.BindParam(":p", ((_IAuthentificable)obj).__PasswordHash); }
                cmd.BindParam(":e", obj.EMail).BindParam(":a", obj.IsAdmin).BindParam(":u", obj.UserName);
                cmd.ExecuteNonQuery();
            }
        }
    }

}
