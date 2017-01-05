//-----------------------------------------------------------------------
// <copyright file="jet_param.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;

using EsentLib.Api;

namespace EsentLib.Jet
{
    /// <summary>ESENT system parameters.
    /// <see cref="IJetInstance.SetSystemParameter(JET_param,IntPtr,string)"/>,
    /// </summary>
    public enum JET_param
    {
        /// <summary>This parameter indicates the relative or absolute file system path of the
        /// folder that will contain the checkpoint file for the instance. The path must be
        /// terminated with a backslash character, which indicates that the target path is a folder.
        /// </summary>
        SystemPath = 0,

        /// <summary>This parameter indicates the relative or absolute file system path of the
        /// folder or file that will contain the temporary database for the instance. If the path
        /// is to a folder that will contain the temporary database then it must be terminated
        /// with a backslash character.</summary>
        TempPath = 1,

        /// <summary>This parameter indicates the relative or absolute file system path of the
        /// folder that will contain the transaction logs for the instance. The path must be
        /// terminated with a backslash character, which indicates that the target path is a
        /// folder.</summary>
        LogFilePath = 2,

        /// <summary>This parameter sets the three letter prefix used for many of the files
        /// used by the database engine. For example, the checkpoint file is called EDB.CHK
        /// by default because EDB is the default base name.</summary>
        BaseName = 3,

        /// <summary>This parameter supplies an application specific string that will be added
        /// to any event log messages that are emitted by the database engine. This allows
        /// easy correlation of event log messages with the source application. By default
        /// the host application executable name will be used.</summary>
        EventSource = 4,

        /// <summary>This parameter reserves the requested number of session resources for
        /// use by an instance. A session resource directly corresponds to a JET_SESID data
        /// type. This setting will affect how many sessions can be used at the same time.</summary>
        MaxSessions = 5,

        /// <summary>This parameter reserves the requested number of B+ Tree resources for
        /// use by an instance. This setting will affect how many tables can be used at the
        /// same time.</summary>
        MaxOpenTables = 6,

        // PreferredMaxOpenTables(7) is obsolete

        /// <summary>This parameter reserves the requested number of cursor resources for use
        /// by an instance. A cursor resource directly corresponds to a JET_TABLEID data type.
        /// This setting will affect how many cursors can be used at the same time. A cursor
        /// resource cannot be shared by different sessions so this parameter must be set to
        /// a large enough value so that each session can use as many cursors as are required.</summary>
        MaxCursors = 8,

        /// <summary>This parameter reserves the requested number of version store pages for
        /// use by an instance.</summary>
        MaxVerPages = 9,

        /// <summary>This parameter reserves the requested number of temporary table resources
        /// for use by an instance. This setting will affect how many temporary tables can be
        /// used at the same time. If this system parameter is set to zero then no temporary
        /// database will be created and any activity that requires use of the temporary database
        /// will fail. This setting can be useful to avoid the I/O required to create the
        /// temporary database if it is known that it will not be used.</summary>
        /// <remarks>The use of a temporary table also requires a cursor resource.</remarks>
        MaxTemporaryTables = 10,

        /// <summary>This parameter will configure the size of the transaction log files. Each
        /// transaction log file is a fixed size. The size is equal to the setting of this
        /// system parameter in units of 1024 bytes.</summary>
        LogFileSize = 11,

        /// <summary>This parameter will configure the amount of memory used to cache log
        /// records before they are written to the transaction log file. The unit for this
        /// parameter is the sector size of the volume that holds the transaction log files.
        /// The sector size is almost always 512 bytes, so it is safe to assume that size
        /// for the unit. This parameter has an impact on performance. When the database
        /// engine is under heavy update load, this buffer can become full very rapidly.
        /// A larger cache size for the transaction log file is critical for good update
        /// performance under such a high load condition. The default is known to be too
        /// small for this case. Do not set this parameter to a number of buffers that is
        /// larger (in bytes) than half the size of a transaction log file.</summary>
        LogBuffers = 12,

        /// <summary>This parameter configures how transaction log files are managed by the
        /// database engine. When circular logging is off, all transaction log files that
        /// are generated are retained on disk until they are no longer needed because a
        /// full backup of the database has been performed. When circular logging is on,
        /// only transaction log files that are younger than the current checkpoint are
        /// retained on disk. The benefit of this mode is that backups are not required to
        /// retire old transaction log files. </summary>
        CircularLog = 17,

