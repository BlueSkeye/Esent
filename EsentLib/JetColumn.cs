using EsentLib.Api;

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
