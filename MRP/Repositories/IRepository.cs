using MRP.System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Repositories
{
    public interface IRepository
    {
        public object? Get(object id, Session? session = null);

        public IEnumerable GetAll(Session? session = null);

        public void Refresh(object obj);

        public void Save(object obj);

        public void Delete(object obj);
    }
}
