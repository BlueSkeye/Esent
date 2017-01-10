using System;
using System.Collections.Generic;
using System.Text;

using EsentLib.Jet;
using EsentLib.Jet.Types;

namespace EsentLib.Api
{
    /// <summary>A delegate that will filter out unwanted records.</summary>
    /// <returns></returns>
    public delegate bool FilterDelegate();
    /// <summary></summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public delegate T ItemRetriever<T>();

    /// <summary></summary>
    public struct BasicColumnDesriptor
    {
        internal BasicColumnDesriptor(string name, JET_COLUMNID id)
        {
            Name = name;
            Id = id;
        }

        /// <summary></summary>
        public JET_COLUMNID Id { get; private set; }
        /// <summary></summary>
        public string Name { get; private set; }
    }

    /// <summary></summary>
    [CLSCompliant(false)]
    public interface IJetTable : IDisposable
    {
        /// <summary>Returns the database this table belongs to.</summary>
        IJetDatabase Database { get; }

        /// <summary>Add a new column to an existing table.</summary>
        /// <param name="session">Session to use.</param>
        /// <param name="column">The name of the column.</param>
        /// <param name="columndef">The definition of the column.</param>
        /// <param name="defaultValue">The default value of the column.</param>
        /// <param name="defaultValueSize">The size of the default value.</param>
        IJetColumn AddColumn(IJetSession session, string column, JET_COLUMNDEF columndef,
            byte[] defaultValue, int defaultValueSize);

        /// <summary>Close an open table.</summary>
        void Close();

        /// <summary>Walks each index of a table to exactly compute the number of entries in
        /// an index, and the number of distinct keys in an index. This information, together
        /// with the number of database pages allocated for an index and the current time of
        /// the computation is stored in index metadata in the database. This data can be
        /// subsequently retrieved with information operations.</summary>
        /// <param name="session">Session to use.</param>
        void ComputeStatistics(IJetSession session);
        
        /// <summary>Creates an index over data in an ESE database. An index can be used to
        /// locate specific data quickly.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="indexName">Pointer to a null-terminated string that specifies the
        /// name of the index to create. </param>
        /// <param name="grbit">Index creation options.</param>
        /// <param name="keyDescription">Pointer to a double null-terminated string of null
        /// delimited tokens.</param>
        /// <param name="keyDescriptionLength">The length, in characters, of szKey including
        /// the two terminating nulls.</param>
        /// <param name="density">Initial B+ tree density.</param>
        void CreateIndex(IJetSession session, string indexName, CreateIndexGrbit grbit,
            string keyDescription, int keyDescriptionLength, int density);

        /// <summary>Creates indexes over data in an ESE database.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="indexcreates">Array of objects describing the indexes to be created.</param>
        /// <param name="numIndexCreates">Number of index description objects.</param>
        /// <returns>An error code.</returns>
        /// <seealso cref="IJetTable.CreateIndex(IJetSession, string, CreateIndexGrbit, string, int, int)"/>
        void CreateIndex(IJetSession session, JET_INDEXCREATE[] indexcreates, int numIndexCreates);

        /// <summary>Deletes a column from a database table.</summary>
        /// <param name="session">Session to use.</param>
        /// <param name="column">The name of the column to be deleted.</param>
        /// <param name="grbit">Optional deletion flag.</param>
        void DeleteColumn(IJetSession session, string column, DeleteColumnGrbit grbit = DeleteColumnGrbit.None);

        /// <summary>Deletes an index from a database table.</summary>
        /// <param name="session">Session to use.</param>
        /// <param name="index">The name of the index to be deleted.</param>
        void DeleteIndex(IJetSession session, string index);

        /// <summary>Enumerate the name and identifier of the columns in this table.</summary>
        /// <returns>An enumerable object.</returns>
        IJetTemporaryTable<BasicColumnDesriptor> EnumerateColumns();

        /// <summary></summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="skipRecordFilter">This delegate, if not null, will be invoked on each
        /// record. It has to return true if the current record should be skipped and not be
        /// returned by the enumerator.</param>
        /// <param name="retriever"></param>
        /// <returns></returns>
        IEnumerable<T> Enumerate<T>(FilterDelegate skipRecordFilter, ItemRetriever<T> retriever);

        /// <summary>Retrieves information about all columns in the table.</summary>
        /// <param name="grbit">Additional options for JetGetTableColumnInfo.</param>
        /// <returns>Filled in with information about the columns in the table.</returns>
        ICollection<IJetColumn> GetColumns(ColInfoGrbit grbit = ColInfoGrbit.None);

