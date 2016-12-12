using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsentLib
{
    /// <summary></summary>
    public class JetDatabase : IJetDatabase
    {
        internal JetDatabase(JET_DBID dbid)
        {
            _dbid = dbid;
        }

        private JET_DBID _dbid;
    }
}
