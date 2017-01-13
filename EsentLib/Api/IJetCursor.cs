using System;
using System.Collections.Generic;
using System.Text;
using EsentLib.Jet;

namespace EsentLib.Api
{
    /// <summary>A delegate that will filter out unwanted records.</summary>
    /// <returns></returns>
    public delegate bool FilterDelegate();
    /// <summary>A delegate that will be provided with those reords that passed the filtering
    /// step.</summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public delegate T ItemRetrieverDelegate<T>();

    /// <summary>A cursor is a special structure that allow for non sequential access on a
    /// <see cref="IJetTable"/>. An instance can be retrieved with <see cref="IJetTable.PrepareUpdate"/>
    /// or with <see cref="IJetDatabase.OpenTable"/></summary>
    [CLSCompliant(false)]
    public interface IJetCursor : IDisposable
    {
        /// <summary>Get the name of the current index of a given cursor.</summary>
        /// <remarks>This accessor is also used to later re-select that index as the current
        /// index. It can also be used to discover the properties of that index using
        /// JetGetTableIndexInfo. The returned name of the index will be an empty string if
        /// the current index is the clustered index and no primary index was explicitly
        /// defined.</remarks>
        string CurrentIndex { get; }

        /// <summary>Retrieve the underlying table.</summary>
        IJetTable Table { get; }

        /// <summary>Close an open cursor.</summary>
        void Close();

        /// <summary></summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="skipRecordFilter">This delegate, if not null, will be invoked on each
        /// record. It has to return true if the current record should be skipped and not be
        /// returned by the enumerator.</param>
        /// <param name="retriever"></param>
        /// <returns></returns>
        IEnumerable<T> Enumerate<T>(FilterDelegate skipRecordFilter, ItemRetrieverDelegate<T> retriever);

        /// <summary>Positions a cursor to an index entry for the record that is associated
        /// with the specified bookmark. The bookmark can be used with any index defined over
        /// a table. The bookmark for a record can be retrieved using
        /// <see cref="IJetInstance.JetGetBookmark"/>.</summary>
        /// <param name="bookmark">The bookmark used to position the cursor.</param>
        /// <param name="bookmarkSize">The size of the bookmark.</param>
        void GotoBookmark(byte[] bookmark, int bookmarkSize);

        /// <summary>Constructs search keys that may then be used by JetSeek and JetSetIndexRange.
        /// </summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        void MakeKey(byte[] data, MakeKeyGrbit grbit = MakeKeyGrbit.None);

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange.
        /// </summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        void MakeKey(string data, MakeKeyGrbit grbit = MakeKeyGrbit.None);

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange.
        /// </summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="encoding">The encoding used to convert the string.</param>
        /// <param name="grbit">Key options.</param>
        void MakeKey(string data, Encoding encoding, MakeKeyGrbit grbit = MakeKeyGrbit.None);

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange.
        /// </summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        void MakeKey(bool data, MakeKeyGrbit grbit = MakeKeyGrbit.None);

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange.</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        void MakeKey(byte data, MakeKeyGrbit grbit = MakeKeyGrbit.None);

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange.</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        void MakeKey(short data, MakeKeyGrbit grbit = MakeKeyGrbit.None);

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        void MakeKey(int data, MakeKeyGrbit grbit = MakeKeyGrbit.None);

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        void MakeKey(long data, MakeKeyGrbit grbit = MakeKeyGrbit.None);

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        void MakeKey(Guid data, MakeKeyGrbit grbit = MakeKeyGrbit.None);

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        void MakeKey(DateTime data, MakeKeyGrbit grbit = MakeKeyGrbit.None);

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        void MakeKey(float data, MakeKeyGrbit grbit = MakeKeyGrbit.None);

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        void MakeKey(double data, MakeKeyGrbit grbit = MakeKeyGrbit.None);

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        // [CLSCompliant(false)]
        void MakeKey(ushort data, MakeKeyGrbit grbit = MakeKeyGrbit.None);

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        // [CLSCompliant(false)]
        void MakeKey(uint data, MakeKeyGrbit grbit = MakeKeyGrbit.None);

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        // [CLSCompliant(false)]
        void MakeKey(ulong data, MakeKeyGrbit grbit = MakeKeyGrbit.None);

        /// <summary>Constructs search keys that may then be used by JetSeek and JetSetIndexRange.</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="dataSize">Size of the data.</param>
        /// <param name="grbit">Key options.</param>
        void MakeKey(byte[] data, int dataSize, MakeKeyGrbit grbit = MakeKeyGrbit.None);

        /// <summary>Navigate through an index. The cursor can be positioned at the start or
        /// end of the index and moved backwards and forwards by a specified number of index
        /// entries.</summary>
        /// <param name="numRows">An offset which indicates how far to move the cursor.</param>
        /// <param name="grbit">Move options.</param>
        /// <returns>An error if the call fails.</returns>
        int Move(int numRows, MoveGrbit grbit = MoveGrbit.None);

        /// <summary>Retrieves a single column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.
        /// Alternatively, this function can retrieve a column from a record being created in
        /// the cursor copy buffer. This function can also retrieve column data from an index
        /// entry that references the current record. In addition to retrieving the actual
        /// column value, JetRetrieveColumn can also be used to retrieve the size of a column,
        /// before retrieving the column data itself so that application buffers can be sized
        /// appropriately.</summary>
        /// <remarks>The RetrieveColumnAs functions provide datatype-specific retrieval functions.</remarks>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="data">The data buffer to be retrieved into.</param>
        /// <param name="dataSize">The size of the data buffer.</param>
        /// <param name="actualDataSize">Returns the actual size of the data buffer.</param>
        /// <param name="grbit">Retrieve column options.</param>
        /// <param name="retinfo">If pretinfo is give as NULL then the function behaves as though an itagSequence
        /// of 1 and an ibLongValue of 0 (zero) were given. This causes column retrieval to
        /// retrieve the first value of a multi-valued column, and to retrieve long data at
        /// offset 0 (zero).</param>
        /// <returns>An error or warning.</returns>
        JET_wrn RetrieveColumn(JET_COLUMNID columnid, IntPtr data, int dataSize, out int actualDataSize,
            RetrieveColumnGrbit grbit, JET_RETINFO retinfo);

