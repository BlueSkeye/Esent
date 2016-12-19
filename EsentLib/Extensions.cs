using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EsentLib.Jet;

namespace EsentLib
{
    /// <summary></summary>
    public static class Extensions
    {
        /// <summary>Make an array of native columndefs from JET_COLUMNDEFs.</summary>
        /// <param name="columns">Columndefs to convert.</param>
        /// <returns>An array of native columndefs.</returns>
        internal static NATIVE_COLUMNDEF[] GetNativecolumndefs(this IList<JET_COLUMNDEF> columns)
        {
            int columnsCount = columns.Count;
            NATIVE_COLUMNDEF[] result = new NATIVE_COLUMNDEF[columnsCount];
            for (int index = 0; index < columnsCount; ++index) {
                result[index] = columns[index].GetNativeColumndef();
            }
            return result;
        }

        /// <summary>Set managed columnids from unmanaged columnids. This also sets the columnids
        /// in the columndefs.</summary>
        /// <param name="columns">The column definitions.</param>
        /// <param name="columnids">The columnids to set.</param>
        /// <param name="nativecolumnids">The native columnids.</param>
        [CLSCompliant(false)]
        public static void SetColumnids(this IList<JET_COLUMNDEF> columns, IList<JET_COLUMNID> columnids,
            IList<uint> nativecolumnids)
        {
            for (int index = 0; index < nativecolumnids.Count; ++index) {
                columnids[index] = new JET_COLUMNID { Value = nativecolumnids[index] };
                columns[index].columnid = columnids[index];
            }
        }

    }
}
