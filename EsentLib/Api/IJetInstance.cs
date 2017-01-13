//-----------------------------------------------------------------------
// <copyright file="IJetApi.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

using EsentLib.Jet;
using EsentLib.Jet.Types;

namespace EsentLib.Api
{
    /// <summary>This interface describes all the methods which have a P/Invoke
    /// implementation. Concrete instances of this interface provide methods that
    /// call ESENT.</summary>
    [CLSCompliant(false)]
    public interface IJetInstance : IDisposable
    {
        /// <summary>Gets a description of the capabilities of the current version
        /// of ESENT.</summary>
        IJetCapabilities Capabilities { get; }

        /// <summary></summary>
        IntPtr Id { get; }

        ///// <summary>Get a collection of sessions that were born on the calling thread.</summary>
        //IJetThreadBornSessionsCollection ThreadBornSessions { get; }

        /// <summary>Performs a streaming backup of an instance, including all the attached
        /// databases, to a directory. With multiple backup methods supported by the engine,
        /// this is the simplest and most encapsulated function.</summary>
        /// <param name="destination">The directory where the backup is to be stored. If the
        /// backup path is null to use the function will truncate the logs, if possible.</param>
        /// <param name="grbit">Backup options.</param>
        /// <param name="statusCallback">Optional status notification callback.</param>
        /// <returns>An error code.</returns>
        void Backup(string destination, BackupGrbit grbit, JET_PFNSTATUS statusCallback);

        /// <summary>Initialize a new ESENT session.</summary>
        /// <returns>A new session.</returns>
        IJetSession BeginSession();

        /// <summary>Terminate an instance that was created with
        /// JetInstance.Create(string,string,CreateInstanceGrbit).</summary>
        /// <param name="grbit">Termination options.</param>
        void Close(TermGrbit grbit = TermGrbit.None);

        /// <summary>Ends an external backup session. This API is the last API in a series
        /// of APIs that must be called to execute a successful online (non-VSS based)
        /// backup.</summary>
        /// <param name="grbit">Options that specify how the backup ended.</param>
        void CompleteBackup(EndExternalBackupGrbit grbit = EndExternalBackupGrbit.None);

        /// <summary>Used during a backup initiated by <see cref="IJetInstance.PrepareBackup"/>
        /// to query an instance for the names of database files that should become part of
        /// the backup file set. Only databases that are currently attached to the instance
        /// using <see cref="IJetSession.AttachDatabase"/> will be considered. These files may
        /// subsequently be opened using <see cref="IJetBackupInstance.OpenFile"/> and read using
        /// <see cref="JET_HANDLE.Read"/>.</summary>
        /// <returns>Returns a list of strings describing the set of database files that should
        /// be a part of the backup file set. The list of strings returned .</returns>
        List<string> GetBackupFiles();

        /// <summary>Used during a backup initiated by <see cref="IJetInstance.PrepareBackup"/>
        /// to query an instance for the names of database patch files and logfiles that should
        /// become part of the backup file set. These files may subsequently be opened using
        /// <see cref="IJetBackupInstance.OpenFile"/> and read using <see cref="JET_HANDLE.Read"/>.
        /// </summary>
        /// <returns>Returns a list of strings describing the set of database files that should
        /// be a part of the backup file set. The list of strings returned .</returns>
        List<string> GetBackupLogFiles();

        /// <summary>Used during a backup initiated by <see cref="IJetInstance.PrepareBackup"/>
        /// to query an instance for the names of the transaction log files that can be safely
        /// deleted after the backup has successfully completed.</summary>
        /// <remarks>It is important to note that this API does not return an error or warning
        /// if the output buffer is too small to accept the full list of files that should be
        /// part of the backup file set.</remarks>
        /// <returns>Returns a list of strings describing the set of files that are ready for
        /// truncation.</returns>
        List<string> GetBackupTruncateReadyLogFiles();

        /// <summary>Retrieves information about an instance.</summary>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error code if the call fails.</returns>
        JET_SIGNATURE GetInfo(JET_InstanceMiscInfo infoLevel);

        /// <summary>Initialize the ESENT database engine.</summary>
        /// <param name="grbit">Initialization options.</param>
        /// <param name="recoveryOptions">Additional recovery parameters for remapping
        /// databases during recovery, position where to stop recovery at, or recovery
        /// status.</param>
        /// <returns>An error if the call fails.</returns>
        void Initialize(InitGrbit grbit = InitGrbit.None, JET_RSTINFO recoveryOptions = null);

        /// <summary>Initiates an external backup while the engine and database are online
        /// and active.</summary>
        /// <param name="incremental">Optional : default is false. If true an inremental backup
        /// is initiated which means that only the log files since the last full or incremental
        /// backup will be backed up, as opposed to a full backup.</param>
        /// <returns>A <see cref="IJetBackupInstance"/> implementation.</returns>
        IJetBackupInstance PrepareBackup(bool incremental = false);

        /// <summary>Restores and recovers a streaming backup of an instance including all
        /// the attached databases. It is designed to work with a backup created with the
        /// <see cref="IJetInstance.Backup"/> function. This is the simplest and most encapsulated
        /// restore function.</summary>
        /// <param name="source">Location of the backup. The backup should have been created
        /// with <see cref="IJetInstance.Backup"/>.</param>
        /// <param name="destination">Name of the folder where the database files from the
        /// backup set will be copied and recovered. If this is set to null, the database
        /// files will be copied and recovered to their original location.</param>
        /// <param name="statusCallback">Optional status notification callback.</param>
        /// <returns>An error code.</returns>
        void Restore(string source, string destination, JET_PFNSTATUS statusCallback);

        /// <summary>Sets parameter on a specific instance.
        /// <seealso cref="IJetEnvironment.SetSystemParameter(JET_param, JET_CALLBACK, string)"/></summary>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="paramValue">The value of the parameter to set, if the parameter is a
        /// JET_CALLBACK.</param>
        /// <param name="paramString">The value of the parameter to set, if the parameter is a
        /// string type.</param>
        /// <returns>An ESENT warning code.</returns>
        void SetSystemParameter(JET_param paramid, JET_CALLBACK paramValue, string paramString);

