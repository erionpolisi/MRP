using MRP.Infrastructure;
using MRP.System;
using Npgsql;
using System.Collections;
using System.Data;

using Microsoft.Extensions.Configuration;
using MRP.Repositories.Ext;

namespace MRP.Repositories
{
    public abstract class Repository<T> : IRepository<T>, IRepository where T : IAtom, __IVerifiable, new()
    {
        private static IDbConnection? _DbConnection;


        protected static IDbConnection _Cn
        {
            get
            {
                if (_DbConnection == null)
                {
                    var cs = AppConfig.Configuration
                        .GetConnectionString("Postgres");

                    if (string.IsNullOrWhiteSpace(cs))
                        throw new InvalidOperationException(
                            "Postgres connection string not configured.");

                    _DbConnection = new NpgsqlConnection(cs);
                    _DbConnection.Open();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("DB Connection Established");
                }

                return _DbConnection;
            }
        }


        protected virtual T _CreateObject(IDataReader re)
        {
            T rval = new();
            ((__IVerifiable)rval).__InternalID = re.GetGuid("ID");

            return _RefreshObject(re, rval);
        }

        protected abstract T _RefreshObject(IDataReader re, T rval);


        public abstract T? Get(object id, Session? session = null);

        public abstract IEnumerable<T> GetAll(Session? session = null);

        public abstract void Refresh(T obj);

        public abstract void Save(T obj);

        public abstract void Delete(T obj);


        object? IRepository.Get(object id, Session? session)
        {
            return Get(id, session);
        }

        IEnumerable IRepository.GetAll(Session? session)
        {
            return GetAll(session);
        }

        void IRepository.Refresh(object obj)
        {
            Refresh((T)obj);
        }

        void IRepository.Save(object obj)
        {
            Save((T)obj);
        }

        void IRepository.Delete(object obj)
        {
            Delete((T)obj);
        }
    }
}
