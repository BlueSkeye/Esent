using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EsentLib.Jet;

namespace EsentLib.Api
{
    /// <summary></summary>
    public interface ICursor
    {
        /// <summary>Positions a cursor to an index entry for the record that is associated
        /// with the specified bookmark. The bookmark can be used with any index defined over
        /// a table. The bookmark for a record can be retrieved using
        /// <see cref="IJetInstance.JetGetBookmark"/>.</summary>
        /// <param name="bookmark">The bookmark used to position the cursor.</param>
        /// <param name="bookmarkSize">The size of the bookmark.</param>
        void GotoBookmark(byte[] bookmark, int bookmarkSize);

        /// <summary>Set the current index of a cursor.</summary>
        /// <param name="index">The name of the index to be selected. If this is null or empty
        /// the primary index will be selected.</param>
        /// <param name="grbit">Set index options.</param>
        /// <param name="itagSequence">Sequence number of the multi-valued column value which
        /// will be used to position the cursor on the new index. This parameter is only used
        /// in conjunction with <see cref="SetCurrentIndexGrbit.NoMove"/>. When this parameter
        /// is not present or is set to zero, its value is presumed to be 1.</param>
        /// <returns>An error if the call fails.</returns>
        void SetCurrentIndex(string index, SetCurrentIndexGrbit grbit = SetCurrentIndexGrbit.None,
            int itagSequence = 1);

        /// <summary>Set the current index of a cursor.</summary>
        /// <param name="indexid">The id of the index to select. This id can be obtained using
        /// JetGetIndexInfo or JetGetTableIndexInfo with the <see cref="JET_IdxInfo.IndexId"/>
        /// option.</param>
        /// <param name="grbit">Set index options.</param>
        /// <param name="itagSequence">Sequence number of the multi-valued column value which
        /// will be used to position the cursor on the new index. This parameter is only used
        /// in conjunction with <see cref="SetCurrentIndexGrbit.NoMove"/>. When this parameter
        /// is not present or is set to zero, its value is presumed to be 1.</param>
        /// <returns>An error if the call fails.</returns>
        void SetCurrentIndex(JET_INDEXID indexid, SetCurrentIndexGrbit grbit = SetCurrentIndexGrbit.None,
            int itagSequence = 1);
    }
}