        /// <summary>Sets parameter on a specific instance.
        /// <seealso cref="IJetEnvironment.SetSystemParameter(JET_param, IntPtr, string)"/></summary>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="paramValue">The value of the parameter to set, if the parameter is an
        /// integer type.</param>
        /// <param name="paramString">The value of the parameter to set, if the parameter is a
        /// string type.</param>
        /// <returns>An error or warning.</returns>
        void SetSystemParameter(JET_param paramid, IntPtr paramValue, string paramString);

        /// <summary>Prepares an instance for termination.</summary>
        /// <param name="grbit">The options to stop or resume the instance.</param>
        void Stop(StopServiceGrbit grbit = StopServiceGrbit.All);

        /// <summary>Prevents streaming backup-related activity from continuing on a specific
        /// running instance, thus ending the streaming backup in a predictable way.</summary>
        void StopBackup();

        /// <summary>Used during a backup initiated by PrepareBackup to delete any transaction
        /// log files that will no longer be needed once the current backup completes
        /// successfully.</summary>
        void TruncateBackupLogs();

        //---------------------------------------------------------------//
        //---------------------------------------------------------------//
        //---------------------------------------------------------------//
        //---------------------------------------------------------------//

        #region Tables

        /// <summary>Duplicates an open cursor and returns a handle to the duplicated cursor.
        /// If the cursor that was duplicated was a read-only cursor then the duplicated cursor
        /// is also a read-only cursor. Any state related to constructing a search key or
        /// updating a record is not copied into the duplicated cursor. In addition, the location
        /// of the original cursor is not duplicated into the duplicated cursor. The duplicated
        /// cursor is always opened on the clustered index and its location is always on the
        /// first row of the table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to duplicate.</param>
        /// <param name="newTableid">The duplicated cursor.</param>
        /// <param name="grbit">Reserved for future use.</param>
        /// <returns>An error if the call fails.</returns>
        int JetDupCursor(JET_SESID sesid, JET_TABLEID tableid, out JET_TABLEID newTableid, DupCursorGrbit grbit);

        /// <summary>Enables the application to associate a context handle known as Local Storage
        /// with a cursor or the table associated with that cursor. This context handle can be
        /// used by the application to store auxiliary data that is associated with a cursor or
        /// table. The application is later notified using a runtime callback when the context
        /// handle must be released. This makes it possible to associate dynamically allocated
        /// state with a cursor or table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to use.</param>
        /// <param name="ls">The context handle to be associated with the session or cursor.</param>
        /// <param name="grbit">Set options.</param>
        /// <returns>An error if the call fails.</returns>
        int JetSetLS(JET_SESID sesid, JET_TABLEID tableid, JET_LS ls, LsGrbit grbit);

        /// <summary>Enables the application to retrieve the context handle known as Local
        /// Storage that is associated with a cursor or the table associated with that cursor.
        /// This context handle must have been previously set using <see cref="JetSetLS"/>.
        /// JetGetLS can also be used to simultaneously fetch the current context handle for
        /// a cursor or table and reset that context handle.  </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to use.</param>
        /// <param name="ls">Returns the retrieved context handle.</param>
        /// <param name="grbit">Retrieve options.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetLS(JET_SESID sesid, JET_TABLEID tableid, out JET_LS ls, LsGrbit grbit);

        #endregion

        #region Transactions

        ///// <summary>
        ///// Causes a session to enter a transaction or create a new save point in an existing
        ///// transaction.
        ///// </summary>
        ///// <param name="sesid">The session to begin the transaction for.</param>
        ///// <returns>An error if the call fails.</returns>
        //int JetBeginTransaction(JET_SESID sesid);

        ///// <summary>
        ///// Causes a session to enter a transaction or create a new save point in an existing
        ///// transaction.
        ///// </summary>
        ///// <param name="sesid">The session to begin the transaction for.</param>
        ///// <param name="grbit">Transaction options.</param>
        ///// <returns>An error if the call fails.</returns>
        //int JetBeginTransaction2(JET_SESID sesid, BeginTransactionGrbit grbit);

        ///// <summary>
        ///// Commits the changes made to the state of the database during the current save point
        ///// and migrates them to the previous save point. If the outermost save point is committed
        ///// then the changes made during that save point will be committed to the state of the
        ///// database and the session will exit the transaction.
        ///// </summary>
        ///// <param name="sesid">The session to commit the transaction for.</param>
        ///// <param name="grbit">Commit options.</param>
        ///// <returns>An error if the call fails.</returns>
        //int JetCommitTransaction(JET_SESID sesid, CommitTransactionGrbit grbit);

        ///// <summary>
        ///// Undoes the changes made to the state of the database
        ///// and returns to the last save point. JetRollback will also close any cursors
        ///// opened during the save point. If the outermost save point is undone, the
        ///// session will exit the transaction.
        ///// </summary>
        ///// <param name="sesid">The session to rollback the transaction for.</param>
        ///// <param name="grbit">Rollback options.</param>
        ///// <returns>An error if the call fails.</returns>
        //int JetRollback(JET_SESID sesid, RollbackTransactionGrbit grbit);

        #endregion

        #region DDL

        // NOT IMPLEMENTED
        ///// <summary>
        ///// Creates indexes over data in an ESE database.
        ///// </summary>
        ///// <param name="sesid">The session to use.</param>
        ///// <param name="tableid">The table to create the index on.</param>
        ///// <param name="indexcreates">Array of objects describing the indexes to be created.</param>
        ///// <param name="numIndexCreates">Number of index description objects.</param>
        ///// <returns>An error code.</returns>
        //int JetCreateIndex2(JET_SESID sesid, JET_TABLEID tableid, JET_INDEXCREATE[] indexcreates,
        //    int numIndexCreates);

        // NOT IMPLEMENTED
        ///// <summary>
        ///// Creates a table, adds columns, and indices on that table.
        ///// </summary>
        ///// <param name="sesid">The session to use.</param>
        ///// <param name="dbid">The database to which to add the new table.</param>
        ///// <param name="tablecreate">Object describing the table to create.</param>
        ///// <returns>An error if the call fails.</returns>
        //int JetCreateTableColumnIndex3(JET_SESID sesid, JET_DBID dbid, JET_TABLECREATE tablecreate);