        /// <summary>This parameter controls the amount of space that is added to a database
        /// file each time it needs to grow to accommodate more data. The size is in database
        /// pages.</summary>
        DbExtensionSize = 18,

        /// <summary>This parameter controls the initial size of the temporary database. The
        /// size is in database pages. A size of zero indicates that the default size of an
        /// ordinary database should be used. It is often desirable for small applications to
        /// configure the temporary database to be as small as possible. Setting this parameter
        /// to SystemParameters.PageTempDBSmallest will achieve the smallest temporary database
        /// possible.</summary>
        PageTempDBMin = 19,

        /// <summary>
        /// This parameter configures the maximum size of the database page cache. The size
        /// is in database pages. If this parameter is left to its default value, then the
        /// maximum size of the cache will be set to the size of physical memory when JetInit
        /// is called.
        /// </summary>
        CacheSizeMax = 23,

        /// <summary>
        /// This parameter controls how aggressively database pages are flushed from the
        /// database page cache to minimize the amount of time it will take to recover from a
        /// crash. The parameter is a threshold in bytes for about how many transaction log
        /// files will need to be replayed after a crash. If circular logging is enabled using
        /// JET_param.CircularLog then this parameter will also control the approximate amount
        /// of transaction log files that will be retained on disk.
        /// </summary>
        CheckpointDepthMax = 24,

        /// <summary>
        /// This parameter controls the correlation interval of ESE's LRU-K page replacement
        /// algorithm.
        /// </summary>
        LrukCorrInterval = 25,

        /// <summary>
        /// This parameter controls the timeout interval of ESE's LRU-K page replacement
        /// algorithm.
        /// </summary>
        LrukTimeout = 28,

        /// <summary>
        /// This parameter controls how many database file I/Os can be queued
        /// per-disk in the host operating system at one time.  A larger value
        /// for this parameter can significantly help the performance of a large
        /// database application.
        /// </summary>
        OutstandingIOMax = 30,

        /// <summary>
        /// This parameter controls when the database page cache begins evicting pages from the
        /// cache to make room for pages that are not cached. When the number of page buffers in the cache
        /// drops below this threshold then a background process will be started to replenish that pool
        /// of available buffers. This threshold is always relative to the maximum cache size as set by
        /// JET_paramCacheSizeMax. This threshold must also always be less than the stop threshold as
        /// set by JET_paramStopFlushThreshold.
        /// <para>
        /// The distance height of the start threshold will determine the response time that the database
        ///  page cache must have to produce available buffers before the application needs them. A high
        /// start threshold will give the background process more time to react. However, a high start
        /// threshold implies a higher stop threshold and that will reduce the effective size of the
        /// database page cache for modified pages (Windows 2000) or for all pages (Windows XP and later).
        /// </para>
        /// </summary>
        StartFlushThreshold = 31,

        /// <summary>
        /// This parameter controls when the database page cache ends evicting pages from the cache to make
        /// room for pages that are not cached. When the number of page buffers in the cache rises above
        /// this threshold then the background process that was started to replenish that pool of available
        /// buffers is stopped. This threshold is always relative to the maximum cache size as set by
        /// JET_paramCacheSizeMax. This threshold must also always be greater than the start threshold
        /// as set by JET_paramStartFlushThreshold.
        /// <para>
        /// The distance between the start threshold and the stop threshold affects the efficiency with
        /// which database pages are flushed by the background process. A larger gap will make it
        /// more likely that writes to neighboring pages may be combined. However, a high stop
        /// threshold will reduce the effective size of the database page cache for modified
        /// pages (Windows 2000) or for all pages (Windows XP and later).
        /// </para>
        /// </summary>
        StopFlushThreshold = 32,

        /// <summary>
        /// This parameter is the master switch that controls crash recovery for an instance.
        /// If this parameter is set to "On" then ARIES style recovery will be used to bring all
        /// databases in the instance to a consistent state in the event of a process or machine
        /// crash. If this parameter is set to "Off" then all databases in the instance will be
        /// managed without the benefit of crash recovery. That is to say, that if the instance
        /// is not shut down cleanly using JetTerm prior to the process exiting or machine shutdown
        /// then the contents of all databases in that instance will be corrupted.
        /// </summary>
        Recovery = 34,

