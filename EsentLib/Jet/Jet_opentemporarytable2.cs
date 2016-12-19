//-----------------------------------------------------------------------
// <copyright file="jet_opentemporarytable2.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace EsentLib.Jet
{
    /// <summary>The native version of the JET_OPENTEMPORARYTABLE2 structure.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules",
        "SA1305:FieldNamesMustNotUseHungarianNotation",
        Justification = "This should match the unmanaged API, which isn't capitalized.")]
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.NamingRules",
        "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter",
        Justification = "This should match the unmanaged API, which isn't capitalized.")]
    internal unsafe struct NATIVE_OPENTEMPORARYTABLE2
    {
        /// <summary>
        /// Size of the structure.
        /// </summary>
        public uint cbStruct;

        /// <summary>
        /// Columns to create.
        /// </summary>
        public NATIVE_COLUMNDEF* prgcolumndef;

        /// <summary>
        /// Number of entries in prgcolumndef.
        /// </summary>
        public uint ccolumn;

        /// <summary>
        /// Optional pointer to unicode index information.
        /// </summary>
        public NATIVE_UNICODEINDEX2* pidxunicode;

        /// <summary>
        /// Table options.
        /// </summary>
        public uint grbit;

        /// <summary>
        /// Pointer to array of returned columnids. This
        /// should have at least ccolumn entries.
        /// </summary>
        public uint* rgcolumnid;

        /// <summary>
        /// Maximum key size.
        /// </summary>
        public uint cbKeyMost;

        /// <summary>
        /// Maximum amount of data used to construct a key.
        /// </summary>
        public uint cbVarSegMac;

        /// <summary>
        /// Returns the tableid of the new table.
        /// </summary>
        public IntPtr tableid;
    }
}
