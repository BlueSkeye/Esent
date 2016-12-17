//-----------------------------------------------------------------------
// <copyright file="Windows8IdxInfo.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using EsentLib.Jet;
using Win7 = EsentLib.Platform.Windows7;

namespace EsentLib.Platform.Windows8
{
    /// <summary>
    /// Index info levels that have been added to the Windows 8 version of ESENT.
    /// </summary>
    /// <seealso cref="JET_IdxInfo"/>
    /// <seealso cref="Win7.Windows7IdxInfo"/>
    public static class Windows8IdxInfo
    {
        /// <summary>
        /// Introduced in Windows 8. Returns a JET_INDEXCREATE3 structure. This is similar to
        /// <see cref="JET_INDEXCREATE"/> structure,  but it contains some additional
        /// members, and it also uses a locale name in the <see cref="JET_UNICODEINDEX"/>
        /// index definition, not an LCID.
        /// The returned object is suitable for use by <see cref="IJetTable.CreateIndex(JET_SESID, JET_INDEXCREATE[], int)"/>.
        /// </summary>
        /// <remarks>Not currently implemented in this layer, but provided for completeness to
        /// match the underlying API layer.</remarks>
        internal const JET_IdxInfo InfoCreateIndex3 = (JET_IdxInfo)13;

        /// <summary>
        /// Introduced in Windows 8. Returns the locale name of the given index.
        /// </summary>
        internal const JET_IdxInfo LocaleName = (JET_IdxInfo)14;
    }
}
