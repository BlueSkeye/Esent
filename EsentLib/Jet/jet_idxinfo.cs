//-----------------------------------------------------------------------
// <copyright file="jet_idxinfo.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace EsentLib.Jet
{
    /// <summary>Info levels for retrieve index information with JetGetIndexInfo. and
    /// JetGetTableIndexInfo.</summary>
    public enum JET_IdxInfo
    {
        /// <summary>Returns a <see cref="JET_INDEXLIST"/> structure with information about
        /// the index.</summary>
        Default = 0,

        /// <summary>Returns a <see cref="JET_INDEXLIST"/> structure with information about
        /// the index.</summary>
        List = 1,

        /// <summary>SysTabCursor is obsolete.</summary>
        [Obsolete("This value is not used, and is provided for completeness to match the published header in the SDK.")]
        SysTabCursor = 2,

        /// <summary>OLC is obsolete.</summary>
        [Obsolete("This value is not used, and is provided for completeness to match the published header in the SDK.")]
        OLC = 3,

        /// <summary>Reset OLC is obsolete.</summary>
        [Obsolete("This value is not used, and is provided for completeness to match the published header in the SDK.")]
        ResetOLC = 4,

        /// <summary>Returns an integer with the space usage of the index.</summary>
        SpaceAlloc = 5,

        /// <summary>Returns an integer with the LCID of the index.</summary>
        LCID = 6,

        /// <summary>Langid is obsolete. Use <see cref="LCID"/> instead.</summary>
        [Obsolete("Use JET_IdxInfo.LCID")]
        Langid = 6,

        /// <summary>Returns an integer with the count of indexes in the table.</summary>
        Count = 7,

        /// <summary>Returns a ushort with the value of cbVarSegMac the index was created
        /// with.</summary>
        VarSegMac = 8,

        /// <summary>Returns a <see cref="JET_INDEXID"/> identifying the index.</summary>
        IndexId = 9,

        /// <summary>Introduced in Windows Vista. Returns a ushort with the value of cbKeyMost
        /// the  index was created with.</summary>
        KeyMost = 10,

        // --------- //
        // WINDOWS 7 //
        // --------- //
        /// <summary>Introduced in Windows 7. Returns a <see cref="JET_INDEXCREATE"/> structure
        /// suitable for use by see cref="Api.JetCreateIndex2".</summary>
        /// <remarks>Not currently implemented in this layer.</remarks>
        CreateIndex = 11,

        /// <summary>
        /// Introduced in Windows 7. Returns a JET_INDEXCREATE2 structure (similar to
        /// <see cref="JET_INDEXCREATE"/> structure, but it contains a
        /// <see cref="JET_SPACEHINTS"/> member called pSpacehints). This structure is
        /// suitable for use by see cref="Api.JetCreateIndex2".</summary>
        /// <remarks>Not currently implemented in this layer.</remarks>
        CreateIndex2 = 12,

        // --------- //
        // WINDOWS 8 //
        // --------- //
        /// <summary>Introduced in Windows 8. Returns a JET_INDEXCREATE3 structure. This is
        /// similar to <see cref="JET_INDEXCREATE"/> structure,  but it contains some additional
        /// members, and it also uses a locale name in the <see cref="JET_UNICODEINDEX"/> index
        /// definition, not an LCID. The returned object is suitable for use by
        /// <see cref="IJetTable.CreateIndex(IJetSession, JET_INDEXCREATE[], int)"/>.</summary>
        /// <remarks>Not currently implemented in this layer, but provided for completeness to
        /// match the underlying API layer.</remarks>
        InfoCreateIndex3 = 13,

        /// <summary>Introduced in Windows 8. Returns the locale name of the given index.</summary>
        LocaleName = 14,
    }
}
