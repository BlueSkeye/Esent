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

        /// <summary>Resizes a currently open database. Windows 8: Only supports growing a database file.
        /// Windows 8.1: When <see cref="JET_param.EnableShrinkDatabase"/> is set to
        /// <see cref="ShrinkDatabaseGrbit.On"/>, and if the file system supports Sparse
        /// Files, then space may be freed up in the middle of the file.</summary>
        /// <remarks>Many APIs return the logical size of the file, not how many bytes it takes up on disk.
        /// Win32's GetCompressedFileSize returns the correct on-disk size. <see cref="GetInfo(JET_DbInfo)"/>
        /// returns the on-disk size when used with <see cref="JET_DbInfo.FilesizeOnDisk"/></remarks>
        /// <param name="session">Session to use.</param>
        /// <param name="desiredPages">The desired size of the database, in pages.</param>
        /// <param name="grbit">Resize options.</param>
        /// <returns>The size of the database, in pages, after the call.</returns>
        int Resize(IJetSession session, int desiredPages,
            ResizeDatabaseGrbit grbit = ResizeDatabaseGrbit.None);
    }
}