        /// <summary>Retrieves a boolean column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a boolean. Null if the column is null.</returns>
        bool? RetrieveColumnAsBoolean(JET_COLUMNID columnid, RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None);

        /// <summary>Retrieves a byte column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a byte. Null if the column is null.</returns>
        byte? RetrieveColumnAsByte(JET_COLUMNID columnid, RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None);

        /// <summary>Retrieves a byte column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="length"></param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a byte. Null if the column is null.</returns>
        byte[] RetrieveColumnAsByteArray(JET_COLUMNID columnid, int length = 0,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None);

        /// <summary>Retrieves a datetime column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a datetime. Null if the column is null.</returns>
        DateTime? RetrieveColumnAsDateTime(JET_COLUMNID columnid, RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None);

        /// <summary>Retrieves a double column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a double. Null if the column is null.</returns>
        double? RetrieveColumnAsDouble(JET_COLUMNID columnid, RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None);

        /// <summary>Retrieves a float column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a float. Null if the column is null.</returns>
        float? RetrieveColumnAsFloat(JET_COLUMNID columnid, RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None);

        /// <summary>Retrieves a guid column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a guid. Null if the column is null.</returns>
        Guid? RetrieveColumnAsGuid(JET_COLUMNID columnid, RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None);

        /// <summary>Retrieves an int16 column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a short. Null if the column is null.</returns>
        short? RetrieveColumnAsInt16(JET_COLUMNID columnid, RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None);

        /// <summary>Retrieves an int32 column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an int. Null if the column is null.</returns>
        int? RetrieveColumnAsInt32(JET_COLUMNID columnid, RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None);

        /// <summary>Retrieves an int32 column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an int. Null if the column is null.</returns>
        int? RetrieveColumnAsInt32(uint columnid, RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None);

        /// <summary>Retrieves a uint16 column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an UInt16. Null if the column is null.</returns>
        // [CLSCompliant(false)]
        ushort? RetrieveColumnAsUInt16(JET_COLUMNID columnid, RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None);

        /// <summary>Retrieves a uint32 column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an UInt32. Null if the column is null.</returns>
        // [CLSCompliant(false)]
        uint? RetrieveColumnAsUInt32(JET_COLUMNID columnid, RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None);

        /// <summary>Retrieves a uint64 column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an UInt64. Null if the column is null.</returns>
        // [CLSCompliant(false)]
        ulong? RetrieveColumnAsUInt64(JET_COLUMNID columnid, RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None);

        /// <summary>Retrieves a string column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a string. Null if the column is null.</returns>
        string RetrieveColumnAsString(JET_COLUMNID columnid, RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None);

        /// <summary>Efficiently positions a cursor to an index entry that matches the search
        /// criteria specified by the search key in that cursor and the specified inequality.
        /// A search key must have been previously constructed using IJetTable.MakeKey.</summary>
        /// <param name="grbit">Seek options.</param>
        /// <returns>An ESENT warning.</returns>
        void Seek(SeekGrbit grbit = SeekGrbit.SeekEQ);

        /// <summary>Set the current index of a cursor.</summary>
        /// <param name="index">The name of the index to be selected. If this is null or empty the
        /// primary index will be selected.</param>
        /// <param name="grbit">Set index options.</param>
        /// <param name="itagSequence">Sequence number of the multi-valued column value which will be
        /// used to position the cursor on the new index. This parameter is only used in conjunction
        /// with <see cref="SetCurrentIndexGrbit.NoMove"/>. When this parameter is not present or is
        /// set to zero, its value is presumed to be 1.</param>
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

        /// <summary>Temporarily limits the set of index entries that the cursor can walk using
        /// <see cref="IJetCursor.Move"/> to those starting from the current index entry and ending at
        /// the index entry that matches the search criteria specified by the search key in that cursor
        /// and the specified bound criteria. A search key must have been previously constructed using
        /// JetMakeKey.</summary>
        /// <param name="grbit">Index range options.</param>
        /// <returns>An error if the call fails.</returns>
        void SetIndexRange(SetIndexRangeGrbit grbit = SetIndexRangeGrbit.None);

        /// <summary>Try to navigate through an index. If the navigation succeeds this method
        /// returns true. If there is no record to navigate to this method returns false; an
        /// exception will be thrown for other errors.</summary>
        /// <param name="move">The direction to move in.</param>
        /// <param name="grbit">Move options.</param>
        /// <returns>True if the move was successful.</returns>
        bool TryMove(JET_Move move, MoveGrbit grbit = MoveGrbit.None);

        /// <summary>Try to move to the first record in the table. If the table is empty this
        /// returns false, if a different error is encountered an exception is thrown.</summary>
        /// <returns>True if the move was successful.</returns>
        bool TryMoveFirst();

        /// <summary>Try to move to the last record in the table. If the table is empty this
        /// returns false, if a different error is encountered an exception is thrown.</summary>
        /// <returns>True if the move was successful.</returns>
        bool TryMoveLast();

        /// <summary>Try to move to the next record in the table. If there is not a next
        /// record this returns false, if a different error is encountered an exception is
        /// thrown.</summary>
        /// <returns>True if the move was successful.</returns>
        bool TryMoveNext();
    }
}
