//-----------------------------------------------------------------------
// <copyright file="Update.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

using EsentLib.Api;
using EsentLib.Implementation;
using EsentLib.Jet;

namespace EsentLib
{
    /// <summary>A cursor allowing for reading and writing a table.</summary>
    [CLSCompliant(false)]
    public class JetCursor : EsentResource, IJetCursor
    {
        internal JetCursor(IJetSession session, JET_TABLEID cursorId, JET_prep prep = (JET_prep)(-1))
        {
            if (JET_prep.Cancel == prep) {
                throw new ArgumentException("Cannot create an Update for JET_prep.Cancel", "prep");
            }
            this._session = (JetSession)session;
            this._cursorId = cursorId;
            this.prep = prep;
            ResourceWasAllocated();
        }

        /// <summary>Get the underlying table/</summary>
        public IJetTable Table { get; private set; }

        /// <summary>Get the name of the current index of a given cursor.</summary>
        /// <remarks>This accessor is also used to later re-select that index as the current
        /// index. It can also be used to discover the properties of that index using
        /// JetGetTableIndexInfo. The returned name of the index will be an empty string if
        /// the current index is the clustered index and no primary index was explicitly
        /// defined.</remarks>
        public string CurrentIndex
        {
            get
            {
                Tracing.TraceFunctionCall("CurrentIndex");
                int maxNameLength = Constants.NameMost + 1;
                StringBuilder name = new StringBuilder(maxNameLength);
                int returnCode = NativeMethods.JetGetCurrentIndex(_session.Id, _cursorId.Value,
                    name, checked((uint)maxNameLength));
                Tracing.TraceResult(returnCode);
                EsentExceptionHelper.Check(returnCode);
                string indexName = name.ToString();
                return StringCache.TryToIntern(indexName);
            }
        }

        /// <summary>Cancel the update.</summary>
        public void Cancel()
        {
            this.CheckObjectIsNotDisposed();
            if (!this.HasResource) { throw new InvalidOperationException("Not in an update"); }
            Tracing.TraceFunctionCall("Cancel");
            int returnCode = NativeMethods.JetPrepareUpdate(_session.Id, _cursorId.Value,
                unchecked((uint)JET_prep.Cancel));
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            this.ResourceWasReleased();
        }

