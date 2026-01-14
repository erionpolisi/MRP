using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.System
{
    public interface __IVerifiable
    {
        public object? __InternalID { get; set; }

        public void __VerifySession(Session? session = null);

        public void __EndEdit();

        public void __EnsureAdmin();

        public void __EnsureAdminOrOwner(string? owner);
    }
}
