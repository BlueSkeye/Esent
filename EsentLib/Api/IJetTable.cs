using System;
using System.Collections.Generic;
using System.Text;

using EsentLib.Jet;
using EsentLib.Jet.Types;

namespace EsentLib.Api
{
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

        /// <summary>Retrieves information about all columns in the table.</summary>
        /// <param name="grbit">Additional options for JetGetTableColumnInfo.</param>
        /// <returns>Filled in with information about the columns in the table.</returns>
        ICollection<IJetColumn> GetColumns(ColInfoGrbit grbit = ColInfoGrbit.None);

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <returns>An error if the call fails.</returns>
        uint GetIndexAllocatedSpace(IJetSession session, string indexname);

        /// <summary>Retrieves information about all indexes on the table.</summary>
        /// <returns>Filled in with information about the columns in the table.</returns>
        ICollection<IJetIndex> GetIndexes();

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <returns>An error if the call fails.</returns>
        uint GetIndexesCount(IJetSession session, string indexname);

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <returns>An error if the call fails.</returns>
        uint GetIndexLocaleID(IJetSession session, string indexname);

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <returns>An error if the call fails.</returns>
        ushort GetIndexMaxColumnLength(IJetSession session, string indexname);

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <returns>An error if the call fails.</returns>
        ushort GetIndexMaxKeyLength(IJetSession session, string indexname);

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

        /// <summary>Prepare a cursor for update.</summary>
        /// <param name="session">The session which is starting the update.</param>
        /// <param name="prep">The type of update to prepare.</param>
        /// <returns>An instance of the ongoing update.</returns>
        IJetCursor PrepareUpdate(IJetSession session, JET_prep prep);

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
    }
}
