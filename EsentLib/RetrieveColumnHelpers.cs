//-----------------------------------------------------------------------
// <copyright file="RetrieveColumnHelpers.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
#if MANAGEDESENT_SUPPORTS_SERIALIZATION
using System.Runtime.Serialization.Formatters.Binary;
#endif
using System.Text;

using EsentLib.Api;
using EsentLib.Api.Data;
using EsentLib.Jet;
using EsentLib.Jet.Types;

namespace EsentLib
{
    /// <summary>Helper methods for the ESENT API. These aren't interop versions
    /// of the API, but encapsulate very common uses of the functions. </summary>
    public static partial class LegacyApi
    {
        /// <summary>Encoding to use to decode ASCII text. We use this because UTF8.GetString
        /// is faster than ASCII.GetString.</summary>
        private static readonly Encoding AsciiDecoder = new UTF8Encoding(false, true);

        /// <summary>Retrieves the bookmark for the record that is associated with the
        /// index entry at the current position of a cursor. This bookmark can then be
        /// used to reposition that cursor back to the same record using
        /// <see cref="IJetCursor.GotoBookmark"/>. </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the bookmark from.</param>
        /// <returns>The bookmark of the record.</returns>
        public static byte[] GetBookmark(JET_SESID sesid, JET_TABLEID tableid)
        {
            byte[] buffer = null;
            byte[] bookmark;

            try
            {
                buffer = MemoryCache.BookmarkCache.Allocate();
                int bookmarkSize;
                LegacyApi.JetGetBookmark(sesid, tableid, buffer, buffer.Length, out bookmarkSize);
                bookmark = MemoryCache.Duplicate(buffer, bookmarkSize);
            }
            finally { if (buffer != null) { MemoryCache.BookmarkCache.Free(ref buffer); } }
            
            return bookmark;
        }

        /// <summary>Retrieves the bookmark for the record that is associated with the index
        /// entry at the current position of a cursor. This bookmark can then be used to
        /// reposition that cursor back to the same record using <see cref="IJetCursor.GotoBookmark"/>. 
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the bookmark from.</param>
        /// <param name="primaryBookmark">Returns the primary bookmark.</param>
        /// <returns>The secondary bookmark of the record.</returns>
        public static byte[] GetSecondaryBookmark(JET_SESID sesid, JET_TABLEID tableid,
            out byte[] primaryBookmark)
        {
            byte[] bufferPrimary = null;
            byte[] bufferSecondary = null;
            byte[] secondaryBookmark;
            primaryBookmark = null;

            try {
                bufferPrimary = MemoryCache.BookmarkCache.Allocate();
                bufferSecondary = MemoryCache.SecondaryBookmarkCache.Allocate();
                int bookmarkSizePrimary;
                int bookmarkSizeSecondary;
                LegacyApi.JetGetSecondaryIndexBookmark(sesid, tableid, bufferSecondary,
                    bufferSecondary.Length, out bookmarkSizeSecondary, bufferPrimary,
                    bufferPrimary.Length, out bookmarkSizePrimary, GetSecondaryIndexBookmarkGrbit.None);
                primaryBookmark = MemoryCache.Duplicate(bufferPrimary, bookmarkSizePrimary);
                secondaryBookmark = MemoryCache.Duplicate(bufferSecondary, bookmarkSizeSecondary);
            }
            finally {
                if (null != bufferPrimary) {
                    MemoryCache.BookmarkCache.Free(ref bufferPrimary);
                }

                if (null != bufferSecondary) {
                    MemoryCache.BookmarkCache.Free(ref bufferSecondary);
                }
            }
            return secondaryBookmark;
        }

        /// <summary>
        /// Retrieves the key for the index entry at the current position of a cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the key from.</param>
        /// <param name="grbit">Retrieve key options.</param>
        /// <returns>The retrieved key.</returns>
        public static byte[] RetrieveKey(JET_SESID sesid, JET_TABLEID tableid, RetrieveKeyGrbit grbit)
        {
            byte[] buffer = null;
            try {
                buffer = MemoryCache.BookmarkCache.Allocate();
                int keySize;
                LegacyApi.JetRetrieveKey(sesid, tableid, buffer, buffer.Length, out keySize, grbit);
                return MemoryCache.Duplicate(buffer, keySize);
            }
            finally { if (buffer != null) { MemoryCache.BookmarkCache.Free(ref buffer); } }
        }

#if MANAGEDESENT_SUPPORTS_SERIALIZATION
        /// <summary>
        /// Deserialize an object from a column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to read from.</param>
        /// <param name="columnid">The column to read from.</param>
        /// <returns>The deserialized object. Null if the column was null.</returns>
        public static object DeserializeObjectFromColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return DeserializeObjectFromColumn(sesid, tableid, columnid, RetrieveColumnGrbit.None);
        }

