﻿//-----------------------------------------------------------------------
// <copyright file="SetColumnHelpers.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
#if MANAGEDESENT_SUPPORTS_SERIALIZATION
using System.Runtime.Serialization.Formatters.Binary;
#endif
using System.Text;

using EsentLib.Api.Data;
using EsentLib.Jet;
using EsentLib.Jet.Types;

namespace EsentLib
{
    /// <summary>Helper methods for the ESENT API. These do data conversion for setting
    /// columns.</summary>
    public static partial class LegacyApi
    {
        /// <summary>Modifies a single column value in a modified record to be inserted
        /// or toupdate the current record.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        /// <param name="encoding">The encoding used to convert the string.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid,
            string data, Encoding encoding)
        {
            SetColumn(sesid, tableid, columnid, data, encoding, SetColumnGrbit.None);
        }

        /// <summary>Modifies a single column value in a modified record to be inserted or to
        /// update the current record.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        /// <param name="encoding">The encoding used to convert the string.</param>
        /// <param name="grbit">SetColumn options.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid,
            string data, Encoding encoding, SetColumnGrbit grbit)
        {
            Helpers.CheckEncodingIsValid(encoding);
            if (null == data) {
                JetSetColumn(sesid, tableid, columnid, null, 0, grbit, null);
                return;
            }
            if (0 == data.Length) {
                JetSetColumn(sesid, tableid, columnid, null, 0, grbit | SetColumnGrbit.ZeroLength, null);
                return;
            }
            if (Encoding.Unicode == encoding) {
                // Optimization for setting Unicode strings.
                unsafe {
                    fixed (char* buffer = data) {
                        InternalApi.JetSetColumn(sesid, tableid, columnid, new IntPtr(buffer),
                            checked(data.Length * sizeof(char)), grbit, null); }
                }
                return;
            }
            if (encoding.GetMaxByteCount(data.Length) <= MemoryCache.ColumnCache.BufferSize) {
                // The encoding output will fix in a cached buffer. Get one to avoid 
                // more memory allocations.
                byte[] buffer = null;

                try {
                    buffer = MemoryCache.ColumnCache.Allocate();
                    unsafe {
                        fixed (char* chars = data) {
                            fixed (byte* bytes = buffer) {
                                int dataSize = encoding.GetBytes(chars, data.Length, bytes, buffer.Length);
                                InternalApi.JetSetColumn(sesid, tableid, columnid, new IntPtr(bytes),
                                    dataSize, grbit, null);
                            }
                        }
                    }
                }
                finally { if (buffer != null) { MemoryCache.ColumnCache.Free(ref buffer); } }
                return;
            }
            byte[] encodedBytes = encoding.GetBytes(data);
            JetSetColumn(sesid, tableid, columnid, encodedBytes, encodedBytes.Length, grbit, null);
            return;
        }

        /// <summary>Modifies a single column value in a modified record to be inserted or to
        /// update the current record.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid,
            byte[] data)
        {
            SetColumn(sesid, tableid, columnid, data, SetColumnGrbit.None);
        }

        /// <summary>Modifies a single column value in a modified record to be inserted or to
        /// update the current record.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        /// <param name="grbit">SetColumn options.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid,
            byte[] data, SetColumnGrbit grbit)
        {
            if ((null != data) && (0 == data.Length)) {
                grbit |= SetColumnGrbit.ZeroLength;
            }
            int dataLength = (null == data) ? 0 : data.Length;
            JetSetColumn(sesid, tableid, columnid, data, dataLength, grbit, null);
        }

        /// <summary>Modifies a single column value in a modified record to be inserted or to
        /// update the current record.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid,
            bool data)
        {
            SetColumn(sesid, tableid, columnid, data ? (byte)0xff : (byte)0x0);
        }

        /// <summary>Modifies a single column value in a modified record to be inserted or to
        /// update the current record.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid,
            byte data)
        {
            unsafe {
                const int DataSize = sizeof(byte);
                InternalApi.JetSetColumn(sesid, tableid, columnid, new IntPtr(&data), DataSize,
                    SetColumnGrbit.None, null);
            }
        }

        /// <summary>Modifies a single column value in a modified record to be inserted or to
        /// update the current record.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid,
            short data)
        {
            unsafe {
                const int DataSize = sizeof(short);
                InternalApi.JetSetColumn(sesid, tableid, columnid, new IntPtr(&data), DataSize,
                    SetColumnGrbit.None, null);
            }
        }

        /// <summary>Modifies a single column value in a modified record to be inserted or to
        /// update the current record.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid,
            int data)
        {
            unsafe {
                const int DataSize = sizeof(int);
                InternalApi.JetSetColumn(sesid, tableid, columnid, new IntPtr(&data), DataSize,
                    SetColumnGrbit.None, null);
            }
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, long data)
        {
            unsafe {
                const int DataSize = sizeof(long);
                InternalApi.JetSetColumn(sesid, tableid, columnid, new IntPtr(&data), DataSize,
                    SetColumnGrbit.None, null);
            }
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, Guid data)
        {
            unsafe {
                const int DataSize = 16; // sizeof(Guid) isn't a compile-time constant
                InternalApi.JetSetColumn(sesid, tableid, columnid, new IntPtr(&data), DataSize,
                    SetColumnGrbit.None, null);
            }
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, DateTime data)
        {
            SetColumn(sesid, tableid, columnid, data.ToOADate());
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, float data)
        {
            unsafe
            {
                const int DataSize = sizeof(float);
                var pointer = new IntPtr(&data);
                InternalApi.JetSetColumn(sesid, tableid, columnid, pointer, DataSize, SetColumnGrbit.None, null);
            }
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, double data)
        {
            unsafe
            {
                const int DataSize = sizeof(double);
                var pointer = new IntPtr(&data);
                InternalApi.JetSetColumn(sesid, tableid, columnid, pointer, DataSize, SetColumnGrbit.None, null);
            }
        }

        /// <summary>
        /// Perform atomic addition on one column. The column must be of type
        /// <see cref="JET_coltyp.Long"/>. This function allows multiple sessions to update the
        /// same record concurrently without conflicts.
        /// </summary>
        /// <remarks>
        /// This method wraps <see cref="JetEscrowUpdate"/>.
        /// </remarks>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update.</param>
        /// <param name="columnid">The column to update. This must be an escrow-updatable column.</param>
        /// <param name="delta">The delta to apply to the column.</param>
        /// <returns>The current value of the column as stored in the database (versioning is ignored).</returns>
        public static int EscrowUpdate(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, int delta)
        {
            var previousValue = new byte[sizeof(int)];
            int actualPreviousValueLength;
            JetEscrowUpdate(
                sesid,
                tableid,
                columnid,
                BitConverter.GetBytes(delta),
                sizeof(int),
                previousValue,
                previousValue.Length,
                out actualPreviousValueLength,
                EscrowUpdateGrbit.None);
            Debug.Assert(
                previousValue.Length == actualPreviousValueLength,
                "Unexpected previous value length. Expected an Int32");
            return BitConverter.ToInt32(previousValue, 0);
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        [CLSCompliant(false)]
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, ushort data)
        {
            unsafe
            {
                const int DataSize = sizeof(ushort);
                var pointer = new IntPtr(&data);
                InternalApi.JetSetColumn(sesid, tableid, columnid, pointer, DataSize, SetColumnGrbit.None, null);
            }
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        [CLSCompliant(false)]
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, uint data)
        {
            unsafe
            {
                const int DataSize = sizeof(uint);
                var pointer = new IntPtr(&data);
                InternalApi.JetSetColumn(sesid, tableid, columnid, pointer, DataSize, SetColumnGrbit.None, null);
            }
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        [CLSCompliant(false)]
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, ulong data)
        {
            unsafe
            {
                const int DataSize = sizeof(ulong);
                var pointer = new IntPtr(&data);
                InternalApi.JetSetColumn(sesid, tableid, columnid, pointer, DataSize, SetColumnGrbit.None, null);
            }
        }