        /// <summary>Close an open table.</summary>
        public void Close()
        {
            Tracing.TraceFunctionCall("Close");
            int returnCode = NativeMethods.JetCloseTable(_session.Id, _cursorId.Value);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Create the nullable return value.</summary>
        /// <typeparam name="T">The (struct) type to return.</typeparam>
        /// <param name="data">The data retrieved from the column.</param>
        /// <param name="dataSize">The size of the data.</param>
        /// <param name="wrn">The warning code from esent.</param>
        /// <param name="actualDataSize">The actual size of the data retireved fomr esent.</param>
        /// <returns>A nullable struct of type T.</returns>
        private static T? CreateReturnValue<T>(T data, int dataSize, JET_wrn wrn, int actualDataSize)
            where T : struct
        {
            if (JET_wrn.ColumnNull == wrn) { return new T?(); }
            Helpers.CheckDataSize(dataSize, actualDataSize);
            return data;
        }

        /// <summary></summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="skipRecordFilter">This delegate, if not null, will be invoked on each
        /// record. It has to return true if the current record should be skipped and not be
        /// returned by the enumerator.</param>
        /// <param name="retriever"></param>
        /// <returns></returns>
        public IEnumerable<T> Enumerate<T>(FilterDelegate skipRecordFilter, ItemRetrieverDelegate<T> retriever)
        {
            if (!TryMoveFirst()) { yield break; }
            do {
                if ((null == skipRecordFilter) || !skipRecordFilter()) {
                    yield return retriever();
                }
            } while (TryMoveNext());
            yield break;
        }

        /// <summary>Positions a cursor to an index entry for the record that is associated
        /// with the specified bookmark. The bookmark can be used with any index defined over
        /// a table. The bookmark for a record can be retrieved using
        /// <see cref="IJetInstance.JetGetBookmark"/>.</summary>
        /// <param name="bookmark">The bookmark used to position the cursor.</param>
        /// <param name="bookmarkSize">The size of the bookmark.</param>
        public void GotoBookmark(byte[] bookmark, int bookmarkSize)
        {
            Tracing.TraceFunctionCall("GotoBookmark");
            Helpers.CheckNotNull(bookmark, "bookmark");
            Helpers.CheckDataSize(bookmark, bookmarkSize, "bookmarkSize");
            int returnCode = NativeMethods.JetGotoBookmark(this._session.Id, _cursorId.Value,
                bookmark, checked((uint)bookmarkSize));
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return;
        }

        /// <summary>Constructs search keys that may then be used by JetSeek and JetSetIndexRange.
        /// </summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(byte[] data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            if (null == data) { MakeKey(null, 0, grbit); }
            else if (0 == data.Length)
            {
                MakeKey(data, data.Length, grbit | MakeKeyGrbit.KeyDataZeroLength);
            }
            else { MakeKey(data, data.Length, grbit); }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange.
        /// </summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(string data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            MakeKey(data, Encoding.Unicode, grbit);
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange.
        /// </summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="encoding">The encoding used to convert the string.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(string data, Encoding encoding, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            Helpers.CheckEncodingIsValid(encoding);
            if (null == data) { MakeKey(null, 0, grbit); }
            else if (0 == data.Length)
            {
                MakeKey(null, 0, grbit | MakeKeyGrbit.KeyDataZeroLength);
            }
            else if (Encoding.Unicode == encoding)
            {
                // Optimization for Unicode strings
                unsafe
                {
                    fixed (char* buffer = data)
                    {
                        MakeKey(new IntPtr(buffer), checked(data.Length * sizeof(char)), grbit);
                    }
                }
            }
            else
            {
                // Convert the string using a cached column buffer. The column buffer is far larger
                // than the maximum key size, so any data truncation here won't matter.
                byte[] buffer = null;
                try
                {
                    buffer = MemoryCache.ColumnCache.Allocate();
                    int dataSize;
                    unsafe
                    {
                        fixed (char* chars = data)
                        fixed (byte* bytes = buffer)
                        {
                            dataSize = encoding.GetBytes(chars, data.Length, bytes, buffer.Length);
                        }
                    }
                    MakeKey(buffer, dataSize, grbit);
                }
                finally { if (buffer != null) { MemoryCache.ColumnCache.Free(ref buffer); } }
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange.
        /// </summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(bool data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            byte b = data ? (byte)0xff : (byte)0x0;
            MakeKey(b, grbit);
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange.</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(byte data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe
            {
                const int DataSize = sizeof(byte);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange.</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(short data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe
            {
                const int DataSize = sizeof(short);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(int data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe
            {
                const int DataSize = sizeof(int);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(long data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe
            {
                const int DataSize = sizeof(long);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(Guid data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe
            {
                const int DataSize = 16 /* sizeof(Guid) */;
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(DateTime data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            MakeKey(data.ToOADate(), grbit);
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(float data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe
            {
                const int DataSize = sizeof(float);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(double data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe
            {
                const int DataSize = sizeof(double);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        // [CLSCompliant(false)]
        public void MakeKey(ushort data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe
            {
                const int DataSize = sizeof(ushort);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        // [CLSCompliant(false)]
        public void MakeKey(uint data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe
            {
                const int DataSize = sizeof(uint);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        // [CLSCompliant(false)]
        public void MakeKey(ulong data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe
            {
                const int DataSize = sizeof(ulong);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs search keys that may then be used by JetSeek and JetSetIndexRange.</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="dataSize">Size of the data.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(byte[] data, int dataSize, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            if (((null == data) && (0 != dataSize))
                || ((null != data) && (dataSize > data.Length)))
            {
                throw new ArgumentOutOfRangeException(string.Format(
                    "dataSize {0} cannot be greater than the length of the data", dataSize));
            }
            unsafe { fixed (byte* pointer = data) { MakeKey(data, dataSize, grbit); } }
        }

        /// <summary>Constructs search keys that may then be used by JetSeek and JetSetIndexRange.</summary>
        /// <param name="pointer">Column data for the current key column of the current index.</param>
        /// <param name="dataSize">Size of the data.</param>
        /// <param name="grbit">Key options.</param>
        private void MakeKey(IntPtr pointer, int dataSize, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            Tracing.TraceFunctionCall("MakeKey");
            Helpers.CheckNotNegative(dataSize, "dataSize");
            int returnCode = NativeMethods.JetMakeKey(_session.Id, _cursorId.Value,
                pointer, checked((uint)dataSize), unchecked((uint)grbit));
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Navigate through an index. The cursor can be positioned at the start or
        /// end of the index and moved backwards and forwards by a specified number of index
        /// entries.</summary>
        /// <param name="numRows">An offset which indicates how far to move the cursor.</param>
        /// <param name="grbit">Move options.</param>
        /// <returns>An error if the call fails.</returns>
        public int Move(int numRows, MoveGrbit grbit = MoveGrbit.None)
        {
            Tracing.TraceFunctionCall("Move");
            int returnCode = NativeMethods.JetMove(_session.Id, this._cursorId.Value, numRows,
                unchecked((uint)grbit));
            return Tracing.TraceResult(returnCode);
        }

        /// <summary>Called when the transaction is being disposed while active. This should
        /// rollback the transaction.</summary>
        protected override void ReleaseResource()
        {
            this.Cancel();
        }

        /// <summary>Retrieves a single column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.
        /// Alternatively, this function can retrieve a column from a record being created in
        /// the cursor copy buffer. This function can also retrieve column data from an index
        /// entry that references the current record. In addition to retrieving the actual
        /// column value, JetRetrieveColumn can also be used to retrieve the size of a column,
        /// before retrieving the column data itself so that application buffers can be sized
        /// appropriately.</summary>
        /// <remarks>The RetrieveColumnAs functions provide datatype-specific retrieval
        /// functions.</remarks>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="data">The data buffer to be retrieved into.</param>
        /// <param name="dataSize">The size of the data buffer.</param>
        /// <param name="actualDataSize">Returns the actual size of the data buffer.</param>
        /// <param name="grbit">Retrieve column options.</param>
        /// <param name="retinfo">If pretinfo is give as NULL then the function behaves as
        /// though an itagSequence of 1 and an ibLongValue of 0 (zero) were given. This causes
        /// column retrieval to retrieve the first value of a multi-valued column, and to
        /// retrieve long data at offset 0 (zero).</param>
        /// <returns>An error or warning.</returns>
        public JET_wrn RetrieveColumn(JET_COLUMNID columnid, IntPtr data, int dataSize,
            out int actualDataSize, RetrieveColumnGrbit grbit, JET_RETINFO retinfo)
        {
            return RetrieveColumn(columnid.Value, data, dataSize, out actualDataSize, grbit,
                retinfo);
        }

        /// <summary>Retrieves a single column value from the current record. The record
        /// is that record associated with the index entry at the current position of the
        /// cursor. Alternatively, this function can retrieve a column from a record being
        /// created in the cursor copy buffer. This function can also retrieve column data
        /// from an index entry that references the current record. In addition to retrieving
        /// the actual column value, JetRetrieveColumn can also be used to retrieve the size
        /// of a column, before retrieving the column data itself so that application buffers
        /// can be sized appropriately.</summary>
        /// <remarks>This is an internal method that takes a buffer offset as well as size.</remarks>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="data">The data buffer to be retrieved into.</param>
        /// <param name="dataOffset">Offset into the data buffer to read data into.</param>
        /// <param name="actualDataSize">Returns the actual size of the data buffer.</param>
        /// <param name="grbit">Retrieve column options.</param>
        /// <param name="retinfo">If pretinfo is give as NULL then the function behaves
        /// as though an itagSequence of 1 and an ibLongValue of 0 (zero) were given.
        /// This causes column retrieval to retrieve the first value of a multi-valued
        /// column, and to retrieve long data at offset 0 (zero).</param>
        /// <returns>An ESENT warning code.</returns>
        public JET_wrn RetrieveColumn(JET_COLUMNID columnid, byte[] data, int dataOffset,
            out int actualDataSize, RetrieveColumnGrbit grbit, JET_RETINFO retinfo)
        {
            return RetrieveColumn(columnid, data, (null == data) ? 0 : data.Length, dataOffset,
                out actualDataSize, grbit, retinfo);
        }

        /// <summary>Retrieves a single column value from the current record. The record
        /// is that record associated with the index entry at the current position of the
        /// cursor. Alternatively, this function can retrieve a column from a record being
        /// created in the cursor copy buffer. This function can also retrieve column data
        /// from an index entry that references the current record. In addition to retrieving
        /// the actual column value, JetRetrieveColumn can also be used to retrieve the size
        /// of a column, before retrieving the column data itself so that application buffers
        /// can be sized appropriately.</summary>
        /// <remarks>This is an internal method that takes a buffer offset as well as size.</remarks>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="data">The data buffer to be retrieved into.</param>
        /// <param name="dataSize">The size of the data buffer.</param>
        /// <param name="dataOffset">Offset into the data buffer to read data into.</param>
        /// <param name="actualDataSize">Returns the actual size of the data buffer.</param>
        /// <param name="grbit">Retrieve column options.</param>
        /// <param name="retinfo">If pretinfo is give as NULL then the function behaves
        /// as though an itagSequence of 1 and an ibLongValue of 0 (zero) were given.
        /// This causes column retrieval to retrieve the first value of a multi-valued
        /// column, and to retrieve long data at offset 0 (zero).</param>
        /// <returns>An ESENT warning code.</returns>
        public JET_wrn RetrieveColumn(JET_COLUMNID columnid, byte[] data, int dataSize,
            int dataOffset, out int actualDataSize, RetrieveColumnGrbit grbit,
            JET_RETINFO retinfo)
        {
            if ((0 > dataOffset)
                || ((null != data) && (0 != dataSize) && (dataOffset >= data.Length))
                || ((null == data) && (0 != dataOffset)))
            {
                throw new ArgumentOutOfRangeException(
                    string.Format("dataOffset {0} must be inside the data buffer", dataOffset));
            }
            if ((null == data && dataSize > 0) || (null != data && dataSize > data.Length)) {
                throw new ArgumentOutOfRangeException(
                    string.Format("dataSize {0} cannot be greater than the length of the data", dataSize));
            }
            unsafe {
                fixed (byte* pointer = data) {
                    return RetrieveColumn(columnid.Value, new IntPtr(pointer + dataOffset),
                        dataSize, out actualDataSize, grbit, retinfo);
                }
            }
        }

        /// <summary>Retrieves a single column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.
        /// Alternatively, this function can retrieve a column from a record being created in
        /// the cursor copy buffer. This function can also retrieve column data from an index
        /// entry that references the current record. In addition to retrieving the actual
        /// column value, JetRetrieveColumn can also be used to retrieve the size of a column,
        /// before retrieving the column data itself so that application buffers can be sized
        /// appropriately.</summary>
        /// <remarks>The RetrieveColumnAs functions provide datatype-specific retrieval
        /// functions.</remarks>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="data">The data buffer to be retrieved into.</param>
        /// <param name="dataSize">The size of the data buffer.</param>
        /// <param name="actualDataSize">Returns the actual size of the data buffer.</param>
        /// <param name="grbit">Retrieve column options.</param>
        /// <param name="retinfo">If pretinfo is give as NULL then the function behaves as
        /// though an itagSequence of 1 and an ibLongValue of 0 (zero) were given. This causes
        /// column retrieval to retrieve the first value of a multi-valued column, and to
        /// retrieve long data at offset 0 (zero).</param>
        /// <returns>An error or warning.</returns>
        public JET_wrn RetrieveColumn(uint columnid, IntPtr data, int dataSize,
            out int actualDataSize, RetrieveColumnGrbit grbit, JET_RETINFO retinfo)
        {
            Tracing.TraceFunctionCall("RetrieveColumn");
            Helpers.CheckNotNegative(dataSize, "dataSize");
            int returnCode;
            uint bytesActual = 0;
            if (null != retinfo)
            {
                NATIVE_RETINFO nativeRetinfo = retinfo.GetNativeRetinfo();
                returnCode = NativeMethods.JetRetrieveColumn(_session.Id, _cursorId.Value,
                    columnid, data, checked((uint)dataSize), out bytesActual,
                    unchecked((uint)grbit), ref nativeRetinfo);
                retinfo.SetFromNativeRetinfo(nativeRetinfo);
            }
            else
            {
                returnCode = NativeMethods.JetRetrieveColumn(_session.Id, _cursorId.Value,
                    columnid, data, checked((uint)dataSize), out bytesActual,
                    unchecked((uint)grbit), IntPtr.Zero);
            }
            Tracing.TraceResult(returnCode);
            actualDataSize = checked((int)bytesActual);
            return EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Retrieves a single column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.
        /// Alternatively, this function can retrieve a column from a record being created in
        /// the cursor copy buffer. This function can also retrieve column data from an index
        /// entry that references the current record. In addition to retrieving the 
        /// column value, JetRetrieveColumn can also be used to retrieve the size of a column,
        /// before retrieving the column data itself so that application buffers can be sized
        /// appropriately.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieve column options.</param>
        /// <param name="retinfo">If pretinfo is give as NULL then the function behaves as
        /// though an itagSequence of 1 and an ibLongValue of 0 (zero) were given. This causes
        /// column retrieval to retrieve the first value of a multi-valued column, and to
        /// retrieve long data at offset 0 (zero).</param>
        /// <returns>The data retrieved from the column. Null if the column is null.</returns>
        public byte[] RetrieveColumn(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None, JET_RETINFO retinfo = null)
        {
            // We expect most column values retrieved this way to be small (retrieving a 1GB LV as one
            // chunk is a bit extreme!). Allocate a cached buffer and use that, allocating a larger one
            // if needed.
            byte[] cache = null;
            byte[] data;

            try {
                cache = MemoryCache.ColumnCache.Allocate();
                data = cache;
                int dataSize;
                //JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid,
                // byte[] data, int dataSize, int dataOffset, out int actualDataSize,
                // RetrieveColumnGrbit grbit, JET_RETINFO retinfo

                JET_wrn wrn = RetrieveColumn(columnid, data, data.Length, out dataSize,
                    grbit, retinfo);
                if (JET_wrn.ColumnNull == wrn) {
                    // null column
                    data = null;
                }
                else if (JET_wrn.Success == wrn) {
                    data = MemoryCache.Duplicate(data, dataSize);
                }
                else {
                    // there is more data to retrieve
                    Debug.Assert(JET_wrn.BufferTruncated == wrn, "Unexpected warning: " + wrn.ToString());
                    data = new byte[dataSize];
                    wrn = RetrieveColumn(columnid, data, data.Length, out dataSize, grbit, retinfo);
                    if (JET_wrn.BufferTruncated == wrn) {
                        string error = string.Format(CultureInfo.CurrentCulture,
                            "Column size changed from {0} to {1}. The record was probably updated by another thread.",
                            data.Length, dataSize);
                        Trace.TraceError(error);
                        throw new InvalidOperationException(error);
                    }
                }
            }
            finally { if (cache != null) { MemoryCache.ColumnCache.Free(ref cache); } }
            return data;
        }

        /// <summary>Retrieves a boolean column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a boolean. Null if the column is null.</returns>
        public bool? RetrieveColumnAsBoolean(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            byte? b = RetrieveColumnAsByte(columnid, grbit);
            return (b.HasValue) ? (0 != b.Value) :new bool?();
        }

        /// <summary>Retrieves a byte column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a byte. Null if the column is null.</returns>
        public byte? RetrieveColumnAsByte(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) {  // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(byte);
                byte data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid.Value, pointer, DataSize, out actualDataSize,
                    grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves a byte column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="length"></param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a byte. Null if the column is null.</returns>
        public byte[] RetrieveColumnAsByteArray(JET_COLUMNID columnid, int length = 0,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) { // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }

            if (0 == length) {
                uint actualSize = 0;
                JET_wrn wrn = (JET_wrn)NativeMethods.JetRetrieveColumn(_session.Id, _cursorId.Value,
                    columnid.Value, IntPtr.Zero, 0, out actualSize, unchecked((uint)grbit),
                    IntPtr.Zero);
                if (JET_wrn.BufferTruncated != wrn) {
                    EsentExceptionHelper.Check((int)wrn);
                }
                length = checked((int)actualSize);
                if (0 == length) { return null; }
            }
            unsafe {
                byte[] data = new byte[length];
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid, data, 0, out actualDataSize, grbit, null);
                if (JET_wrn.ColumnNull == wrn) { return null; }
                Helpers.CheckDataSize(length, actualDataSize);
                return data;
            }
        }

        /// <summary>Retrieves a datetime column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a datetime. Null if the column is null.</returns>
        public DateTime? RetrieveColumnAsDateTime(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // Internally DateTime is stored in OLE Automation format
            double? oadate = RetrieveColumnAsDouble(columnid, grbit);
            return (oadate.HasValue)
                ? Conversions.ConvertDoubleToDateTime(oadate.Value)
                : new DateTime?();
        }

        /// <summary>Retrieves a double column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a double. Null if the column is null.</returns>
        public double? RetrieveColumnAsDouble(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) {  // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(double);
                double data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid.Value, pointer, DataSize, out actualDataSize,
                    grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves a float column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a float. Null if the column is null.</returns>
        public float? RetrieveColumnAsFloat(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) {  // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(float);
                float data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid.Value, pointer, DataSize,
                    out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves an int16 column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a short. Null if the column is null.</returns>
        public short? RetrieveColumnAsInt16(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) {  // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(short);
                short data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid.Value, pointer, DataSize,
                    out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves a guid column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a guid. Null if the column is null.</returns>
        public Guid? RetrieveColumnAsGuid(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) {  // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = 16;
                Guid data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid.Value, pointer, DataSize, out actualDataSize,
                    grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves an int32 column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an int. Null if the column is null.</returns>
        public int? RetrieveColumnAsInt32(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            return RetrieveColumnAsInt32(columnid.Value, grbit);
        }

        /// <summary>Retrieves an int32 column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an int. Null if the column is null.</returns>
        public int? RetrieveColumnAsInt32(uint columnid, RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) {  // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(int);
                int data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid, pointer, DataSize,
                    out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves a single column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a long. Null if the column is null.</returns>
        public long? RetrieveColumnAsInt64(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) { // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(long);
                long data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid.Value, pointer, DataSize, out actualDataSize,
                    grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves a string column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a string. Null if the column is null.</returns>
        public string RetrieveColumnAsString(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            Debug.Assert((grbit & (RetrieveColumnGrbit)0x00020000) == 0,
                "UnpublishedGrbits.RetrieveAsRefIfNotInRecord is not supported.");
            // 512 Unicode characters (1kb on stack)
            const int BufferSize = 512;
            unsafe {
                char* buffer = stackalloc char[BufferSize];
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid.Value, new IntPtr(buffer), BufferSize * sizeof(char),
                    out actualDataSize, grbit, null);
                if (JET_wrn.ColumnNull == wrn) { return null; }
                if (JET_wrn.Success == wrn) {
                    ////return StringCache.GetString(buffer, 0, actualDataSize);
                    return new string(buffer, 0, actualDataSize / sizeof(char));
                }
                Debug.Assert(JET_wrn.BufferTruncated == wrn, "Unexpected warning code");
                // Create a fake string of the appropriate size and then fill it in.
                var s = new string('\0', actualDataSize / sizeof(char));
                fixed (char* p = s) {
                    int newDataSize;
                    wrn = this.RetrieveColumn(columnid.Value, new IntPtr(p), actualDataSize, out newDataSize,
                        grbit, null);
                    if (JET_wrn.BufferTruncated != wrn) { return s; }
                    string error = string.Format(CultureInfo.CurrentCulture,
                        "Column size changed from {0} to {1}. The record was probably updated by another thread.",
                        actualDataSize, newDataSize);
                    Trace.TraceError(error);
                    throw new InvalidOperationException(error);
                }
            }
        }

        /// <summary>Retrieves a uint16 column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an UInt16. Null if the column is null.</returns>
        // [CLSCompliant(false)]
        public ushort? RetrieveColumnAsUInt16(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) { // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(ushort);
                ushort data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid.Value, pointer, DataSize, out actualDataSize,
                    grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves a uint32 column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an UInt32. Null if the column is null.</returns>
        // [CLSCompliant(false)]
        public uint? RetrieveColumnAsUInt32(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) { // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(uint);
                uint data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid.Value, pointer, DataSize, out actualDataSize,
                    grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves a uint64 column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an UInt64. Null if the column is null.</returns>
        // [CLSCompliant(false)]
        public ulong? RetrieveColumnAsUInt64(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) { // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(ulong);
                ulong data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid.Value, pointer, DataSize, out actualDataSize,
                    grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves the size of a single column value from the current record. The
        /// record is that record associated with the index entry at the current position of
        /// the cursor. Alternatively, this function can retrieve a column from a record being
        /// created in the cursor copy buffer. This function can also retrieve column data
        /// from an index entry that references the current record.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="itagSequence">The sequence number of value in a multi-valued column.
        /// The array of values is one-based. The first value is sequence 1, not 0. If the
        /// record column has only one value then 1 should be passed as the itagSequence.</param>
        /// <param name="grbit">Retrieve column options.</param>
        /// <returns>The size of the column. 0 if the column is null.</returns>
        public int? RetrieveColumnSize(JET_COLUMNID columnid, int itagSequence,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) { // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            var retinfo = new JET_RETINFO { itagSequence = itagSequence };
            int dataSize;
            JET_wrn wrn = RetrieveColumn(columnid, null, 0, out dataSize,
                grbit, retinfo);
            return (JET_wrn.ColumnNull == wrn) ? (int?)null : dataSize;
        }

        /// <summary>Update the tableid.</summary>
        /// <param name="bookmark">Returns the bookmark of the updated record. This can be null.</param>
        /// <param name="bookmarkSize">The size of the bookmark buffer.</param>
        /// <returns>Returns the actual size of the bookmark.</returns>
        /// <remarks>Save is the final step in performing an insert or an update. The update is
        /// begun by calling creating an Update object and then by calling JetSetColumn or
        /// JetSetColumns one or more times to set the record state. Finally, Update is called
        /// to complete the update operation. Indexes are updated only by Update or and not
        /// during JetSetColumn or JetSetColumns.</remarks>
        public int Save(byte[] bookmark = null, int bookmarkSize = 0)
        {
            this.CheckObjectIsNotDisposed();
            if (!this.HasResource) { throw new InvalidOperationException("Not in an update"); }
            try { return this.Update(bookmark); }
            finally { this.ResourceWasReleased(); }
        }

        /// <summary>Update the tableid and position the tableid on the record that was modified.
        /// This can be useful when inserting a record because by default the tableid remains
        /// in its old location.</summary>
        /// <remarks>Save is the final step in performing an insert or an update. The update
        /// is begun by calling creating an Update object and then by calling JetSetColumn or
        /// JetSetColumns one or more times to set the record state. Finally, Update is called
        /// to complete the update operation. Indexes are updated only by Update or and not
        /// during JetSetColumn or JetSetColumns.</remarks>
        public void SaveAndGotoBookmark()
        {
            byte[] bookmark = null;
            try {
                bookmark = MemoryCache.BookmarkCache.Allocate();
                int actualBookmarkSize = Save(bookmark, bookmark.Length);
                GotoBookmark(bookmark, actualBookmarkSize);
            }
            finally { if (bookmark != null) { MemoryCache.BookmarkCache.Free(ref bookmark); } }
        }

        // Also see <seealso cref="TrySeek"/>.
        /// <summary>Efficiently positions a cursor to an index entry that matches the search
        /// criteria specified by the search key in that cursor and the specified inequality.
        /// A search key must have been previously constructed using IJetTable.MakeKey.</summary>
        /// <param name="grbit">Seek options.</param>
        /// <returns>An ESENT warning.</returns>
        public void Seek(SeekGrbit grbit = SeekGrbit.SeekEQ)
        {
            Tracing.TraceFunctionCall("Seek");
            int returnCode = NativeMethods.JetSeek(_session.Id, _cursorId.Value, unchecked((uint)grbit));
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Set the current index of a cursor.</summary>
        /// <param name="index">The name of the index to be selected. If this is null or empty
        /// the primary index will be selected.</param>
        /// <param name="grbit">Set index options.</param>
        /// <param name="itagSequence">Sequence number of the multi-valued column value which
        /// will be used to position the cursor on the new index. This parameter is only used
        /// in conjunction with <see cref="SetCurrentIndexGrbit.NoMove"/>. When this parameter
        /// is not present or is set to zero, its value is presumed to be 1.</param>
        /// <returns>An error if the call fails.</returns>
        public void SetCurrentIndex(string index, SetCurrentIndexGrbit grbit = SetCurrentIndexGrbit.None,
            int itagSequence = 1)
        {
            Tracing.TraceFunctionCall("SetCurrentIndex");
            int returnCode = (SetCurrentIndexGrbit.None == grbit)
                ? NativeMethods.JetSetCurrentIndex(_session.Id, _cursorId.Value, index)
                : (1 == itagSequence)
                    ? NativeMethods.JetSetCurrentIndex2(_session.Id, _cursorId.Value, index, (uint)grbit)
                    : NativeMethods.JetSetCurrentIndex3(_session.Id, _cursorId.Value, index, (uint)grbit, checked((uint)itagSequence));
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return;
        }

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
        public void SetCurrentIndex(JET_INDEXID indexid, SetCurrentIndexGrbit grbit = SetCurrentIndexGrbit.None,
            int itagSequence = 1)
        {
            Tracing.TraceFunctionCall("SetCurrentIndex");
            // A null index name is valid here -- it will set the table to the primary index
            int returnCode = NativeMethods.JetSetCurrentIndex4(_session.Id, _cursorId.Value, null,
                ref indexid, (uint)grbit, checked((uint)itagSequence));
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return;
        }

        /// <summary>Temporarily limits the set of index entries that the cursor can walk using
        /// <see cref="Move"/> to those starting from the current index entry and ending at the
        /// index entry that matches the search criteria specified by the search key in that
        /// cursor and the specified bound criteria. A search key must have been previously
        /// constructed using IJetCursor.MakeKey.</summary>
        /// <param name="grbit">Index range options.</param>
        public void SetIndexRange(SetIndexRangeGrbit grbit = SetIndexRangeGrbit.None)
        {
            Tracing.TraceFunctionCall("SetIndexRange");
            int returnCode = NativeMethods.JetSetIndexRange(_session.Id, _cursorId.Value,
                unchecked((uint)grbit));
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Returns a <see cref="T:System.String"/> that represents the current
        /// <see cref="JetCursor"/>.</summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current
        /// <see cref="JetCursor"/>.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Update ({0})", this.prep);
        }

        /// <summary>Try to navigate through an index. If the navigation succeeds this method
        /// returns true. If there is no record to navigate to this method returns false; an
        /// exception will be thrown for other errors.</summary>
        /// <param name="move">The direction to move in.</param>
        /// <param name="grbit">Move options.</param>
        /// <returns>True if the move was successful.</returns>
        public bool TryMove(JET_Move move, MoveGrbit grbit = MoveGrbit.None)
        {
            JET_err err = (JET_err)Move((int)move, grbit);
            if (JET_err.NoCurrentRecord == err) { return false; }
            EsentExceptionHelper.Check((int)err);
            Debug.Assert((JET_err.Success <= err), "Exception should have been thrown in case of error");
            return true;
        }

        /// <summary>Try to move to the first record in the table. If the table is empty this
        /// returns false, if a different error is encountered an exception is thrown.</summary>
        /// <returns>True if the move was successful.</returns>
        public bool TryMoveFirst()
        {
            return TryMove(JET_Move.First);
        }

        /// <summary>Try to move to the last record in the table. If the table is empty this
        /// returns false, if a different error is encountered an exception is thrown.</summary>
        /// <returns>True if the move was successful.</returns>
        public bool TryMoveLast()
        {
            return TryMove(JET_Move.Last);
        }

        /// <summary>Try to move to the next record in the table. If there is not a next
        /// record this returns false, if a different error is encountered an exception is
        /// thrown.</summary>
        /// <returns>True if the move was successful.</returns>
        public bool TryMoveNext()
        {
            return TryMove(JET_Move.Next, MoveGrbit.None);
        }

        /// <summary>The JetUpdate function performs an update operation including inserting
        /// a new row into a table or updating an existing row. Deleting a table row is
        /// performed by calling<see cref="IJetInstance.JetDelete"/>.</summary>
        /// <param name="bookmark">Returns the bookmark of the updated record. This can be null.
        /// </param>
        /// <param name="bookmarkSize">The size of the bookmark buffer.</param>
        /// <param name="actualBookmarkSize">Returns the actual size of the bookmark.</param>
        /// <remarks>JetUpdate is the final step in performing an insert or an update. The
        /// update is begun by calling <see cref="IJetTable.PrepareUpdate"/> and then by
        /// calling JetSetColumn one or more times to set the record state. Finally, JetUpdate
        /// is called to complete the update operation. Indexes are updated only by JetUpdate
        /// or and not during JetSetColumn.</remarks>
        /// <returns>An error if the call fails.</returns>
        public int Update(byte[] bookmark, int bookmarkSize, out int actualBookmarkSize)
        {
            Tracing.TraceFunctionCall("JetUpdate");
            Helpers.CheckDataSize(bookmark, bookmarkSize, "bookmarkSize");
            uint bytesActual;
            int returnCode = NativeMethods.JetUpdate(_session.Id, _cursorId.Value, bookmark,
                checked((uint)bookmarkSize), out bytesActual);
            Tracing.TraceResult(returnCode);
            actualBookmarkSize = Helpers.GetActualSize(bytesActual);
            return returnCode;
        }

        /// <summary>The JetUpdate function performs an update operation including inserting
        /// a new row into a table or updating an existing row. Deleting a table row is performed
        /// by calling IJetApi.JetDelete</summary>
        /// <param name="bookmark">Returns the bookmark of the updated record. This can be null.</param>
        /// <param name="grbit">Update options.</param>
        /// <returns>Returns the actual size of the bookmark.</returns>
        /// <remarks>JetUpdate is the final step in performing an insert or an update. The update
        /// is begun by calling <see cref="IJetTable.PrepareUpdate"/> and then by calling JetSetColumn
        /// one or more times to set the record state. Finally, JetUpdate is called to complete
        /// the update operation. Indexes are updated only by JetUpdate or and not during
        /// JetSetColumn.</remarks>
        public int Update(byte[] bookmark, UpdateGrbit grbit = UpdateGrbit.None)
        {
            Tracing.TraceFunctionCall("JetUpdate");
            int bookmarkSize = (null == bookmark) ? 0 : bookmark.Length;
            Helpers.CheckDataSize(bookmark, bookmarkSize, "bookmarkSize");
            uint bytesActual;
            int returnCode = (UpdateGrbit.None == grbit)
                ? NativeMethods.JetUpdate(_session.Id, _cursorId.Value, bookmark,
                    checked((uint)bookmarkSize), out bytesActual)
                : NativeMethods.JetUpdate2(_session.Id, _cursorId.Value, bookmark,
                    checked((uint)bookmarkSize), out bytesActual, (uint)grbit);
            EsentExceptionHelper.Check(returnCode);
            return Helpers.GetActualSize(bytesActual);
        }

        private JET_TABLEID _cursorId;
        /// <summary>The underlying JET_SESID.</summary>
        private readonly JetSession _session;
        /// <summary>The type of update.</summary>
        private readonly JET_prep prep;
    }
}