        /// <summary>
        /// Deserialize an object from a column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to read from.</param>
        /// <param name="columnid">The column to read from.</param>
        /// <param name="grbit">The retrieval options to use.</param>
        /// <returns>The deserialized object. Null if the column was null.</returns>
        [SuppressMessage("Exchange.Security", "EX0043:DoNotUseBinarySoapFormatter", Justification = "Suppress warning in current code base.The usage has already been verified.")]
        public static object DeserializeObjectFromColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, RetrieveColumnGrbit grbit)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0!= (grbit & (RetrieveColumnGrbit)0x00020000)) { // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            // int actualSize;
            throw new NotImplementedException();
            //if (JET_wrn.ColumnNull == JetTableApi.JetRetrieveColumn(sesid, tableid, columnid,
            //    null, 0, out actualSize, grbit, null))
            //{
            //    return null;
            //}

            //using (var stream = new ColumnStream(sesid, tableid, columnid)) {
            //    var deseriaizer = new BinaryFormatter();
            //    return deseriaizer.Deserialize(stream);
            //}
        }
#endif

        /// <summary>
        /// Retrieves columns into ColumnValue objects.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor retrieve the data from. The cursor should be positioned on a record.</param>
        /// <param name="values">The values to retrieve.</param>
        public static void RetrieveColumns(JET_SESID sesid, JET_TABLEID tableid, params ColumnValue[] values)
        {
            if (null == values) { throw new ArgumentNullException("values"); }
            if (0 == values.Length) {
                throw new ArgumentOutOfRangeException("values", values.Length, "must have at least one value");
            }
            ColumnValue.RetrieveColumns(sesid, tableid, values);
        }

        /// <summary>Efficiently retrieves a set of columns and their values from the current
        /// record of a cursor or the copy buffer of that cursor.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve data from.</param>
        /// <param name="grbit">Enumerate options.</param>
        /// <returns>The discovered columns and their values.</returns>
        public static IEnumerable<EnumeratedColumn> EnumerateColumns(JET_SESID sesid,
            JET_TABLEID tableid, EnumerateColumnsGrbit grbit)
        {
            IEnumerable<EnumeratedColumn> enumeratedColumns;
            LegacyApi.JetEnumerateColumns(sesid, tableid, grbit, out enumeratedColumns);
            return enumeratedColumns;
        }

        /// <summary>Recursively pin the retrieve buffers in the JET_RETRIEVECOLUMN structures
        /// and then retrieve the columns. This is done to avoid creating GCHandles, which are
        /// expensive. This function pins the current retrievecolumn structure (indicated by i)
        /// and then recursively calls itself until all structures are pinned. This is done because
        /// it isn't possible to create an arbitrary number of pinned variables in a method.</summary>
        /// <param name="sesid">The session to use</param>
        /// <param name="tableid">The table to retrieve from.</param>
        /// <param name="nativeretrievecolumns">The nativeretrievecolumns structure.</param>
        /// <param name="retrievecolumns">The managed retrieve columns structure.</param>
        /// <param name="numColumns">The number of columns.</param>
        /// <param name="i">The column currently being processed.</param>
        /// <returns>An error code from JetRetrieveColumns.</returns>
        private static unsafe int PinColumnsAndRetrieve(JET_SESID sesid, JET_TABLEID tableid,
            NATIVE_RETRIEVECOLUMN* nativeretrievecolumns, IList<JET_RETRIEVECOLUMN> retrievecolumns,
            int numColumns, int i)
        {
            // If consecutive JET_RETRIEVECOLUMN structures are using the same buffer then only pin it once.
            fixed (byte* pinnedBuffer = retrievecolumns[i].pvData)
            {
                do
                {
                    retrievecolumns[i].CheckDataSize();
                    retrievecolumns[i].GetNativeRetrievecolumn(ref nativeretrievecolumns[i]);
                    nativeretrievecolumns[i].pvData = new IntPtr(pinnedBuffer + retrievecolumns[i].ibData);
                    i++;
                }
                while (i < numColumns && retrievecolumns[i].pvData == retrievecolumns[i - 1].pvData);

                return i == numColumns ?
                    Impl.JetRetrieveColumns(sesid, tableid, nativeretrievecolumns, numColumns)
                    : PinColumnsAndRetrieve(sesid, tableid, nativeretrievecolumns, retrievecolumns, numColumns, i);
            }
        }
    }
}