        /// <summary>Explicitly reserve the ability to update a row, write lock, or to explicitly
        /// prevent a row from being updated by any other session, read lock. Normally, row write
        /// locks are acquired implicitly as a result of updating rows. Read locks are usually not
        /// required because of record versioning. However, in some cases a transaction may desire
        /// to explicitly lock a row to enforce serialization, or to ensure that a subsequent
        /// operation will succeed.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="readLock">Acquire a read lock on the current record. Read locks are
        /// incompatible with write locks already held by other sessions but are compatible with
        /// read locks held by other sessions.</param>
        /// <param name="writeLock">Acquire a write lock on the current record. Write locks are not
        /// compatible with write or read locks held by other sessions but are compatible with read
        /// locks held by the same session.</param>
        void GetLock(IJetSession session, bool readLock, bool writeLock);

        /// <summary>Determine whether an update of the current record of a cursor will result
        /// in a write conflict, based on the current update status of the record. It is possible
        /// that a write conflict will ultimately be returned even if IsWriteConflictExpected
        /// returns successfully. because another session may update the record before the current
        /// session is able to update the same record.</summary>
        /// <param name="session">The session to use.</param>
        /// <returns>An error if the call fails.</returns>
        bool IsWriteConflictExpected(IJetSession session);

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
        /// <param name="sesid">The session to use for the call.</param>
        /// <param name="numRows">An offset which indicates how far to move the cursor.</param>
        /// <param name="grbit">Move options.</param>
        /// <returns>An error if the call fails.</returns>
        int Move(JET_SESID sesid, int numRows, MoveGrbit grbit = MoveGrbit.None);

        /// <summary>Prepare a cursor for update.</summary>
        /// <param name="session">The session which is starting the update.</param>
        /// <param name="prep">The type of update to prepare.</param>
        /// <returns>An instance of the ongoing update.</returns>
        ICursor PrepareUpdate(IJetSession session, JET_prep prep);

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

        /// <summary>Explicitly reserve the ability to update a row, write lock, or to explicitly
        /// prevent a row from being updated by any other session, read lock. Normally, row write
        /// locks are acquired implicitly as a result of updating rows. Read locks are usually not
        /// required because of record versioning. However, in some cases a transaction may desire
        /// to explicitly lock a row to enforce serialization, or to ensure that a subsequent
        /// operation will succeed. </summary>
        /// <param name="session">The session to use.</param>
        /// <param name="readLock">Acquire a read lock on the current record. Read locks are
        /// incompatible with write locks already held by other sessions but are compatible with
        /// read locks held by other sessions.</param>
        /// <param name="writeLock">Acquire a write lock on the current record. Write locks are not
        /// compatible with write or read locks held by other sessions but are compatible with read
        /// locks held by the same session.</param>
        /// <returns>True if the lock was obtained, false otherwise. An exception is thrown if an
        /// unexpected error is encountered.</returns>
        bool TryGetLock(IJetSession session, bool readLock, bool writeLock);

        /// <summary>Try to move to the first record in the table. If the table is empty this
        /// returns false, if a different error is encountered an exception is thrown.</summary>
        /// <returns>True if the move was successful.</returns>
        bool TryMoveFirst();

        /// <summary>Try to move to the next record in the table. If there is not a next
        /// record this returns false, if a different error is encountered an exception is
        /// thrown.</summary>
        /// <returns>True if the move was successful.</returns>
        bool TryMoveNext();

        /// <summary>The JetUpdate function performs an update operation including inserting
        /// a new row into a table or updating an existing row. Deleting a table row is performed
        /// by calling IJetApi.JetDelete</summary>
        /// <param name="session">The session which started the update.</param>
        /// <param name="bookmark">Returns the bookmark of the updated record. This can be null.</param>
        /// <param name="grbit">Update options.</param>
        /// <returns>Returns the actual size of the bookmark.</returns>
        /// <remarks>JetUpdate is the final step in performing an insert or an update. The update
        /// is begun by calling <see cref="IJetTable.PrepareUpdate"/> and then by calling JetSetColumn
        /// one or more times to set the record state. Finally, JetUpdate is called to complete
        /// the update operation. Indexes are updated only by JetUpdate or and not during
        /// JetSetColumn.</remarks>
        int Update(IJetSession session, byte[] bookmark, UpdateGrbit grbit = UpdateGrbit.None);
    }
}
