using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsentLib
{
    /// <summary></summary>
    public class JetColumn : IJetColumn
    {
        internal JetColumn(IJetTable owner, JET_COLUMNID id)
        {
            _owner = owner;
            _id = id;
        }

        private IJetTable _owner;
        private JET_COLUMNID _id;
    }
}