        /// <summary>
        /// This parameter controls the behavior of online defragmentation when initiated using
        /// <see cref="LegacyApi.JetDefragment"/> and <see cref="LegacyApi.JetDefragment2"/>.
        /// </summary>
        EnableOnlineDefrag = 35,

        /// <summary>
        /// This parameter can be used to control the size of the database page cache at run time.
        /// Ordinarily, the cache will automatically tune its size as a function of database and
        /// machine activity levels. If the application sets this parameter to zero, then the cache
        /// will tune its own size in this manner. However, if the application sets this parameter
        /// to a non-zero value then the cache will adjust itself to that target size.
        /// </summary>
        CacheSize = 41,

        /// <summary>
        /// When this parameter is true, every database is checked at JetAttachDatabase time for
        /// indexes over Unicode key columns that were built using an older version of the NLS
        /// library in the operating system. This must be done because the database engine persists
        /// the sort keys generated by LCMapStringW and the value of these sort keys change from release to release.
        /// If a primary index is detected to be in this state then JetAttachDatabase will always fail with
        /// JET_err.PrimaryIndexCorrupted.
        /// If any secondary indexes are detected to be in this state then there are two possible outcomes.
        /// If AttachDatabaseGrbit.DeleteCorruptIndexes was passed to JetAttachDatabase then these indexes
        /// will be deleted and JET_wrnCorruptIndexDeleted will be returned from JetAttachDatabase. These
        /// indexes will need to be recreated by your application. If AttachDatabaseGrbit.DeleteCorruptIndexes
        /// was not passed to JetAttachDatabase then the call will fail with JET_errSecondaryIndexCorrupted.
        /// </summary>
        EnableIndexChecking = 45,

        /// <summary>
        /// This parameter can be used to control which event log the database engine uses for its event log
        /// messages. By default, all event log messages will go to the Application event log. If the registry
        /// key name for another event log is configured then the event log messages will go there instead.
        /// </summary>        
        EventSourceKey = 49,

        /// <summary>
        /// When this parameter is true, informational event log messages that would ordinarily be generated by
        /// the database engine will be suppressed.
        /// </summary>
        NoInformationEvent = 50,

        /// <summary>
        /// Configures the detail level of eventlog messages that are emitted
        /// to the eventlog by the database engine. Higher numbers will result
        /// in more detailed eventlog messages.
        /// </summary>
        EventLoggingLevel = 51,

        /// <summary>
        /// Delete the log files that are not matching (generation wise) during soft recovery.
        /// </summary>
        DeleteOutOfRangeLogs = 52,

        /// <summary>
        /// <para>
        /// After Windows 7, it was discovered that JET_paramEnableIndexCleanup had some implementation limitations, reducing its effectiveness.
        /// Rather than update it to work with locale names, the functionality is removed altogether.
        /// </para>
        /// <para>
        /// Unfortunately JET_paramEnableIndexCleanup can not be ignored altogether. JET_paramEnableIndexChecking defaults to false, so if
        /// JET_paramEnableIndexCleanup were to be removed entirely, then by default there were would be no checks for NLS changes!
        /// </para>
        /// <para>
        /// The current behavious (when enabled) is to track the language sort versions for the indices, and when the sort version for that
        /// particular locale changes, the engine knows which indices are now invalid. For example, if the sort version for only "de-de" changes,
        /// then the "de-de" indices are invalid, but the "en-us" indices will be fine.
        /// </para>
        /// <para>
        /// Post-Windows 8:
        /// JET_paramEnableIndexChecking accepts JET_INDEXCHECKING (which is an enum). The values of '0' and '1' have the same meaning as before,
        /// but '2' is JET_IndexCheckingDeferToOpenTable, which means that the NLS up-to-date-ness is NOT checked when the database is attached.
        /// It is deferred to IJetDatabase.OpenTable(), which may now fail with JET_errPrimaryIndexCorrupted or JET_errSecondaryIndexCorrupted (which
        /// are NOT actual corruptions, but instead reflect an NLS sort change).
        /// </para>
        /// <para>
        /// IN SUMMARY:
        /// New code should explicitly set both IndexChecking and IndexCleanup to the same value.
        /// </para>
        /// </summary>
        EnableIndexCleanup = 54,

