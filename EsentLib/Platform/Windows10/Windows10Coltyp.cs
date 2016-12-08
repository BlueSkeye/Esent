//-----------------------------------------------------------------------
// <copyright file="Windows10Coltyp.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using EsentLib.Jet;

namespace EsentLib.Platform.Windows10
{
    /// <summary>
    /// Column types that have been added to the Windows 10 version of ESENT.
    /// </summary>
    public static class Windows10Coltyp
    {
        /// <summary>
        /// Unsigned 64-bit number.
        /// </summary>
        public const JET_coltyp UnsignedLongLong = (JET_coltyp)18;
    }
}