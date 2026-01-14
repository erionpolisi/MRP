using MRP.System;
using System.Collections;

namespace MRP.Repositories
{
    public interface IRepository<T> : IRepository where T : IAtom, __IVerifiable, new()
    {
        public new T? Get(object id, Session? session = null);

        public new IEnumerable<T> GetAll(Session? session = null);

        public void Refresh(T obj);

        public void Save(T obj);

        public void Delete(T obj);
    }
}