        /// <summary>
        /// This parameter configures the minimum size of the database page cache. The size is in database pages.
        /// </summary>
        CacheSizeMin = 60,

        /// <summary>
        /// This parameter represents a threshold relative to <see cref="JET_param.MaxVerPages"/> that controls
        /// the discretionary use of version pages by the database engine. If the size of the version store exceeds
        /// this threshold then any information that is only used for optional background tasks, such as reclaiming
        /// deleted space in the database, is instead sacrificed to preserve room for transactional information.
        /// </summary>
        PreferredVerPages = 63,

        /// <summary>
        /// This parameter configures the page size for the database. The page
        /// size is the smallest unit of space allocation possible for a database
        /// file. The database page size is also very important because it sets
        /// the upper limit on the size of an individual record in the database. 
        /// </summary>
        /// <remarks>
        /// Only one database page size is supported per process at this time.
        /// This means that if you are in a single process that contains different
        /// applications that use the database engine then they must all agree on
        /// a database page size.
        /// </remarks>
        DatabasePageSize = 64,

        /// <summary>This parameter can be used to convert a JET_ERR into a string.
        /// This should only be used with JetGetSystemParameter.</summary>
        ErrorToString = 70,

        /// <summary>
        /// Configures the engine with a <see cref="JET_CALLBACK"/> delegate.
        /// This callback may be called for the following reasons:
        /// <see cref="JET_cbtyp.FreeCursorLS"/>, <see cref="JET_cbtyp.FreeTableLS"/>
        /// or <see cref="JET_cbtyp.Null"/>. See <see cref="LegacyApi.JetSetLS"/>
        /// for more information. This parameter cannot currently be retrieved.
        /// </summary>
        RuntimeCallback = 73,

        /// <summary>
        /// This parameter controls the outcome of JetInit when the database
        /// engine is configured to start using transaction log files on disk
        /// that are of a different size than what is configured. Normally,
        /// <see cref="EsentLib.Implementation.JetInstance.Initialize"/> will successfully recover the databases
        /// but will fail with <see cref="JET_err.LogFileSizeMismatchDatabasesConsistent"/>
        /// to indicate that the log file size is misconfigured. However, when
        /// this parameter is set to true then the database engine will silently
        /// delete all the old log files, start a new set of transaction log files
        /// using the configured log file size. This parameter is useful when the
        /// application wishes to transparently change its transaction log file
        /// size yet still work transparently in upgrade and restore scenarios.
        /// </summary>
        CleanupMismatchedLogFiles = 77,

        /// <summary>
        /// This parameter controls what happens when an exception is thrown by the 
        /// database engine or code that is called by the database engine. When set 
        /// to JET_ExceptionMsgBox, any exception will be thrown to the Windows unhandled 
        /// exception filter. This will result in the exception being handled as an 
        /// application failure. The intent is to prevent application code from erroneously 
        /// trying to catch and ignore an exception generated by the database engine. 
        /// This cannot be allowed because database corruption could occur. If the application 
        /// wishes to properly handle these exceptions then the protection can be disabled 
        /// by setting this parameter to JET_ExceptionNone.
        /// </summary>
        ExceptionAction = 98,

        /// <summary>
        /// When this parameter is set to true then any folder that is missing in a file system path in use by
        /// the database engine will be silently created. Otherwise, the operation that uses the missing file system
        /// path will fail with JET_err.InvalidPath.
        /// </summary>
        CreatePathIfNotExist = 100,

        /// <summary>
        /// When this parameter is true then only one database is allowed to
        /// be opened using JetOpenDatabase by a given session at one time.
        /// The temporary database is excluded from this restriction. 
        /// </summary>
        OneDatabasePerSession = 102,

        /// <summary>
        /// This parameter controls the maximum number of instances that can be created in a single process.
        /// </summary>
        MaxInstances = 104,

