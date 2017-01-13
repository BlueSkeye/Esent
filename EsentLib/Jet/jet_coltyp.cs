//-----------------------------------------------------------------------
// <copyright file="jet_coltyp.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace EsentLib.Jet
{
    /// <summary>ESENT column types. This list is extensive.</summary>
    public enum JET_coltyp
    {
        /// <summary>Null column type. Invalid for column creation.</summary>
        Nil = 0,
        /// <summary>True, False or NULL.</summary>
        Bit = 1,
        /// <summary>1-byte integer, unsigned.</summary>
        UnsignedByte = 2,
        /// <summary>2-byte integer, signed.</summary>
        Short = 3,
        /// <summary>4-byte integer, signed.</summary>
        Long = 4,
        /// <summary>8-byte integer, signed.</summary>
        Currency = 5,
        /// <summary>4-byte IEEE single-precisions.</summary>
        IEEESingle = 6,
        /// <summary>8-byte IEEE double-precision.</summary>
        IEEEDouble = 7,
        /// <summary>Integral date, fractional time.</summary>
        DateTime = 8,
        /// <summary>Binary data, up to 255 bytes.</summary>
        Binary = 9,
        /// <summary>Text data, up to 255 bytes.</summary>
        Text = 10,
        /// <summary>Binary data, up to 2GB.</summary>
        LongBinary = 11,
        /// <summary>Text data, up to 2GB.</summary>
        LongText = 12,

        // ----- //
        // VISTA //
        // ----- //
        /// <summary>Unsigned 32-bit number.</summary>
        UnsignedLong = 14,
        /// <summary>Signed 64-bit number.</summary>
        LongLong = 15,
        /// <summary>16-byte GUID.</summary>
        GUID = 16,
        /// <summary>Unsigned 16-bit number.</summary>
        UnsignedShort = 17,

        // ---------- //
        // WINDOWS 10 //
        // ---------- //
        /// <summary>Unsigned 64-bit number.</summary>
        UnsignedLongLong = 18,
    }
}