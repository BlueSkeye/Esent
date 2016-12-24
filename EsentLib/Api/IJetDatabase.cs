using System;

using EsentLib.Api.Flags;
using EsentLib.Jet;

namespace EsentLib.Api
{
    /// <summary></summary>
    [CLSCompliant(false)]
    public interface IJetDatabase
    {
        /// <summary>Closes a database file that was previously opened with
        /// <see cref="IJetSession.OpenDatabase"/> or created with
        /// <see cref="IJetSession.CreateDatabase"/>.</summary>
        /// <param name="grbit">Close options.</param>
        void Close(CloseDatabaseGrbit grbit);

        /// <summary>Makes a copy of an existing database. The copy is compacted to a state
        /// optimal for usage. Data in the copied data will be packed according to the measures
        /// chosen for the indexes at index create. In this way, compacted data may be
        /// stored as densely as possible. Alternatively, compacted data may reserve space
        /// for subsequent record growth or index insertions.</summary>
        /// <param name="destinationDatabase">The name to use for the compacted database.</param>
        /// <param name="statusCallback">A callback function that can be called periodically
        /// through the database compact operation to report progress.</param>
        /// <param name="grbit">Compact options.</param>
        void CompactDatabase(string destinationDatabase, JET_PFNSTATUS statusCallback, CompactGrbit grbit);

        /// <summary>Create an empty table. The newly created table is opened exclusively.</summary>
        /// <param name="table">The name of the table to create.</param>
        /// <param name="pages">Initial number of pages in the table.</param>
        /// <param name="density">
        /// The default density of the table. This is used when doing sequential inserts.
        /// </param>
        IJetTable CreateTable(string table, int pages, int density);

        /// <summary>Deletes a table from a database.</summary>
        /// <param name="table">The name of the table to delete.</param>
        void DeleteTable(string table);

        /// <summary>Retrieves certain information about the given database.</summary>
        /// <param name="infoLevel">The specific data to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        JET_DBINFOMISC GetFileInfo(JET_DbInfo infoLevel);

        /// <summary>Retrieves certain information about the given database.</summary>
        /// <param name="infoLevel">The specific data to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        JET_DBINFOMISC GetInfo(JET_DbInfo infoLevel);

        /// <summary>Retrieves certain information about the given database.</summary>
        /// <param name="infoLevel">The specific data to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        int GetInt32FileInfo(JET_DbInfo infoLevel);

        /// <summary>Retrieves certain information about the given database.</summary>
        /// <param name="infoLevel">The specific data to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        int GetInt32Info(JET_DbInfo infoLevel);

        /// <summary>Retrieves certain information about the given database.</summary>
        /// <param name="infoLevel">The specific data to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        long GetInt64FileInfo(JET_DbInfo infoLevel);

        /// <summary>Retrieves certain information about the given database.</summary>
        /// <param name="infoLevel">The specific data to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        string GetStringInfo(JET_DbInfo infoLevel);

        /// <summary>Extends the size of a database that is currently open.</summary>
        /// <param name="desiredPages">The desired size of the database, in pages.</param>
        /// <param name="actualPages">The size of the database, in pages, after the call.</param>
        void Grow(int desiredPages, out int actualPages);

        /// <summary>Opens a cursor on a previously created table.</summary>
        /// <param name="tablename">The name of the table to open.</param>
        /// <param name="parameters">The parameter is not used.</param>
        /// <param name="grbit">Table open options.</param>
        /// <returns>An ESENT warning.</returns>
        IJetTable OpenTable(string tablename, byte[] parameters, OpenTableGrbit grbit);

        /// <summary>Creates a temporary table with a single index. A temporary table stores and
        /// retrieves records just like an ordinary table created using JetCreateTableColumnIndex.
        /// However, temporary tables are much faster than ordinary tables due to their volatile
        /// nature. They can also be used to very quickly sort and perform duplicate removal on
        /// record sets when accessed in a purely sequential manner.</summary>
        /// <param name="session">Session to use.</param>
        /// <param name="columns">Column definitions for the columns created in the temporary table.
        /// </param>
        /// <param name="grbit">Table creation options.</param>
        /// <param name="columnids">The output buffer that receives the array of column IDs generated
        /// during the creation of the temporary table. The column IDs in this array will exactly
        /// correspond to the input array of column definitions. As a result, the size of this buffer
        /// must correspond to the size of the input array.</param>
        /// <param name="lcid">The locale ID to use to compare any Unicode key column data in
        /// the temporary table. Any locale may be used as long as the appropriate language pack
        /// has been installed on the machine. </param>
        /// <param name="unicodeindex">The Locale ID and normalization flags that will be used
        /// to compare any Unicode key column data in the temporary table. When this is not
        /// present then the default options are used. </param>
        /// <returns>Returns the tableid of the temporary table. Closing this tableid with
        /// <see cref="IJetTable.Close"/> frees the resources associated with the temporary table.</returns>
        IJetTable OpenTemporaryTable(IJetSession session, JET_COLUMNDEF[] columns,
            TemporaryTableCreationFlags grbit, JET_COLUMNID[] columnids, int lcid /* JetOpenTempTable2*/,
            JET_UNICODEINDEX unicodeindex /* JetOpenTempTable3 */);

        /// <summary>Extends the size of a database that is currently open.</summary>
        /// <param name="desiredPages">The desired size of the database, in pages.</param>
        /// <param name="actualPages">The size of the database, in pages, after the call.</param>
        /// <returns>An error if the call fails.</returns>
        int SetSize(int desiredPages, out int actualPages);
    }
}