        /// <summary>
        /// This parameter controls the number of background cleanup work items that
        /// can be queued to the database engine thread pool at any one time.
        /// </summary>
        VersionStoreTaskQueueMax = 105,

        /// <summary>
        /// This parameter controls whether perfmon counters should be enabled or not.
        /// By default, perfmon counters are enabled, but there is memory overhead for enabling
        /// them.
        /// </summary>
        DisablePerfmon = 107,

        // ------------ //
        // WINDOWS 2003 //
        // ------------ //
        /// <summary>The full path to each database is persisted in the transaction logs at run
        /// time. Ordinarily, these databases must remain at the original location for transaction
        /// replay to function correctly. This parameter can be used to force crash recovery or
        /// a restore operation to look for the databases referenced in the transaction log in
        /// the specified folder.</summary>
        AlternateDatabaseRecoveryPath = 113,

        // ----- //
        // VISTA //
        // ----- //
        /// <summary>
        /// This parameter controls the number of B+ Tree resources cached by
        /// the instance after the tables they represent have been closed by
        /// the application. Large values for this parameter will cause the
        /// database engine to use more memory but will increase the speed
        /// with which a large number of tables can be opened randomly by
        /// the application. This is useful for applications that have a
        /// schema with a very large number of tables.
        /// </summary>
        CachedClosedTables = 125,

        /// <summary>
        /// Enable the use of the OS file cache for all managed files.
        /// </summary>
        EnableFileCache = 126,

        /// <summary>
        /// Enable the use of memory mapped file I/O for database files.
        /// </summary>
        EnableViewCache = 127,

        /// <summary>
        /// This parameter exposes multiple sets of default values for the
        /// entire set of system parameters. When this parameter is set to
        /// a specific configuration, all system parameter values are reset
        /// to their default values for that configuration. If the
        /// configuration is set for a specific instance then global system
        /// parameters will not be reset to their default values.
        /// Small Configuration (0): The database engine is optimized for memory use. 
        /// Legacy Configuration (1): The database engine has its traditional defaults.
        /// </summary>
        Configuration = 129,

        /// <summary>
        /// This parameter is used to control when the database engine accepts
        /// or rejects changes to a subset of the system parameters. This
        /// parameter is used in conjunction with <see cref="Configuration"/> to
        /// prevent some system parameters from being set away from the selected
        /// configuration's defaults.
        /// </summary>
        EnableAdvanced = 130,

        /// <summary>
        /// This read-only parameter indicates the maximum allowable index key
        /// length that can be selected for the current database page size
        /// (as configured by <see cref="JET_param.DatabasePageSize"/>).
        /// </summary>
        KeyMost = 134,

        /// <summary>
        /// This parameter provides backwards compatibility with the file naming conventions of earlier releases of the database engine.
        /// </summary>
        LegacyFileNames = 136,

        /// <summary>Set the name associated with table class 1.</summary>
        TableClass1Name = 137,

        /// <summary>Set the name associated with table class 2.</summary>
        TableClass2Name = 138,

        /// <summary>Set the name associated with table class 3.</summary>
        TableClass3Name = 139,

        /// <summary>Set the name associated with table class 4.</summary>
        TableClass4Name = 140,

        /// <summary>Set the name associated with table class 5.</summary>
        TableClass5Name = 141,

        /// <summary>Set the name associated with table class 6.</summary>
        TableClass6Name = 142,

        /// <summary>Set the name associated with table class 7.</summary>
        TableClass7Name = 143,

        /// <summary>Set the name associated with table class 8.</summary>
        TableClass8Name = 144,

        /// <summary>Set the name associated with table class 9.</summary>
        TableClass9Name = 145,

        /// <summary>Set the name associated with table class 10.</summary>
        TableClass10Name = 146,

        /// <summary>Set the name associated with table class 11.</summary>
        TableClass11Name = 147,

        /// <summary>Set the name associated with table class 12.</summary>
        TableClass12Name = 148,

        /// <summary>Set the name associated with table class 13.</summary>
        TableClass13Name = 149,

        /// <summary>Set the name associated with table class 14.</summary>
        TableClass14Name = 150,

        /// <summary>Set the name associated with table class 15.</summary>
        TableClass15Name = 151,