        #region JetGetTableColumnInfo overloads

        /// <summary>
        /// Retrieves information about a table column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columndef">Filled in with information about the column.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetTableColumnInfo(JET_SESID sesid, JET_TABLEID tableid, string columnName,
            out JET_COLUMNDEF columndef);

        /// <summary>
        /// Retrieves information about a table column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="columnid">The columnid of the column.</param>
        /// <param name="columndef">Filled in with information about the column.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetTableColumnInfo(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid,
            out JET_COLUMNDEF columndef);

        /// <summary>
        /// Retrieves information about a table column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columnbase">Filled in with information about the column.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetTableColumnInfo(JET_SESID sesid, JET_TABLEID tableid, string columnName,
            out JET_COLUMNBASE columnbase);

        /// <summary>
        /// Retrieves information about a table column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="columnid">The columnid of the column.</param>
        /// <param name="columnbase">Filled in with information about the column.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetTableColumnInfo(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid,
            out JET_COLUMNBASE columnbase);

        #endregion

        #region JetGetColumnInfo overloads

        /// <summary>
        /// Retrieves information about a table column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database that contains the table.</param>
        /// <param name="tablename">The name of the table containing the column.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columndef">Filled in with information about the column.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetColumnInfo(JET_SESID sesid, JET_DBID dbid, string tablename, string columnName,
            out JET_COLUMNDEF columndef);

        ///// <summary>Retrieves information about all columns in a table.</summary>
        ///// <param name="sesid">The session to use.</param>
        ///// <param name="dbid">The database that contains the table.</param>
        ///// <param name="tablename">The name of the table containing the column.</param>
        ///// <param name="ignored">This parameter is ignored.</param>
        ///// <param name="columnlist">Filled in with information about the columns in the table.</param>
        ///// <returns>An error if the call fails.</returns>
        //int JetGetColumnInfo(JET_SESID sesid, JET_DBID dbid, string tablename, string ignored,
        //    out JET_COLUMNLIST columnlist);

        /// <summary>
        /// Retrieves information about a column in a table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database that contains the table.</param>
        /// <param name="tablename">The name of the table containing the column.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columnbase">Filled in with information about the columns in the table.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetColumnInfo(JET_SESID sesid, JET_DBID dbid, string tablename, string columnName,
            out JET_COLUMNBASE columnbase);

        /// <summary>
        /// Retrieves information about a column in a table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database that contains the table.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columnid">The ID of the column.</param>
        /// <param name="columnbase">Filled in with information about the columns in the table.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetColumnInfo(JET_SESID sesid, JET_DBID dbid, string columnName,
            JET_COLUMNID columnid, out JET_COLUMNBASE columnbase);
        #endregion

        #region JetGetObjectInfo overloads

        /// <summary>
        /// Retrieves information about database objects.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to use.</param>
        /// <param name="objtyp">The type of the object.</param>
        /// <param name="objectName">The object name about which to retrieve information.</param>
        /// <param name="objectinfo">Filled in with information about the objects in the database.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetObjectInfo(JET_SESID sesid, JET_DBID dbid, JET_objtyp objtyp, string objectName,
            out JET_OBJECTINFO objectinfo);

        #endregion

        #region JetGetTableInfo overloads

        /// <summary>
        /// Retrieves various pieces of information about a table in a database.
        /// </summary>
        /// <remarks>
        /// This overload is used with <see cref="JET_TblInfo.Default"/>.
        /// </remarks>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve information about.</param>
        /// <param name="result">Retrieved information.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetTableInfo(JET_SESID sesid, JET_TABLEID tableid, out JET_OBJECTINFO result, JET_TblInfo infoLevel);

        /// <summary>
        /// Retrieves various pieces of information about a table in a database.
        /// </summary>
        /// <remarks>
        /// This overload is used with <see cref="JET_TblInfo.Name"/> and
        /// <see cref="JET_TblInfo.TemplateTableName"/>.
        /// </remarks>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve information about.</param>
        /// <param name="result">Retrieved information.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetTableInfo(JET_SESID sesid, JET_TABLEID tableid, out string result, JET_TblInfo infoLevel);

        /// <summary>
        /// Retrieves various pieces of information about a table in a database.
        /// </summary>
        /// <remarks>
        /// This overload is used with <see cref="JET_TblInfo.Dbid"/>.
        /// </remarks>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve information about.</param>
        /// <param name="result">Retrieved information.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetTableInfo(JET_SESID sesid, JET_TABLEID tableid, out JET_DBID result, JET_TblInfo infoLevel);

        /// <summary>
        /// Retrieves various pieces of information about a table in a database.
        /// </summary>
        /// <remarks>
        /// This overload is used with <see cref="JET_TblInfo.SpaceUsage"/> and
        /// <see cref="JET_TblInfo.SpaceAlloc"/>.
        /// </remarks>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve information about.</param>
        /// <param name="result">Retrieved information.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetTableInfo(JET_SESID sesid, JET_TABLEID tableid, int[] result, JET_TblInfo infoLevel);

        /// <summary>
        /// Retrieves various pieces of information about a table in a database.
        /// </summary>
        /// <remarks>
        /// This overload is used with <see cref="JET_TblInfo.SpaceOwned"/> and
        /// <see cref="JET_TblInfo.SpaceAvailable"/>.
        /// </remarks>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve information about.</param>
        /// <param name="result">Retrieved information.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetTableInfo(JET_SESID sesid, JET_TABLEID tableid, out int result, JET_TblInfo infoLevel);

        #endregion

        #region JetGetIndexInfo overloads

        /// <summary>
        /// Retrieves information about indexes on a table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to use.</param>
        /// <param name="tablename">The name of the table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index to retrieve information about.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetIndexInfo(
            JET_SESID sesid,
            JET_DBID dbid,
            string tablename,
            string indexname,
            out ushort result,
            JET_IdxInfo infoLevel);

        /// <summary>
        /// Retrieves information about indexes on a table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to use.</param>
        /// <param name="tablename">The name of the table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index to retrieve information about.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetIndexInfo(
            JET_SESID sesid,
            JET_DBID dbid,
            string tablename,
            string indexname,
            out int result,
            JET_IdxInfo infoLevel);

        /// <summary>
        /// Retrieves information about indexes on a table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to use.</param>
        /// <param name="tablename">The name of the table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index to retrieve information about.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetIndexInfo(
            JET_SESID sesid,
            JET_DBID dbid,
            string tablename,
            string indexname,
            out JET_INDEXID result,
            JET_IdxInfo infoLevel);

        /// <summary>
        /// Retrieves information about indexes on a table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to use.</param>
        /// <param name="tablename">The name of the table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index to retrieve information about.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetIndexInfo(
            JET_SESID sesid,
            JET_DBID dbid,
            string tablename,
            string indexname,
            out JET_INDEXLIST result,
            JET_IdxInfo infoLevel);

        /// <summary>
        /// Retrieves information about indexes on a table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to use.</param>
        /// <param name="tablename">The name of the table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index to retrieve information about.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetIndexInfo(
            JET_SESID sesid,
            JET_DBID dbid,
            string tablename,
            string indexname,
            out string result,
            JET_IdxInfo infoLevel);

        #endregion
        /// <summary>Changes the name of an existing column.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="name">The name of the column.</param>
        /// <param name="newName">The new name of the column.</param>
        /// <param name="grbit">Column rename options.</param>
        /// <returns>An error if the call fails.</returns>
        int JetRenameColumn(JET_SESID sesid, JET_TABLEID tableid, string name, string newName, RenameColumnGrbit grbit);

        /// <summary>
        /// Changes the default value of an existing column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database containing the column.</param>
        /// <param name="tableName">The name of the table containing the column.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="data">The new default value.</param>
        /// <param name="dataSize">Size of the new default value.</param>
        /// <param name="grbit">Column default value options.</param>
        /// <returns>An error if the call fails.</returns>
        int JetSetColumnDefaultValue(
            JET_SESID sesid, JET_DBID dbid, string tableName, string columnName, byte[] data, int dataSize, SetColumnDefaultValueGrbit grbit);

        #endregion

        #region Navigation

        /// <summary>
        /// Positions a cursor to an index entry that is associated with the
        /// specified secondary index bookmark. The secondary index bookmark
        /// must be used with the same index over the same table from which it
        /// was originally retrieved. The secondary index bookmark for an index
        /// entry can be retrieved using <see cref="JetGotoSecondaryIndexBookmark"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table cursor to position.</param>
        /// <param name="secondaryKey">The buffer that contains the secondary key.</param>
        /// <param name="secondaryKeySize">The size of the secondary key.</param>
        /// <param name="primaryKey">The buffer that contains the primary key.</param>
        /// <param name="primaryKeySize">The size of the primary key.</param>
        /// <param name="grbit">Options for positioning the bookmark.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGotoSecondaryIndexBookmark(JET_SESID sesid, JET_TABLEID tableid, byte[] secondaryKey,
            int secondaryKeySize, byte[] primaryKey, int primaryKeySize, GotoSecondaryIndexBookmarkGrbit grbit);

        /// <summary>Computes the intersection between multiple sets of index entries from different
        /// secondary indices over the same table. This operation is useful for finding the set of records
        /// in a table that match two or more criteria that can be expressed using index ranges.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="ranges">An the index ranges to intersect. The tableids in the ranges must have
        /// index ranges set on them.</param>
        /// <param name="numRanges">The number of index ranges.</param>
        /// <param name="recordlist">Returns information about the temporary table containing the
        /// intersection results.</param>
        /// <param name="grbit">Intersection options.</param>
        /// <returns>An error if the call fails.</returns>
        int JetIntersectIndexes(JET_SESID sesid, JET_INDEXRANGE[] ranges, int numRanges,
            out JET_RECORDLIST recordlist, IntersectIndexesGrbit grbit);

        /// <summary>Counts the number of entries in the current index from the current position
        /// forward. The current position is included in the count. The count can be greater than
        /// the total number of records in the table if the current index is over a multi-valued
        /// column and instances of the column have multiple-values. If the table is empty, then
        /// 0 will be returned for the count. </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to count the records in.</param>
        /// <param name="numRecords">Returns the number of records.</param>
        /// <param name="maxRecordsToCount">The maximum number of records to count.</param>
        /// <returns>An error if the call fails.</returns>
        int JetIndexRecordCount(JET_SESID sesid, JET_TABLEID tableid, out int numRecords,
            int maxRecordsToCount);

        /// <summary>Notifies the database engine that the application is scanning the entire index
        /// that the cursor is positioned on. Consequently, the methods that are used to access the
        /// index data will be tuned to make this scenario as fast as possible. </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor that will be accessing the data.</param>
        /// <param name="grbit">Reserved for future use.</param>
        /// <returns>An error if the call fails.</returns>
        int JetSetTableSequential(JET_SESID sesid, JET_TABLEID tableid, SetTableSequentialGrbit grbit);

        /// <summary>Notifies the database engine that the application is no longer scanning the entire
        /// index the cursor is positioned on. This call reverses a notification sent by JetSetTableSequential.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor that was accessing the data.</param>
        /// <param name="grbit">Reserved for future use.</param>
        /// <returns>An error if the call fails.</returns>
        int JetResetTableSequential(JET_SESID sesid, JET_TABLEID tableid, ResetTableSequentialGrbit grbit);

        /// <summary>Returns the fractional position of the current record in the current index in the
        /// form of a JET_RECPOS structure.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor positioned on the record.</param>
        /// <param name="recpos">Returns the approximate fractional position of the record.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetRecordPosition(JET_SESID sesid, JET_TABLEID tableid, out JET_RECPOS recpos);

        /// <summary>Moves a cursor to a new location that is a fraction of the way through the current
        /// index. </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <param name="recpos">The approximate position to move to.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGotoPosition(JET_SESID sesid, JET_TABLEID tableid, JET_RECPOS recpos);

        /// <summary>If the records with the specified keys are not in the buffer cache then start
        /// asynchronous reads to bring the records into the database buffer cache.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to issue the prereads against.</param>
        /// <param name="keys">The keys to preread. The keys must be sorted.</param>
        /// <param name="keyLengths">The lengths of the keys to preread.</param>
        /// <param name="keyIndex">The index of the first key in the keys array to read.</param>
        /// <param name="keyCount">The maximum number of keys to preread.</param>
        /// <param name="keysPreread">Returns the number of keys to actually preread.</param>
        /// <param name="grbit">Preread options. Used to specify the direction of the preread.</param>
        /// <returns>An error or warning.</returns>
        int JetPrereadKeys(JET_SESID sesid, JET_TABLEID tableid, byte[][] keys, int[] keyLengths,
            int keyIndex, int keyCount, out int keysPreread, PrereadKeysGrbit grbit);
        #endregion

        #region Data Retrieval

        /// <summary>Retrieves the bookmark for the record that is associated with the index
        /// entry at the current position of a cursor. This bookmark can then be used to
        /// reposition that cursor back to the same record using
        /// <see cref="IJetCursor.GotoBookmark"/>. The bookmark will be no longer than
        /// <see cref="JetEnvironment.BookmarkMost"/> bytes.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the bookmark from.</param>
        /// <param name="bookmark">Buffer to contain the bookmark.</param>
        /// <param name="bookmarkSize">Size of the bookmark buffer.</param>
        /// <param name="actualBookmarkSize">Returns the actual size of the bookmark.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetBookmark(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize,
            out int actualBookmarkSize);

        /// <summary>
        /// Retrieves a special bookmark for the secondary index entry at the
        /// current position of a cursor. This bookmark can then be used to
        /// efficiently reposition that cursor back to the same index entry
        /// using JetGotoSecondaryIndexBookmark. This is most useful when
        /// repositioning on a secondary index that contains duplicate keys or
        /// that contains multiple index entries for the same record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the bookmark from.</param>
        /// <param name="secondaryKey">Output buffer for the secondary key.</param>
        /// <param name="secondaryKeySize">Size of the secondary key buffer.</param>
        /// <param name="actualSecondaryKeySize">Returns the size of the secondary key.</param>
        /// <param name="primaryKey">Output buffer for the primary key.</param>
        /// <param name="primaryKeySize">Size of the primary key buffer.</param>
        /// <param name="actualPrimaryKeySize">Returns the size of the primary key.</param>
        /// <param name="grbit">Options for the call.</param>
        /// <returns>An error if the call fails.</returns>
        int JetGetSecondaryIndexBookmark(
            JET_SESID sesid,
            JET_TABLEID tableid,
            byte[] secondaryKey,
            int secondaryKeySize,
            out int actualSecondaryKeySize,
            byte[] primaryKey,
            int primaryKeySize,
            out int actualPrimaryKeySize,
            GetSecondaryIndexBookmarkGrbit grbit);

        /// <summary>
        /// Retrieves the key for the index entry at the current position of a cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the key from.</param>
        /// <param name="data">The buffer to retrieve the key into.</param>
        /// <param name="dataSize">The size of the buffer.</param>
        /// <param name="actualDataSize">Returns the actual size of the data.</param>
        /// <param name="grbit">Retrieve key options.</param>
        /// <returns>An error if the call fails.</returns>
        int JetRetrieveKey(JET_SESID sesid, JET_TABLEID tableid, byte[] data, int dataSize, out int actualDataSize, RetrieveKeyGrbit grbit);

        /// <summary>The JetRetrieveColumns function retrieves multiple column values from
        /// the current record in a single operation. An array of <see cref="NATIVE_RETRIEVECOLUMN"/>
        /// structures is used to describe the set of column values to be retrieved, and
        /// to describe output buffers for each column value to be retrieved.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve columns from.</param>
        /// <param name="retrievecolumns">An array of one or more JET_RETRIEVECOLUMN structures.
        /// Each structure includes descriptions of which column value to retrieve and where
        /// to store returned data.</param>
        /// <param name="numColumns">Number of structures in the array given by retrievecolumns.</param>
        /// <returns>An error or warning.</returns>
        unsafe int JetRetrieveColumns(JET_SESID sesid, JET_TABLEID tableid,
            NATIVE_RETRIEVECOLUMN* retrievecolumns, int numColumns);

        /// <summary>
        /// Efficiently retrieves a set of columns and their values from the
        /// current record of a cursor or the copy buffer of that cursor. The
        /// columns and values retrieved can be restricted by a list of
        /// column IDs, itagSequence numbers, and other characteristics. This
        /// column retrieval API is unique in that it returns information in
        /// dynamically allocated memory that is obtained using a
        /// user-provided realloc compatible callback. This new flexibility
        /// permits the efficient retrieval of column data with specific
        /// characteristics (such as size and multiplicity) that are unknown
        /// to the caller. This eliminates the need for the use of the discovery
        /// modes of JetRetrieveColumn to determine those
        /// characteristics in order to setup a final call to
        /// JetRetrieveColumn that will successfully retrieve
        /// the desired data.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve data from.</param>
        /// <param name="numColumnids">The numbers of JET_ENUMCOLUMNIDS.</param>
        /// <param name="columnids">
        /// An optional array of column IDs, each with an optional array of itagSequence
        /// numbers to enumerate.
        /// </param>
        /// <param name="numColumnValues">
        /// Returns the number of column values retrieved.
        /// </param>
        /// <param name="columnValues">
        /// Returns the enumerated column values.
        /// </param>
        /// <param name="allocator">
        /// Callback used to allocate memory.
        /// </param>
        /// <param name="allocatorContext">
        /// Context for the allocation callback.
        /// </param>
        /// <param name="maxDataSize">
        /// Sets a cap on the amount of data to return from a long text or long
        /// binary column. This parameter can be used to prevent the enumeration
        /// of an extremely large column value.
        /// </param>
        /// <param name="grbit">Retrieve options.</param>
        /// <returns>A warning, error or success.</returns>
        int JetEnumerateColumns(
            JET_SESID sesid,
            JET_TABLEID tableid,
            int numColumnids,
            JET_ENUMCOLUMNID[] columnids,
            out int numColumnValues,
            out JET_ENUMCOLUMN[] columnValues,
            JET_PFNREALLOC allocator,
            IntPtr allocatorContext,
            int maxDataSize,
            EnumerateColumnsGrbit grbit);

        /// <summary>
        /// Efficiently retrieves a set of columns and their values from the
        /// current record of a cursor or the copy buffer of that cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve data from.</param>
        /// <param name="grbit">Enumerate options.</param>
        /// <param name="enumeratedColumns">The discovered columns and their values.</param>
        /// <returns>A warning or success.</returns>
        int JetEnumerateColumns(JET_SESID sesid, JET_TABLEID tableid, EnumerateColumnsGrbit grbit,
            out IEnumerable<EnumeratedColumn> enumeratedColumns);

        /// <summary>
        /// Retrieves record size information from the desired location.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">
        /// The cursor that will be used for the API call. The cursor must be
        /// positioned on a record, or have an update prepared.
        /// </param>
        /// <param name="recsize">Returns the size of the record.</param>
        /// <param name="grbit">Call options.</param>
        /// <returns>A warning, error or success.</returns>
        int JetGetRecordSize(JET_SESID sesid, JET_TABLEID tableid, ref JET_RECSIZE recsize, GetRecordSizeGrbit grbit);
        #endregion

        #region DML

        /// <summary>
        /// Deletes the current record in a database table.
        /// </summary>
        /// <param name="sesid">The session that opened the cursor.</param>
        /// <param name="tableid">The cursor on a database table. The current row will be deleted.</param>
        /// <returns>An error if the call fails.</returns>
        int JetDelete(JET_SESID sesid, JET_TABLEID tableid);

        /// <summary>
        /// The JetSetColumn function modifies a single column value in a modified record to be inserted or to
        /// update the current record. It can overwrite an existing value, add a new value to a sequence of
        /// values in a multi-valued column, remove a value from a sequence of values in a multi-valued column,
        /// or update all or part of a long value (a column of type <see cref="JET_coltyp.LongText"/>
        /// or <see cref="JET_coltyp.LongBinary"/>). 
        /// </summary>
        /// <remarks>
        /// The SetColumn methods provide datatype-specific overrides which may be more efficient.
        /// </remarks>
        /// <param name="sesid">The session which is performing the update.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        /// <param name="dataSize">The size of data to set.</param>
        /// <param name="grbit">SetColumn options.</param>
        /// <param name="setinfo">Used to specify itag or long-value offset.</param>
        /// <returns>An error if the call fails.</returns>
        int JetSetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, IntPtr data, int dataSize, SetColumnGrbit grbit, JET_SETINFO setinfo);

        /// <summary>
        /// Allows an application to set multiple column values in a single
        /// operation. An array of <see cref="NATIVE_SETCOLUMN"/> structures is
        /// used to describe the set of column values to be set, and to describe
        /// input buffers for each column value to be set.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to set the columns on.</param>
        /// <param name="setcolumns">
        /// An array of <see cref="NATIVE_SETCOLUMN"/> structures describing the
        /// data to set.
        /// </param>
        /// <param name="numColumns">
        /// Number of entries in the setcolumns parameter.
        /// </param>
        /// <returns>An error code or warning.</returns>
        unsafe int JetSetColumns(JET_SESID sesid, JET_TABLEID tableid, NATIVE_SETCOLUMN* setcolumns, int numColumns);

        /// <summary>
        /// Performs an atomic addition operation on one column. This function allows
        /// multiple sessions to update the same record concurrently without conflicts.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update.</param>
        /// <param name="columnid">
        /// The column to update. This must be an escrow updatable column.
        /// </param>
        /// <param name="delta">The buffer containing the addend.</param>
        /// <param name="deltaSize">The size of the addend.</param>
        /// <param name="previousValue">
        /// An output buffer that will recieve the current value of the column. This buffer
        /// can be null.
        /// </param>
        /// <param name="previousValueLength">The size of the previousValue buffer.</param>
        /// <param name="actualPreviousValueLength">Returns the actual size of the previousValue.</param>
        /// <param name="grbit">Escrow update options.</param>
        /// <returns>An error code if the operation fails.</returns>
        int JetEscrowUpdate(
            JET_SESID sesid,
            JET_TABLEID tableid,
            JET_COLUMNID columnid,
            byte[] delta,
            int deltaSize,
            byte[] previousValue,
            int previousValueLength,
            out int actualPreviousValueLength,
            EscrowUpdateGrbit grbit);

        #endregion

        #region Callbacks

        /// <summary>
        /// Allows the application to configure the database engine to issue
        /// notifications to the application for specific events. These
        /// notifications are associated with a specific table and remain in
        /// effect only until the instance containing the table is shut down
        /// using.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">
        /// A cursor opened on the table that the callback should be
        /// registered on.
        /// </param>
        /// <param name="cbtyp">
        /// The callback reasons for which the application wishes to receive notifications.
        /// </param>
        /// <param name="callback">The callback function.</param>
        /// <param name="context">A context that will be given to the callback.</param>
        /// <param name="callbackId">
        /// A handle that can later be used to cancel the registration of the given
        /// callback function using <see cref="JetUnregisterCallback"/>.
        /// </param>
        /// <returns>An error if the call fails.</returns>
        int JetRegisterCallback(
            JET_SESID sesid,
            JET_TABLEID tableid,
            JET_cbtyp cbtyp,
            JET_CALLBACK callback,
            IntPtr context,
            out JET_HANDLE callbackId);

        /// <summary>
        /// Configures the database engine to stop issuing notifications to the
        /// application as previously requested through
        /// <see cref="JetRegisterCallback"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">
        /// A cursor opened on the table that the callback should be
        /// registered on.
        /// </param>
        /// <param name="cbtyp">
        /// The callback reasons for which the application no longer wishes to receive notifications.
        /// </param>
        /// <param name="callbackId">
        /// The handle of the registered callback that was returned by <see cref="JetRegisterCallback"/>.
        /// </param>
        /// <returns>An error if the call fails.</returns>
        int JetUnregisterCallback(JET_SESID sesid, JET_TABLEID tableid, JET_cbtyp cbtyp, JET_HANDLE callbackId);

        #endregion

        #region Online Maintenance

        /// <summary>
        /// Starts and stops database defragmentation tasks that improves data
        /// organization within a database.
        /// </summary>
        /// <param name="sesid">The session to use for the call.</param>
        /// <param name="dbid">The database to be defragmented.</param>
        /// <param name="tableName">
        /// Under some options defragmentation is performed for the entire database described by the given 
        /// database ID, and other options require the name of the table to defragment. </param>
        /// <param name="passes">
        /// When starting an online defragmentation task, this parameter sets the maximum number of defragmentation
        /// passes. When stopping an online defragmentation task, this parameter is set to the number of passes
        /// performed. This is not honored in all modes .</param>
        /// <param name="seconds">
        /// When starting an online defragmentation task, this parameter sets
        /// the maximum time for defragmentation. When stopping an online
        /// defragmentation task, this output buffer is set to the length of
        /// time used for defragmentation. This is not honored in all modes .</param>
        /// <param name="grbit">Defragmentation options.</param>
        /// <returns>An error code or warning.</returns>
        /// <seealso cref="IJetInstance.Defragment"/>.
        int JetDefragment(JET_SESID sesid, JET_DBID dbid, string tableName, ref int passes,
            ref int seconds, DefragGrbit grbit);

        /// <summary>
        /// Starts and stops database defragmentation tasks that improves data
        /// organization within a database.
        /// </summary>
        /// <param name="sesid">The session to use for the call.</param>
        /// <param name="dbid">The database to be defragmented.</param>
        /// <param name="tableName">
        /// Under some options defragmentation is performed for the entire database described by the given 
        /// database ID, and other options require the name of the table to defragment.</param>
        /// <param name="grbit">Defragmentation options.</param>
        /// <returns>An error code or warning.</returns>
        /// <seealso cref="IJetInstance.JetDefragment"/>.
        int Defragment(
            JET_SESID sesid,
            JET_DBID dbid,
            string tableName,
            DefragGrbit grbit);

        /// <summary>
        /// Starts and stops database defragmentation tasks that improves data
        /// organization within a database.
        /// </summary>
        /// <param name="sesid">The session to use for the call.</param>
        /// <param name="dbid">The database to be defragmented.</param>
        /// <param name="tableName">
        /// Unused parameter. Defragmentation is performed for the entire database described by the given database ID.
        /// </param>
        /// <param name="passes">
        /// When starting an online defragmentation task, this parameter sets the maximum number of defragmentation
        /// passes. When stopping an online defragmentation task, this parameter is set to the number of passes
        /// performed.
        /// </param>
        /// <param name="seconds">
        /// When starting an online defragmentation task, this parameter sets
        /// the maximum time for defragmentation. When stopping an online
        /// defragmentation task, this output buffer is set to the length of
        /// time used for defragmentation.
        /// </param>
        /// <param name="callback">Callback function that defrag uses to report progress.</param>
        /// <param name="grbit">Defragmentation options.</param>
        /// <returns>An error code or warning.</returns>
        int JetDefragment2(
            JET_SESID sesid,
            JET_DBID dbid,
            string tableName,
            ref int passes,
            ref int seconds,
            JET_CALLBACK callback,
            DefragGrbit grbit);

        /// <summary>
        /// Performs idle cleanup tasks or checks the version store status in ESE.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="grbit">A combination of JetIdleGrbit flags.</param>
        /// <returns>An error code if the operation fails.</returns>
        int JetIdle(JET_SESID sesid, IdleGrbit grbit);

        #endregion

        #region Misc

        /// <summary>
        /// Crash dump options for Watson.
        /// </summary>
        /// <param name="grbit">Crash dump options.</param>
        /// <returns>An error code.</returns>
        int JetConfigureProcessForCrashDump(CrashDumpGrbit grbit);

        #endregion

        // --------- //
        // WINDOWS 8 //
        // --------- //
        #region Transactions

        ///// <summary>Commits the changes made to the state of the database during the current
        ///// save point and migrates them to the previous save point. If the outermost save point
        ///// is committed then the changes made during that save point will be committed to the
        ///// state of the database and the session will exit the transaction.</summary>
        ///// <param name="sesid">The session to commit the transaction for.</param>
        ///// <param name="grbit">Commit options.</param>
        ///// <param name="durableCommit">Duration to commit lazy transaction.</param>
        ///// <param name="commitId">Commit-id associated with this commit record.</param>
        ///// <returns>An error if the call fails.</returns>
        //int JetCommitTransaction2(JET_SESID sesid, CommitTransactionGrbit grbit, TimeSpan durableCommit,
        //    out JET_COMMIT_ID commitId);

        #endregion

        /// <summary>Gets extended information about an error.</summary>
        /// <param name="error">The error code about which to retrieve information.</param>
        /// <param name="errinfo">Information about the specified error code.</param>
        /// <returns>An error code.</returns>
        int JetGetErrorInfo(JET_err error, out JET_ERRINFOBASIC errinfo);

        #region DDL

        /// <summary>Creates a table, adds columns, and indices on that table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to which to add the new table.</param>
        /// <param name="tablecreate">Object describing the table to create.</param>
        /// <returns>An error if the call fails.</returns>
        int JetCreateTableColumnIndex4(JET_SESID sesid, JET_DBID dbid, JET_TABLECREATE tablecreate);
        #endregion

        #region Session Parameters
        ///// <summary>
        ///// Gets a parameter on the provided session state, used for the lifetime of this session or until reset.
        ///// </summary>
        ///// <param name="sesid">The session to set the parameter on.</param>
        ///// <param name="sesparamid">The ID of the session parameter to set, see
        ///// <see cref="JET_sesparam"/> and <see cref="EsentLib.Jet.JET_sesparam"/>.</param>
        ///// <param name="value">A 32-bit integer to retrieve.</param>
        ///// <returns>An error if the call fails.</returns>
        //int JetGetSessionParameter(JET_SESID sesid, JET_sesparam sesparamid, out int value);

        ///// <summary> Gets a parameter on the provided session state, used for the lifetime of
        ///// this session or until reset.</summary>
        ///// <param name="sesid">The session to set the parameter on.</param>
        ///// <param name="sesparamid">The ID of the session parameter to set, see
        ///// <see cref="JET_sesparam"/> and <see cref="EsentLib.Jet.JET_sesparam"/>.</param>
        ///// <param name="data">A byte array to retrieve.</param>
        ///// <param name="length">AThe length of the data array.</param>
        ///// <param name="actualDataSize">The actual size of the data field.</param>
        ///// <returns>An error if the call fails.</returns>
        //int JetGetSessionParameter(JET_SESID sesid, JET_sesparam sesparamid, byte[] data, int length,
        //    out int actualDataSize);

        // This one is for Windows 10
        ///// <summary>Sets a parameter on the provided session state, used for the lifetime of
        ///// this session or until reset.</summary>
        ///// <param name="sesid">The session to set the parameter on.</param>
        ///// <param name="sesparamid">The ID of the session parameter to retrieve.</param>
        ///// <param name="operationContext">An operation context to retrieve.</param>
        ///// <seealso cref="JET_OPERATIONCONTEXT"/>
        ///// <returns>An error code.</returns>
        //int JetGetSessionParameter(JET_SESID sesid, JET_sesparam sesparamid,
        //    out JET_OPERATIONCONTEXT operationContext);

        ///// <summary>Sets a parameter on the provided session state, used for the lifetime of
        ///// this session or until reset.</summary>
        ///// <param name="sesid">The session to set the parameter on.</param>
        ///// <param name="sesparamid">The ID of the session parameter to set.</param>
        ///// <param name="value">A 32-bit integer to set.</param>
        ///// <returns>An error if the call fails.</returns>
        //int JetSetSessionParameter(JET_SESID sesid, JET_sesparam sesparamid, int value);

        ///// <summary>Sets a parameter on the provided session state, used for the lifetime of
        ///// this session or until reset.</summary>
        ///// <param name="sesid">The session to set the parameter on.</param>
        ///// <param name="sesparamid">The ID of the session parameter to set.</param>
        ///// <param name="data">Data to set in this session parameter.</param>
        ///// <param name="dataSize">Size of the data provided.</param>
        ///// <returns>An error if the call fails.</returns>
        //int JetSetSessionParameter(JET_SESID sesid, JET_sesparam sesparamid, byte[] data, int dataSize);

        #endregion

        #region Misc

        /// <summary>If the records with the specified key ranges are not in the buffer cache,
        /// then start asynchronous reads to bring the records into the database buffer cache.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to issue the prereads against.</param>
        /// <param name="indexRanges">The key rangess to preread.</param>
        /// <param name="rangeIndex">The index of the first key range in the array to read.</param>
        /// <param name="rangeCount">The maximum number of key ranges to preread.</param>
        /// <param name="rangesPreread">Returns the number of keys actually preread.</param>
        /// <param name="columnsPreread">List of column ids for long value columns to preread.</param>
        /// <param name="grbit">Preread options. Used to specify the direction of the preread.</param>
        /// <returns>An error if the call fails.</returns>
        int JetPrereadIndexRanges(JET_SESID sesid, JET_TABLEID tableid, JET_INDEX_RANGE[] indexRanges,
            int rangeIndex, int rangeCount, out int rangesPreread, JET_COLUMNID[] columnsPreread,
            PrereadIndexRangesGrbit grbit);

        /// <summary>If the records with the specified key ranges are not in the buffer cache
        /// then start asynchronous reads to bring the records into the database buffer cache.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to issue the prereads against.</param>
        /// <param name="keysStart">The start of key ranges to preread.</param>
        /// <param name="keyStartLengths">The lengths of the start keys to preread.</param>
        /// <param name="keysEnd">The end of key rangess to preread.</param>
        /// <param name="keyEndLengths">The lengths of the end keys to preread.</param>
        /// <param name="rangeIndex">The index of the first key range in the array to read.</param>
        /// <param name="rangeCount">The maximum number of key ranges to preread.</param>
        /// <param name="rangesPreread">Returns the number of keys actually preread.</param>
        /// <param name="columnsPreread">List of column ids for long value columns to preread.</param>
        /// <param name="grbit">Preread options. Used to specify the direction of the preread.</param>
        /// <returns>An error or warning.</returns>
        int JetPrereadKeyRanges(JET_SESID sesid, JET_TABLEID tableid, byte[][] keysStart,
            int[] keyStartLengths, byte[][] keysEnd, int[] keyEndLengths, int rangeIndex,
            int rangeCount, out int rangesPreread, JET_COLUMNID[] columnsPreread,
            PrereadIndexRangesGrbit grbit);

        /// <summary> Set an array of simple filters for
        /// <see cref="LegacyApi.JetMove(JET_SESID,JET_TABLEID,int,MoveGrbit)"/></summary>
        /// <param name="sesid">The session to use for the call.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <param name="filters">Simple record filters.</param>
        /// <param name="grbit">Move options.</param>
        /// <returns>An error if the call fails.</returns>
        int JetSetCursorFilter(JET_SESID sesid, JET_TABLEID tableid, JET_INDEX_COLUMN[] filters,
            CursorFilterGrbit grbit);

        #endregion

        // ---------- //
        // WINDOWS 10 //
        // ---------- //

        ///// <summary>Sets a parameter on the provided session state, used for the lifetime
        ///// of this session or until reset.</summary>
        ///// <param name="sesid">The session to set the parameter on.</param>
        ///// <param name="sesparamid">The ID of the session parameter to set.</param>
        ///// <param name="operationContext">An operation context to set.</param>
        ///// <returns>An error code.</returns>
        //int JetSetSessionParameter(JET_SESID sesid, JET_sesparam sesparamid, JET_OPERATIONCONTEXT operationContext);
    }
}