#if MANAGEDESENT_SUPPORTS_SERIALIZATION
        /// <summary>
        /// Write a serialized form of an object to a column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to write to. An update should be prepared.</param>
        /// <param name="columnid">The column to write to.</param>
        /// <param name="value">The object to write. The object must be serializable.</param>
        [SuppressMessage("Exchange.Security", "EX0043:DoNotUseBinarySoapFormatter", Justification = "Suppress warning in current code base.The usage has already been verified.")]
        public static void SerializeObjectToColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, object value)
        {
            if (null == value)
            {
                LegacyApi.SetColumn(sesid, tableid, columnid, null);
            }
            else
            {
                using (var stream = new ColumnStream(sesid, tableid, columnid))
                {
                    var serializer = new BinaryFormatter
                    {
                        Context = new StreamingContext(StreamingContextStates.Persistence)
                    };
                    serializer.Serialize(stream, value);
                }
            }
        }
#endif

        /// <summary>
        /// Sets columns from ColumnValue objects.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="values">The values to set.</param>
        public static void SetColumns(JET_SESID sesid, JET_TABLEID tableid, params ColumnValue[] values)
        {
            if (null == values)
            {
                throw new ArgumentNullException("values");
            }

            if (0 == values.Length)
            {
                throw new ArgumentOutOfRangeException("values", values.Length, "must have at least one value");
            }

            unsafe
            {
                NATIVE_SETCOLUMN* nativeSetcolumns = stackalloc NATIVE_SETCOLUMN[values.Length];
                EsentExceptionHelper.Check(values[0].SetColumns(sesid, tableid, values, nativeSetcolumns, 0));
            }
        }
    }
}