        /// <summary>Sets the IO priority per instance, anytime. This is used mainly for
        /// background recovery (log replay). Does not affect the pending IOs, just the
        /// subsequent ones issued. The valid values for this parameter are contained in the
        /// <see cref="JET_IOPriority"/> enumeration.</summary>
        IOPriority = 152,

        // --------- //
        // WINDOWS 7 //
        // --------- //
        /// <summary>
        /// This parameter sets the number of logs that esent will defer database
        /// flushes for. This can be used to increase database recoverability if
        /// failures cause logfiles to be lost.
        /// </summary>
        WaypointLatency = 153,

        /// <summary>Turn on/off automatic sequential B-tree defragmentation tasks (On by default, but
        /// also requires <see cref="SpaceHintsGrbit"/> flags / <see cref="SpaceHintsGrbit"/>.RetrieveHintTableScan*
        /// to trigger on any given tables).</summary>
        DefragmentSequentialBTrees = 160,

        /// <summary>Determine how frequently B-tree density is checked (Note: currently not
        /// implemented).</summary>
        DefragmentSequentialBTreesDensityCheckFrequency = 161,

        /// <summary>This parameter is used to retrieve the chunk size of long-value (blob) data.
        /// Setting and retrieving data in multiples of this size increases efficiency.</summary>
        LVChunkSizeMost = 163,

        /// <summary>Maximum number of bytes that can be grouped for a coalesced read operation.</summary>
        MaxCoalesceReadSize = 164,

        /// <summary>Maximum number of bytes that can be grouped for a coalesced write operation.</summary>
        MaxCoalesceWriteSize = 165,

        /// <summary>Maximum number of bytes that can be gapped for a coalesced read IO operation.</summary>
        MaxCoalesceReadGapSize = 166,

        /// <summary>Maximum number of bytes that can be gapped for a coalesced write IO operation.</summary>
        MaxCoalesceWriteGapSize = 167,

        /// <summary>Enable Database Maintenance during recovery.</summary>
        EnableDbScanInRecovery = 169,

        /// <summary>Throttling of the database scan, in milliseconds.</summary>
        DbScanThrottle = 170,

        /// <summary>Minimum interval to repeat the database scan, in seconds.</summary>
        DbScanIntervalMinSec = 171,

        /// <summary>Maximum interval to allow the database scan to finish, in seconds.</summary>
        DbScanIntervalMaxSec = 172,

        // --------- //
        // WINDOWS 8 //
        // --------- //
        /// <summary>Per-instance property for relative cache priorities (default = 100).</summary>
        CachePriority = 177,

        /// <summary>Percentage of version store that can be used by oldest transaction
        /// before <see cref="JET_err.VersionStoreOutOfMemory"/> (default = 100).
        /// </summary>
        MaxTransactionSize = 178,

        /// <summary>Maximum number of I/O operations dispatched for a given purpose.</summary>
        PrereadIOMax = 179,

        /// <summary>Database Maintenance serialization is enabled for databases sharing the
        /// same disk.</summary>
        EnableDBScanSerialization = 180,

        /// <summary>The threshold for what is considered a hung IO that should be acted upon.</summary>
        HungIOThreshold = 181,

        /// <summary>A set of actions to be taken on IOs that appear hung.</summary>
        HungIOActions = 182,

        /// <summary>Smallest amount of data that should be compressed with xpress compression.</summary>
        MinDataForXpress = 183,

        /// <summary>Friendly name for this instance of the process.</summary>
        ProcessFriendlyName = 186,

        /// <summary>Callback for when log is flushed.</summary>
        DurableCommitCallback = 187,

        // ----------- //
        // WINDOWS 8.1 //
        // ----------- //
        /// <summary>Whether to free space back to the OS after deleting data. This may free space
        /// in the middle of files (done in the units of database extents). This uses Sparse Files,
        /// which is available on NTFS and ReFS (not FAT). The exact method of releasing space is an
        /// implementation detail and is subject to change.</summary>
        EnableShrinkDatabase = 184,

        // ---------- //
        // WINDOWS 10 //
        // ---------- //
        /// <summary>This allows the client to specify a registry path preceded by a reg: to
        /// optionally configure loading or overriding parameters from the registry.</summary>
        ConfigStoreSpec = 189,
    }
}
