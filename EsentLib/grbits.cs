﻿//-----------------------------------------------------------------------
// <copyright file="grbits.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

using EsentLib.Api;
using EsentLib.Implementation;
using EsentLib.Jet;
using EsentLib.Jet.Types;

namespace EsentLib
{
    /// <summary>Options for <see cref="JetInstance.Create(string,string,CreateInstanceGrbit)"/>.</summary>
    [Flags]
    public enum CreateInstanceGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,
    }

    /// <summary>Options for <see cref="JetInstance.Initialize"/>.</summary>
    [Flags]
    public enum InitGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        // ----- //
        // VISTA //
        // ----- //
        /// <summary>Perform recovery, but halt at the Undo phase. Allows whatever logs are present to
        /// be replayed, then later additional logs can be copied and replayed.</summary>
        RecoveryWithoutUndo = 0x8,

        /// <summary>On successful soft recovery, truncate log files.</summary>
        TruncateLogsAfterRecovery = 0x00000010,

        /// <summary>Missing database map entry default to same location.</summary>
        ReplayMissingMapEntryDB = 0x00000020,

        /// <summary>Transaction logs must exist in the log file directory (i.e. can't auto-start
        /// a new stream).</summary>
        LogStreamMustExist = 0x40,

        // --------- //
        // WINDOWS 7 //
        // --------- //
        /// <summary>Recover without error even if uncommitted logs have been lost. Set 
        /// the recovery waypoint with Windows7Param.WaypointLatency to enable this type
        /// of recovery.</summary>
        ReplayIgnoreLostLogs = 0x80,

        // --------- //
        // WINDOWS 8 //
        // --------- //
        /// <summary>Allows db to remain attached at the end of recovery (for faster transition
        /// to running state).</summary>
        KeepDbAttachedAtEndOfRecovery = 0x1000,
    }

    /// <summary>Options for <see cref="IJetInstance.Close(TermGrbit)"/>.</summary>
    [Flags]
    public enum TermGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,
        /// <summary>Requests that the instance be shut down cleanly. Any optional cleanup
        /// work that would ordinarily be done in the background at run time is completed
        /// immediately.</summary>
        Complete = 1,
        /// <summary>Requests that the instance be shut down as quickly as possible. Any
        /// optional work that would ordinarily be done in the background at run time is
        /// abandoned.</summary>
        Abrupt = 2,

        // --------- //
        // WINDOWS 7 //
        // --------- //
        /// <summary>Terminate without flushing the database cache.</summary>
        Dirty = 0x8,
    }

    /// <summary>Options for <see cref="JetSession.CreateDatabase"/>.</summary>
    [Flags]
    public enum CreateDatabaseGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>By default, if JetCreateDatabase is called and the database already exists,
        /// the Api call will fail and the original database will not be overwritten. OverwriteExisting
        /// changes this behavior, and the old database will be overwritten with a new one.</summary>
        OverwriteExisting = 0x200,

        /// <summary>Turns off logging. Setting this bit loses the ability to replay log files
        /// and recover the database to a consistent usable state after a crash.</summary>
        RecoveryOff = 0x8,

        // --------- //
        // WINDOWS 7 //
        // --------- //
        /// <summary>The database engine will initiate automatic background database maintenance
        /// upon database creation.</summary>
        EnableCreateDbBackgroundMaintenance = 0x800,
        
        /// <summary>Compress data in the column, if possible.</summary>
        ColumnCompressed = 0x80000,
    }

    /// <summary>Options for <see cref="JetDatabase.Detach"/>.</summary>
    [Flags]
    public enum DetachDatabaseGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>If <see cref="ForceDetach"/> is used, <see cref="EsentForceDetachNotAllowedException"/>
        /// will be returned.</summary>
        [Obsolete("ForceDetach is no longer used.")]
        ForceDetach = 1,

        /// <summary><see cref="ForceClose"/> is no longer used.</summary>
        [Obsolete("ForceClose is no longer used.")]
        ForceClose = 0x2,

        /// <summary>If <see cref="ForceCloseAndDetach"/> is used,
        /// <see cref="EsentForceDetachNotAllowedException"/> will be returned.</summary>
        [Obsolete("ForceCloseAndDetach is no longer used.")]
        ForceCloseAndDetach = (0x2 | 0x1 /*ForceDetach*/),
    }

    /// <summary>Options for <see cref="JetSession.AttachDatabase"/>.</summary>
    [Flags]
    public enum AttachDatabaseGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>Prevents modifications to the database.</summary>
        ReadOnly = 0x1,

        /// <summary>If JET_paramEnableIndexChecking has been set, all indexes over Unicode data
        /// will be deleted.</summary>
        DeleteCorruptIndexes = 0x10,

        // ------------ //
        // WINDOWS 2003 //
        // ------------ //
        /// <summary>Delete all indexes with unicode columns.</summary>
        DeleteUnicodeIndexes = 0x400,

        // --------- //
        // WINDOWS 7 //
        // --------- //
        /// <summary>The database engine will initiate automatic background database maintenance
        /// upon database attachment.</summary>
        EnableAttachDbBackgroundMaintenance = 0x800,

        // --------- //
        // WINDOWS 8 //
        // --------- //
        /// <summary>Purge database pages on attach.</summary>
        PurgeCacheOnAttach = 0x1000,
    }

    /// <summary>Options for <see cref="JetSession.OpenDatabase"/>.</summary>
    [Flags]
    public enum OpenDatabaseGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>Prevents modifications to the database.</summary>
        ReadOnly = 0x1,

        /// <summary>Allows only a single session to attach a database. Normally, several sessions can
        /// open a database.</summary>
        Exclusive = 0x2,
    }

    /// <summary>Options for <see cref="IJetDatabase.Close"/>.</summary>
    [Flags]
    public enum CloseDatabaseGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,
    }

    /// <summary>Options for <see cref="IJetDatabase.CompactDatabase"/>.</summary>
    [Flags]
    public enum CompactGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>Causes JetCompact to dump statistics on the source database to a file
        /// named DFRGINFO.TXT. Statistics include the name of each table in source database,
        /// number of rows in each table, total size in bytes of all rows in each table, total
        /// size in bytes of all columns of type <see cref="JET_coltyp.LongText"/> or
        /// <see cref="JET_coltyp.LongBinary"/> that were large enough to be stored separate
        /// from the record, number of clustered index leaf pages, and the number of long value
        /// leaf pages. In addition, summary statistics including the size of the source database,
        /// destination database, time required for database compaction, temporary database
        /// space are all dumped as well.</summary>
        Stats = 0x20,

        /// <summary>Used when the source database is known to be corrupt. It enables a whole set
        /// of new behaviors intended to salvage as much data as possible from the source database.
        /// JetCompact with this option set may return <see cref="JET_err.Success"/> but not copy
        /// all of the data created in the source database. Data that was in damaged portions of
        /// the source database will be skipped.</summary>
        [Obsolete("Use esentutl repair functionality instead.")]
        Repair = 0x40,        
    }

    /// <summary>Options for <see cref="IJetSnapshot.Start"/>.</summary>
    [Flags]
    public enum SnapshotFreezeGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,
    }

    /// <summary>Options for <see cref="JetEnvironment.PrepareSnapshot"/>.</summary>
    [Flags]
    public enum SnapshotPrepareGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,
    
        /// <summary>Only logfiles will be taken.</summary>
        IncrementalSnapshot = 0x1,

        /// <summary>A copy snapshot (normal or incremental) with no log truncation.</summary>
        CopySnapshot = 0x2,

        // ----- //
        // VISTA //
        // ----- //
        /// <summary>The snapshot session continues after JetOSSnapshotThaw and will require a
        /// JetOSSnapshotEnd function call.</summary>
        ContinueAfterThaw = 0x4,

        // --------- //
        // WINDOWS 7 //
        // --------- //
        /// <summary>No instances will be prepared by default. Instances must be added
        /// explicitly.</summary>
        ExplicitPrepare = 0x8,

    }

    /// <summary>Options for <see cref="IJetSnapshot.Thaw"/>.</summary>
    [Flags]
    public enum SnapshotThawGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,
    }

    /// <summary>Options for <see cref="JetInstance.Backup"/>.</summary>
    [Flags]
    public enum BackupGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,
        
        /// <summary>Creates an incremental backup as opposed to a full backup. This means that
        /// only the log files created since the last full or incremental backup will be backed
        /// up.</summary>
        Incremental = 0x1,

        /// <summary>Creates a full backup of the database. This allows the preservation of an existing
        /// backup in the same directory if the new backup fails.</summary>
        Atomic = 0x4,
    }

    /// <summary>Options for <see cref="IJetInstance.PrepareBackup"/>.</summary>
    [Flags]
    public enum BeginExternalBackupGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>Creates an incremental backup as opposed to a full backup. This means that
        /// only the log files since the last full or incremental backup will be backed up.</summary>
        Incremental = 0x1,
    }

    /// <summary>Options for <see cref="IJetInstance.CompleteBackup"/>.</summary>
    [Flags]
    public enum EndExternalBackupGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>The client application finished the backup completely, and is ending normally.</summary>
        Normal = 0x1,

        /// <summary>The client application is aborting the backup.</summary>
        Abort = 0x2,

        /// <summary>The engine can mark the database headers as appropriate (for example, a full
        /// backup completed), even though the call to truncate was not completed.</summary>
        TruncateDone = 0x100,
    }

    /// <summary>Options for <see cref="IJetTransaction.Commit()"/>, for
    /// <see cref="IJetSession.FlushTransactions"/> and for
    /// <see cref="IJetSession.RotateTransactionLogs"/></summary>
    [Flags]
    internal enum CommitTransactionGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>The transaction is committed normally but this Api does not wait for the
        /// transaction to be flushed to the transaction log file before returning to the caller.
        /// This drastically reduces the duration of a commit operation at the cost of durability.
        /// Any transaction that is not flushed to the log before a crash will be automatically
        /// aborted during crash recovery during the next call to JetInit. If WaitLastLevel0Commit
        /// or WaitAllLevel0Commit are specified, this option is ignored.</summary>
        LazyFlush = 0x1,

        /// <summary>If the session has previously committed any transactions and they have not
        /// yet been flushed to the transaction log file, they should be flushed immediately.
        /// This Api will wait until the transactions have been flushed before returning to the
        /// caller. This is useful if the application has previously committed several transactions
        /// using JET_bitCommitLazyFlush and now wants to flush all of them to disk.</summary>
        /// <remarks>This option may be used even if the session is not currently in a transaction.
        /// This option cannot be used in combination with any other option.</remarks>
        WaitLastLevel0Commit = 0x2,

        // ------------ //
        // WINDOWS 2003 //
        // ------------ //
        /// <summary>All transactions previously committed by any session that have not yet
        /// been flushed to the transaction log file will be flushed immediately. This API
        /// will wait until the transactions have been flushed before returning to the caller.
        /// This option may be used even if the session is not currently in a transaction.
        /// This option cannot be used in combination with any other option.</summary>
        WaitAllLevel0Commit = 0x8,

        // --------- //
        // WINDOWS 7 //
        // --------- //
        /// <summary>Force a new logfile to be created. This option may be used even if the
        /// session is not currently in a transaction. This option cannot be used in combination
        /// with any other option.</summary>
        ForceNewLog = 0x10,
    }

    /// <summary>Options for JetRollbackTransaction.</summary>
    [Flags]
    public enum RollbackTransactionGrbit
    {
        /// <summary>Default options;/// </summary>
        None = 0,

        /// <summary>This option requests that all changes made to the state of the database
        /// during all save points be undone. As a result, the session will exit the
        /// transaction.</summary>
        RollbackAll = 0x1,
    }

    /// <summary>Options for JetEndSession.</summary>
    [Flags]
    public enum EndSessionGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,
    }

    /// <summary>Options for IJetDatabase.OpenTable.</summary>
    [Flags]
    public enum OpenTableGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>This table cannot be opened for write access by another session.</summary>
        DenyWrite = 0x1,

        /// <summary>This table cannot be opened for read access by another session.</summary>
        DenyRead = 0x2,

        /// <summary>Request read-only access to the table.</summary>
        ReadOnly = 0x4,

        /// <summary>Request write access to the table.</summary>
        Updatable = 0x8,

        /// <summary>Allow DDL modifications to a table flagged as FixedDDL. This option must be
        /// used with DenyRead.</summary>
        PermitDDL = 0x10,

        /// <summary>Do not cache pages for this table.</summary>
        NoCache = 0x20,

        /// <summary>Provides a hint that the table is probably not in the buffer cache, and that
        /// pre-reading may be beneficial to performance.</summary>
        Preread = 0x40,

        /// <summary>Assume a sequential access pattern and prefetch database pages.</summary>
        Sequential = 0x8000,

        /// <summary>Table belongs to stats class 1.</summary>
        TableClass1 = 0x00010000,

        /// <summary>Table belongs to stats class 2.</summary>
        TableClass2 = 0x00020000,

        /// <summary>Table belongs to stats class 3.</summary>
        TableClass3 = 0x00030000,

        /// <summary>Table belongs to stats class 4.</summary>
        TableClass4 = 0x00040000,

        /// <summary>Table belongs to stats class 5.</summary>
        TableClass5 = 0x00050000,

        /// <summary>Table belongs to stats class 6.</summary>
        TableClass6 = 0x00060000,

        /// <summary>Table belongs to stats class 7.</summary>
        TableClass7 = 0x00070000,

        /// <summary>Table belongs to stats class 8.</summary>
        TableClass8 = 0x00080000,

        /// <summary>Table belongs to stats class 9.</summary>
        TableClass9 = 0x00090000,

        /// <summary>Table belongs to stats class 10.</summary>
        TableClass10 = 0x000A0000,

        /// <summary>Table belongs to stats class 11.</summary>
        TableClass11 = 0x000B0000,

        /// <summary>Table belongs to stats class 12.</summary>
        TableClass12 = 0x000C0000,

        /// <summary>Table belongs to stats class 13.</summary>
        TableClass13 = 0x000D0000,

        /// <summary>Table belongs to stats class 14.</summary>
        TableClass14 = 0x000E0000,

        /// <summary>Table belongs to stats class 15.</summary>
        TableClass15 = 0x000F0000,
    }

    /// <summary>Options for <see cref="LegacyApi.JetDupCursor"/>.</summary>
    [Flags]
    public enum DupCursorGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,
    }

    /// <summary>
    /// Options for <see cref="LegacyApi.JetSetLS"/> and <see cref="LegacyApi.JetGetLS"/>.
    /// </summary>
    [Flags]
    public enum LsGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>The context handle for the chosen object should be reset to JET_LSNil.</summary>
        Reset = 0x1,

        /// <summary>Specifies the context handle should be associated with the given cursor.</summary>
        Cursor = 0x2,

        /// <summary>Specifies that the context handle should be associated with the table associated
        /// with the given cursor. It is illegal to use this option with <see cref="Cursor"/>.</summary>
        Table = 0x4,
    }

    /// <summary>Options for the <see cref="EsentLib.InternalApi.JetSetColumn(JET_SESID, JET_TABLEID, JET_COLUMNID, byte[], int, int, SetColumnGrbit, JET_SETINFO)"/>
    /// and its associated overloads.</summary>
    [Flags]
    public enum SetColumnGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>This option is used to append data to a column of type JET_coltypLongText
        /// or JET_coltypLongBinary. The same behavior can be achieved by determining the size
        /// of the existing long value and specifying ibLongValue in psetinfo. However, its
        /// simpler to use this grbit since knowing the size of the existing column value is
        /// not necessary.</summary>
        AppendLV = 0x1,

        /// <summary>This option is used replace the existing long value with the newly provided
        /// data. When this option is used, it is as though the existing long value has been
        /// set to 0 (zero) length prior to setting the new data.</summary>
        OverwriteLV = 0x4,

        /// <summary>This option is only applicable for tagged, sparse or multi-valued columns.
        /// It causes the column to return the default column value on subsequent retrieve
        /// column operations. All existing column values are removed.</summary>
        RevertToDefaultValue = 0x200,

        /// <summary>
        /// This option is used to force a long value, columns of type JET_coltyp.LongText
        /// or JET_coltyp.LongBinary, to be stored separately from the remainder of record
        /// data. This occurs normally when the size of the long value prevents it from being 
        /// stored with remaining record data. However, this option can be used to force the
        /// long value to be stored separately. Note that long values four bytes in size
        /// of smaller cannot be forced to be separate. In such cases, the option is ignored.
        /// </summary>
        SeparateLV = 0x40,

        /// <summary>This option is used to interpret the input buffer as a integer number
        /// of byte to set as the length of the long value described by the given columnid
        /// and if provided, the sequence number in psetinfo->itagSequence. If the size given
        /// is larger than the existing column value, the column will be extended with 0s. If
        /// the size is smaller than the existing column value then the value will be truncated.
        /// </summary>
        SizeLV = 0x8,

        /// <summary>
        /// This option is used to enforce that all values in a multi-valued column are
        /// distinct. This option compares the source column data, without any
        /// transformations, to other existing column values and an error is returned
        /// if a duplicate is found. If this option is given, then AppendLV, OverwriteLV
        /// and SizeLV cannot also be given.
        /// </summary>
        UniqueMultiValues = 0x80,

        /// <summary>
        /// This option is used to enforce that all values in a multi-valued column are
        /// distinct. This option compares the key normalized transformation of column
        /// data, to other similarly transformed existing column values and an error is
        /// returned if a duplicate is found. If this option is given, then AppendLV, 
        /// OverwriteLV and SizeLV cannot also be given.
        /// </summary>
        UniqueNormalizedMultiValues = 0x100,

        /// <summary>
        /// This option is used to set a value to zero length. Normally, a column value
        /// is set to NULL by passing a cbMax of 0 (zero). However, for some types, like
        /// JET_coltyp.Text, a column value can be 0 (zero) length instead of NULL, and
        /// this option is used to differentiate between NULL and 0 (zero) length.
        /// </summary>
        ZeroLength = 0x20,

        /// <summary>
        /// Try to store long-value columns in the record, even if they exceed the default
        /// separation size.
        /// </summary>
        IntrinsicLV = 0x400,

        // --------- //
        // WINDOWS 7 //
        // --------- //
        /// <summary>Try to compress the data when storing it.</summary>
        Compressed = 0x20000,

        /// <summary>Don't compress the data when storing it.</summary>
        Uncompressed = 0x10000,

    }

    /// <summary>Options for JetRetrieveColumn.</summary>
    [Flags]
    public enum RetrieveColumnGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>This flag causes retrieve column to retrieve the modified value instead
        /// of the original value. If the value has not been modified, then the original
        /// value is retrieved. In this way, a value that has not yet been inserted or
        /// updated may be retrieved during the operation of inserting or updating a record.</summary>
        RetrieveCopy = 0x1,

        /// <summary>This option is used to retrieve column values from the index, if
        /// possible, without accessing the record. In this way, unnecessary loading of
        /// records can be avoided when needed data is available from index entries
        /// themselves.</summary>
        RetrieveFromIndex = 0x2,
        
        /// <summary>This option is used to retrieve column values from the index bookmark,
        /// and may differ from the index value when a column appears both in the primary
        /// index and the current index. This option should not be specified if the current
        /// index is the clustered, or primary, index. This bit cannot be set if
        /// RetrieveFromIndex is also set. </summary>
        RetrieveFromPrimaryBookmark = 0x4,

        /// <summary>This option is used to retrieve the sequence number of a multi-valued
        /// column value in JET_RETINFO.itagSequence. Retrieving the sequence number can be
        /// a costly operation and should only be done if necessary.</summary>
        RetrieveTag = 0x8,

        /// <summary>This option is used to retrieve multi-valued column NULL values. If
        /// this option is not specified, multi-valued column NULL values will automatically
        /// be skipped. </summary>
        RetrieveNull = 0x10,

        /// <summary>This option affects only multi-valued columns and causes a NULL value
        /// to be returned when the requested sequence number is 1 and there are no set
        /// values for the column in the record. </summary>
        RetrieveIgnoreDefault = 0x20,
    }

    /// <summary>Options for <see cref="LegacyApi.JetEnumerateColumns(JET_SESID, JET_TABLEID, EnumerateColumnsGrbit, out IEnumerable&lt;EnumeratedColumn&gt;)"/>
    /// and its associated overloads.</summary>
    [Flags]
    public enum EnumerateColumnsGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>When enumerating column values, all columns for which we are retrieving
        /// all values and that have only one non-NULL column value may be returned in a
        /// compressed format. The status for such columns will be set to
        /// <see cref="JET_wrn.ColumnSingleValue"/> and the size of the column value and the
        /// memory containing the column value will be returned directly in the
        /// <see cref="JET_ENUMCOLUMN"/> structure. It is not guaranteed that all eligible
        /// columns are compressed in this manner. See <see cref="JET_ENUMCOLUMN"/> for more
        /// information.</summary>
        EnumerateCompressOutput = 0x00080000,

        /// <summary>This option indicates that the modified column values of the record
        /// should be enumerated rather than the original column values. If a column value
        /// has not been modified, the original column value is enumerated. In this way, a
        /// column value that has not yet been inserted or updated may be enumerated when
        /// inserting or updating a record.</summary>
        /// <remarks>This option is identical to <see cref="RetrieveColumnGrbit.RetrieveCopy"/>.</remarks>
        EnumerateCopy = 0x1,

        /// <summary>If a given column is not present in the record then no column value will
        /// be returned. Ordinarily, the default value for the column, if any, would be
        /// returned in this case. It is guaranteed that if the column is set to a value
        /// different than the default value then that different value will be returned
        /// (that is, if a column with a default value is explicitly set to NULL then a NULL
        /// will be returned as the value for that column). Even if this option is requested,
        /// it is still possible to see a column value that happens to be equal to the default
        /// value. No effort is made to remove column values that match their default values.
        /// It is important to remember that this option affects the output of
        /// <see cref="LegacyApi.JetEnumerateColumns(JET_SESID, JET_TABLEID, EnumerateColumnsGrbit, out IEnumerable&lt;EnumeratedColumn&gt;)"/>
        /// and its associated overloads when used with 
        /// <see cref="EnumerateColumnsGrbit.EnumeratePresenceOnly"/> or
        /// <see cref="EnumerateColumnsGrbit.EnumerateTaggedOnly"/>.</summary>
        EnumerateIgnoreDefault = 0x20,

        /// <summary>If a non-NULL value exists for the requested column or column value then
        /// the associated data is not returned. Instead, the associated status for that
        /// column or column value will be set to <see cref="JET_wrn.ColumnPresent"/>. If the
        /// column or column value is NULL then <see cref="JET_wrn.ColumnNull"/> will be
        /// returned as usual.</summary>
        EnumeratePresenceOnly = 0x00020000,

        /// <summary>When enumerating all column values in the record (for example,that is when
        /// numColumnids is zero), only tagged column values will be returned. This option is
        /// not allowed when enumerating a specific array of column IDs.</summary>
        EnumerateTaggedOnly = 0x00040000,

        /// <summary>If a given column is not present in the record and it has a user defined
        /// default value then no column value will be returned. This option will prevent the
        /// callback that computes the user defined default value for the column from being
        /// called when enumerating the values for that column.</summary>
        /// <remarks>This option is only available for Windows Server 2003 SP1 and later
        /// operating systems.</remarks>
        EnumerateIgnoreUserDefinedDefault = 0x00100000,

        // --------- //
        // WINDOWS 7 //
        // --------- //
        /// <summary>When enumerating column values only retrieve data that is present in the
        /// record. This means that BLOB columns will not always be retrieved.</summary>
        EnumerateInRecordOnly = 0x00200000,
    }

    /// <summary>Options for <see cref="LegacyApi.JetGetRecordSize"/>.</summary>
    [Flags]
    public enum GetRecordSizeGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>Retrieve the size of the record that is in the copy buffer prepared or
        /// update. Otherwise, the tableid must be positioned on a record, and that record
        /// will be used.</summary>
        InCopyBuffer = 0x1,

        /// <summary>The JET_RECSIZE is not zeroed before filling the contents, effectively
        /// acting as an accumulation of the statistics for multiple records visited or updated.
        /// </summary>
        RunningTotal = 0x2,

        /// <summary>Ignore non-intrinsic Long Values. Only the local record on the page will be used.
        /// </summary>
        Local = 0x4,
    }

    /// <summary>Options for <see cref="LegacyApi.JetGetSecondaryIndexBookmark"/>.</summary>
    [Flags]
    public enum GetSecondaryIndexBookmarkGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,
    }

    /// <summary>Options for <see cref="LegacyApi.JetGotoSecondaryIndexBookmark"/>.</summary>
    [Flags]
    public enum GotoSecondaryIndexBookmarkGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>In the event that the index entry can no longer be found, the cursor will
        /// be left positioned where that index entry was previously found. The operation will
        /// still fail with JET_errRecordDeleted; however, it will be possible to move to the
        /// next or previous index entry relative to the index entry that is now missing.</summary>
        BookmarkPermitVirtualCurrency = 0x1,
    }

    /// <summary>Options for JetMove.</summary>
    [Flags]
    public enum MoveGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>Moves the cursor forward or backward by the number of index entries required
        /// to skip the requested number of index key values encountered in the index. This has
        /// the effect of collapsing index entries with duplicate key values into a single index
        /// entry.</summary>
        MoveKeyNE = 0x1,
    }

    /// <summary>Options for JetMakeKey.</summary>
    [Flags]
    public enum MakeKeyGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>A new search key should be constructed. Any previously existing search
        /// key is discarded.</summary>
        NewKey = 0x1,

        /// <summary>When this option is specified, all other options are ignored, any previously
        /// existing search key is discarded, and the contents of the input buffer are loaded as
        /// the new search key.</summary>
        NormalizedKey = 0x8,

        /// <summary>If the size of the input buffer is zero and the current key column is a
        /// variable length column, this option indicates that the input buffer contains a
        /// zero length value. Otherwise, an input buffer size of zero would indicate a NULL
        /// value.</summary>
        KeyDataZeroLength = 0x10,

        /// <summary>This option indicates that the search key should be constructed such that
        /// any key columns that come after the current key column should be considered to be
        /// wildcards.</summary>
        StrLimit = 0x2,

        /// <summary>This option indicates that the search key should be constructed such that
        /// the current key column is considered to be a prefix wildcard and that any key columns
        /// that come after the current key column should be considered to be wildcards.</summary>
        SubStrLimit = 0x4,

        /// <summary>The search key should be constructed such that any key columns that come
        /// after the current key column should be considered to be wildcards.</summary>
        FullColumnStartLimit = 0x100,

        /// <summary>
        /// The search key should be constructed in such a way that any key
        /// columns that come after the current key column are considered to
        /// be wildcards.
        /// </summary>
        FullColumnEndLimit = 0x200,

        /// <summary>
        /// The search key should be constructed such that the current key
        /// column is considered to be a prefix wildcard and that any key
        /// columns that come after the current key column should be considered
        /// to be wildcards. 
        /// </summary>
        PartialColumnStartLimit = 0x400,

        /// <summary>
        /// The search key should be constructed such that the current key
        /// column is considered to be a prefix wildcard and that any key
        /// columns that come after the current key column should be considered
        /// to be wildcards.
        /// </summary>
        PartialColumnEndLimit = 0x800,
    }

    /// <summary>
    /// Options for JetRetrieveKey.
    /// </summary>
    [Flags]
    public enum RetrieveKeyGrbit
    {
        /// <summary>
        /// Default options.
        /// </summary>
        None = 0,

        /// <summary>
        /// Retrieve the currently constructed key.
        /// </summary>
        RetrieveCopy = 0x1,
    }

    /// <summary>Options for <see cref="IJetCursor.Seek"/>.</summary>
    [Flags]
    public enum SeekGrbit
    {
        /// <summary>The cursor will be positioned at the index entry closest to the start of the index
        /// that exactly matches the search key.</summary>
        SeekEQ = 0x1,

        /// <summary>The cursor will be positioned at the index entry closest to the end of the index
        /// that is less than an index entry that would exactly match the search criteria.</summary>
        SeekLT = 0x2,

        /// <summary>The cursor will be positioned at the index entry closest to the end of the index
        /// that is less than or equal to an index entry that would exactly match the search criteria.</summary>
        SeekLE = 0x4,

        /// <summary>The cursor will be positioned at the index entry closest to the start of the index
        /// that is greater than or equal to an index entry that would exactly match the search criteria.</summary>
        SeekGE = 0x8,

        /// <summary>The cursor will be positioned at the index entry closest to the start of the index
        /// that is greater than an index entry that would exactly match the search criteria.</summary>
        SeekGT = 0x10,

        /// <summary>An index range will automatically be setup for all keys that exactly match the
        /// search key. </summary>
        SetIndexRange = 0x20,
    }

    /// <summary>Options for <see cref="IJetCursor.SetIndexRange"/>.</summary>
    [Flags]
    public enum SetIndexRangeGrbit
    {
        /// <summary>Default options.</summary>
        None = 0x0,

        /// <summary>This option indicates that the limit of the index range is inclusive.</summary>
        RangeInclusive = 0x1,

        /// <summary>The search key in the cursor represents the search criteria for the index entry
        /// closest to the end of the index that will match the index range.</summary>
        RangeUpperLimit = 0x2,

        /// <summary>The index range should be removed as soon as it has been established. This is
        /// useful for testing for the existence of index entries that match the search criteria.</summary>
        RangeInstantDuration = 0x4,

        /// <summary>Cancel and existing index range.</summary>
        RangeRemove = 0x8,
    }

    /// <summary>Options for the <see cref="JET_INDEXRANGE"/> object.</summary>
    [Flags]
    public enum IndexRangeGrbit
    {
        /// <summary>Records in the cursors indexrange should be included in the output.</summary>
        RecordInIndex = 0x1,
    }

    /// <summary>Options for <see cref="LegacyApi.JetIntersectIndexes"/>.</summary>
    [Flags]
    public enum IntersectIndexesGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,
    }

    /// <summary>Options for <see cref="IJetCursor.SetCurrentIndex(JET_INDEXID, SetCurrentIndexGrbit, int)"/>
    /// and <see cref="IJetCursor.SetCurrentIndex(string, SetCurrentIndexGrbit, int)"/>.</summary>
    [Flags]
    public enum SetCurrentIndexGrbit
    {
        /// <summary>Default options. This is the same as <see cref="MoveFirst"/>.</summary>
        None = 0,

        /// <summary>Indicates that the cursor should be positioned on the first entry of
        /// the specified index. If the current index is being selected then this option is
        /// ignored.</summary>
        MoveFirst = 0,

        /// <summary>Indicates that the cursor should be positioned on the index entry of the
        /// new index that corresponds to the record associated with the index entry at the
        /// current position of the cursor on the old index.</summary>
        NoMove = 0x2,
    }

    /// <summary>Options for <see cref="LegacyApi.JetSetTableSequential"/>.</summary>
    [Flags]
    public enum SetTableSequentialGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        // --------- //
        // WINDOWS 7 //
        // --------- //
        /// <summary>Hint that the sequential traversal will be in the forward direction.</summary>
        Forward = 0x1,

        /// <summary>Hint that the sequential traversal will be in the backward direction.</summary>
        Backward = 0x2,
    }

    /// <summary>Options for <see cref="LegacyApi.JetResetTableSequential"/>.</summary>
    [Flags]
    public enum ResetTableSequentialGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,
    }

    /// <summary>Options for <see cref="LegacyApi.JetEscrowUpdate"/>.</summary>
    [Flags]
    public enum EscrowUpdateGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>Even if the session performing the escrow update has its transaction
        /// rollback this update will not be undone. As the log records may not be flushed
        /// to disk, recent escrow updates done with this flag may be lost if there is a
        /// crash.</summary>
        NoRollback = 0x1,
    }

    /// <summary>Options for the <see cref="JET_COLUMNDEF"/> structure.</summary>
    [Flags]
    public enum ColumndefGrbit
    {
        /// <summary>Default options.</summary>
        None = 0x0,

        /// <summary>The column will be fixed. It will always use the same amount of space in a row,
        /// regardless of how much data is being stored in the column. ColumnFixed cannot be used
        /// with ColumnTagged. This bit cannot be used with long values (that is JET_coltyp.LongText
        /// and JET_coltyp.LongBinary).</summary>
        ColumnFixed = 0x1,

        /// <summary>The column will be tagged. Tagged columns do not take up any space in the database
        ///  if they do not contain data. This bit cannot be used with ColumnFixed.</summary>
        ColumnTagged = 0x2,

        /// <summary>The column must never be set to a NULL value. On Windows XP this can only
        /// be applied to fixed columns (bit, byte, integer, etc).</summary>
        ColumnNotNULL = 0x4,

        /// <summary>The column is a version column that specifies the version of the row. The value
        /// of this column starts at zero and will be automatically incremented for each update on
        /// the row. This option can only be applied to <see cref="JET_coltyp.Long"/> columns. This
        /// option cannot be used with <see cref="ColumnAutoincrement"/>, <see cref="ColumnEscrowUpdate"/>,
        /// or <see cref="ColumnTagged"/>.</summary>
        ColumnVersion = 0x8,

        /// <summary>The column will automatically be incremented. The number is an increasing
        /// number, and is guaranteed to be unique within a table. The numbers, however, might
        /// not be continuous. For example, if five rows are inserted into a table, the
        /// "autoincrement" column could contain the values { 1, 2, 6, 7, 8 }. This bit can
        /// only be used on columns of type<see cref="JET_coltyp.Long"/> or <see cref="JET_coltyp.Currency"/>.
        /// </summary>
        ColumnAutoincrement = 0x10,

        /// <summary>
        /// The column can be updated. This is NOT a valid grbit to set on input to any API. It is
        /// returned as part of the <see cref="JET_COLUMNDEF"/> structure's grbit member, as an
        /// output from Api.JetGetColumnInfo.
        /// </summary>
        ColumnUpdatable = 0x20,

        /// <summary>
        /// The column can be multi-valued.
        /// A multi-valued column can have zero, one, or more values
        /// associated with it. The various values in a multi-valued column are identified by a number
        /// called the itagSequence member, which belongs to various structures, including:
        /// <see cref="JET_RETINFO"/>, <see cref="JET_SETINFO"/>, <see cref="JET_SETCOLUMN"/>, <see cref="JET_RETRIEVECOLUMN"/>, and <see cref="JET_ENUMCOLUMNVALUE"/>.
        /// Multi-valued columns must be tagged columns; that is, they cannot be fixed-length or
        /// variable-length columns.
        /// </summary>
        ColumnMultiValued = 0x400,

        /// <summary>
        ///  Specifies that a column is an escrow update column. An escrow update column can be
        ///  updated concurrently by different sessions with JetEscrowUpdate and will maintain
        ///  transactional consistency. An escrow update column must also meet the following conditions:
        ///  An escrow update column can be created only when the table is empty. 
        ///  An escrow update column must be of type JET_coltypLong. 
        ///  An escrow update column must have a default value.
        ///  ColumnEscrowUpdate cannot be used in conjunction with <see cref="ColumnTagged"/>,
        ///  <see cref="ColumnVersion"/>, or <see cref="ColumnAutoincrement"/>. 
        /// </summary>
        ColumnEscrowUpdate = 0x800,

        /// <summary>
        /// The column will be created in an without version information. This means that other
        /// transactions that attempt to add a column with the same name will fail. This bit
        /// is only useful with IJetTable.AddColumn. It cannot be used within a transaction.
        /// </summary>
        ColumnUnversioned = 0x1000,

        /// <summary>
        /// In doing an outer join, the retrieve column operation might not have a match
        /// from the inner table.
        /// </summary>
        ColumnMaybeNull = 0x2000,

        /// <summary>
        /// When the escrow-update column reaches a value of zero, the callback function will be invoked.
        /// </summary>
        ColumnFinalize = 0x4000,

        /// <summary>
        /// The default value for a column will be provided by a callback function. A column that
        /// has a user-defined default must be a tagged column. Specifying <see cref="ColumnUserDefinedDefault"/>
        /// means that pvDefault must point to a JET_USERDEFINEDDEFAULT structure, and cbDefault must be
        /// set to sizeof( JET_USERDEFINEDDEFAULT ).
        /// </summary>
        ColumnUserDefinedDefault = 0x8000,

        /// <summary>
        /// The column will be a key column for the temporary table. The order
        /// of the column definitions with this option specified in the input
        /// array will determine the precedence of each key column for the
        /// temporary table. The first column definition in the array that
        /// has this option set will be the most significant key column and
        /// so on. If more key columns are requested than can be supported
        /// by the database engine then this option is ignored for the
        /// unsupportable key columns.
        /// </summary>
        TTKey = 0x40,

        /// <summary>
        /// The sort order of the key column for the temporary table should
        /// be descending rather than ascending. If this option is specified
        ///  without <see cref="TTKey"/> then this option is ignored.
        /// </summary>
        TTDescending = 0x80,

        // ------------ //
        // WINDOWS 2003 //
        // ------------ //
        /// <summary>This is a finalizable column (delete record if escrow value equals 0).</summary>
        ColumnDeleteOnZero = 0x20000,
    }

    /// <summary>Options for the <see cref="JET_TABLECREATE"/> parameter used by
    /// Api.JetCreateTableColumnIndex3.</summary>
    [Flags]
    public enum CreateTableColumnIndexGrbit
    {
        /// <summary>Default options.</summary>
        None = 0x0,

        /// <summary>The DDL is fixed.</summary>
        FixedDDL = 0x1,

        /// <summary>The DDL is inheritable. Implies FixedDDL.</summary>
        TemplateTable = 0x2,

        /// <summary>Used in conjunction with TemplateTable.</summary>
        NoFixedVarColumnsInDerivedTables = 0x4,

        // ---------- //
        // WINDOWS 10 //
        // ---------- //
        /// <summary>Do not write to the input structures, so that the structures can be stored
        /// in readonly memory. Additionally, do not return any auto-opened tableid.</summary>
        /// seealso cref="Api.JetCreateTableColumnIndex3"
        TableCreateImmutableStructure = 0x8,
    }

    /// <summary>Options for <see cref="IJetTable.CreateIndex(IJetSession, string, CreateIndexGrbit, string, int, int)"/>
    /// and <see cref="JET_INDEXCREATE"/>.</summary>
    [Flags]
    public enum CreateIndexGrbit
    {        
        /// <summary>Default options.</summary>
        None = 0x0,

        /// <summary>Duplicate index entries (keys) are disallowed. This is enforced when JetUpdate is called,
        /// not when JetSetColumn is called.</summary>
        IndexUnique  = 0x1,

        /// <summary>The index is a primary (clustered) index. Every table must have exactly one primary index.
        /// If no primary index is explicitly defined over a table, then the database engine will create its
        /// own primary index.</summary>
        IndexPrimary = 0x2,

        /// <summary>None of the columns over which the index is created may contain a NULL value.</summary>
        IndexDisallowNull = 0x4,

        /// <summary>Do not add an index entry for a row if all of the columns being indexed are NULL.</summary>
        IndexIgnoreNull = 0x8,

        /// <summary>Do not add an index entry for a row if any of the columns being indexed are NULL.</summary>
        IndexIgnoreAnyNull = 0x20,

        /// <summary>Do not add an index entry for a row if the first column being indexed is NULL.</summary>
        IndexIgnoreFirstNull = 0x40,

        /// <summary>Specifies that the index operations will be logged lazily. JET_bitIndexLazyFlush does not
        /// affect the laziness of data updates. If the indexing operations is interrupted by process termination,
        /// Soft Recovery will still be able to able to get the database to a consistent state, but the index may
        /// not be present.</summary>
        IndexLazyFlush = 0x80,

        /// <summary>Do not attempt to build the index, because all entries would evaluate to NULL. grbit MUST
        /// also specify JET_bitIgnoreAnyNull when JET_bitIndexEmpty is passed. This is a performance enhancement.
        /// For example if a new column is added to a table, then an index is created over this newly added column,
        /// all of the records in the table would be scanned even though they would never get added to the index
        /// anyway. Specifying JET_bitIndexEmpty skips the scanning of the table, which could potentially take a
        /// long time.</summary>
        IndexEmpty = 0x100,

        /// <summary>Causes index creation to be visible to other transactions. Normally a session in a
        /// transaction will not be able to see an index creation operation in another session. This
        /// flag can be useful if another transaction is likely to create the same index, so that the
        /// second index-create will simply fail instead of potentially causing many unnecessary database
        /// operations. The second transaction may not be able to use the index immediately. The index
        /// creation operation needs to complete before it is usable. The session must not currently be in
        /// a transaction to create an index without version information.</summary>
        IndexUnversioned = 0x200,

        /// <summary>Specifying this flag causes NULL values to be sorted after data for all columns in
        /// the index.</summary>
        IndexSortNullsHigh = 0x400,

        // ----- //
        // VISTA //
        // ----- //
        /// <summary>Specifying this flag for an index that has more than one key column that
        /// is a multi-valued column will result in an index entry being created for each result
        /// of a cross product of all the values in those key columns. Otherwise, the index
        /// would only have one entry for each multi-value in the most significant key column
        /// that is a multi-valued column and each of those index entries would use the first
        /// multi-value from any other key columns that are multi-valued columns.
        /// <para>For example, if you specified this flag for an index over column A that has
        /// the values "red" and "blue" and over column B that has the values "1" and "2" then
        /// the following index entries would b created: "red", "1"; "red", "2"; "blue", "1";
        /// "blue", "2". Otherwise, the following index entries would be created: "red", "1";
        /// "blue", "1".</para></summary>
        IndexCrossProduct = 0x4000,

        /// <summary>Specifying this flag will cause any update to the index that would result
        /// in a truncated key to fail with <see cref="EsentLib.Jet.JET_err.KeyTruncated"/>.
        /// Otherwise, keys will be silently truncated.</summary>
        IndexDisallowTruncation = 0x10000,

        /// <summary>Index over multiple multi-valued columns but only with values of same
        /// itagSequence.</summary>
        IndexNestedTable = 0x20000,

        /// <summary>Specifying this flag will cause the index to use the maximum key size specified
        /// in the cbKeyMost field in the structure. Otherwise, the index will use JET_cbKeyMost
        /// (255) as its maximum key size.</summary>
        /// <remarks>Set internally when the NATIVE_INDEXCREATE structure is generated</remarks>
        IndexKeyMost = 0x8000,

        /// <summary>LCID field of JET_INDEXCREATE actually points to a JET_UNICODEINDEX struct to
        /// allow user-defined LCMapString() flags.</summary>
        IndexUnicode = 0x00000800,

        // --------- //
        // WINDOWS 8 //
        // --------- //
        /// <summary>Specifying this flag will change GUID sort order to .Net standard.</summary>
        IndexDotNetGuid = 0x00040000,

        // ---------- //
        // WINDOWS 10 //
        // ---------- //
        /// <summary>Do not write to the input structures, so that the structures can be
        /// stored in readonly memory.</summary>
        /// <remarks>This was primarily introduced for the C API so that the input structures
        /// could be stored in read-only memory. It is of limited use in this managed code
        /// interop, since the input structures are generated on the fly. It is provided here
        /// for completeness.</remarks>
        /// seealso cref="Api.JetCreateIndex2"
        IndexCreateImmutableStructure = 0x80000,
    }

    /// <summary>Key definition grbits. Used when retrieving information about an index, contained
    /// in the column specified in <see cref="JET_INDEXLIST.columnidgrbitColumn"/>.</summary>
    [Flags]
    public enum IndexKeyGrbit
    {
        /// <summary>Key segment is ascending.</summary>
        Ascending = 0x0,

        /// <summary>Key segment is descending.</summary>
        Descending = 0x1,
    }

    /// <summary>Options for the <seealso cref="JET_CONDITIONALCOLUMN"/> structure.</summary>
    [Flags]
    public enum ConditionalColumnGrbit
    {
        /// <summary>The column must be null for an index entry to appear in the index.</summary>
        ColumnMustBeNull = 0x1,

        /// <summary>The column must be non-null for an index entry to appear in the index.</summary>
        ColumnMustBeNonNull = 0x2,
    }


    /// <summary>Options for <see cref="IJetTable.DeleteColumn"/>.</summary>
    [Flags]
    public enum DeleteColumnGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>The API should only attempt to delete columns in the derived table. If a column
        /// of that name exists in the base table it will be ignored.</summary>
        IgnoreTemplateColumns = 0x1,
    }

    /// <summary>
    /// Options for <see cref="LegacyApi.JetRenameColumn"/>.
    /// </summary>
    [Flags]
    public enum RenameColumnGrbit
    {
        /// <summary>
        /// Default options.
        /// </summary>
        None = 0,
    }

    /// <summary>
    /// Options for <see cref="LegacyApi.JetSetColumnDefaultValue"/>.
    /// </summary>
    [Flags]
    public enum SetColumnDefaultValueGrbit
    {
        /// <summary>
        /// Default options.
        /// </summary>
        None = 0,
    }

    /// <summary>
    /// Options for <see cref="LegacyApi.JetIdle"/>.
    /// </summary>
    [Flags]
    public enum IdleGrbit
    {
        /// <summary>
        /// Default options.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Reserved for future use. If this flag is specified, the API will return <see cref="JET_err.InvalidGrbit"/>.
        /// </summary>
        FlushBuffers = 0x01,

        /// <summary>Triggers cleanup of the version store.</summary>
        Compact = 0x02,

        /// <summary>
        /// Returns <see cref="JET_wrn.IdleFull"/> if version store is more than half full.
        /// </summary>
        GetStatus = 0x04,
    }

    /// <summary>Options for <see cref="LegacyApi.JetDefragment"/>.</summary>
    [Flags]
    public enum DefragGrbit
    {
        /// <summary>
        /// Defragments the available space portion of ESE database space
        /// allocation. Database space is divided into two types, owned
        /// space and available space. Owned space is allocated to a table
        /// or index while available space is ready for use within the table
        /// or index, respectively. Available space is much more dynamic in
        /// behavior and requires on-line defragmentation more so than owned
        /// space or table or index data.
        /// </summary>
        AvailSpaceTreesOnly = 0x40,

        /// <summary>
        /// Starts a new defragmentation task.
        /// </summary>
        BatchStart = 0x1,

        /// <summary>
        /// Stops a defragmentation task.
        /// </summary>
        BatchStop = 0x2,

        // --------- //
        // WINDOWS 7 //
        // --------- //
        /// <summary>While running Online Defragmentation, do not perform partial merges of pages.
        /// </summary>
        NoPartialMerges = 0x80,

        /// <summary>Defragment a single BTree.</summary>
        DefragmentBTree = 0x100,
    }

    /// <summary>Grbits for the various Api.JetGetColumnInfo and Api.JetGetTableColumnInfo
    /// overloads.</summary>
    /// <remarks>Internally this value is OR'ed together with the <see cref="JET_ColInfo"/> info
    /// level. The info level is not publically exposed in this CLR code because it's only used
    /// to differentiate the type of the output parameter, which is covered by having explicit
    /// function overloads with different signatures. There is no need to expose JET_ColInfo to
    /// CLR.</remarks>
    [Flags]
    public enum ColInfoGrbit
    {
        /// <summary>Default options.</summary>
        None = 0x0,

        /// <summary>For lists (example: <see cref="JET_ColInfo.List"/>), only return non-derived
        /// columns (if the table is derived from a template).</summary>
        /// <remarks>This value is 0x80000000.</remarks>
        NonDerivedColumnsOnly = int.MinValue,

        /// <summary>For lists (example: <see cref="JET_ColInfo.List"/>), only return the
        /// column name and columnid of each column.</summary>
        MinimalInfo = 0x40000000,

        /// <summary>For lists (example: <see cref="JET_ColInfo.List"/>), sort returned
        /// column list by columnid (the default is to sort list by column name).</summary>
        SortByColumnid = 0x20000000,
    }

    /// <summary>
    /// Options for <see cref="JET_SPACEHINTS"/>.
    /// </summary>
    [Flags]
    public enum SpaceHintsGrbit
    {
        /// <summary>
        /// Default options.
        /// </summary>
        None = 0x0,
        
        // Generic bits.

        /// <summary>
        /// This changes the internal allocation policy to get space hierarchically
        /// from a B-Tree's immediate parent.
        /// </summary>
        SpaceHintUtilizeParentSpace = 0x00000001,

        // Create bits.

        /// <summary>
        /// This bit will enable Append split behavior to grow according to the
        /// growth dynamics of the table (set by cbMinExtent, ulGrowth, cbMaxExtent).
        /// </summary>
        CreateHintAppendSequential = 0x00000002,

        /// <summary>
        /// This bit will enable Hotpoint split behavior to grow according to the
        /// growth dynamics of the table (set by cbMinExtent, ulGrowth, cbMaxExtent).
        /// </summary>
        CreateHintHotpointSequential = 0x00000004,

        // Retrieve bits.

        /// <summary>
        /// Reserved and ignored.
        /// </summary>
        RetrieveHintReserve1 = 0x00000008,

        /// <summary>
        /// By setting this the client indicates that forward sequential scan is
        /// the predominant usage pattern of this table (causing B+ Tree defrag to 
        /// be auto-triggered to clean it up if fragmented).
        /// </summary>
        RetrieveHintTableScanForward = 0x00000010,

        /// <summary>
        /// By setting this the client indicates that backwards sequential scan
        /// is the predominant usage pattern of this table(causing B+ Tree defrag to 
        /// be auto-triggered to clean it up if fragmented).
        /// </summary>
        RetrieveHintTableScanBackward = 0x00000020,

        /// <summary>
        /// Reserved and ignored.
        /// </summary>
        RetrieveHintReserve2 = 0x00000040,

        /// <summary>
        /// Reserved and ignored.
        /// </summary>
        RetrieveHintReserve3 = 0x00000080,

        // Delete bits.

        /// <summary>
        /// The application expects this table to be cleaned up in-order
        /// sequentially (from lowest key to highest key).
        /// </summary>
        DeleteHintTableSequential = 0x00000100,
    }
    /// <summary>Options for <see cref="IJetSnapshot.Abort"/>.</summary>
    [Flags]
    public enum SnapshotAbortGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,
    }

    /// <summary>Options for IJetCursor.Update.</summary>
    [Flags]
    public enum UpdateGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>This flag causes the update to return an error if the update would not
        /// have been possible in the Windows 2000 version of ESE, which enforced a smaller
        /// maximum number of multi-valued column instances in each record than later versions
        /// of ESE. This is important only for applications that wish to replicate data between
        /// applications hosted on Windows 2000 and applications hosted on Windows  2003, or
        /// later versions of ESE. It should not be necessary for most applications.</summary>
        [Obsolete("Only needed for legacy replication applications.")]
        CheckESE97Compatibility = 0x1,
    }

    /// <summary>Options for <see cref="IJetSnapshot.End"/>.</summary>
    [Flags]
    public enum SnapshotEndGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>The snapshot session aborted.</summary>
        AbortSnapshot = 0x1,
    }

    /// <summary>Options for <see cref="IJetSnapshot.BindInstance"/>.</summary>
    [Flags]
    public enum SnapshotPrepareInstanceGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,
    }

    /// <summary>Options for <see cref="IJetSnapshot.TruncateLog"/> and
    /// <see cref="IJetSnapshot.TruncateInstanceLog"/>.</summary>
    [Flags]
    public enum SnapshotTruncateLogGrbit
    {
        /// <summary>No truncation will occur.</summary>
        None = 0,

        /// <summary>All the databases are attached so the storage engine can compute and do
        /// the log truncation.</summary>
        AllDatabasesSnapshot = 0x1,
    }

    /// <summary>Options for <see cref="IJetSnapshot.GetInfo"/>.</summary>
    [Flags]
    public enum SnapshotGetFreezeInfoGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,
    }

    /// <summary>Information levels for <see cref="EsentLib.Implementation.JetInstance.GetInfo"/>.</summary>
    [Flags]
    public enum JET_InstanceMiscInfo
    {
        /// <summary>Get the signature of the transaction log associated with this sequence.</summary>
        LogSignature = 0,
    }
    /// <summary>Options for <see cref="LegacyApi.JetConfigureProcessForCrashDump"/>.</summary>
    [Flags]
    public enum CrashDumpGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>Dump minimum includes <see cref="CacheMinimum"/>.</summary>
        Minimum = 0x1,

        /// <summary>Dump maximum includes <see cref="CacheMaximum"/>.</summary>
        Maximum = 0x2,

        /// <summary>CacheMinimum includes pages that are latched. CacheMinimum includes pages
        /// that are used for memory. CacheMinimum includes pages that are flagged with errors.
        /// </summary>
        CacheMinimum = 0x4,

        /// <summary>Cache maximum includes cache minimum. Cache maximum includes the entire
        /// cache image.</summary>
        CacheMaximum = 0x8,

        /// <summary>Dump includes pages that are modified.</summary>
        CacheIncludeDirtyPages = 0x10,

        /// <summary>Dump includes pages that contain valid data.</summary>
        CacheIncludeCachedPages = 0x20,

        /// <summary>Dump includes pages that are corrupted (expensive to compute).</summary>
        CacheIncludeCorruptedPages = 0x40,
    }

    /// <summary>Options for <see cref="LegacyApi.JetPrereadKeys(JET_SESID, JET_TABLEID, byte[][], int[], int, int, out int, PrereadKeysGrbit)"/>.
    /// </summary>
    [Flags]
    public enum PrereadKeysGrbit
    {
        /// <summary>Preread forward.</summary>
        Forward = 0x1,

        /// <summary>Preread backwards.</summary>
        Backwards = 0x2,
    }
    /// <summary>Options for <see cref="LegacyApi.JetGetErrorInfo"/>.</summary>
    public enum ErrorInfoGrbit
    {
        /// <summary>No option.</summary>
        None = 0,
    }


    /// <summary>Options passed to log flush callback.</summary>
    [Flags]
    public enum DurableCommitCallbackGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        // ---------- //
        // WINDOWS 10 //
        // ---------- //
        /// <summary>Passed back to durable commit callback to let it know that log is down
        /// (and all pending commits will not be flushed to disk).Used with
        /// <see cref="EsentLib.Jet.JET_param.DurableCommitCallback"/>.</summary>
        LogUnavailable = 0x1,
    }

    /// <summary>Options for <see cref="LegacyApi.JetPrereadIndexRanges"/>.</summary>
    [Flags]
    public enum PrereadIndexRangesGrbit
    {
        /// <summary>Preread forward.</summary>
        Forward = 0x1,

        /// <summary>Preread backwards.</summary>
        Backwards = 0x2,

        /// <summary>Preread only first page of any long column.</summary>
        FirstPageOnly = 0x4,

        /// <summary>Normalized key/bookmark provided instead of column value.</summary>
        NormalizedKey = 0x8,
    }

    /// <summary>Options for <see cref="EsentLib.Implementation.JetInstance.Stop"/>.</summary>
    [Flags]
    public enum StopServiceGrbit
    {
        /// <summary>
        /// Stops all ESE services for the specified instance.
        /// </summary>
        All = 0x00000000,

        /// <summary>
        /// Stops restartable client specificed background maintenance tasks (B+ Tree Defrag).
        /// </summary>
        BackgroundUserTasks = 0x00000002,

        /// <summary>
        /// Quiesces all dirty caches to disk. Asynchronous. Quiescing is cancelled if the <see cref="Resume"/>
        /// bit is called subsequently.
        /// </summary>
        QuiesceCaches = 0x00000004,

        /// <summary>
        /// Resumes previously issued StopService operations, i.e. "restarts service".  Can be combined
        /// with above grbits to Resume specific services, or with 0x0 Resumes all previous stopped services.
        /// </summary>
        /// <remarks>
        /// Warning: This bit can only be used to resume JET_bitStopServiceBackground and JET_bitStopServiceQuiesceCaches, if you 
        /// did a JET_bitStopServiceAll or JET_bitStopServiceAPI, attempting to use JET_bitStopServiceResume will fail. 
        /// </remarks>
        Resume = int.MinValue, // 0x80000000
    }

    /// <summary>Options passed while setting cursor filters.</summary>
    /// <seealso cref="LegacyApi.JetSetCursorFilter"/>
    [Flags]
    public enum CursorFilterGrbit
    {
        /// <summary>Default options.</summary>
        None = 0
    }

    /// <summary>
    /// Options for <see cref="EsentLib.Jet.JET_INDEX_COLUMN"/>.
    /// </summary>
    [Flags]
    public enum JetIndexColumnGrbit
    {
        /// <summary>
        /// Default options.
        /// </summary>
        None = 0,

        /// <summary>
        /// Zero-length value (non-null).
        /// </summary>
        ZeroLength = 0x1,
    }
}