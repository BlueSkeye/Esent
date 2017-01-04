//-----------------------------------------------------------------------
// <copyright file="JetApi.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;

using EsentLib.Api;
using EsentLib.Api.Flags;
using EsentLib.Jet;
using EsentLib.Jet.Types;
using EsentLib.Jet.Windows8;

namespace EsentLib.Implementation
{
    /// <summary>Calls to the ESENT interop layer. These calls take the managed types
    /// (e.g. JET_SESID) and return errors.</summary>
    internal class JetInstance :
        // TODO : SafeHandleZeroOrMinusOneIsInvalid,
        IJetInstance, IDisposable
    {
        /// <summary>Initializes static members of the JetApi class.</summary>
        static JetInstance()
        {
            // TODO : Most of these methods have been removed by IDispose implementation.
            // Find hat to prepare now.
            Type thisType = typeof(JetInstance);
            // Prepare these methods for inclusion in a constrained execution region (CER).
            // This is needed by the Instance class. Instance accesses these methods
            // virtually so RemoveUnnecessaryCode won't be able to prepare them.
            foreach (string methodName in new string[] { "JetCreateInstance", "JetCreateInstance2",
                "JetInit", "JetInit2", "JetInit3", "JetTerm", "JetTerm2"})
            {
                RuntimeHelpers.PrepareMethod(thisType.GetMethod(methodName).MethodHandle);
            }
        }

        /// <summary>Initializes a new instance of the JetApi class. This allows the version
        /// to be set.</summary>
        /// <param name="version">The version of Esent. This is used to override the results
        /// of JetGetVersion.</param>
        protected JetInstance(uint version)
        {
            this.versionOverride = version;
            _capabilities = JetEnvironment.DetermineCapabilities(version);
        }

        /// <summary>Initializes a new instance of the JetEngine class.</summary>
        protected JetInstance()
        {
            this.versionOverride = 8250 << 8;
            _capabilities = JetEnvironment.Current.DefaultCapabilities;
        }

        #region PROPERTIES
        /// <summary>Gets or sets the relative or absolute file system path of the a folder
        /// where crash recovery or a restore operation can find the databases referenced in
        /// the transaction log in the specified folder.</summary>
        /// <remarks>This parameter is ignored on Windows XP.</remarks>
        public string AlternateDatabaseRecoveryDirectory
        {
            get
            {
                if (EsentVersion.SupportsServer2003Features) {
                    return Util.AddTrailingDirectorySeparator(
                        NativeHelpers.GetStringParameter(_instance,
                            JET_param.AlternateDatabaseRecoveryPath));
                }
                return null;
            }
            set
            {
                if (EsentVersion.SupportsServer2003Features) {
                    NativeHelpers.SetParameter(_instance, JET_param.AlternateDatabaseRecoveryPath,
                        Util.AddTrailingDirectorySeparator(value));
                }
            }
        }

        /// <summary>
        /// Gets or sets the three letter prefix used for many of the files used by
        /// the database engine. For example, the checkpoint file is called EDB.CHK by
        /// default because EDB is the default base name.
        /// </summary>
        public string BaseName
        {
            get { return NativeHelpers.GetStringParameter(_instance, JET_param.BaseName); }
            set { NativeHelpers.SetParameter(_instance, JET_param.BaseName, value); }
        }

        /// <summary>Gets or sets a value giving the number of B+ Tree resources cached by the
        /// instance after the tables they represent have been closed by the application.
        /// Large values for this parameter will cause the database engine to use more
        /// memory but will increase the speed with which a large number of tables can be
        /// opened randomly by the application. This is useful for applications that have a
        /// schema with a very large number of tables.<para>
        /// Supported on Windows Vista and up. Ignored on Windows XP and Windows Server 2003.</para>
        /// </summary>
        public int CachedClosedTables
        {
            get
            {
                return (EsentVersion.SupportsVistaFeatures)
                    ? NativeHelpers.GetInt32Parameter(_instance, JET_param.CachedClosedTables)
                    : 0;
            }
            set
            {
                if (EsentVersion.SupportsVistaFeatures) {
                    NativeHelpers.SetParameter(_instance, JET_param.CachedClosedTables, value);
                }
            }
        }

        /// <summary>Gets or sets the per-instance property for relative cache priorities (default = 100). </summary>
        public int CachePriority
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.CachePriority); }
            set { NativeHelpers.SetParameter(_instance, JET_param.CachePriority, value); }
        }

        /// <summary>Gets a description of the capabilities of the current version
        /// of ESENT.</summary>
        public IJetCapabilities Capabilities
        {
            get { return _Capabilities; }
        }

        internal JetCapabilities _Capabilities
        {
            get { return _capabilities ?? JetEnvironment.Current.DefaultCapabilities; }
        }

        /// <summary>Gets or sets the threshold in bytes for about how many transaction log
        /// files will need to be replayed after a crash. If circular logging is enabled using
        /// CircularLog then this parameter will also control the approximate amount of
        /// transaction log files that will be retained on disk.</summary>
        public int CheckpointDepthMax
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.CheckpointDepthMax); }
            set { NativeHelpers.SetParameter(_instance, JET_param.CheckpointDepthMax, value); }
        }

        /// <summary>Gets or sets a value indicating whether circular logging is on. When circular
        /// logging is off, all transaction log files that are generated are retained on disk until
        /// they are no longer needed because a full backup of the database has been performed.
        /// When circular logging is on, only transaction log files that are younger than the
        /// current checkpoint are retained on disk. The benefit of this mode is that backups are
        /// not required to retire old transaction log files.</summary>
        public bool CircularLog
        {
            get { return NativeHelpers.GetBoolParameter(_instance, JET_param.CircularLog); }
            set { NativeHelpers.SetParameter(_instance, JET_param.CircularLog, value); }
        }

        /// <summary>Gets or sets a value indicating whether JetInit fails when the database
        /// engine is configured to start using transaction log files on disk that are of a
        /// different size than what is configured. Normally,
        /// <see cref="EsentLib.Implementation.JetInstance.Initialize(InitGrbit, JET_RSTINFO)"/>
        /// will successfully recover the databases but will fail with <see cref="JET_err.LogFileSizeMismatchDatabasesConsistent"/>
        /// to indicate that the log file size is misconfigured. However, when this parameter
        /// is set to true then the database engine will silently delete all the old log
        /// files, start a new set of transaction log files using the configured log file
        /// size. This parameter is useful when the application wishes to transparently
        /// change its transaction log file size yet still work transparently in upgrade and
        /// restore scenarios.</summary>
        public bool CleanupMismatchedLogFiles
        {
            get { return NativeHelpers.GetBoolParameter(_instance, JET_param.CleanupMismatchedLogFiles); }
            set { NativeHelpers.SetParameter(_instance, JET_param.CleanupMismatchedLogFiles, value); }
        }

        /// <summary>Gets or sets a value indicating whether ESENT will silently create
        /// folders that are missing in its filesystem paths.</summary>
        public bool CreatePathIfNotExist
        {
            get { return NativeHelpers.GetBoolParameter(_instance, JET_param.CreatePathIfNotExist); }
            set { NativeHelpers.SetParameter(_instance, JET_param.CreatePathIfNotExist, value); }
        }

        /// <summary>Gets or sets the number of pages that are added to a database file each
        /// time it needs to grow to accommodate more data.</summary>
        public int DbExtensionSize
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.DbExtensionSize); }
            set { NativeHelpers.SetParameter(_instance, JET_param.DbExtensionSize, value); }
        }

        /// <summary>Gets or sets the throttling of the database scan, in milliseconds.</summary>
        public int DbScanThrottle
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.DbScanThrottle); }
            set { NativeHelpers.SetParameter(_instance, JET_param.DbScanThrottle, value); }
        }

        /// <summary>Gets or sets the minimum interval to repeat the database scan, in seconds.</summary>
        public int DbScanIntervalMinSec
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.DbScanIntervalMinSec); }
            set { NativeHelpers.SetParameter(_instance, JET_param.DbScanIntervalMinSec, value); }
        }

        /// <summary>Gets or sets the maximum interval to allow the database scan to finish, in seconds. </summary>
        public int DbScanIntervalMaxSec
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.DbScanIntervalMaxSec); }
            set { NativeHelpers.SetParameter(_instance, JET_param.DbScanIntervalMaxSec, value); }
        }

        public string DisplayName { get; private set; }

        /// <summary>Get or set the callback for log flush.</summary>
        public NATIVE_JET_PFNDURABLECOMMITCALLBACK DurableCommitCallback
        {
            get
            {
                IntPtr rawValue = NativeHelpers.GetIntPtrParameter(_instance, JET_param.DurableCommitCallback);
                return (IntPtr.Zero == rawValue)
                    ? null
                    : (NATIVE_JET_PFNDURABLECOMMITCALLBACK)Marshal.GetDelegateForFunctionPointer(rawValue,
                        typeof(NATIVE_JET_PFNDURABLECOMMITCALLBACK));
            }
            set
            {
                IntPtr nativeDelegate = (null == value)
                    ? IntPtr.Zero
                    : Marshal.GetFunctionPointerForDelegate(value);
                NativeHelpers.SetParameter(_instance, JET_param.DurableCommitCallback, nativeDelegate);
            }
        }

        /// <summary>Gets or sets a value indicating whether the database engine accepts or
        /// rejects changes to a subset of the system parameters. This parameter is used in
        /// conjunction with Configuration to prevent some system parameters
        /// from being set away from the selected configuration's defaults.
        /// <para>Supported on Windows Vista and up. Ignored on Windows XP and Windows Server
        /// 2003.</para>
        /// </summary>
        public bool EnableAdvanced
        {
            get
            {
                return !EsentVersion.SupportsVistaFeatures
                    || NativeHelpers.GetBoolParameter(_instance, JET_param.EnableAdvanced);
            }
            set
            { 
                if (EsentVersion.SupportsVistaFeatures) {
                    NativeHelpers.SetParameter(_instance, JET_param.EnableAdvanced, value);
                }
            }
        }
        /// <summary>Gets or sets a value indicating whether Database Maintenance should run during recovery.</summary>
        public int EnableDbScanInRecovery
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.EnableDbScanInRecovery); }
            set { NativeHelpers.SetParameter(_instance, JET_param.EnableDbScanInRecovery, value); }
        }

        /// <summary>Gets or sets a value indicating whether database Maintenance serialization
        /// is enabled for databases sharing the same disk.</summary>
        public bool EnableDBScanSerialization
        {
            get { return NativeHelpers.GetBoolParameter(_instance, JET_param.EnableDBScanSerialization); }
            set { NativeHelpers.SetParameter(_instance, JET_param.EnableDBScanSerialization, value); }
        }

        /// <summary>Gets or sets a value indicating whether the database engine should use
        /// the OS file cache for all managed files.</summary>
        public bool EnableFileCache
        {
            get
            {
                return EsentVersion.SupportsVistaFeatures
                    && NativeHelpers.GetBoolParameter(_instance, JET_param.EnableFileCache);
            }
            set
            {
                if (EsentVersion.SupportsVistaFeatures) {
                    NativeHelpers.SetParameter(_instance, JET_param.EnableFileCache, value);
                }
            }
        }

        /// <summary>Gets or sets a value indicating whether <see cref="JetSession.AttachDatabase"/>
        /// will check for indexes that were build using an older version of the NLS library
        /// in the operating system.</summary>
        public bool EnableIndexChecking
        {
            get { return NativeHelpers.GetBoolParameter(_instance, JET_param.EnableIndexChecking); }
            set { NativeHelpers.SetParameter(_instance, JET_param.EnableIndexChecking, value); }
        }

        /// <summary>Gets or sets a value indicating whether online defragmentation is enabled.</summary>
        public bool EnableOnlineDefrag
        {
            get { return NativeHelpers.GetBoolParameter(_instance, JET_param.EnableOnlineDefrag); }
            set { NativeHelpers.SetParameter(_instance, JET_param.EnableOnlineDefrag, value); }
        }

        /// <summary>Gets or sets whether to free space back to the OS after deleting data. This may free space
        /// in the middle of files (done in the units of database extents). This uses Sparse Files,
        /// which is available on NTFS and ReFS (not FAT). The exact method of releasing space is an
        /// implementation detail and is subject to change.</summary>
        public ShrinkDatabaseGrbit EnableShrinkDatabase
        {
            get { return (ShrinkDatabaseGrbit)NativeHelpers.GetInt32Parameter(_instance, JET_param.EnableShrinkDatabase); }
            set { NativeHelpers.SetParameter(_instance, JET_param.EnableShrinkDatabase, (int)value); }
        }

        /// <summary>Gets or sets a value indicating whether the database engine should use
        /// memory mapped file I/O for database files.</summary>
        public bool EnableViewCache
        {
            get
            {
                return (EsentVersion.SupportsVistaFeatures)
                    && NativeHelpers.GetBoolParameter(_instance, JET_param.EnableViewCache);
            }
            set
            {
                if (EsentVersion.SupportsVistaFeatures) {
                    NativeHelpers.SetParameter(_instance, JET_param.EnableViewCache, value);
                }
            }
        }

        /// <summary>Gets or sets the detail level of eventlog messages that are emitted to the
        /// eventlog by the database engine. Higher numbers will result in more detailed eventlog
        /// messages.</summary>
        public EventLoggingLevels EventLoggingLevel
        {
            get
            {
                return (EventLoggingLevels)NativeHelpers.GetInt32Parameter(_instance, JET_param.EventLoggingLevel);
            }
            set { NativeHelpers.SetParameter(_instance, JET_param.EventLoggingLevel, (int)value); }
        }

        /// <summary>Gets or sets an application specific string that will be added to any event
        /// log messages that are emitted by the database engine. This allows easy correlation of
        /// event log messages with the source application. By default the host application
        /// executable name will be used.</summary>
        public string EventSource
        {
            get { return NativeHelpers.GetStringParameter(_instance, JET_param.EventSource); }
            set { NativeHelpers.SetParameter(_instance, JET_param.EventSource, value); }
        }

        /// <summary>Gets or sets the name of the event log the database engine uses for its
        /// event log messages. By default, all event log messages will go to the Application
        /// event log. If the registry key name for another event log is configured then the
        /// event log messages will go there instead.</summary>  
        public string EventSourceKey
        {
            get { return NativeHelpers.GetStringParameter(_instance, JET_param.EventSourceKey); }
            set { NativeHelpers.SetParameter(_instance, JET_param.EventSourceKey, value); }
        }

        /// <summary></summary>
        public IntPtr Id
        {
            get { return _instance.Value; }
        }

        /// <summary>Gets or sets the amount of memory used to cache log records before they
        /// are written to the transaction log file. The unit for this parameter is the sector
        /// size of the volume that holds the transaction log files. The sector size is almost
        /// always 512 bytes, so it is safe to assume that size for the unit. This parameter
        /// has an impact on performance. When the database engine is under heavy update load,
        /// this buffer can become full very rapidly. A larger cache size for the transaction
        /// log file is critical for good update performance under such a high load condition.
        /// The default is known to be too small for this case.
        /// Do not set this parameter to a number of buffers that is larger (in bytes) than
        /// half the size of a transaction log file.</summary>
        public int LogBuffers
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.LogBuffers); }
            set { NativeHelpers.SetParameter(_instance, JET_param.LogBuffers, value); }
        }

        /// <summary>Gets or sets the relative or absolute file system path of the folder that
        /// will contain the transaction logs for the instance.</summary>
        public string LogFileDirectory
        {
            get
            {
                return Util.AddTrailingDirectorySeparator(
                    NativeHelpers.GetStringParameter(_instance, JET_param.LogFilePath));
            }
            set
            {
                NativeHelpers.SetParameter(_instance, JET_param.LogFilePath,
                    Util.AddTrailingDirectorySeparator(value));
            }
        }

        /// <summary>Gets or sets the size of the transaction log files. This parameter should be
        /// set in units of 1024 bytes (e.g. a setting of 2048 will give 2MB logfiles).</summary>
        public int LogFileSize
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.LogFileSize); }
            set { NativeHelpers.SetParameter(_instance, JET_param.LogFileSize, value); }
        }

        /// <summary>Gets or sets the number of cursor resources reserved for this instance.
        /// A cursor resource directly corresponds to a JET_TABLEID.</summary>
        public int MaxCursors
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.MaxCursors); }
            set { NativeHelpers.SetParameter(_instance, JET_param.MaxCursors, value); }
        }

        /// <summary>Gets or sets the number of B+ Tree resources reserved for this instance.</summary>
        public int MaxOpenTables
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.MaxOpenTables); }
            set { NativeHelpers.SetParameter(_instance, JET_param.MaxOpenTables, value); }
        }

        /// <summary>Gets or sets the number of sessions resources reserved for this instance.
        /// A session resource directly corresponds to a JET_SESID.</summary>
        public int MaxSessions
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.MaxSessions); }
            set { NativeHelpers.SetParameter(_instance, JET_param.MaxSessions, value); }
        }

        /// <summary>Gets or sets the number of temporary table resources for use by an instance.
        /// This setting will affect how many temporary tables can be used at the same time. If
        /// this system parameter is set to zero then no temporary database will be created and
        /// any activity that requires use of the temporary database will fail. This setting can
        /// be useful to avoid the I/O required to create the temporary database if it is known
        /// that it will not be used.</summary>
        /// <remarks>The use of a temporary table also requires a cursor resource.</remarks>
        public int MaxTemporaryTables
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.MaxTemporaryTables); }
            set { NativeHelpers.SetParameter(_instance, JET_param.MaxTemporaryTables, value); }
        }

        /// <summary>Gets or sets the percentage of version store that can be used by oldest transaction
        /// before <see cref="JET_err.VersionStoreOutOfMemory"/> (default = 100).</summary>
        public int MaxTransactionSize
        {
            get { return NativeHelpers.GetInt32Parameter(JET_param.MaxTransactionSize); }
            set { NativeHelpers.SetParameter(_instance, JET_param.MaxTransactionSize, value); }
        }

        /// <summary>Gets or sets the maximum number of version store pages reserved for this
        /// instance. </summary>
        public int MaxVerPages
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.MaxVerPages); }
            set { NativeHelpers.SetParameter(_instance, JET_param.MaxVerPages, value); }
        }

        public string Name { get; private set; }

        /// <summary>Gets or sets a value indicating whether informational event log messages
        /// that would ordinarily be generated by the database engine will be suppressed.</summary>
        public bool NoInformationEvent
        {
            get { return NativeHelpers.GetBoolParameter(_instance, JET_param.NoInformationEvent); }
            set { NativeHelpers.SetParameter(_instance, JET_param.NoInformationEvent, value); }
        }

        /// <summary>Gets or sets a value indicating whether only one database is allowed to
        /// be opened using JetOpenDatabase by a given session at one time. The temporary
        /// database is excluded from this restriction.</summary>
        public bool OneDatabasePerSession
        {
            get { return NativeHelpers.GetBoolParameter(_instance, JET_param.OneDatabasePerSession); }
            set { NativeHelpers.SetParameter(_instance, JET_param.OneDatabasePerSession, value); }
        }

        internal uint OverridenVersion
        {
            get { return versionOverride; }
        }

        /// <summary>Gets or sets the initial size of the temporary database. The size is in
        /// database pages. A size of zero indicates that the default size of an ordinary
        /// database should be used. It is often desirable for small applications to configure
        /// the temporary database to be as small as possible. Setting this parameter to
        /// <see cref="Constants.PageTempDBSmallest"/> will achieve the smallest temporary
        /// database possible.</summary>
        public int PageTempDBMin
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.PageTempDBMin); }
            set { NativeHelpers.SetParameter(_instance, JET_param.PageTempDBMin, value); }
        }

        /// <summary>Gets or sets the preferred number of version store pages reserved for this
        /// instance. If the size of the version store exceeds this threshold then any information
        /// that is only used for optional background tasks, such as reclaiming deleted space in
        /// the database, is instead sacrificed to preserve room for transactional information.</summary>
        public int PreferredVerPages
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.PreferredVerPages); }
            set { NativeHelpers.SetParameter(_instance, JET_param.PreferredVerPages, value); }
        }

        /// <summary>Gets or sets the maximum number of I/O operations dispatched for a given purpose.</summary>
        public int PrereadIOMax
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.PrereadIOMax); }
            set { NativeHelpers.SetParameter(_instance, JET_param.PrereadIOMax, value); }
        }

        /// <summary>Gets or sets a value indicating whether crash recovery is on.</summary>
        public bool Recovery
        {
            get
            {
                return (0 == string.Compare(NativeHelpers.GetStringParameter(_instance, JET_param.Recovery),
                    "on", StringComparison.OrdinalIgnoreCase));
            }
            set { NativeHelpers.SetParameter(_instance, JET_param.Recovery, (value) ? "on" : "off"); }
        }

        /// <summary>Gets or sets the relative or absolute file system path of the folder that
        /// will contain the checkpoint file for the instance.</summary>
        public string SystemDirectory
        {
            get
            {
                return Util.AddTrailingDirectorySeparator(
                    NativeHelpers.GetStringParameter(_instance, JET_param.SystemPath));
            }
            set
            {
                NativeHelpers.SetParameter(_instance, JET_param.SystemPath,
                    Util.AddTrailingDirectorySeparator(value));
            }
        }

        /// <summary>Gets or sets the relative or absolute file system path of the folder that
        /// will contain the temporary database for the instance.</summary>
        public string TempDirectory
        {
            get
            {
                // Older versions of Esent (e.g. Windows XP) will return the
                // full path of the temporary database. Extract the directory name.
                string path = NativeHelpers.GetStringParameter(_instance, JET_param.TempPath);
                string dir = Path.GetDirectoryName(path);
                return Util.AddTrailingDirectorySeparator(dir);
            }
            set
            {
                NativeHelpers.SetParameter(_instance, JET_param.TempPath,
                    Util.AddTrailingDirectorySeparator(value));
            }
        }

        public TermGrbit TerminationBits { get; private set; }

        ///// <summary>Get a collection of sessions that were born on the calling thread.</summary>
        //public IJetThreadBornSessionsCollection ThreadBornSessions
        //{
        //    get
        //    {
        //        int threadId = Thread.CurrentThread.ManagedThreadId;
        //        IJetThreadBornSessionsCollection result;
        //        return (_threadBornSessions.TryGetValue(threadId, out result)) ? result : null;
        //    }
        //}

        /// <summary>Gets or sets the the number of background cleanup work items that can be
        /// queued to the database engine thread pool at any one time.</summary>
        public int VersionStoreTaskQueueMax
        {
            get { return NativeHelpers.GetInt32Parameter(_instance, JET_param.VersionStoreTaskQueueMax); }
            set { NativeHelpers.SetParameter(_instance, JET_param.VersionStoreTaskQueueMax, value); }
        }

        /// <summary>Gets or sets a the number of logs that esent will defer database flushes for.
        /// This can be used to increase database recoverability if failures cause logfiles to be
        /// lost.<para>
        /// Supported on Windows 7 and up. Ignored on Windows XP, Windows Server 2003, Windows Vista
        /// and Windows Server 2008.</para></summary>
        public int WaypointLatency
        {
            get
            {
                return (EsentVersion.SupportsWindows7Features) 
                    ? NativeHelpers.GetInt32Parameter(_instance, JET_param.WaypointLatency)
                    : 0;
            }
            set
            {
                if (EsentVersion.SupportsWindows7Features) {
                    NativeHelpers.SetParameter(_instance, JET_param.WaypointLatency, value);
                }
            }
        }
        #endregion

        /// <summary>Performs a streaming backup of an instance, including all the attached
        /// databases, to a directory. With multiple backup methods supported by the engine,
        /// this is the simplest and most encapsulated function.</summary>
        /// <param name="destination">The directory where the backup is to be stored. If the
        /// backup path isnull to use the function will truncate the logs, if possible.</param>
        /// <param name="grbit">Backup options.</param>
        /// <param name="statusCallback">Optional status notification callback.</param>
        public void Backup(string destination, BackupGrbit grbit, JET_PFNSTATUS statusCallback)
        {
            Tracing.TraceFunctionCall("Backup");
            StatusCallbackWrapper callbackWrapper = new StatusCallbackWrapper(statusCallback);
            IntPtr functionPointer = (null == statusCallback)
                ? IntPtr.Zero
                : Marshal.GetFunctionPointerForDelegate(callbackWrapper.NativeCallback);
#if DEBUG
            GC.Collect();
#endif
            int returnCode = NativeMethods.JetBackupInstanceW(_instance.Value, destination,
                (uint)grbit, functionPointer);
            Tracing.TraceResult(returnCode);
            callbackWrapper.ThrowSavedException();
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Initialize a new ESENT session.</summary>
        /// <param name="username">The parameter is not used.</param>
        /// <param name="password">The parameter is not used.</param>
        /// <returns>A new session.</returns>
        /* <seealso cref="Api.BeginSession"/> */
        public IJetSession BeginSession(string username, string password)
        {
            Tracing.TraceFunctionCall("BeginSession");
            JET_SESID sessionId = JET_SESID.Nil;
            int returnCode = NativeMethods.JetBeginSession(_instance.Value, out sessionId.Value, username, password);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            JetSession result = new JetSession(this, sessionId);
            // TODO : Consider storing the associated thtrad identifier.
            int threadId = Thread.CurrentThread.ManagedThreadId;
            List<JetSession> thisThreadSessions;
            if (!_activeSessions.TryGetValue(threadId, out thisThreadSessions)) {
                thisThreadSessions = new List<JetSession>();
                _activeSessions.Add(threadId, thisThreadSessions);
            }
            thisThreadSessions.Add(result);
            _perSessionThreadId.Add(result, threadId);
            _perInterfaceSession.Add((IJetSession)result, result);
            return result;
        }

        /// <summary>Terminate an instance that was created with
        /// <see cref="JetInstance.Create(string,string,CreateInstanceGrbit)"/>.</summary>
        /// <param name="grbit">Termination options.</param>
        public void Close(TermGrbit grbit = TermGrbit.None)
        {
            Tracing.TraceFunctionCall("JetTerm");
            this.callbackWrappers.Collect();
            if (!_instance.IsInvalid) {
                int returnCode = (TermGrbit.None == grbit)
                    ? NativeMethods.JetTerm(_instance.Value)
                    : NativeMethods.JetTerm2(_instance.Value, (uint)grbit);
                Tracing.TraceResult(returnCode);
                EsentExceptionHelper.Check(returnCode);
            }
        }

        /// <summary>Ends an external backup session. This API is the last API in a series
        /// of APIs that must be called to execute a successful online (non-VSS based)
        /// backup.</summary>
        /// <param name="grbit">Options that specify how the backup ended.</param>
        public void CompleteBackup(EndExternalBackupGrbit grbit = EndExternalBackupGrbit.None)
        {
            Tracing.TraceFunctionCall("CompleteBackup");
            int returnCode = (EndExternalBackupGrbit.None == grbit)
                ? NativeMethods.JetEndExternalBackupInstance(_instance.Value)
                : NativeMethods.JetEndExternalBackupInstance2(_instance.Value, (uint)grbit);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Allocates a new instance of the database engine.</summary>
        /// <param name="name">The name of the instance. Names must be unique.</param>
        /// <param name="displayName">A display name for the instance to be created. This
        /// will be used in eventlog entries.</param>
        /// <param name="grbit">Creation options.</param>
        /// <returns>The new instance, otherwise throw an exception.</returns>
        [SecurityPermissionAttribute(SecurityAction.LinkDemand)]
        internal static JetInstance Create(string name, string displayName = null,
            CreateInstanceGrbit grbit = CreateInstanceGrbit.None)
        {
            // TOOD : Add TerminationBits initializer and associated parameter.
            Tracing.TraceFunctionCall("Create");
            JetInstance result = new JetInstance() {
                DisplayName = displayName,
                Name = name
            };
            result._instance.Value = IntPtr.Zero;
            RuntimeHelpers.PrepareConstrainedRegions();
            int nativeResult = 0;
            if (string.IsNullOrEmpty(displayName)
                && (CreateInstanceGrbit.None == grbit))
            {
                nativeResult = (JetEnvironment.Current.TrueCapabilities.SupportsUnicodePaths)
                    ? NativeMethods.JetCreateInstanceW(out result._instance.Value, name)
                    : NativeMethods.JetCreateInstance(out result._instance.Value, name);
            }
            else {
                nativeResult = (JetEnvironment.Current.TrueCapabilities.SupportsUnicodePaths)
                    ? NativeMethods.JetCreateInstance2W(out result._instance.Value, name,
                        displayName, (uint)grbit)
                    : NativeMethods.JetCreateInstance2(out result._instance.Value, name,
                        displayName, (uint)grbit);
            }
            Tracing.TraceResult(nativeResult);
            EsentExceptionHelper.Check(nativeResult);
            return result;
        }

        /// <summary>Allocate and immediately initialize an ESENT database engine. The instance
        /// will opérate in single instance mode.</summary>
        /// <returns>An error if the call fails.</returns>
        public static JetInstance CreateSingleInstanceMode()
        {
            Tracing.TraceFunctionCall("CreateSingleInstanceMode");
            JET_INSTANCE hInstance = new JET_INSTANCE();
            int returnCode = NativeMethods.JetInit(ref hInstance.Value);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return new JetInstance() { _instance = hInstance };
        }

        /// <summary></summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary></summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (disposing) { GC.SuppressFinalize(this); }
            if (!_instance.IsNil) {
                Close();
            }
        }

        /// <summary>Used during a backup initiated by <see cref="IJetInstance.PrepareBackup"/>
        /// to query an instance for the names of database files that should become part of
        /// the backup file set. Only databases that are currently attached to the instance
        /// using <see cref="JetSession.AttachDatabase"/> will be considered. These files may
        /// subsequently be opened using <see cref="IJetBackupInstance.OpenFile"/> and read
        /// using <see cref="JET_HANDLE.Read"/>.</summary>
        /// <remarks>It is important to note that this API does not return an error or warning
        /// if the output buffer is too small to accept the full list of files that should be
        /// part of the backup file set. </remarks>
        /// <returns>Returns a list of strings describing the set of database files that should
        /// be a part of the backup file set. The list of strings returned .</returns>
        public List<string> GetBackupFiles()
        {
            Tracing.TraceFunctionCall("GetBackupFiles");
            // Strings have embedded nulls so we can't use a StringBuilder.
            while (true) {
                uint bytesMax = 4096;
                byte[] tempBuffer = new byte[bytesMax];
                uint bytesActual = 0;
                int returnCode = NativeMethods.JetGetAttachInfoInstanceW(_instance.Value, tempBuffer,
                    bytesMax, out bytesActual);
                Tracing.TraceResult(returnCode);
                if (bytesActual >= bytesMax) {
                    bytesMax *= 2;
                    continue;
                }
                EsentExceptionHelper.Check(returnCode);
                string decoded = Encoding.Unicode.GetString(tempBuffer, 0, (int)bytesActual);
                return new List<string>(decoded.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        /// <summary>Used during a backup initiated by <see cref="IJetInstance.PrepareBackup"/>
        /// to query an instance for the names of database patch files and logfiles that should
        /// become part of the backup file set. These files may subsequently be opened using
        /// <see cref="IJetBackupInstance.OpenFile"/> and read using <see cref="JET_HANDLE.Read"/>.
        /// </summary>
        /// <returns>Returns a list of strings describing the set of database files that should
        /// be a part of the backup file set. The list of strings returned .</returns>
        public List<string> GetBackupLogFiles()
        {
            Tracing.TraceFunctionCall("GetBackupLogFiles");
            // Strings have embedded nulls so we can't use a StringBuilder.
            while (true) {
                uint bytesMax = 4096;
                byte[] tempBuffer = new byte[bytesMax];
                uint bytesActual = 0;
                int returnCode = NativeMethods.JetGetLogInfoInstanceW(_instance.Value, tempBuffer,
                    bytesMax, out bytesActual);
                Tracing.TraceResult(returnCode);
                if (bytesActual >= bytesMax) {
                    bytesMax *= 2;
                    continue;
                }
                EsentExceptionHelper.Check(returnCode);
                string decoded = Encoding.Unicode.GetString(tempBuffer, 0, (int)bytesActual);
                return new List<string>(decoded.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        /// <summary>Retrieves information about an instance.</summary>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error code if the call fails.</returns>
        public JET_SIGNATURE GetInfo(JET_InstanceMiscInfo infoLevel)
        {
            Tracing.TraceFunctionCall("GetInfo");
            _capabilities.CheckSupportsVistaFeatures("JetGetInstanceMiscInfo");

            NATIVE_SIGNATURE nativeSignature = new NATIVE_SIGNATURE();
            int returnCode = NativeMethods.JetGetInstanceMiscInfo(_instance.Value, ref nativeSignature,
                checked((uint)NATIVE_SIGNATURE.Size), unchecked((uint)infoLevel));
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return new JET_SIGNATURE(nativeSignature);
        }

        /// <summary>Used during a backup initiated by <see cref="IJetInstance.PrepareBackup"/>
        /// to query an instance for the names of the transaction log files that can be safely
        /// deleted after the backup has successfully completed.</summary>
        /// <remarks>It is important to note that this API does not return an error or warning
        /// if the output buffer is too small to accept the full list of files that should be
        /// part of the backup file set.</remarks>
        /// <returns>Returns a list of strings describing the set of files that are ready for
        /// truncation.</returns>
        public List<string> GetBackupTruncateReadyLogFiles()
        {
            Tracing.TraceFunctionCall("GetBackupTruncateReadyLogFiles");
            // Strings have embedded nulls so we can't use a StringBuilder.
            while (true) {
                uint bytesMax = 4096;
                byte[] tempBuffer = new byte[bytesMax];
                uint bytesActual = 0;
                int returnCode = NativeMethods.JetGetTruncateLogInfoInstanceW(_instance.Value, tempBuffer,
                    bytesMax, out bytesActual);
                Tracing.TraceResult(returnCode);
                if (bytesActual >= bytesMax) {
                    bytesMax *= 2;
                    continue;
                }
                EsentExceptionHelper.Check(returnCode);
                string decoded = Encoding.Unicode.GetString(tempBuffer, 0, (int)bytesActual);
                return new List<string>(decoded.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        internal static JetSession GetSession(IJetSession candidate)
        {
            JetSession result;
            if (_perInterfaceSession.TryGetValue(candidate, out result)) { return result; }
            throw new ApplicationException();
        }

        /// <summary>Retrieves the version of the database engine.</summary>
        public uint GetVersion()
        {
            Tracing.TraceFunctionCall("GetVersion");
            uint result = OverridenVersion;
            if (0 != result) {
                // We have an explicitly set version
                Tracing.TraceVerboseLine(string.Format(CultureInfo.InvariantCulture,
                    "GetVersion overridden with 0x{0:X}", result));
                return result;
            }
            if (0 == _version) {
                // Get the version from Esent
                using (IJetSession session = this.BeginSession(string.Empty, string.Empty)) {
                    int returnCode = NativeMethods.JetGetVersion(Id, out _version);
                    Tracing.TraceResult(returnCode);
                    EsentExceptionHelper.Check(returnCode);
                }
            }
            return _version;
        }

        /// <summary>Initialize the ESENT database engine.</summary>
        /// <param name="grbit">Initialization options.</param>
        /// <param name="recoveryOptions">Additional recovery parameters for remapping
        /// databases during recovery, position where to stop recovery at, or recovery
        /// status.</param>
        /// <returns>An error if the call fails.</returns>
        public void Initialize(InitGrbit grbit = InitGrbit.None,
            JET_RSTINFO recoveryOptions = null)
        {
            Tracing.TraceFunctionCall("Initialize");
            int returnCode;
            if (null == recoveryOptions) {
                returnCode = (InitGrbit.None == grbit)
                    ? NativeMethods.JetInit(ref _instance.Value)
                    : NativeMethods.JetInit2(ref _instance.Value, (uint)grbit);
            }
            else {
                _capabilities.CheckSupportsVistaFeatures("JetInit3");
                if (null != recoveryOptions) {
                    StatusCallbackWrapper callbackWrapper =
                        new StatusCallbackWrapper(recoveryOptions.pfnStatus);
                    NATIVE_RSTINFO rstinfo = recoveryOptions.GetNativeRstinfo();
                    unsafe {
                        int numMaps = (null == recoveryOptions.rgrstmap)
                            ? 0
                            : recoveryOptions.rgrstmap.Length;
                        try {
                            NATIVE_RSTMAP* maps = stackalloc NATIVE_RSTMAP[numMaps];
                            if (0 < numMaps) {
                                rstinfo.rgrstmap = maps;
                                for (int i = 0; i < numMaps; ++i) {
                                    rstinfo.rgrstmap[i] = recoveryOptions.rgrstmap[i].GetNativeRstmap();
                                }
                            }
                            rstinfo.pfnStatus = callbackWrapper.NativeCallback;
                            returnCode = NativeMethods.JetInit3W(ref _instance.Value,
                                ref rstinfo, (uint)grbit);
                            callbackWrapper.ThrowSavedException();
                        }
                        finally {
                            if (null != rstinfo.rgrstmap) {
                                for (int i = 0; i < numMaps; ++i) {
                                    rstinfo.rgrstmap[i].FreeHGlobal();
                                }
                            }
                        }
                    }
                }
                else {
                    returnCode = NativeMethods.JetInit3W(ref _instance.Value, IntPtr.Zero,
                        (uint)grbit);
                }
            }
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        internal void NotifySessionClosed(JetSession session)
        {
            int threadId;
            if (!_perSessionThreadId.TryGetValue(session, out threadId)
                || !_perSessionThreadId.Remove(session)) {
                throw new ApplicationException();
            }
            List<JetSession> sessions;
            if (!_activeSessions.TryGetValue(threadId, out sessions)) {
                throw new ApplicationException();
            }
            if (!sessions.Remove(session)) {
                throw new ApplicationException();
            }
            if (0 == sessions.Count) {
                _activeSessions.Remove(threadId);
            }
            _perInterfaceSession.Remove((IJetSession)session);
            return;
        }

        /// <summary>Initiates an external backup while the engine and database are online
        /// and active.</summary>
        /// <param name="incremental">Optional : default is false. If true an inremental backup
        /// is initiated which means that only the log files since the last full or incremental
        /// backup will be backed up, as opposed to a full backup.</param>
        /// <returns>A <see cref="IJetBackupInstance"/> implementation.</returns>
        public IJetBackupInstance PrepareBackup(bool incremental = false)
        {
            Tracing.TraceFunctionCall("PrepareBackup");
            int returnCode = NativeMethods.JetBeginExternalBackupInstance(_instance.Value,
                (incremental) ? (uint)BeginExternalBackupGrbit.Incremental : 0);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return new JetBackupInstance();
        }

        /// <summary>Reports the exception to a central authority.</summary>
        /// <param name="exception">An unhandled exception.</param>
        /// <param name="description">A string description of the scenario.</param>
        internal static void ReportUnhandledException(Exception exception, string description)
        {
            return;
        }

        /// <summary>Restores and recovers a streaming backup of an instance including all the
        /// attached databases. It is designed to work with a backup created with the
        /// <see cref="JetInstance.Backup"/> function. This is the simplest and most encapsulated
        /// restore function.</summary>
        /// <param name="source">Location of the backup. The backup should have been created with
        /// <see cref="JetInstance.Backup"/>.</param>
        /// <param name="destination">Name of the folder where the database files from the backup set
        /// will be copied and recovered. If this is set to null, the database files will be copied
        /// and recovered to their original location.</param>
        /// <param name="statusCallback">Optional status notification callback.</param>
        /// <returns>An error code.</returns>
        public void Restore(string source, string destination, JET_PFNSTATUS statusCallback)
        {
            Tracing.TraceFunctionCall("Restore");
            Helpers.CheckNotNull(source, "source");
            StatusCallbackWrapper callbackWrapper = new StatusCallbackWrapper(statusCallback);
            IntPtr functionPointer = (null == statusCallback)
                ? IntPtr.Zero
                : Marshal.GetFunctionPointerForDelegate(callbackWrapper.NativeCallback);
#if DEBUG
            GC.Collect();
#endif
            int returnCode = NativeMethods.JetRestoreInstanceW(_instance.Value, source, destination,
                functionPointer);
            Tracing.TraceResult(returnCode);
            callbackWrapper.ThrowSavedException();
            EsentExceptionHelper.Check(returnCode);
            return;
        }

        /// <summary>Sets database configuration options.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="paramValue">The value of the parameter to set, if the parameter
        /// is a JET_CALLBACK.</param>
        /// <param name="paramString">The value of the parameter to set, if the parameter
        /// is a string type.</param>
        /// <returns>An ESENT warning code.</returns>
        internal void SetSystemParameter(JET_SESID sesid, JET_param paramid,
            JET_CALLBACK paramValue, string paramString)
        {
            // TODO : Make this usable without an instance to set the parameter on every
            // instance.
            this.SetSystemParameter(sesid, paramid, paramValue, paramString);
        }

        /// <summary>Sets database configuration options.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="paramValue">The value of the parameter to set, if the parameter is an integer type.</param>
        /// <param name="paramString">The value of the parameter to set, if the parameter is a string type.</param>
        /// <returns>An error or warning.</returns>
        internal void SetSystemParameter(JET_SESID sesid, JET_param paramid, IntPtr paramValue,
            string paramString)
        {
            // TODO : Make this usable without an instance to set the parameter on every
            // instance.
            Tracing.TraceFunctionCall("SetSystemParameter");
            unsafe {
                int returnCode = 0;
                fixed (IntPtr* pinstance = &_instance.Value) {
                    returnCode = NativeMethods.JetSetSystemParameterW(
                        (IntPtr.Zero == this._instance.Value) ? null : pinstance,
                        sesid.Value, (uint)paramid, paramValue, paramString);
                }
                Tracing.TraceResult(returnCode);
                EsentExceptionHelper.Check(returnCode);
            }
        }

        /// <summary>Prepares an instance for termination.</summary>
        /// <param name="grbit">The options to stop or resume the instance.</param>
        public void Stop(StopServiceGrbit grbit = StopServiceGrbit.All)
        {
            Tracing.TraceFunctionCall("Stop");
            int returnCode;
            if (StopServiceGrbit.All == grbit) {
                returnCode = NativeMethods.JetStopServiceInstance(_instance.Value);
            }
            else {
                _capabilities.CheckSupportsWindows8Features("JetStopServiceInstance2");
                returnCode = NativeMethods.JetStopServiceInstance2(_instance.Value,
                    unchecked((uint)grbit));
            }
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Prevents streaming backup-related activity from continuing on a specific
        /// running instance, thus ending the streaming backup in a predictable way.</summary>
        public void StopBackup()
        {
            Tracing.TraceFunctionCall("StopBackup");
            int returnCode = NativeMethods.JetStopBackupInstance(_instance.Value);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Used during a backup initiated by PrepareBackup to delete any transaction
        /// log files that will no longer be needed once the current backup completes
        /// successfully.</summary>
        public void TruncateBackupLogs()
        {
            Tracing.TraceFunctionCall("TruncateBackupLogs");
            int returnCode = NativeMethods.JetTruncateLogInstance(_instance.Value);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return;
        }

        // ----------------------------------------------------------------------------- //
        // ----------------------------------------------------------------------------- //
        // ----------------------------------------------------------------------------- //
        // ----------------------------------------------------------------------------- //
        // ----------------------------------------------------------------------------- //

        /// <summary>Sets database configuration options. This overload is used when the
        /// parameter being set is of type JET_CALLBACK.</summary>
        /// <param name="instance">The instance to set the option on or <see cref="JET_INSTANCE.Nil"/>
        /// to set the option on all instances.</param>
        /// <param name="sesid">The session to use.</param>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="paramValue">The value of the parameter to set.</param>
        /// <param name="paramString">The value of the string parameter to set.</param>
        /// <returns>An error or warning.</returns>
        public int JetSetSystemParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid, JET_CALLBACK paramValue, string paramString)
        {
            Tracing.TraceFunctionCall("JetSetSystemParameter");
            unsafe {
                // We are interested in the callback, not the string so we always use the ASCII API.
                IntPtr* pinstance = (IntPtr.Zero == instance.Value) ? null : &instance.Value;
                if (null == paramValue) {
                    return Tracing.TraceResult(
                        NativeMethods.JetSetSystemParameter(pinstance, sesid.Value, (uint)paramid,
                        IntPtr.Zero, paramString));
                }
                JetCallbackWrapper wrapper = this.callbackWrappers.Add(paramValue);
                this.callbackWrappers.Collect();
                IntPtr functionPointer = Marshal.GetFunctionPointerForDelegate(wrapper.NativeCallback);
#if DEBUG
                GC.Collect();
#endif // DEBUG
                return Tracing.TraceResult(
                    NativeMethods.JetSetSystemParameter(pinstance, sesid.Value, (uint)paramid,
                    functionPointer, paramString));
            }
        }

        #region Databases


        /// <summary>
        /// Creates and attaches a database file with a maximum database size specified.
        /// <seealso cref="JetAttachDatabase2"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="database">The path to the database file to create.</param>
        /// <param name="maxPages">
        /// The maximum size, in database pages, of the database. Passing 0 means there is
        /// no enforced maximum.
        /// </param>
        /// <param name="dbid">Returns the dbid of the new database.</param>
        /// <param name="grbit">Database creation options.</param>
        /// <returns>An error or warning.</returns>
        public int JetCreateDatabase2(JET_SESID sesid, string database, int maxPages, out JET_DBID dbid, CreateDatabaseGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetCreateDatabase2");
            Helpers.CheckNotNull(database, "database");
            Helpers.CheckNotNegative(maxPages, "maxPages");

            dbid = JET_DBID.Nil;
            uint cpgDatabaseSizeMax = checked((uint)maxPages);
            if (_capabilities.SupportsUnicodePaths) {
                return Tracing.TraceResult(NativeMethods.JetCreateDatabase2W(sesid.Value, database, cpgDatabaseSizeMax, out dbid.Value, (uint)grbit));
            }

            return Tracing.TraceResult(NativeMethods.JetCreateDatabase2(sesid.Value, database, cpgDatabaseSizeMax, out dbid.Value, (uint)grbit));
        }

        /// <summary>
        /// Attaches a database file for use with a database instance. In order to use the
        /// database, it will need to be subsequently opened with <see cref="JetSession.OpenDatabase"/>.
        /// <seealso cref="JetCreateDatabase2"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="database">The database to attach.</param>
        /// <param name="maxPages">
        /// The maximum size, in database pages, of the database. Passing 0 means there is
        /// no enforced maximum.
        /// </param>
        /// <param name="grbit">Attach options.</param>
        /// <returns>An error or warning.</returns>
        public int JetAttachDatabase2(JET_SESID sesid, string database, int maxPages, AttachDatabaseGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetAttachDatabase2");
            Helpers.CheckNotNull(database, "database");
            Helpers.CheckNotNegative(maxPages, "maxPages");

            if (_capabilities.SupportsUnicodePaths)
            {
                return Tracing.TraceResult(NativeMethods.JetAttachDatabase2W(sesid.Value, database, checked((uint)maxPages), (uint)grbit));
            }

            return Tracing.TraceResult(NativeMethods.JetAttachDatabase2(sesid.Value, database, checked((uint)maxPages), (uint)grbit));
        }

        #endregion

        #region Tables

        /// <summary>
        /// Duplicates an open cursor and returns a handle to the duplicated cursor.
        /// If the cursor that was duplicated was a read-only cursor then the
        /// duplicated cursor is also a read-only cursor.
        /// Any state related to constructing a search key or updating a record is
        /// not copied into the duplicated cursor. In addition, the location of the
        /// original cursor is not duplicated into the duplicated cursor. The
        /// duplicated cursor is always opened on the clustered index and its
        /// location is always on the first row of the table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to duplicate.</param>
        /// <param name="newTableid">The duplicated cursor.</param>
        /// <param name="grbit">Reserved for future use.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetDupCursor(JET_SESID sesid, JET_TABLEID tableid, out JET_TABLEID newTableid, DupCursorGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetDupCursor");
            newTableid = JET_TABLEID.Nil;
            return Tracing.TraceResult(NativeMethods.JetDupCursor(sesid.Value, tableid.Value, out newTableid.Value, (uint)grbit));
        }

        /// <summary>
        /// Enables the application to associate a context handle known as
        /// Local Storage with a cursor or the table associated with that
        /// cursor. This context handle can be used by the application to
        /// store auxiliary data that is associated with a cursor or table.
        /// The application is later notified using a runtime callback when
        /// the context handle must be released. This makes it possible to
        /// associate dynamically allocated state with a cursor or table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to use.</param>
        /// <param name="ls">The context handle to be associated with the session or cursor.</param>
        /// <param name="grbit">Set options.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetSetLS(JET_SESID sesid, JET_TABLEID tableid, JET_LS ls, LsGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetSetLS");
            return Tracing.TraceResult(NativeMethods.JetSetLS(sesid.Value, tableid.Value, ls.Value, (uint)grbit));
        }

        /// <summary>
        /// Enables the application to retrieve the context handle known
        /// as Local Storage that is associated with a cursor or the table
        /// associated with that cursor. This context handle must have been
        /// previously set using <see cref="JetSetLS"/>. JetGetLS can also
        /// be used to simultaneously fetch the current context handle for
        /// a cursor or table and reset that context handle.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to use.</param>
        /// <param name="ls">Returns the retrieved context handle.</param>
        /// <param name="grbit">Retrieve options.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetGetLS(JET_SESID sesid, JET_TABLEID tableid, out JET_LS ls, LsGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetGetLS");
            IntPtr native;
            int err = NativeMethods.JetGetLS(sesid.Value, tableid.Value, out native, (uint)grbit);
            ls = new JET_LS { Value = native };
            return Tracing.TraceResult(err);
        }
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
        //public int JetCreateIndex2(
        //    JET_SESID sesid,
        //    JET_TABLEID tableid,
        //    JET_INDEXCREATE[] indexcreates,
        //    int numIndexCreates)
        //{
        //    Tracing.TraceFunctionCall("JetCreateIndex2");
        //    Helpers.CheckNotNull(indexcreates, "indexcreates");
        //    Helpers.CheckNotNegative(numIndexCreates, "numIndexCreates");
        //    if (numIndexCreates > indexcreates.Length)
        //    {
        //        throw new ArgumentOutOfRangeException(
        //            "numIndexCreates", numIndexCreates, "numIndexCreates is larger than the number of indexes passed in");
        //    }

        //    // NOTE: Don't call CreateIndexes3() on Win8. Unlike other APIs, CreateIndexes3() is
        //    // not a superset. It requires locale names, and if the user called JetCreateIndex2(),
        //    // the input will likely have LCIDs.
        //    if (_capabilities.SupportsWindows7Features)
        //    {
        //        return CreateIndexes2(sesid, tableid, indexcreates, numIndexCreates);
        //    }

        //    if (_capabilities.SupportsVistaFeatures)
        //    {
        //        return CreateIndexes1(sesid, tableid, indexcreates, numIndexCreates);
        //    }

        //    return CreateIndexes(sesid, tableid, indexcreates, numIndexCreates);
        //}

        /// <summary>Creates a temporary table with a single index. A temporary table stores and
        /// retrieves records just like an ordinary table created using JetCreateTableColumnIndex.
        /// However, temporary tables are much faster than ordinary tables due to their volatile
        /// nature. They can also be used to very quickly sort and perform duplicate removal on
        /// record sets when accessed in a purely sequential manner.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="columns">Column definitions for the columns created in the temporary
        /// table.</param>
        /// <param name="numColumns">Number of column definitions.</param>
        /// <param name="lcid">The locale ID to use to compare any Unicode key column data in the
        /// temporary table. Any locale may be used as long as the appropriate language pack has
        /// been installed on the machine.</param>
        /// <param name="grbit">Table creation options.</param>
        /// <param name="tableid">Returns the tableid of the temporary table. Closing this tableid
        /// frees the resources associated with the temporary table.</param>
        /// <param name="columnids">The output buffer that receives the array of column IDs generated
        /// during the creation of the temporary table. The column IDs in this array will exactly
        /// correspond to the input array of column definitions. As a result, the size of this buffer
        /// must correspond to the size of the input array.</param>
        /// <returns>An error code.</returns>
        public int JetOpenTempTable2(JET_SESID sesid, JET_COLUMNDEF[] columns, int numColumns,
            int lcid, TemporaryTableCreationFlags grbit, out JET_TABLEID tableid, JET_COLUMNID[] columnids)
        {
            Tracing.TraceFunctionCall("JetOpenTempTable2");
            Helpers.CheckNotNull(columns, "columnns");
            Helpers.CheckDataSize(columns, numColumns, "numColumns");
            Helpers.CheckNotNull(columnids, "columnids");
            Helpers.CheckDataSize(columnids, numColumns, "numColumns");

            tableid = JET_TABLEID.Nil;

            NATIVE_COLUMNDEF[] nativecolumndefs = columns.GetNativecolumndefs();
            var nativecolumnids = new uint[numColumns];
            int err = Tracing.TraceResult(NativeMethods.JetOpenTempTable2(
                sesid.Value, nativecolumndefs, checked((uint)numColumns), (uint)lcid, (uint)grbit, out tableid.Value, nativecolumnids));
            columns.SetColumnids(columnids, nativecolumnids);
            return err;
        }

        /// <summary>Creates a temporary table with a single index. A temporary table stores
        /// and retrieves records just like an ordinary table created using JetCreateTableColumnIndex.
        /// However, temporary tables are much faster than ordinary tables due to their volatile
        /// nature. They can also be used to very quickly sort and perform duplicate removal on
        /// record sets when accessed in a purely sequential manner.</summary>
        /// <remarks>Introduced in Windows Vista.</remarks>
        /// <param name="sesid">The session to use.</param>
        /// <param name="temporarytable">Description of the temporary table to create on input.
        /// After a successful call, the structure contains the handle to the temporary table
        /// and column identifications.</param>
        /// <returns>An error code.</returns>
        public int JetOpenTemporaryTable(JET_SESID sesid, JET_OPENTEMPORARYTABLE temporarytable)
        {
            Tracing.TraceFunctionCall("JetOpenTemporaryTable");
            _capabilities.CheckSupportsVistaFeatures("JetOpenTemporaryTable");
            Helpers.CheckNotNull(temporarytable, "temporarytable");

            NATIVE_OPENTEMPORARYTABLE nativetemporarytable = temporarytable.GetNativeOpenTemporaryTable();
            var nativecolumnids = new uint[nativetemporarytable.ccolumn];
            NATIVE_COLUMNDEF[] nativecolumndefs = temporarytable.prgcolumndef.GetNativecolumndefs();
            unsafe {
                using (var gchandlecollection = new GCHandleCollection()) {
                    // Pin memory
                    nativetemporarytable.prgcolumndef = (NATIVE_COLUMNDEF*)gchandlecollection.Add(nativecolumndefs);
                    nativetemporarytable.rgcolumnid = (uint*)gchandlecollection.Add(nativecolumnids);
                    if (null != temporarytable.pidxunicode) {
                        nativetemporarytable.pidxunicode = (NATIVE_UNICODEINDEX*)
                            gchandlecollection.Add(temporarytable.pidxunicode.GetNativeUnicodeIndex());
                    }

                    // Call the interop method
                    int err = Tracing.TraceResult(NativeMethods.JetOpenTemporaryTable(sesid.Value,
                        ref nativetemporarytable));
                    // Convert the return values
                    temporarytable.prgcolumndef.SetColumnids(temporarytable.prgcolumnid, nativecolumnids);
                    temporarytable.tableid = new JET_TABLEID { Value = nativetemporarytable.tableid };
                    return err;
                }
            }
        }

        // NOT IMPLEMENTED
        ///// <summary>
        ///// Creates a table, adds columns, and indices on that table.
        ///// </summary>
        ///// <param name="sesid">The session to use.</param>
        ///// <param name="dbid">The database to which to add the new table.</param>
        ///// <param name="tablecreate">Object describing the table to create.</param>
        ///// <returns>An error if the call fails.</returns>
        //public int JetCreateTableColumnIndex3(
        //    JET_SESID sesid,
        //    JET_DBID dbid,
        //    JET_TABLECREATE tablecreate)
        //{
        //    Tracing.TraceFunctionCall("JetCreateTableColumnIndex3");
        //    Helpers.CheckNotNull(tablecreate, "tablecreate");

        //    if (_capabilities.SupportsWindows7Features)
        //    {
        //        return CreateTableColumnIndex3(sesid, dbid, tablecreate);
        //    }

        //    return this.CreateTableColumnIndex2(sesid, dbid, tablecreate);
        //}

        #region JetGetTableColumnInfo overloads

        /// <summary>
        /// Retrieves information about a table column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columndef">Filled in with information about the column.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetGetTableColumnInfo(
                JET_SESID sesid,
                JET_TABLEID tableid,
                string columnName,
                out JET_COLUMNDEF columndef)
        {
            Tracing.TraceFunctionCall("JetGetTableColumnInfo");
            columndef = new JET_COLUMNDEF();
            Helpers.CheckNotNull(columnName, "columnName");

            var nativeColumndef = new NATIVE_COLUMNDEF();
            nativeColumndef.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_COLUMNDEF)));
            int err = Tracing.TraceResult(NativeMethods.JetGetTableColumnInfoW(sesid.Value,
                tableid.Value, columnName, ref nativeColumndef, nativeColumndef.cbStruct,
                (uint)JET_ColInfo.Default));
            columndef.SetFromNativeColumndef(nativeColumndef);
            return err;
        }

        /// <summary>
        /// Retrieves information about a table column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="columnid">The columnid of the column.</param>
        /// <param name="columndef">Filled in with information about the column.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetGetTableColumnInfo(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid,
            out JET_COLUMNDEF columndef)
        {
            Tracing.TraceFunctionCall("JetGetTableColumnInfo");
            columndef = new JET_COLUMNDEF();
            NATIVE_COLUMNDEF nativeColumndef = new NATIVE_COLUMNDEF();
            nativeColumndef.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_COLUMNDEF)));
            int err = Tracing.TraceResult(
                NativeMethods.JetGetTableColumnInfoW(sesid.Value, tableid.Value, ref columnid.Value,
                    ref nativeColumndef, nativeColumndef.cbStruct, (uint)JET_ColInfo.ByColid));
            columndef.SetFromNativeColumndef(nativeColumndef);
            return err;
        }

        /// <summary>Retrieves information about a table column, given its <see cref="JET_TABLEID"/> and name.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columnbase">Filled in with information about the column.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetGetTableColumnInfo(JET_SESID sesid, JET_TABLEID tableid, string columnName,
            out JET_COLUMNBASE columnbase)
        {
            Tracing.TraceFunctionCall("JetGetTableColumnInfo");
            Helpers.CheckNotNull(columnName, "columnName");
            var nativeColumnbase = new NATIVE_COLUMNBASE_WIDE();
            nativeColumnbase.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_COLUMNBASE_WIDE)));
            int err = Tracing.TraceResult(
                NativeMethods.JetGetTableColumnInfoW(sesid.Value, tableid.Value, columnName,
                    ref nativeColumnbase, nativeColumnbase.cbStruct,
                    (uint)JET_ColInfo.Base));
            columnbase = new JET_COLUMNBASE(nativeColumnbase);
            return err;
        }

        /// <summary>Retrieves information about a table column, given its <see cref="JET_TABLEID"/> and <see cref="JET_COLUMNID"/>.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="columnid">The columnid of the column.</param>
        /// <param name="columnbase">Filled in with information about the column.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetGetTableColumnInfo(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid,
            out JET_COLUMNBASE columnbase)
        {
            Tracing.TraceFunctionCall("JetGetTableColumnInfo");
            _capabilities.CheckSupportsVistaFeatures("JetGetTableColumnInfo");
            var nativeColumnbase = new NATIVE_COLUMNBASE_WIDE();
            nativeColumnbase.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_COLUMNBASE_WIDE)));
            int err = Tracing.TraceResult(
                NativeMethods.JetGetTableColumnInfoW(sesid.Value, tableid.Value, ref columnid.Value,
                    ref nativeColumnbase, nativeColumnbase.cbStruct, (uint)JET_ColInfo.BaseByColid));
            columnbase = new JET_COLUMNBASE(nativeColumnbase);
            return err;
        }

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
        public int JetGetColumnInfo(
                JET_SESID sesid,
                JET_DBID dbid,
                string tablename,
                string columnName,
                out JET_COLUMNDEF columndef)
        {
            Tracing.TraceFunctionCall("JetGetColumnInfo");
            columndef = new JET_COLUMNDEF();
            Helpers.CheckNotNull(tablename, "tablename");
            Helpers.CheckNotNull(columnName, "columnName");
            int err;

            var nativeColumndef = new NATIVE_COLUMNDEF();
            nativeColumndef.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_COLUMNDEF)));

            // Technically, this should have worked in Vista. But there was a bug, and
            // it was fixed after Windows 7.
            if (_capabilities.SupportsWindows8Features)
            {
                err = Tracing.TraceResult(NativeMethods.JetGetColumnInfoW(
                        sesid.Value,
                        dbid.Value,
                        tablename,
                        columnName,
                        ref nativeColumndef,
                        nativeColumndef.cbStruct,
                        (uint)JET_ColInfo.Default));
            }
            else
            {
                err = Tracing.TraceResult(NativeMethods.JetGetColumnInfo(
                    sesid.Value,
                    dbid.Value,
                    tablename,
                    columnName,
                    ref nativeColumndef,
                    nativeColumndef.cbStruct,
                    (uint)JET_ColInfo.Default));
            }

            columndef.SetFromNativeColumndef(nativeColumndef);

            return err;
        }

        /// <summary>
        /// Retrieves information about all columns in a table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database that contains the table.</param>
        /// <param name="tablename">The name of the table containing the column.</param>
        /// <param name="ignored">This parameter is ignored.</param>
        /// <param name="columnlist">Filled in with information about the columns in the table.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetGetColumnInfo(
                JET_SESID sesid,
                JET_DBID dbid,
                string tablename,
                string ignored,
                out JET_COLUMNLIST columnlist)
        {
            Tracing.TraceFunctionCall("JetGetColumnInfo");
            columnlist = new JET_COLUMNLIST();
            Helpers.CheckNotNull(tablename, "tablename");
            int err;

            var nativeColumnlist = new NATIVE_COLUMNLIST();
            nativeColumnlist.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_COLUMNLIST)));

            // Technically, this should have worked in Vista. But there was a bug, and
            // it was fixed after Windows 7.
            if (_capabilities.SupportsWindows8Features)
            {
                err = Tracing.TraceResult(NativeMethods.JetGetColumnInfoW(
                    sesid.Value,
                    dbid.Value,
                    tablename,
                    ignored,
                    ref nativeColumnlist,
                    nativeColumnlist.cbStruct,
                    (uint)JET_ColInfo.List));
            }
            else
            {
                err = Tracing.TraceResult(NativeMethods.JetGetColumnInfo(
                    sesid.Value,
                    dbid.Value,
                    tablename,
                    ignored,
                    ref nativeColumnlist,
                    nativeColumnlist.cbStruct,
                    (uint)JET_ColInfo.List));
            }

            columnlist.SetFromNativeColumnlist(nativeColumnlist);

            return err;
        }

        /// <summary>
        /// Retrieves information about a column in a table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database that contains the table.</param>
        /// <param name="tablename">The name of the table containing the column.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columnbase">Filled in with information about the columns in the table.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetGetColumnInfo(
                JET_SESID sesid,
                JET_DBID dbid,
                string tablename,
                string columnName,
                out JET_COLUMNBASE columnbase)
        {
            Tracing.TraceFunctionCall("JetGetColumnInfo");
            Helpers.CheckNotNull(tablename, "tablename");
            Helpers.CheckNotNull(columnName, "columnName");
            int err;

            // Technically, this should have worked in Vista. But there was a bug, and
            // it was fixed after Windows 7.
            if (_capabilities.SupportsWindows8Features)
            {
                var nativeColumnbase = new NATIVE_COLUMNBASE_WIDE();
                nativeColumnbase.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_COLUMNBASE_WIDE)));

                err = Tracing.TraceResult(NativeMethods.JetGetColumnInfoW(
                    sesid.Value,
                    dbid.Value,
                    tablename,
                    columnName,
                    ref nativeColumnbase,
                    nativeColumnbase.cbStruct,
                    (uint)JET_ColInfo.Base));

                columnbase = new JET_COLUMNBASE(nativeColumnbase);
            }
            else
            {
                var nativeColumnbase = new NATIVE_COLUMNBASE();
                nativeColumnbase.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_COLUMNBASE)));

                err = Tracing.TraceResult(NativeMethods.JetGetColumnInfo(
                    sesid.Value,
                    dbid.Value,
                    tablename,
                    columnName,
                    ref nativeColumnbase,
                    nativeColumnbase.cbStruct,
                    (uint)JET_ColInfo.Base));

                columnbase = new JET_COLUMNBASE(nativeColumnbase);
            }

            return err;
        }

        /// <summary>
        /// Retrieves information about a column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database that contains the table.</param>
        /// <param name="tablename">The name of the table containing the column.</param>
        /// <param name="columnid">The columnid of the column.</param>
        /// <param name="columnbase">Filled in with information about the columns in the table.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetGetColumnInfo(JET_SESID sesid, JET_DBID dbid, string tablename,
                JET_COLUMNID columnid, out JET_COLUMNBASE columnbase)
        {
            Tracing.TraceFunctionCall("JetGetColumnInfo");
            _capabilities.CheckSupportsVistaFeatures("JetGetColumnInfo");
            Helpers.CheckNotNull(tablename, "tablename");
            int err;

            // Technically, this should have worked in Vista. But there was a bug, and
            // it was fixed after Windows 7.
            if (_capabilities.SupportsWindows8Features) {
                var nativeColumnbase = new NATIVE_COLUMNBASE_WIDE();
                nativeColumnbase.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_COLUMNBASE_WIDE)));
                err = Tracing.TraceResult(NativeMethods.JetGetColumnInfoW(sesid.Value, dbid.Value,
                    tablename, ref columnid.Value, ref nativeColumnbase, nativeColumnbase.cbStruct,
                    (uint)JET_ColInfo.BaseByColid));
                columnbase = new JET_COLUMNBASE(nativeColumnbase);
            }
            else {
                var nativeColumnbase = new NATIVE_COLUMNBASE();
                nativeColumnbase.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_COLUMNBASE)));
                err = Tracing.TraceResult(NativeMethods.JetGetColumnInfo(sesid.Value, dbid.Value,
                    tablename, ref columnid.Value, ref nativeColumnbase, nativeColumnbase.cbStruct,
                    (uint)JET_ColInfo.BaseByColid));
                columnbase = new JET_COLUMNBASE(nativeColumnbase);
            }
            return err;
        }

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
        public int JetGetObjectInfo(JET_SESID sesid, JET_DBID dbid, JET_objtyp objtyp,
            string objectName, out JET_OBJECTINFO objectinfo)
        {
            Tracing.TraceFunctionCall("JetGetObjectInfo");
            objectinfo = new JET_OBJECTINFO();
            var nativeObjectinfo = new NATIVE_OBJECTINFO();
            nativeObjectinfo.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_OBJECTINFO)));
            int err;
            if (_capabilities.SupportsVistaFeatures)
            {
                err = Tracing.TraceResult(NativeMethods.JetGetObjectInfoW(
                    sesid.Value,
                    dbid.Value,
                    (uint)objtyp,
                    null,
                    objectName,
                    ref nativeObjectinfo,
                    nativeObjectinfo.cbStruct,
                    (uint)JET_ObjInfo.NoStats));
            }
            else
            {
                err = Tracing.TraceResult(NativeMethods.JetGetObjectInfo(
                    sesid.Value,
                    dbid.Value,
                    (uint)objtyp,
                    null,
                    objectName,
                    ref nativeObjectinfo,
                    nativeObjectinfo.cbStruct,
                    (uint)JET_ObjInfo.NoStats));
            }

            objectinfo.SetFromNativeObjectinfo(ref nativeObjectinfo);
            return err;
        }

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
        public int JetGetTableInfo(JET_SESID sesid, JET_TABLEID tableid, out JET_OBJECTINFO result, JET_TblInfo infoLevel)
        {
            Tracing.TraceFunctionCall("JetGetTableInfo");
            var nativeResult = new NATIVE_OBJECTINFO();
            int err;

            if (_capabilities.SupportsVistaFeatures)
            {
                err = Tracing.TraceResult(
                    NativeMethods.JetGetTableInfoW(
                        sesid.Value,
                        tableid.Value,
                        out nativeResult,
                        checked((uint)Marshal.SizeOf(typeof(NATIVE_OBJECTINFO))),
                        (uint)infoLevel));
            }
            else
            {
#if MANAGEDESENT_ON_WSA
                err = Err((int)JET_err.FeatureNotAvailable);
#else
                err = Tracing.TraceResult(
                    NativeMethods.JetGetTableInfo(
                        sesid.Value,
                        tableid.Value,
                        out nativeResult,
                        checked((uint)Marshal.SizeOf(typeof(NATIVE_OBJECTINFO))),
                        (uint)infoLevel));
#endif
            }

            result = new JET_OBJECTINFO();
            result.SetFromNativeObjectinfo(ref nativeResult);
            return err;
        }

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
        public int JetGetTableInfo(JET_SESID sesid, JET_TABLEID tableid, out string result, JET_TblInfo infoLevel)
        {
            Tracing.TraceFunctionCall("JetGetTableInfo");
            var resultBuffer = new StringBuilder(Constants.NameMost + 1);
            int err;

            if (_capabilities.SupportsVistaFeatures)
            {
                err = Tracing.TraceResult(NativeMethods.JetGetTableInfoW(
                    sesid.Value, tableid.Value, resultBuffer, (uint)resultBuffer.Capacity * sizeof(char), (uint)infoLevel));
            }
            else
            {
#if MANAGEDESENT_ON_WSA
                err = Err((int)JET_err.FeatureNotAvailable);
#else
                err = Tracing.TraceResult(NativeMethods.JetGetTableInfo(
                    sesid.Value, tableid.Value, resultBuffer, (uint)resultBuffer.Capacity, (uint)infoLevel));
#endif
            }

            result = resultBuffer.ToString();
            result = StringCache.TryToIntern(result);
            return err;
        }

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
        public int JetGetTableInfo(JET_SESID sesid, JET_TABLEID tableid, out JET_DBID result, JET_TblInfo infoLevel)
        {
            Tracing.TraceFunctionCall("JetGetTableInfo");
            result = JET_DBID.Nil;

            if (_capabilities.SupportsVistaFeatures)
            {
                return Tracing.TraceResult(NativeMethods.JetGetTableInfoW(sesid.Value, tableid.Value, out result.Value, sizeof(uint), (uint)infoLevel));
            }
            else
            {
#if MANAGEDESENT_ON_WSA
                return Err((int)JET_err.FeatureNotAvailable);
#else
                return Tracing.TraceResult(NativeMethods.JetGetTableInfo(sesid.Value, tableid.Value, out result.Value, sizeof(uint), (uint)infoLevel));
#endif
            }
        }

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
        public int JetGetTableInfo(JET_SESID sesid, JET_TABLEID tableid, int[] result, JET_TblInfo infoLevel)
        {
            Tracing.TraceFunctionCall("JetGetTableInfo");
            Helpers.CheckNotNull(result, "result");

            uint bytesResult = checked((uint)(result.Length * sizeof(int)));
            if (_capabilities.SupportsVistaFeatures)
            {
                return Tracing.TraceResult(NativeMethods.JetGetTableInfoW(sesid.Value, tableid.Value, result, bytesResult, (uint)infoLevel));
            }
            else
            {
#if MANAGEDESENT_ON_WSA
                return Err((int)JET_err.FeatureNotAvailable);
#else
                return Tracing.TraceResult(NativeMethods.JetGetTableInfo(sesid.Value, tableid.Value, result, bytesResult, (uint)infoLevel));
#endif
            }
        }

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
        public int JetGetTableInfo(JET_SESID sesid, JET_TABLEID tableid, out int result, JET_TblInfo infoLevel)
        {
            Tracing.TraceFunctionCall("JetGetTableInfo");
            uint nativeResult;
            int err;
            if (_capabilities.SupportsVistaFeatures)
            {
                err = Tracing.TraceResult(NativeMethods.JetGetTableInfoW(sesid.Value, tableid.Value, out nativeResult, sizeof(uint), (uint)infoLevel));
            }
            else
            {
#if MANAGEDESENT_ON_WSA
                nativeResult = 0;
                err = Err((int)JET_err.FeatureNotAvailable);
#else
                err = Tracing.TraceResult(NativeMethods.JetGetTableInfo(sesid.Value, tableid.Value, out nativeResult, sizeof(uint), (uint)infoLevel));
#endif
            }

            result = unchecked((int)nativeResult);
            return err;
        }

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
        public int JetGetIndexInfo(
            JET_SESID sesid,
            JET_DBID dbid,
            string tablename,
            string indexname,
            out ushort result,
            JET_IdxInfo infoLevel)
        {
            Tracing.TraceFunctionCall("JetGetIndexInfo");
            Helpers.CheckNotNull(tablename, "tablename");
            int err;

            if (_capabilities.SupportsVistaFeatures)
            {
                err = Tracing.TraceResult(NativeMethods.JetGetIndexInfoW(
                    sesid.Value,
                    dbid.Value,
                    tablename,
                    indexname,
                    out result,
                    sizeof(ushort),
                    (uint)infoLevel));
            }
            else
            {
                err = Tracing.TraceResult(NativeMethods.JetGetIndexInfo(
                    sesid.Value,
                    dbid.Value,
                    tablename,
                    indexname,
                    out result,
                    sizeof(ushort),
                    (uint)infoLevel));
            }

            return err;
        }

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
        public int JetGetIndexInfo(
            JET_SESID sesid,
            JET_DBID dbid,
            string tablename,
            string indexname,
            out int result,
            JET_IdxInfo infoLevel)
        {
            Tracing.TraceFunctionCall("JetGetIndexInfo");
            Helpers.CheckNotNull(tablename, "tablename");

            uint nativeResult;
            int err;

            if (_capabilities.SupportsVistaFeatures)
            {
                err = Tracing.TraceResult(NativeMethods.JetGetIndexInfoW(
                    sesid.Value,
                    dbid.Value,
                    tablename,
                    indexname,
                    out nativeResult,
                    sizeof(uint),
                    (uint)infoLevel));
            }
            else
            {
                err = Tracing.TraceResult(NativeMethods.JetGetIndexInfo(
                    sesid.Value,
                    dbid.Value,
                    tablename,
                    indexname,
                    out nativeResult,
                    sizeof(uint),
                    (uint)infoLevel));
            }

            result = unchecked((int)nativeResult);
            return err;
        }

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
        public int JetGetIndexInfo(
            JET_SESID sesid,
            JET_DBID dbid,
            string tablename,
            string indexname,
            out JET_INDEXID result,
            JET_IdxInfo infoLevel)
        {
            Tracing.TraceFunctionCall("JetGetIndexInfo");
            Helpers.CheckNotNull(tablename, "tablename");

            int err;

            if (_capabilities.SupportsVistaFeatures)
            {
                err = Tracing.TraceResult(NativeMethods.JetGetIndexInfoW(
                    sesid.Value,
                    dbid.Value,
                    tablename,
                    indexname,
                    out result,
                    JET_INDEXID.SizeOfIndexId,
                    (uint)infoLevel));
            }
            else
            {
                err = Tracing.TraceResult(NativeMethods.JetGetIndexInfo(
                    sesid.Value,
                    dbid.Value,
                    tablename,
                    indexname,
                    out result,
                    JET_INDEXID.SizeOfIndexId,
                    (uint)infoLevel));
            }

            return err;
        }

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
        public int JetGetIndexInfo(
            JET_SESID sesid,
            JET_DBID dbid,
            string tablename,
            string indexname,
            out JET_INDEXLIST result,
            JET_IdxInfo infoLevel)
        {
            Tracing.TraceFunctionCall("JetGetIndexInfo");
            Helpers.CheckNotNull(tablename, "tablename");
            int err;

            var nativeIndexlist = new NATIVE_INDEXLIST();
            nativeIndexlist.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_INDEXLIST)));
            if (_capabilities.SupportsVistaFeatures)
            {
                err = Tracing.TraceResult(NativeMethods.JetGetIndexInfoW(
                    sesid.Value,
                    dbid.Value,
                    tablename,
                    indexname,
                    ref nativeIndexlist,
                    nativeIndexlist.cbStruct,
                    (uint)infoLevel));
            }
            else
            {
                err = Tracing.TraceResult(NativeMethods.JetGetIndexInfo(
                sesid.Value,
                dbid.Value,
                tablename,
                indexname,
                ref nativeIndexlist,
                nativeIndexlist.cbStruct,
                (uint)infoLevel));
            }

            result = new JET_INDEXLIST();
            result.SetFromNativeIndexlist(nativeIndexlist);

            return err;
        }

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
        public int JetGetIndexInfo(
            JET_SESID sesid,
            JET_DBID dbid,
            string tablename,
            string indexname,
            out string result,
            JET_IdxInfo infoLevel)
        {
            Tracing.TraceFunctionCall("JetGetIndexInfo");
            Helpers.CheckNotNull(tablename, "tablename");
            int err;

            // Will need to check for Windows 8 Features.
            //
            // Currently only JET_IdxInfo.LocaleName is supported.
            uint bytesMax = checked((uint)Constants.LocaleNameMaxLength * sizeof(char));

            var stringBuilder = new StringBuilder(Constants.LocaleNameMaxLength);
            err = Tracing.TraceResult(NativeMethods.JetGetIndexInfoW(
                sesid.Value,
                dbid.Value,
                tablename,
                indexname,
                stringBuilder,
                bytesMax,
                (uint)infoLevel));

            result = stringBuilder.ToString();
            result = StringCache.TryToIntern(result);
            return err;
        }

        #endregion

        #region JetGetTableIndexInfo overloads

        /// <summary>
        /// Retrieves information about indexes on a table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetGetTableIndexInfo(
            JET_SESID sesid,
            JET_TABLEID tableid,
            string indexname,
            out ushort result,
            JET_IdxInfo infoLevel)
        {
            Tracing.TraceFunctionCall("JetGetTableIndexInfo");
            int err;

            if (_capabilities.SupportsVistaFeatures)
            {
                err = Tracing.TraceResult(NativeMethods.JetGetTableIndexInfoW(
                    sesid.Value,
                    tableid.Value,
                    indexname,
                    out result,
                    sizeof(ushort),
                    (uint)infoLevel));
            }
            else
            {
                err = Tracing.TraceResult(NativeMethods.JetGetTableIndexInfo(
                    sesid.Value,
                    tableid.Value,
                    indexname,
                    out result,
                    sizeof(ushort),
                    (uint)infoLevel));
            }

            return err;
        }

        /// <summary>
        /// Retrieves information about indexes on a table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetGetTableIndexInfo(
            JET_SESID sesid,
            JET_TABLEID tableid,
            string indexname,
            out int result,
            JET_IdxInfo infoLevel)
        {
            Tracing.TraceFunctionCall("JetGetTableIndexInfo");

            uint nativeResult;
            int err;

            if (_capabilities.SupportsVistaFeatures)
            {
                err = Tracing.TraceResult(NativeMethods.JetGetTableIndexInfoW(
                    sesid.Value,
                    tableid.Value,
                    indexname,
                    out nativeResult,
                    sizeof(uint),
                    (uint)infoLevel));
            }
            else
            {
                err = Tracing.TraceResult(NativeMethods.JetGetTableIndexInfo(
                    sesid.Value,
                    tableid.Value,
                    indexname,
                    out nativeResult,
                    sizeof(uint),
                    (uint)infoLevel));
            }

            result = unchecked((int)nativeResult);
            return err;
        }

        /// <summary>
        /// Retrieves information about indexes on a table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetGetTableIndexInfo(
            JET_SESID sesid,
            JET_TABLEID tableid,
            string indexname,
            out JET_INDEXID result,
            JET_IdxInfo infoLevel)
        {
            Tracing.TraceFunctionCall("JetGetTableIndexInfo");
            int err;

            if (_capabilities.SupportsVistaFeatures)
            {
                err = Tracing.TraceResult(NativeMethods.JetGetTableIndexInfoW(
                    sesid.Value,
                    tableid.Value,
                    indexname,
                    out result,
                    JET_INDEXID.SizeOfIndexId,
                    (uint)infoLevel));
            }
            else
            {
                err = Tracing.TraceResult(NativeMethods.JetGetTableIndexInfo(
                    sesid.Value,
                    tableid.Value,
                    indexname,
                    out result,
                    JET_INDEXID.SizeOfIndexId,
                    (uint)infoLevel));
            }

            return err;
        }

        /// <summary>
        /// Retrieves information about indexes on a table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetGetTableIndexInfo(
            JET_SESID sesid,
            JET_TABLEID tableid,
            string indexname,
            out JET_INDEXLIST result,
            JET_IdxInfo infoLevel)
        {
            Tracing.TraceFunctionCall("JetGetTableIndexInfo");

            var nativeIndexlist = new NATIVE_INDEXLIST();
            nativeIndexlist.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_INDEXLIST)));
            int err;

            if (_capabilities.SupportsVistaFeatures)
            {
                err = Tracing.TraceResult(NativeMethods.JetGetTableIndexInfoW(
                    sesid.Value,
                    tableid.Value,
                    indexname,
                    ref nativeIndexlist,
                    nativeIndexlist.cbStruct,
                    (uint)infoLevel));
            }
            else
            {
                err = Tracing.TraceResult(NativeMethods.JetGetTableIndexInfo(
                    sesid.Value,
                    tableid.Value,
                    indexname,
                    ref nativeIndexlist,
                    nativeIndexlist.cbStruct,
                    (uint)infoLevel));
            }

            result = new JET_INDEXLIST();
            result.SetFromNativeIndexlist(nativeIndexlist);

            return err;
        }

        /// <summary>
        /// Retrieves information about indexes on a table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetGetTableIndexInfo(
            JET_SESID sesid,
            JET_TABLEID tableid,
            string indexname,
            out string result,
            JET_IdxInfo infoLevel)
        {
            Tracing.TraceFunctionCall("JetGetTableIndexInfo");

            // Will need to check for Windows 8 Features.
            //
            // Currently only JET_IdxInfo.LocaleName is supported.
            uint bytesMax = checked((uint)Constants.LocaleNameMaxLength * sizeof(char));

            var stringBuilder = new StringBuilder(Constants.LocaleNameMaxLength);
            int err = Tracing.TraceResult(NativeMethods.JetGetTableIndexInfoW(sesid.Value,
                tableid.Value, indexname, stringBuilder, bytesMax, (uint)infoLevel));
            result = stringBuilder.ToString();
            result = StringCache.TryToIntern(result);
            return err;
        }

        #endregion

        /// <summary>
        /// Changes the name of an existing table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database containing the table.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="newTableName">The new name of the table.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetRenameTable(JET_SESID sesid, JET_DBID dbid, string tableName, string newTableName)
        {
            Tracing.TraceFunctionCall("JetRenameTable");
            Helpers.CheckNotNull(tableName, "tableName");
            Helpers.CheckNotNull(newTableName, "newTableName");
            return Tracing.TraceResult(NativeMethods.JetRenameTable(sesid.Value, dbid.Value, tableName, newTableName));
        }

        /// <summary>
        /// Changes the name of an existing column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="name">The name of the column.</param>
        /// <param name="newName">The new name of the column.</param>
        /// <param name="grbit">Column rename options.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetRenameColumn(JET_SESID sesid, JET_TABLEID tableid, string name, string newName, RenameColumnGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetRenameColumn");
            Helpers.CheckNotNull(name, "name");
            Helpers.CheckNotNull(newName, "newName");
            return Tracing.TraceResult(
                NativeMethods.JetRenameColumn(sesid.Value, tableid.Value, name, newName, (uint)grbit));
        }

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
        public int JetSetColumnDefaultValue(
            JET_SESID sesid, JET_DBID dbid, string tableName, string columnName, byte[] data, int dataSize, SetColumnDefaultValueGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetSetColumnDefaultValue");
            Helpers.CheckNotNull(tableName, "tableName");
            Helpers.CheckNotNull(columnName, "columnName");
            Helpers.CheckDataSize(data, dataSize, "dataSize");
            return Tracing.TraceResult(
                NativeMethods.JetSetColumnDefaultValue(
                    sesid.Value, dbid.Value, tableName, columnName, data, checked((uint)dataSize), (uint)grbit));
        }

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
        public int JetGotoSecondaryIndexBookmark(
            JET_SESID sesid,
            JET_TABLEID tableid,
            byte[] secondaryKey,
            int secondaryKeySize,
            byte[] primaryKey,
            int primaryKeySize,
            GotoSecondaryIndexBookmarkGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetGotoSecondaryIndexBookmark");
            Helpers.CheckNotNull(secondaryKey, "secondaryKey");
            Helpers.CheckDataSize(secondaryKey, secondaryKeySize, "secondaryKeySize");
            Helpers.CheckDataSize(primaryKey, primaryKeySize, "primaryKeySize");

            return
                Tracing.TraceResult(
                    NativeMethods.JetGotoSecondaryIndexBookmark(
                        sesid.Value,
                        tableid.Value,
                        secondaryKey,
                        checked((uint)secondaryKeySize),
                        primaryKey,
                        checked((uint)primaryKeySize),
                        (uint)grbit));
        }

        /// <summary>
        /// Constructs search keys that may then be used by <see cref="IJetInstance.JetSeek"/> and <see cref="IJetInstance.JetSetIndexRange"/>.
        /// </summary>
        /// <remarks>
        /// The MakeKey functions provide datatype-specific make key functionality.
        /// </remarks>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to create the key on.</param>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="dataSize">Size of the data.</param>
        /// <param name="grbit">Key options.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetMakeKey(JET_SESID sesid, JET_TABLEID tableid, IntPtr data, int dataSize, MakeKeyGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetMakeKey");
            Helpers.CheckNotNegative(dataSize, "dataSize");
            return Tracing.TraceResult(NativeMethods.JetMakeKey(sesid.Value, tableid.Value, data, checked((uint)dataSize), unchecked((uint)grbit)));
        }

        /// <summary>
        /// Efficiently positions a cursor to an index entry that matches the search
        /// criteria specified by the search key in that cursor and the specified
        /// inequality. A search key must have been previously constructed using
        /// JetMakeKey.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <param name="grbit">Seek options.</param>
        /// <returns>An error or warning..</returns>
        public int JetSeek(JET_SESID sesid, JET_TABLEID tableid, SeekGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetSeek");
            return Tracing.TraceResult(NativeMethods.JetSeek(sesid.Value, tableid.Value, unchecked((uint)grbit)));
        }

        /// <summary>Temporarily limits the set of index entries that the cursor can walk
        /// using <see cref="IJetTable.Move"/> to those starting from the current index entry
        /// and ending at the index entry that matches the search criteria specified by the
        /// search key in that cursor and the specified bound criteria. A search key must
        /// have been previously constructed using JetMakeKey.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to set the index range on.</param>
        /// <param name="grbit">Index range options.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetSetIndexRange(JET_SESID sesid, JET_TABLEID tableid, SetIndexRangeGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetSetIndexRange");
            return Tracing.TraceResult(NativeMethods.JetSetIndexRange(sesid.Value, tableid.Value, unchecked((uint)grbit)));
        }

        /// <summary>
        /// Computes the intersection between multiple sets of index entries from different secondary
        /// indices over the same table. This operation is useful for finding the set of records in a
        /// table that match two or more criteria that can be expressed using index ranges.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="ranges">
        /// An the index ranges to intersect. The tableids in the ranges
        ///  must have index ranges set on them.
        /// </param>
        /// <param name="numRanges">
        /// The number of index ranges.
        /// </param>
        /// <param name="recordlist">
        /// Returns information about the temporary table containing the intersection results.
        /// </param>
        /// <param name="grbit">Intersection options.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetIntersectIndexes(
            JET_SESID sesid,
            JET_INDEXRANGE[] ranges,
            int numRanges,
            out JET_RECORDLIST recordlist,
            IntersectIndexesGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetIntersectIndexes");
            Helpers.CheckNotNull(ranges, "ranges");
            Helpers.CheckDataSize(ranges, numRanges, "numRanges");
            if (numRanges < 2) {
                throw new ArgumentOutOfRangeException(
                    "numRanges", numRanges, "JetIntersectIndexes requires at least two index ranges.");
            }

            var indexRanges = new NATIVE_INDEXRANGE[numRanges];
            for (int i = 0; i < numRanges; ++i)
            {
                indexRanges[i] = ranges[i].GetNativeIndexRange();
            }

            var nativeRecordlist = new NATIVE_RECORDLIST();
            nativeRecordlist.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_RECORDLIST)));

            int err = Tracing.TraceResult(
                        NativeMethods.JetIntersectIndexes(
                            sesid.Value,
                            indexRanges,
                            checked((uint)indexRanges.Length),
                            ref nativeRecordlist,
                            (uint)grbit));
            recordlist = new JET_RECORDLIST();
            recordlist.SetFromNativeRecordlist(nativeRecordlist);
            return err;
        }

        /// <summary>
        /// Counts the number of entries in the current index from the current position forward.
        /// The current position is included in the count. The count can be greater than the
        /// total number of records in the table if the current index is over a multi-valued
        /// column and instances of the column have multiple-values. If the table is empty,
        /// then 0 will be returned for the count.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to count the records in.</param>
        /// <param name="numRecords">Returns the number of records.</param>
        /// <param name="maxRecordsToCount">
        /// The maximum number of records to count.
        /// </param>
        /// <returns>An error if the call fails.</returns>
        public int JetIndexRecordCount(JET_SESID sesid, JET_TABLEID tableid, out int numRecords, int maxRecordsToCount)
        {
            Tracing.TraceFunctionCall("JetIndexRecordCount");
            Helpers.CheckNotNegative(maxRecordsToCount, "maxRecordsToCount");
            uint crec = 0;
            int err = Tracing.TraceResult(NativeMethods.JetIndexRecordCount(sesid.Value, tableid.Value, out crec, unchecked((uint)maxRecordsToCount))); // -1 is allowed
            numRecords = checked((int)crec);
            return err;
        }

        /// <summary>
        /// Notifies the database engine that the application is scanning the entire
        /// index that the cursor is positioned on. Consequently, the methods that
        /// are used to access the index data will be tuned to make this scenario as
        /// fast as possible.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor that will be accessing the data.</param>
        /// <param name="grbit">Reserved for future use.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetSetTableSequential(JET_SESID sesid, JET_TABLEID tableid, SetTableSequentialGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetSetTableSequential");
            return Tracing.TraceResult(NativeMethods.JetSetTableSequential(sesid.Value, tableid.Value, (uint)grbit));
        }

        /// <summary>
        /// Notifies the database engine that the application is no longer scanning the
        /// entire index the cursor is positioned on. This call reverses a notification
        /// sent by JetSetTableSequential.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor that was accessing the data.</param>
        /// <param name="grbit">Reserved for future use.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetResetTableSequential(JET_SESID sesid, JET_TABLEID tableid, ResetTableSequentialGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetResetTableSequential");
            return Tracing.TraceResult(NativeMethods.JetResetTableSequential(sesid.Value, tableid.Value, (uint)grbit));
        }

        /// <summary>
        /// Returns the fractional position of the current record in the current index
        /// in the form of a JET_RECPOS structure.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor positioned on the record.</param>
        /// <param name="recpos">Returns the approximate fractional position of the record.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetGetRecordPosition(JET_SESID sesid, JET_TABLEID tableid, out JET_RECPOS recpos)
        {
            Tracing.TraceFunctionCall("JetGetRecordPosition");
            recpos = new JET_RECPOS();
            NATIVE_RECPOS native = recpos.GetNativeRecpos();
            int err = Tracing.TraceResult(NativeMethods.JetGetRecordPosition(sesid.Value, tableid.Value, out native, native.cbStruct));
            recpos.SetFromNativeRecpos(native);
            return err;
        }

        /// <summary>
        /// Moves a cursor to a new location that is a fraction of the way through
        /// the current index.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <param name="recpos">The approximate position to move to.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetGotoPosition(JET_SESID sesid, JET_TABLEID tableid, JET_RECPOS recpos)
        {
            Tracing.TraceFunctionCall("JetGotoPosition");
            NATIVE_RECPOS native = recpos.GetNativeRecpos();
            return Tracing.TraceResult(NativeMethods.JetGotoPosition(sesid.Value, tableid.Value, ref native));
        }

        /// <summary>
        /// If the records with the specified keys are not in the buffer cache
        /// then start asynchronous reads to bring the records into the database
        /// buffer cache.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to issue the prereads against.</param>
        /// <param name="keys">
        /// The keys to preread. The keys must be sorted.
        /// </param>
        /// <param name="keyLengths">The lengths of the keys to preread.</param>
        /// <param name="keyIndex">
        /// The index of the first key in the keys array to read.
        /// </param>
        /// <param name="keyCount">
        /// The maximum number of keys to preread.
        /// </param>
        /// <param name="keysPreread">
        /// Returns the number of keys to actually preread.
        /// </param>
        /// <param name="grbit">
        /// Preread options. Used to specify the direction of the preread.
        /// </param>
        /// <returns>An error or warning.</returns>
        public int JetPrereadKeys(
            JET_SESID sesid,
            JET_TABLEID tableid,
            byte[][] keys,
            int[] keyLengths,
            int keyIndex,
            int keyCount,
            out int keysPreread,
            PrereadKeysGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetPrereadKeys");
            _capabilities.CheckSupportsWindows7Features("JetPrereadKeys");
            Helpers.CheckDataSize(keys, keyIndex, "keyIndex", keyCount, "keyCount");
            Helpers.CheckDataSize(keyLengths, keyIndex, "keyIndex", keyCount, "keyCount");
            Helpers.CheckNotNull(keys, "keys");
            Helpers.CheckNotNull(keyLengths, "keyLengths");

            int err;
            unsafe
            {
                void** rgpvKeys = stackalloc void*[keyCount]; // [7/21/2010] StyleCop error? You need at least the 4.4 release of StyleCop
                uint* rgcbKeys = stackalloc uint[keyCount];
                using (var gchandlecollection = new GCHandleCollection())
                {
                    gchandlecollection.SetCapacity(keyCount);

                    for (int i = 0; i < keyCount; ++i)
                    {
                        rgpvKeys[i] = (void*)gchandlecollection.Add(keys[keyIndex + i]);
                        rgcbKeys[i] = checked((uint)keyLengths[keyIndex + i]);
                    }

                    err = Tracing.TraceResult(NativeMethods.JetPrereadKeys(
                                sesid.Value, tableid.Value, rgpvKeys, rgcbKeys, keyCount, out keysPreread, (uint)grbit));
                }
            }

            return err;
        }

        #endregion

        #region Data Retrieval

        /// <summary>
        /// Retrieves the bookmark for the record that is associated with the index entry
        /// at the current position of a cursor. This bookmark can then be used to
        /// reposition that cursor back to the same record using <see cref="ICursor.GotoBookmark"/>.
        /// The bookmark will be no longer than <see cref="JetEnvironment.BookmarkMost"/>
        /// bytes.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the bookmark from.</param>
        /// <param name="bookmark">Buffer to contain the bookmark.</param>
        /// <param name="bookmarkSize">Size of the bookmark buffer.</param>
        /// <param name="actualBookmarkSize">Returns the actual size of the bookmark.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetGetBookmark(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize, out int actualBookmarkSize)
        {
            Tracing.TraceFunctionCall("JetGetBookmark");
            Helpers.CheckDataSize(bookmark, bookmarkSize, "bookmarkSize");

            uint bytesActual = 0;
            int err = Tracing.TraceResult(
                NativeMethods.JetGetBookmark(
                    sesid.Value,
                    tableid.Value,
                    bookmark,
                    checked((uint)bookmarkSize),
                    out bytesActual));

            actualBookmarkSize = GetActualSize(bytesActual);
            return err;
        }

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
        public int JetGetSecondaryIndexBookmark(
            JET_SESID sesid,
            JET_TABLEID tableid,
            byte[] secondaryKey,
            int secondaryKeySize,
            out int actualSecondaryKeySize,
            byte[] primaryKey,
            int primaryKeySize,
            out int actualPrimaryKeySize,
            GetSecondaryIndexBookmarkGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetGetSecondaryIndexBookmark");
            Helpers.CheckDataSize(secondaryKey, secondaryKeySize, "secondaryKeySize");
            Helpers.CheckDataSize(primaryKey, primaryKeySize, "primaryKeySize");

            uint bytesSecondaryKey = 0;
            uint bytesPrimaryKey = 0;
            int err = Tracing.TraceResult(
                NativeMethods.JetGetSecondaryIndexBookmark(
                    sesid.Value,
                    tableid.Value,
                    secondaryKey,
                    checked((uint)secondaryKeySize),
                    out bytesSecondaryKey,
                    primaryKey,
                    checked((uint)primaryKeySize),
                    out bytesPrimaryKey,
                    (uint)grbit));

            actualSecondaryKeySize = GetActualSize(bytesSecondaryKey);
            actualPrimaryKeySize = GetActualSize(bytesPrimaryKey);

            return err;
        }

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
        public int JetRetrieveKey(JET_SESID sesid, JET_TABLEID tableid, byte[] data, int dataSize, out int actualDataSize, RetrieveKeyGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetRetrieveKey");
            Helpers.CheckDataSize(data, dataSize, "dataSize");

            uint bytesActual = 0;
            int err = Tracing.TraceResult(NativeMethods.JetRetrieveKey(sesid.Value, tableid.Value, data, checked((uint)dataSize), out bytesActual, unchecked((uint)grbit)));

            actualDataSize = GetActualSize(bytesActual);
            return err;
        }

        /// <summary>
        /// The JetRetrieveColumns function retrieves multiple column values
        /// from the current record in a single operation. An array of
        /// <see cref="NATIVE_RETRIEVECOLUMN"/> structures is used to
        /// describe the set of column values to be retrieved, and to describe
        /// output buffers for each column value to be retrieved.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve columns from.</param>
        /// <param name="retrievecolumns">
        /// An array of one or more JET_RETRIEVECOLUMN structures. Each
        /// structure includes descriptions of which column value to retrieve
        /// and where to store returned data.
        /// </param>
        /// <param name="numColumns">
        /// Number of structures in the array given by retrievecolumns.
        /// </param>
        /// <returns>
        /// An error or warning.
        /// </returns>
        public unsafe int JetRetrieveColumns(JET_SESID sesid, JET_TABLEID tableid, NATIVE_RETRIEVECOLUMN* retrievecolumns, int numColumns)
        {
            Tracing.TraceFunctionCall("JetRetrieveColumns");
            return Tracing.TraceResult(NativeMethods.JetRetrieveColumns(sesid.Value, tableid.Value, retrievecolumns, checked((uint)numColumns)));
        }

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
        public int JetEnumerateColumns(JET_SESID sesid, JET_TABLEID tableid, int numColumnids,
            JET_ENUMCOLUMNID[] columnids, out int numColumnValues, out JET_ENUMCOLUMN[] columnValues,
            JET_PFNREALLOC allocator, IntPtr allocatorContext, int maxDataSize, EnumerateColumnsGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetEnumerateColumns");
            Helpers.CheckNotNull(allocator, "allocator");
            Helpers.CheckNotNegative(maxDataSize, "maxDataSize");
            Helpers.CheckDataSize(columnids, numColumnids, "numColumnids");

            unsafe
            {
                // Converting to the native structs is a bit complex because we
                // do not want to allocate heap memory for this operations. We
                // allocate the NATIVE_ENUMCOLUMNID array on the stack and
                // convert the managed objects. During the conversion pass we
                // calculate the total size of the tags. An array for the tags
                // is then allocated and a second pass converts the tags.
                //
                // Because we are using stackalloc all the work has to be done
                // in the same method.
                NATIVE_ENUMCOLUMNID* nativecolumnids = stackalloc NATIVE_ENUMCOLUMNID[numColumnids];
                int totalNumTags = ConvertEnumColumnids(columnids, numColumnids, nativecolumnids);

                uint* tags = stackalloc uint[totalNumTags];
                ConvertEnumColumnidTags(columnids, numColumnids, nativecolumnids, tags);

                uint numEnumColumn;
                NATIVE_ENUMCOLUMN* nativeenumcolumns;
                int err = NativeMethods.JetEnumerateColumns(
                    sesid.Value,
                    tableid.Value,
                    checked((uint)numColumnids),
                    numColumnids > 0 ? nativecolumnids : null,
                    out numEnumColumn,
                    out nativeenumcolumns,
                    allocator,
                    allocatorContext,
                    checked((uint)maxDataSize),
                    (uint)grbit);

                ConvertEnumerateColumnsResult(allocator, allocatorContext, numEnumColumn, nativeenumcolumns, out numColumnValues, out columnValues);

                return Tracing.TraceResult(err);
            }
        }

        /// <summary>
        /// Efficiently retrieves a set of columns and their values from the
        /// current record of a cursor or the copy buffer of that cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve data from.</param>
        /// <param name="grbit">Enumerate options.</param>
        /// <param name="enumeratedColumns">The discovered columns and their values.</param>
        /// <returns>A warning or success.</returns>
        public int JetEnumerateColumns(
            JET_SESID sesid,
            JET_TABLEID tableid,
            EnumerateColumnsGrbit grbit,
            out IEnumerable<EnumeratedColumn> enumeratedColumns)
        {
            unsafe
            {
                // NOTE:  We must never throw an exception up through ESE or it will corrupt its internal state!
                Exception allocatorException = null;
                JET_PFNREALLOC allocator = (c, pv, cb) =>
                {
                    try
                    {
                        if (pv == IntPtr.Zero)
                        {
                            // NOTE:  this will allocate memory if cb == 0 and that is what we want.
                            return Marshal.AllocHGlobal(new IntPtr(cb));
                        }

                        if (cb == 0)
                        {
                            Marshal.FreeHGlobal(pv);
                            return IntPtr.Zero;
                        }

                        return Marshal.ReAllocHGlobal(pv, new IntPtr(cb));
                    }
                    catch (OutOfMemoryException)
                    {
                        return IntPtr.Zero;
                    }
                    catch (ThreadAbortException e)
                    {
                        LibraryHelpers.ThreadResetAbort();
                        allocatorException = e;
                        return IntPtr.Zero;
                    }
                    catch (Exception e)
                    {
                        allocatorException = e;
                        return IntPtr.Zero;
                    }
                };

                uint nativeEnumColumnCount;
                NATIVE_ENUMCOLUMN* nativeEnumColumns;
                int err = Implementation.NativeMethods.JetEnumerateColumns(
                    sesid.Value,
                    tableid.Value,
                    0,
                    null,
                    out nativeEnumColumnCount,
                    out nativeEnumColumns,
                    allocator,
                    IntPtr.Zero,
                    int.MaxValue,
                    (uint)(grbit & ~EnumerateColumnsGrbit.EnumerateCompressOutput));

                var columns = new EnumeratedColumn[nativeEnumColumnCount];
                for (int i = 0; i < nativeEnumColumnCount; ++i)
                {
                    columns[i] = new EnumeratedColumn();
                    columns[i].Id = new JET_COLUMNID { Value = nativeEnumColumns[i].columnid };
                    columns[i].Error = nativeEnumColumns[i].err < 0 ? (JET_err)nativeEnumColumns[i].err : JET_err.Success;
                    columns[i].Warning = nativeEnumColumns[i].err > 0 ? (JET_wrn)nativeEnumColumns[i].err : JET_wrn.Success;
                    if ((JET_wrn)nativeEnumColumns[i].err == JET_wrn.Success)
                    {
                        EnumeratedColumn.Value[] values = new EnumeratedColumn.Value[nativeEnumColumns[i].cEnumColumnValue];
                        columns[i].Values = values;
                        for (int j = 0; j < nativeEnumColumns[i].cEnumColumnValue; j++)
                        {
                            values[j] = new EnumeratedColumn.Value();
                            values[j].Ordinal = j + 1;
                            values[j].Warning = (JET_wrn)nativeEnumColumns[i].rgEnumColumnValue[j].err;
                            values[j].Bytes = new byte[(int)nativeEnumColumns[i].rgEnumColumnValue[j].cbData];
                            Marshal.Copy(
                                nativeEnumColumns[i].rgEnumColumnValue[j].pvData,
                                values[j].Bytes,
                                0,
                                (int)nativeEnumColumns[i].rgEnumColumnValue[j].cbData);
                            if (nativeEnumColumns[i].rgEnumColumnValue[j].pvData != IntPtr.Zero)
                            {
                                allocator(IntPtr.Zero, nativeEnumColumns[i].rgEnumColumnValue[j].pvData, 0);
                            }
                        }

                        if (nativeEnumColumns[i].rgEnumColumnValue != null)
                        {
                            allocator(IntPtr.Zero, new IntPtr(nativeEnumColumns[i].rgEnumColumnValue), 0);
                        }
                    }
                }

                if (nativeEnumColumns != null)
                {
                    allocator(IntPtr.Zero, new IntPtr(nativeEnumColumns), 0);
                }

                if (allocatorException != null)
                {
                    if (allocatorException is ThreadAbortException)
                    {
                        Thread.CurrentThread.Abort();
                    }
                    throw allocatorException;
                }

                enumeratedColumns = columns;
                return Tracing.TraceResult(err);
            }
        }

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
        public int JetGetRecordSize(JET_SESID sesid, JET_TABLEID tableid, ref JET_RECSIZE recsize, GetRecordSizeGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetGetRecordSize");
            _capabilities.CheckSupportsVistaFeatures("JetGetRecordSize");
            int err;

            // Use JetGetRecordSize2 if available, otherwise JetGetRecordSize.
            if (_capabilities.SupportsWindows7Features)
            {
                var native = recsize.GetNativeRecsize2();
                err = NativeMethods.JetGetRecordSize2(sesid.Value, tableid.Value, ref native, (uint)grbit);
                recsize.SetFromNativeRecsize(native);
            }
            else
            {
                var native = recsize.GetNativeRecsize();
                err = NativeMethods.JetGetRecordSize(sesid.Value, tableid.Value, ref native, (uint)grbit);
                recsize.SetFromNativeRecsize(native);
            }

            return Tracing.TraceResult(err);
        }

        #endregion

        #region DML

        /// <summary>
        /// Deletes the current record in a database table.
        /// </summary>
        /// <param name="sesid">The session that opened the cursor.</param>
        /// <param name="tableid">The cursor on a database table. The current row will be deleted.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetDelete(JET_SESID sesid, JET_TABLEID tableid)
        {
            Tracing.TraceFunctionCall("JetDelete");
            return Tracing.TraceResult(NativeMethods.JetDelete(sesid.Value, tableid.Value));
        }

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
        public int JetSetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, IntPtr data, int dataSize, SetColumnGrbit grbit, JET_SETINFO setinfo)
        {
            Tracing.TraceFunctionCall("JetSetColumn");
            Helpers.CheckNotNegative(dataSize, "dataSize");
            if (IntPtr.Zero == data)
            {
                if (dataSize > 0 && (SetColumnGrbit.SizeLV != (grbit & SetColumnGrbit.SizeLV)))
                {
                    throw new ArgumentOutOfRangeException(
                        "dataSize",
                        dataSize,
                        "cannot be greater than the length of the data (unless the SizeLV option is used)");
                }
            }

            if (null != setinfo)
            {
                NATIVE_SETINFO nativeSetinfo = setinfo.GetNativeSetinfo();
                return Tracing.TraceResult(NativeMethods.JetSetColumn(sesid.Value, tableid.Value, columnid.Value, data, checked((uint)dataSize), unchecked((uint)grbit), ref nativeSetinfo));
            }

            return Tracing.TraceResult(NativeMethods.JetSetColumn(sesid.Value, tableid.Value, columnid.Value, data, checked((uint)dataSize), unchecked((uint)grbit), IntPtr.Zero));
        }

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
        public unsafe int JetSetColumns(JET_SESID sesid, JET_TABLEID tableid, NATIVE_SETCOLUMN* setcolumns, int numColumns)
        {
            Tracing.TraceFunctionCall("JetSetColumns");
            return Tracing.TraceResult(NativeMethods.JetSetColumns(sesid.Value, tableid.Value, setcolumns, checked((uint)numColumns)));
        }

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
        public int JetEscrowUpdate(
            JET_SESID sesid,
            JET_TABLEID tableid,
            JET_COLUMNID columnid,
            byte[] delta,
            int deltaSize,
            byte[] previousValue,
            int previousValueLength,
            out int actualPreviousValueLength,
            EscrowUpdateGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetEscrowUpdate");
            Helpers.CheckNotNull(delta, "delta");
            Helpers.CheckDataSize(delta, deltaSize, "deltaSize");
            Helpers.CheckDataSize(previousValue, previousValueLength, "previousValueLength");

            uint bytesOldActual = 0;
            int err = Tracing.TraceResult(NativeMethods.JetEscrowUpdate(
                                  sesid.Value,
                                  tableid.Value,
                                  columnid.Value,
                                  delta,
                                  checked((uint)deltaSize),
                                  previousValue,
                                  checked((uint)previousValueLength),
                                  out bytesOldActual,
                                  unchecked((uint)grbit)));
            actualPreviousValueLength = checked((int)bytesOldActual);
            return err;
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// Allows the application to configure the database engine to issue
        /// notifications to the application for specific events. These
        /// notifications are associated with a specific table and remain in
        /// effect only until the instance containing the table is shut down
        /// using.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">A cursor opened on the table that the callback
        /// should be registered on.</param>
        /// <param name="cbtyp">The callback reasons for which the application
        /// wishes to receive notifications.</param>
        /// <param name="callback">The callback function.</param>
        /// <param name="context">A context that will be given to the callback.</param>
        /// <param name="callbackId">A handle that can later be used to cancel
        /// the registration of the given callback function using
        /// <see cref="JetUnregisterCallback"/>.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetRegisterCallback(JET_SESID sesid, JET_TABLEID tableid, JET_cbtyp cbtyp,
            JET_CALLBACK callback, IntPtr context, out JET_HANDLE callbackId)
        {
            Tracing.TraceFunctionCall("JetRegisterCallback");
            Helpers.CheckNotNull(callback, "callback");

            callbackId = JET_HANDLE.Nil;
            return Tracing.TraceResult(NativeMethods.JetRegisterCallback(sesid.Value, tableid.Value,
                unchecked((uint)cbtyp), this.callbackWrappers.Add(callback).NativeCallback,
                context, out callbackId._nativeHandle));
        }

        /// <summary>Configures the database engine to stop issuing notifications to the
        /// application as previously requested through<see cref="JetRegisterCallback"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">A cursor opened on the table that the callback should
        /// be registered on.</param>
        /// <param name="cbtyp">The callback reasons for which the application no longer
        /// wishes to receive notifications.</param>
        /// <param name="callbackId">The handle of the registered callback that was
        /// returned by <see cref="JetRegisterCallback"/>.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetUnregisterCallback(JET_SESID sesid, JET_TABLEID tableid, JET_cbtyp cbtyp, JET_HANDLE callbackId)
        {
            Tracing.TraceFunctionCall("JetUnregisterCallback");
            this.callbackWrappers.Collect();
            return Tracing.TraceResult(NativeMethods.JetUnregisterCallback(
                sesid.Value,
                tableid.Value,
                unchecked((uint)cbtyp),
                callbackId._nativeHandle));
        }

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
        /// database ID, and other options require the name of the table to defragment.
        /// </param>
        /// <param name="passes">
        /// When starting an online defragmentation task, this parameter sets the maximum number of defragmentation
        /// passes. When stopping an online defragmentation task, this parameter is set to the number of passes
        /// performed. This is not honored in all modes.</param>
        /// <param name="seconds">
        /// When starting an online defragmentation task, this parameter sets
        /// the maximum time for defragmentation. When stopping an online
        /// defragmentation task, this output buffer is set to the length of
        /// time used for defragmentation. This is not honored in all modes.</param>
        /// <param name="grbit">Defragmentation options.</param>
        /// <returns>An error code.</returns>
        /// <seealso cref="JetInstance.Defragment"/>.
        /// <seealso cref="JetInstance.JetDefragment2"/>.
        public int JetDefragment(JET_SESID sesid, JET_DBID dbid, string tableName, ref int passes, ref int seconds, DefragGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetDefragment");
            uint nativePasses = unchecked((uint)passes);
            uint nativeSeconds = unchecked((uint)seconds);
            int err = Tracing.TraceResult(NativeMethods.JetDefragment(
                sesid.Value, dbid.Value, tableName, ref nativePasses, ref nativeSeconds, (uint)grbit));
            passes = unchecked((int)nativePasses);
            seconds = unchecked((int)nativeSeconds);
            return err;
        }

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
        /// <returns>An error code.</returns>
        /// <seealso cref="JetInstance.JetDefragment"/>.
        public int Defragment(
            JET_SESID sesid,
            JET_DBID dbid,
            string tableName,
            DefragGrbit grbit)
        {
            Tracing.TraceFunctionCall("Defragment");
            int err = Tracing.TraceResult(NativeMethods.JetDefragment(
                sesid.Value, dbid.Value, tableName, IntPtr.Zero, IntPtr.Zero, (uint)grbit));
            return err;
        }

        /// <summary>
        /// Starts and stops database defragmentation tasks that improves data
        /// organization within a database.
        /// </summary>
        /// <param name="sesid">The session to use for the call.</param>
        /// <param name="dbid">The database to be defragmented.</param>
        /// <param name="tableName">
        /// Under some options defragmentation is performed for the entire database described by the given 
        /// database ID, and other options require the name of the table to defragment.</param>
        /// <param name="passes">
        /// When starting an online defragmentation task, this parameter sets the maximum number of defragmentation
        /// passes. When stopping an online defragmentation task, this parameter is set to the number of passes
        /// performed. This is not honored in all modes .</param>
        /// <param name="seconds">
        /// When starting an online defragmentation task, this parameter sets
        /// the maximum time for defragmentation. When stopping an online
        /// defragmentation task, this output buffer is set to the length of
        /// time used for defragmentation. This is not honored in all modes.</param>
        /// <param name="callback">Callback function that defrag uses to report progress.</param>
        /// <param name="grbit">Defragmentation options.</param>
        /// <returns>An error code or warning.</returns>
        /// <seealso cref="JetInstance.JetDefragment"/>.
        public int JetDefragment2(
            JET_SESID sesid,
            JET_DBID dbid,
            string tableName,
            ref int passes,
            ref int seconds,
            JET_CALLBACK callback,
            DefragGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetDefragment2");
            uint nativePasses = unchecked((uint)passes);
            uint nativeSeconds = unchecked((uint)seconds);

            IntPtr functionPointer;
            if (null == callback)
            {
                functionPointer = IntPtr.Zero;
            }
            else
            {
                JetCallbackWrapper callbackWrapper = this.callbackWrappers.Add(callback);
                functionPointer = Marshal.GetFunctionPointerForDelegate(callbackWrapper.NativeCallback);
#if DEBUG
                GC.Collect();
#endif
            }

            int err = Tracing.TraceResult(NativeMethods.JetDefragment2(
                sesid.Value, dbid.Value, tableName, ref nativePasses, ref nativeSeconds, functionPointer, (uint)grbit));
            passes = unchecked((int)nativePasses);
            seconds = unchecked((int)nativeSeconds);
            this.callbackWrappers.Collect();
            return err;
        }

        //// Currently, this overload of JetDefragment2() is not used outside of WSA.
#if MANAGEDESENT_ON_WSA
        /// <summary>
        /// Starts and stops database defragmentation tasks that improves data
        /// organization within a database.
        /// </summary>
        /// <param name="sesid">The session to use for the call.</param>
        /// <param name="dbid">The database to be defragmented.</param>
        /// <param name="tableName">
        /// Under some options defragmentation is performed for the entire database described by the given 
        /// database ID, and other options (such as <see cref="Windows7Grbits.DefragmentBTree"/>) require
        /// the name of the table to defragment.
        /// </param>
        /// <param name="callback">Callback function that defrag uses to report progress.</param>
        /// <param name="grbit">Defragmentation options.</param>
        /// <returns>An error code or warning.</returns>
        /// <seealso cref="JetApi.JetDefragment2"/>.
        public int Defragment2(
            JET_SESID sesid,
            JET_DBID dbid,
            string tableName,
            JET_CALLBACK callback,
            DefragGrbit grbit)
        {
            Tracing.TraceFunctionCall("Defragment2");

            IntPtr functionPointer;
            if (null == callback)
            {
                functionPointer = IntPtr.Zero;
            }
            else
            {
                JetCallbackWrapper callbackWrapper = this.callbackWrappers.Add(callback);
                functionPointer = Marshal.GetFunctionPointerForDelegate(callbackWrapper.NativeCallback);
#if DEBUG
                GC.Collect();
#endif
            }

            int err = Err(NativeMethods.JetDefragment2(
                sesid.Value, dbid.Value, tableName, IntPtr.Zero, IntPtr.Zero, functionPointer, (uint)grbit));
            this.callbackWrappers.Collect();
            return err;
        }
#endif

        /// <summary>
        /// Performs idle cleanup tasks or checks the version store status in ESE.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="grbit">A combination of JetIdleGrbit flags.</param>
        /// <returns>An error code if the operation fails.</returns>
        public int JetIdle(JET_SESID sesid, IdleGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetIdle");
            return Tracing.TraceResult(NativeMethods.JetIdle(sesid.Value, (uint)grbit));
        }

        #endregion

        #region Misc

        /// <summary>
        /// Crash dump options for Watson.
        /// </summary>
        /// <param name="grbit">Crash dump options.</param>
        /// <returns>An error code.</returns>
        public int JetConfigureProcessForCrashDump(CrashDumpGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetConfigureProcessForCrashDump");
            _capabilities.CheckSupportsWindows7Features("JetConfigureProcessForCrashDump");
            return Tracing.TraceResult(NativeMethods.JetConfigureProcessForCrashDump((uint)grbit));
        }

        #endregion

        #region Internal Helper Methods

        /// <summary>
        /// Given the size returned by ESENT, get the size
        /// to return to the user.
        /// </summary>
        /// <param name="numBytesActual">The size returned by ESENT.</param>
        /// <returns>The bookmark size to return to the user.</returns>
        internal static int GetActualSize(uint numBytesActual)
        {
            // BUG: debug builds of ESENT can fill numBytesActual with this value in case of failure.
            const uint CbActualDebugFill = 0xDDDDDDDD;
            int actualSize;
            if (CbActualDebugFill == numBytesActual)
            {
                actualSize = 0;
            }
            else
            {
                actualSize = checked((int)numBytesActual);
            }

            return actualSize;
        }

        #endregion Internal Helper Methods

        #region Helper Methods

        /// <summary>Convert managed JET_ENUMCOLUMNID objects to NATIVE_ENUMCOLUMNID
        /// structures.</summary>
        /// <param name="columnids">The columnids to convert.</param>
        /// <param name="numColumnids">The number of columnids to convert.</param>
        /// <param name="nativecolumnids">The array to store the converted columnids.</param>
        /// <returns>The total number of tag entries in the converted structures.</returns>
        private static unsafe int ConvertEnumColumnids(IList<JET_ENUMCOLUMNID> columnids, int numColumnids, NATIVE_ENUMCOLUMNID* nativecolumnids)
        {
            int totalNumTags = 0;
            for (int i = 0; i < numColumnids; ++i)
            {
                nativecolumnids[i] = columnids[i].GetNativeEnumColumnid();
                checked
                {
                    totalNumTags += columnids[i].ctagSequence;
                }
            }

            return totalNumTags;
        }

        /// <summary>
        /// Convert managed rgtagSequence to unmanaged rgtagSequence.
        /// </summary>
        /// <param name="columnids">The columnids to convert.</param>
        /// <param name="numColumnids">The number of columnids to covert.</param>
        /// <param name="nativecolumnids">The unmanaged columnids to add the tags to.</param>
        /// <param name="tags">
        /// Memory to use for converted rgtagSequence. This should be large enough to
        /// hold all columnids.
        /// </param>
        private static unsafe void ConvertEnumColumnidTags(IList<JET_ENUMCOLUMNID> columnids, int numColumnids, NATIVE_ENUMCOLUMNID* nativecolumnids, uint* tags)
        {
            for (int i = 0; i < numColumnids; ++i)
            {
                nativecolumnids[i].rgtagSequence = tags;
                for (int j = 0; j < columnids[i].ctagSequence; ++j)
                {
                    nativecolumnids[i].rgtagSequence[j] = checked((uint)columnids[i].rgtagSequence[j]);
                }

                tags += columnids[i].ctagSequence;
            }
        }

        /// <summary>
        /// Convert the native (unmanaged) results of JetEnumerateColumns to
        /// managed objects. This uses the allocator callback to free some
        /// memory as the data is converted.
        /// </summary>
        /// <param name="allocator">The allocator callback used.</param>
        /// <param name="allocatorContext">The allocator callback context.</param>
        /// <param name="numEnumColumn">Number of NATIVE_ENUMCOLUMN structures returned.</param>
        /// <param name="nativeenumcolumns">NATIVE_ENUMCOLUMN structures.</param>
        /// <param name="numColumnValues">Returns the number of converted JET_ENUMCOLUMN objects.</param>
        /// <param name="columnValues">Returns the convertd column values.</param>
        private static unsafe void ConvertEnumerateColumnsResult(JET_PFNREALLOC allocator, IntPtr allocatorContext, uint numEnumColumn, NATIVE_ENUMCOLUMN* nativeenumcolumns, out int numColumnValues, out JET_ENUMCOLUMN[] columnValues)
        {
            numColumnValues = checked((int)numEnumColumn);
            columnValues = new JET_ENUMCOLUMN[numColumnValues];
            for (int i = 0; i < numColumnValues; ++i)
            {
                columnValues[i] = new JET_ENUMCOLUMN();
                columnValues[i].SetFromNativeEnumColumn(nativeenumcolumns[i]);
                if (JET_wrn.ColumnSingleValue != columnValues[i].err)
                {
                    columnValues[i].rgEnumColumnValue = new JET_ENUMCOLUMNVALUE[columnValues[i].cEnumColumnValue];
                    for (int j = 0; j < columnValues[i].cEnumColumnValue; ++j)
                    {
                        columnValues[i].rgEnumColumnValue[j] = new JET_ENUMCOLUMNVALUE();
                        columnValues[i].rgEnumColumnValue[j].SetFromNativeEnumColumnValue(nativeenumcolumns[i].rgEnumColumnValue[j]);
                    }

                    // the NATIVE_ENUMCOLUMNVALUES have been converted
                    // free their memory
                    allocator(allocatorContext, new IntPtr(nativeenumcolumns[i].rgEnumColumnValue), 0);
                    nativeenumcolumns[i].rgEnumColumnValue = null;
                }
            }

            // Now we have converted all the NATIVE_ENUMCOLUMNS we can
            // free the memory they use
            allocator(allocatorContext, new IntPtr(nativeenumcolumns), 0);
            nativeenumcolumns = null;
        }

        /// <summary>Make native columncreate structures from the managed ones.</summary>
        /// <param name="managedColumnCreates">Column create structures to convert.</param>
        /// <param name="useUnicodeData">Wehether to convert the strings with UTF-16.</param>
        /// <param name="handles">The handle collection used to pin the data.</param>
        /// <returns>Pinned native versions of the column creates.</returns>
        private static IntPtr GetNativeColumnCreates(IList<JET_COLUMNCREATE> managedColumnCreates,
            bool useUnicodeData,ref GCHandleCollection handles)
        {
            IntPtr nativeBuffer = IntPtr.Zero;
            if (managedColumnCreates != null && managedColumnCreates.Count > 0) {
                var nativeColumns = new NATIVE_COLUMNCREATE[managedColumnCreates.Count];
                for (int i = 0; i < managedColumnCreates.Count; ++i) {
                    if (managedColumnCreates[i] != null) {
                        JET_COLUMNCREATE managedColumn = managedColumnCreates[i];
                        nativeColumns[i] = managedColumn.GetNativeColumnCreate();
                        nativeColumns[i].szColumnName = (useUnicodeData) 
                            ? handles.Add(Util.ConvertToNullTerminatedUnicodeByteArray(managedColumn.szColumnName))
                            : nativeColumns[i].szColumnName = handles.Add(Util.ConvertToNullTerminatedAsciiByteArray(managedColumn.szColumnName));
                        if (managedColumn.cbDefault > 0) {
                            nativeColumns[i].pvDefault = handles.Add(managedColumn.pvDefault);
                        }
                    }
                }
                nativeBuffer = handles.Add(nativeColumns);
            }
            return nativeBuffer;
        }

        // NOT IMPLEMENTED
        ///// <summary>
        ///// Make native indexcreate structures from the managed ones.
        ///// Only supports Ascii data, since it could be used on XP.
        ///// </summary>
        ///// <param name="managedIndexCreates">Index create structures to convert.</param>
        ///// <param name="handles">The handle collection used to pin the data.</param>
        ///// <returns>Pinned native versions of the index creates.</returns>
        //private static unsafe JET_INDEXCREATE.NATIVE_INDEXCREATE[] GetNativeIndexCreates(
        //    IList<JET_INDEXCREATE> managedIndexCreates,
        //    ref GCHandleCollection handles)
        //{
        //    JET_INDEXCREATE.NATIVE_INDEXCREATE[] nativeIndices = null;

        //    if (managedIndexCreates != null && managedIndexCreates.Count > 0)
        //    {
        //        nativeIndices = new JET_INDEXCREATE.NATIVE_INDEXCREATE[managedIndexCreates.Count];

        //        for (int i = 0; i < managedIndexCreates.Count; ++i)
        //        {
        //            nativeIndices[i] = managedIndexCreates[i].GetNativeIndexcreate();

        //            if (null != managedIndexCreates[i].pidxUnicode) {
        //                NATIVE_UNICODEINDEX unicode = managedIndexCreates[i].pidxUnicode.GetNativeUnicodeIndex();
        //                nativeIndices[i].pidxUnicode = (NATIVE_UNICODEINDEX*)handles.Add(unicode);
        //                nativeIndices[i].grbit |= (uint)CreateIndexGrbit.IndexUnicode;
        //            }
        //            nativeIndices[i].szKey = handles.Add(Util.ConvertToNullTerminatedAsciiByteArray(managedIndexCreates[i].szKey));
        //            nativeIndices[i].szIndexName = handles.Add(Util.ConvertToNullTerminatedAsciiByteArray(managedIndexCreates[i].szIndexName));
        //            nativeIndices[i].rgconditionalcolumn = GetNativeConditionalColumns(managedIndexCreates[i].rgconditionalcolumn, false, ref handles);
        //        }
        //    }

        //    return nativeIndices;
        //}

        // NOT IMPLEMENTED
        ///// <summary>
        ///// Make native indexcreate structures from the managed ones.
        ///// Only supports Unicode data, since it was introduced in Vista.
        ///// </summary>
        ///// <param name="managedIndexCreates">Index create structures to convert.</param>
        ///// <param name="handles">The handle collection used to pin the data.</param>
        ///// <returns>Pinned native versions of the index creates.</returns>
        //private static unsafe JET_INDEXCREATE.NATIVE_INDEXCREATE1[] GetNativeIndexCreate1s(
        //    IList<JET_INDEXCREATE> managedIndexCreates,
        //    ref GCHandleCollection handles)
        //{
        //    JET_INDEXCREATE.NATIVE_INDEXCREATE1[] nativeIndices = null;

        //    if (managedIndexCreates != null && managedIndexCreates.Count > 0)
        //    {
        //        nativeIndices = new JET_INDEXCREATE.NATIVE_INDEXCREATE1[managedIndexCreates.Count];

        //        for (int i = 0; i < managedIndexCreates.Count; ++i)
        //        {
        //            nativeIndices[i] = managedIndexCreates[i].GetNativeIndexcreate1();

        //            if (null != managedIndexCreates[i].pidxUnicode)
        //            {
        //                NATIVE_UNICODEINDEX unicode = managedIndexCreates[i].pidxUnicode.GetNativeUnicodeIndex();
        //                nativeIndices[i].indexcreate.pidxUnicode = (NATIVE_UNICODEINDEX*)handles.Add(unicode);
        //                nativeIndices[i].indexcreate.grbit |= (uint)CreateIndexGrbit.IndexUnicode;
        //            }

        //            nativeIndices[i].indexcreate.szKey = handles.Add(Util.ConvertToNullTerminatedUnicodeByteArray(managedIndexCreates[i].szKey));
        //            nativeIndices[i].indexcreate.cbKey *= sizeof(char);
        //            nativeIndices[i].indexcreate.szIndexName = handles.Add(Util.ConvertToNullTerminatedUnicodeByteArray(managedIndexCreates[i].szIndexName));
        //            nativeIndices[i].indexcreate.rgconditionalcolumn = GetNativeConditionalColumns(managedIndexCreates[i].rgconditionalcolumn, false, ref handles);
        //        }
        //    }

        //    return nativeIndices;
        //}

        // NOT IMPLEMENTED
        ///// <summary>
        ///// Make native indexcreate structures from the managed ones.
        ///// Only supports Unicode data, since it was introduced in Win7.
        ///// </summary>
        ///// <param name="managedIndexCreates">Index create structures to convert.</param>
        ///// <param name="handles">The handle collection used to pin the data.</param>
        ///// <returns>Pinned native versions of the index creates.</returns>
        //private static unsafe JET_INDEXCREATE.NATIVE_INDEXCREATE2[] GetNativeIndexCreate2s(
        //    IList<JET_INDEXCREATE> managedIndexCreates,
        //    ref GCHandleCollection handles)
        //{
        //    JET_INDEXCREATE.NATIVE_INDEXCREATE2[] nativeIndices = null;

        //    if (managedIndexCreates != null && managedIndexCreates.Count > 0)
        //    {
        //        nativeIndices = new JET_INDEXCREATE.NATIVE_INDEXCREATE2[managedIndexCreates.Count];

        //        for (int i = 0; i < managedIndexCreates.Count; ++i)
        //        {
        //            nativeIndices[i] = managedIndexCreates[i].GetNativeIndexcreate2();

        //            if (null != managedIndexCreates[i].pidxUnicode)
        //            {
        //                NATIVE_UNICODEINDEX unicode = managedIndexCreates[i].pidxUnicode.GetNativeUnicodeIndex();
        //                nativeIndices[i].indexcreate1.indexcreate.pidxUnicode = (NATIVE_UNICODEINDEX*)handles.Add(unicode);
        //                nativeIndices[i].indexcreate1.indexcreate.grbit |= (uint)CreateIndexGrbit.IndexUnicode;
        //            }

        //            nativeIndices[i].indexcreate1.indexcreate.szKey = handles.Add(Util.ConvertToNullTerminatedUnicodeByteArray(managedIndexCreates[i].szKey));
        //            nativeIndices[i].indexcreate1.indexcreate.cbKey *= sizeof(char);
        //            nativeIndices[i].indexcreate1.indexcreate.szIndexName = handles.Add(Util.ConvertToNullTerminatedUnicodeByteArray(managedIndexCreates[i].szIndexName));
        //            nativeIndices[i].indexcreate1.indexcreate.rgconditionalcolumn = GetNativeConditionalColumns(managedIndexCreates[i].rgconditionalcolumn, true, ref handles);

        //            // Convert pSpaceHints.
        //            if (managedIndexCreates[i].pSpaceHints != null)
        //            {
        //                NATIVE_SPACEHINTS nativeSpaceHints = managedIndexCreates[i].pSpaceHints.GetNativeSpaceHints();

        //                nativeIndices[i].pSpaceHints = handles.Add(nativeSpaceHints);
        //            }
        //        }
        //    }

        //    return nativeIndices;
        //}

        // NOT IMPLEMENTED
        ///// <summary>
        ///// Creates indexes over data in an ESE database.
        ///// </summary>
        ///// <param name="sesid">The session to use.</param>
        ///// <param name="tableid">The table to create the index on.</param>
        ///// <param name="indexcreates">Array of objects describing the indexes to be created.</param>
        ///// <param name="numIndexCreates">Number of index description objects.</param>
        ///// <returns>An error code.</returns>
        //private static int CreateIndexes(JET_SESID sesid, JET_TABLEID tableid, IList<JET_INDEXCREATE> indexcreates, int numIndexCreates)
        //{
        //    // pin the memory
        //    var handles = new GCHandleCollection();
        //    try
        //    {
        //        JET_INDEXCREATE.NATIVE_INDEXCREATE[] nativeIndexcreates = GetNativeIndexCreates(indexcreates, ref handles);
        //        return Tracing.TraceResult(NativeMethods.JetCreateIndex2(sesid.Value, tableid.Value, nativeIndexcreates, checked((uint)numIndexCreates)));
        //    }
        //    finally
        //    {
        //        handles.Dispose();
        //    }
        //}

        // NOT IMPLEMENTED
        ///// <summary>
        ///// Creates indexes over data in an ESE database.
        ///// </summary>
        ///// <param name="sesid">The session to use.</param>
        ///// <param name="tableid">The table to create the index on.</param>
        ///// <param name="indexcreates">Array of objects describing the indexes to be created.</param>
        ///// <param name="numIndexCreates">Number of index description objects.</param>
        ///// <returns>An error code.</returns>
        //private static int CreateIndexes1(JET_SESID sesid, JET_TABLEID tableid, IList<JET_INDEXCREATE> indexcreates, int numIndexCreates)
        //{
        //    // pin the memory
        //    var handles = new GCHandleCollection();
        //    try
        //    {
        //        JET_INDEXCREATE.NATIVE_INDEXCREATE1[] nativeIndexcreates = GetNativeIndexCreate1s(indexcreates, ref handles);
        //        return Tracing.TraceResult(NativeMethods.JetCreateIndex2W(sesid.Value, tableid.Value, nativeIndexcreates, checked((uint)numIndexCreates)));
        //    }
        //    finally
        //    {
        //        handles.Dispose();
        //    }
        //}

        // NOT IMPLEMENTED : 
        ///// <summary>
        ///// Creates indexes over data in an ESE database.
        ///// </summary>
        ///// <param name="sesid">The session to use.</param>
        ///// <param name="tableid">The table to create the index on.</param>
        ///// <param name="indexcreates">Array of objects describing the indexes to be created.</param>
        ///// <param name="numIndexCreates">Number of index description objects.</param>
        ///// <returns>An error code.</returns>
        //private static int CreateIndexes2(JET_SESID sesid, JET_TABLEID tableid, IList<JET_INDEXCREATE> indexcreates, int numIndexCreates)
        //{
        //    // pin the memory
        //    var handles = new GCHandleCollection();
        //    try
        //    {
        //        JET_INDEXCREATE.NATIVE_INDEXCREATE2[] nativeIndexcreates = GetNativeIndexCreate2s(indexcreates, ref handles);
        //        return Tracing.TraceResult(NativeMethods.JetCreateIndex3W(sesid.Value, tableid.Value, nativeIndexcreates, checked((uint)numIndexCreates)));
        //    }
        //    finally
        //    {
        //        handles.Dispose();
        //    }
        //}

        // NOT IMPLEMENTED
        ///// <summary>
        ///// Creates a table, adds columns, and indices on that table.
        ///// </summary>
        ///// <param name="sesid">The session to use.</param>
        ///// <param name="dbid">The database to which to add the new table.</param>
        ///// <param name="tablecreate">Object describing the table to create.</param>
        ///// <returns>An error if the call fails.</returns>
        //private static int CreateTableColumnIndex3(
        //    JET_SESID sesid,
        //    JET_DBID dbid,
        //    JET_TABLECREATE tablecreate)
        //{
        //    JET_TABLECREATE.NATIVE_TABLECREATE3 nativeTableCreate = tablecreate.GetNativeTableCreate3();

        //    unsafe
        //    {
        //        var handles = new GCHandleCollection();
        //        try
        //        {
        //            // Convert/pin the column definitions.
        //            nativeTableCreate.rgcolumncreate = (NATIVE_COLUMNCREATE*)GetNativeColumnCreates(tablecreate.rgcolumncreate, true, ref handles);

        //            // Convert/pin the index definitions.
        //            JET_INDEXCREATE.NATIVE_INDEXCREATE2[] nativeIndexCreates = GetNativeIndexCreate2s(tablecreate.rgindexcreate, ref handles);
        //            nativeTableCreate.rgindexcreate = handles.Add(nativeIndexCreates);

        //            // Convert/pin the space hints.
        //            if (tablecreate.pSeqSpacehints != null)
        //            {
        //                NATIVE_SPACEHINTS nativeSpaceHints = tablecreate.pSeqSpacehints.GetNativeSpaceHints();
        //                nativeTableCreate.pSeqSpacehints = (NATIVE_SPACEHINTS*)handles.Add(nativeSpaceHints);
        //            }

        //            if (tablecreate.pLVSpacehints != null)
        //            {
        //                NATIVE_SPACEHINTS nativeSpaceHints = tablecreate.pLVSpacehints.GetNativeSpaceHints();
        //                nativeTableCreate.pLVSpacehints = (NATIVE_SPACEHINTS*)handles.Add(nativeSpaceHints);
        //            }

        //            int err = NativeMethods.JetCreateTableColumnIndex3W(sesid.Value, dbid.Value, ref nativeTableCreate);

        //            // Modified fields.
        //            tablecreate.tableid = new JET_TABLEID
        //            {
        //                Value = nativeTableCreate.tableid
        //            };

        //            tablecreate.cCreated = checked((int)nativeTableCreate.cCreated);

        //            if (tablecreate.rgcolumncreate != null)
        //            {
        //                for (int i = 0; i < tablecreate.rgcolumncreate.Length; ++i)
        //                {
        //                    tablecreate.rgcolumncreate[i].SetFromNativeColumnCreate(nativeTableCreate.rgcolumncreate[i]);
        //                }
        //            }

        //            if (tablecreate.rgindexcreate != null)
        //            {
        //                for (int i = 0; i < tablecreate.rgindexcreate.Length; ++i)
        //                {
        //                    tablecreate.rgindexcreate[i].SetFromNativeIndexCreate(nativeIndexCreates[i]);
        //                }
        //            }

        //            return Tracing.TraceResult(err);
        //        }
        //        finally
        //        {
        //            handles.Dispose();
        //        }
        //    }
        //}

        #endregion

        #region Capability Checking

        #endregion

        #region Non-static Helper Methods

        // This overload takes an IntPtr rather than a JET_INDEXID. It's meant to only be called by
        // our JetSetCurrentIndex1-3, to 'up-cast' to JetSetCurrentIndex4().

        // NOT IMPLEMENTED
        ///// <summary>
        ///// Creates a table, adds columns, and indices on that table.
        ///// </summary>
        ///// <param name="sesid">The session to use.</param>
        ///// <param name="dbid">The database to which to add the new table.</param>
        ///// <param name="tablecreate">Object describing the table to create.</param>
        ///// <returns>An error if the call fails.</returns>
        //private int CreateTableColumnIndex2(JET_SESID sesid, JET_DBID dbid, JET_TABLECREATE tablecreate)
        //{
        //    JET_TABLECREATE.NATIVE_TABLECREATE2 nativeTableCreate = tablecreate.GetNativeTableCreate2();

        //    unsafe
        //    {
        //        var handles = new GCHandleCollection();
        //        try
        //        {
        //            JET_INDEXCREATE.NATIVE_INDEXCREATE1[] nativeIndexCreate1s = null;
        //            JET_INDEXCREATE.NATIVE_INDEXCREATE[] nativeIndexCreates = null;
        //            int err;

        //            if (_capabilities.SupportsVistaFeatures)
        //            {
        //                // Convert/pin the column definitions.
        //                nativeTableCreate.rgcolumncreate = (NATIVE_COLUMNCREATE*)GetNativeColumnCreates(tablecreate.rgcolumncreate, true, ref handles);

        //                // Convert/pin the index definitions.
        //                nativeIndexCreate1s = GetNativeIndexCreate1s(tablecreate.rgindexcreate, ref handles);
        //                nativeTableCreate.rgindexcreate = handles.Add(nativeIndexCreate1s);
        //                err = NativeMethods.JetCreateTableColumnIndex2W(sesid.Value, dbid.Value, ref nativeTableCreate);
        //            }
        //            else
        //            {
        //                // Convert/pin the column definitions.
        //                nativeTableCreate.rgcolumncreate = (NATIVE_COLUMNCREATE*)GetNativeColumnCreates(tablecreate.rgcolumncreate, false, ref handles);

        //                // Convert/pin the index definitions.
        //                nativeIndexCreates = GetNativeIndexCreates(tablecreate.rgindexcreate, ref handles);
        //                nativeTableCreate.rgindexcreate = handles.Add(nativeIndexCreates);
        //                err = NativeMethods.JetCreateTableColumnIndex2(sesid.Value, dbid.Value, ref nativeTableCreate);
        //            }

        //            // Modified fields.
        //            tablecreate.tableid = new JET_TABLEID
        //                {
        //                    Value = nativeTableCreate.tableid
        //                };

        //            tablecreate.cCreated = checked((int)nativeTableCreate.cCreated);

        //            if (tablecreate.rgcolumncreate != null)
        //            {
        //                for (int i = 0; i < tablecreate.rgcolumncreate.Length; ++i)
        //                {
        //                    tablecreate.rgcolumncreate[i].SetFromNativeColumnCreate(nativeTableCreate.rgcolumncreate[i]);
        //                }
        //            }

        //            if (tablecreate.rgindexcreate != null)
        //            {
        //                for (int i = 0; i < tablecreate.rgindexcreate.Length; ++i)
        //                {
        //                    if (null != nativeIndexCreate1s)
        //                    {
        //                        tablecreate.rgindexcreate[i].SetFromNativeIndexCreate(nativeIndexCreate1s[i]);
        //                    }
        //                    else
        //                    {
        //                        tablecreate.rgindexcreate[i].SetFromNativeIndexCreate(nativeIndexCreates[i]);
        //                    }
        //                }
        //            }

        //            return Tracing.TraceResult(err);
        //        }
        //        finally
        //        {
        //            handles.Dispose();
        //        }
        //    }
        //}

        #endregion

        // --------- //
        // WINDOWS 8 //
        // --------- //

        /// <summary>Gets extended information about an error.</summary>
        /// <param name="error">The error code about which to retrieve information.</param>
        /// <param name="errinfo">Information about the specified error code.</param>
        /// <returns>An error code.</returns>
        public int JetGetErrorInfo(JET_err error, out JET_ERRINFOBASIC errinfo)
        {
            Tracing.TraceFunctionCall("JetGetErrorInfo");
            this._Capabilities.CheckSupportsWindows8Features("JetGetErrorInfo");

            NATIVE_ERRINFOBASIC nativeErrinfobasic = new NATIVE_ERRINFOBASIC();
            errinfo = new JET_ERRINFOBASIC();
            nativeErrinfobasic.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_ERRINFOBASIC)));
            int nativeErr = (int)error;
            int err = Implementation.NativeMethods.JetGetErrorInfoW(ref nativeErr, ref nativeErrinfobasic,
                nativeErrinfobasic.cbStruct, (uint)JET_ErrorInfo.SpecificErr, (uint)ErrorInfoGrbit.None);
            errinfo.SetFromNative(ref nativeErrinfobasic);

            return err;
        }

        // OMITTED : workaround method.
        ///// <summary>Creates a temporary table with a single index. A temporary table stores and
        ///// retrieves records just like an ordinary table created using JetCreateTableColumnIndex.
        ///// However, temporary tables are much faster than ordinary tables due to their volatile
        ///// nature. They can also be used to very quickly sort and perform duplicate removal on
        ///// record sets when accessed in a purely sequential manner.</summary>
        ///// <param name="sesid">The session to use.</param>
        ///// <param name="temporarytable">Description of the temporary table to create on input.
        ///// After a successful call, the structure contains the handle to the temporary table and
        ///// column identifications.</param>
        ///// <returns>An error code.</returns>
        //public int JetOpenTemporaryTable2(JET_SESID sesid, JET_OPENTEMPORARYTABLE temporarytable)
        //{
        //    Tracing.TraceFunctionCall("JetOpenTemporaryTable2");
        //    this._Capabilities.CheckSupportsWindows8Features("JetOpenTemporaryTable2");
        //    Helpers.CheckNotNull(temporarytable, "temporarytable");

        //    NATIVE_OPENTEMPORARYTABLE2 nativetemporarytable = temporarytable.GetNativeOpenTemporaryTable2();
        //    var nativecolumnids = new uint[nativetemporarytable.ccolumn];
        //    NATIVE_COLUMNDEF[] nativecolumndefs = temporarytable.prgcolumndef.GetNativecolumndefs();
        //    unsafe {
        //        using (var gchandlecollection = new GCHandleCollection()) {
        //            // Pin memory
        //            nativetemporarytable.prgcolumndef = (NATIVE_COLUMNDEF*)gchandlecollection.Add(nativecolumndefs);
        //            nativetemporarytable.rgcolumnid = (uint*)gchandlecollection.Add(nativecolumnids);
        //            if (null != temporarytable.pidxunicode) {
        //                NATIVE_UNICODEINDEX2 unicode = temporarytable.pidxunicode.GetNativeUnicodeIndex2();
        //                unicode.szLocaleName = gchandlecollection.Add(
        //                    Util.ConvertToNullTerminatedUnicodeByteArray(
        //                        temporarytable.pidxunicode.GetEffectiveLocaleName()));
        //                nativetemporarytable.pidxunicode = (NATIVE_UNICODEINDEX2*)gchandlecollection.Add(unicode);
        //            }

        //            // Call the interop method
        //            int err = Tracing.TraceResult(NativeMethods.JetOpenTemporaryTable2(sesid.Value, ref nativetemporarytable));
        //            // Convert the return values
        //            temporarytable.prgcolumndef.SetColumnids(temporarytable.prgcolumnid, nativecolumnids);
        //            temporarytable.tableid = new JET_TABLEID { Value = nativetemporarytable.tableid };
        //            return err;
        //        }
        //    }
        //}

        /// <summary>Creates a table, adds columns, and indices on that table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to which to add the new table.</param>
        /// <param name="tablecreate">Object describing the table to create.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetCreateTableColumnIndex4(JET_SESID sesid, JET_DBID dbid, JET_TABLECREATE tablecreate)
        {
            Tracing.TraceFunctionCall("JetCreateTableColumnIndex4");
            this._Capabilities.CheckSupportsWindows8Features("JetCreateTableColumnIndex4");
            Helpers.CheckNotNull(tablecreate, "tablecreate");
            return CreateTableColumnIndex4(sesid, dbid, tablecreate);
        }

        #region Session Parameters

        /// <summary>Gets a parameter on the provided session state, used for the lifetime of
        /// this session or until reset.</summary>
        /// <param name="sesid">The session to set the parameter on.</param>
        /// <param name="sesparamid">The ID of the session parameter to set, see
        /// <see cref="JET_sesparam"/> and <see cref="EsentLib.Jet.JET_sesparam"/>.</param>
        /// <param name="data">A byte array to retrieve.</param>
        /// <param name="length">AThe length of the data array.</param>
        /// <param name="actualDataSize">The actual size of the data field.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetGetSessionParameter(JET_SESID sesid, JET_sesparam sesparamid, byte[] data,
            int length, out int actualDataSize)
        {
            Tracing.TraceFunctionCall("JetGetSessionParameter");
            this._Capabilities.CheckSupportsWindows8Features("JetGetSessionParameter");
            Helpers.CheckDataSize(data, length, "length");
            int err = NativeMethods.JetGetSessionParameter(sesid.Value, (uint)sesparamid,
                data, length, out actualDataSize);
            return Tracing.TraceResult(err);
        }

        ///// <summary>
        ///// Sets a parameter on the provided session state, used for the lifetime of this session or until reset.
        ///// </summary>
        ///// <param name="sesid">The session to set the parameter on.</param>
        ///// <param name="sesparamid">The ID of the session parameter to set.</param>
        ///// <param name="data">Data to set in this session parameter.</param>
        ///// <param name="dataSize">Size of the data provided.</param>
        ///// <returns>An error if the call fails.</returns>
        //public int JetSetSessionParameter(JET_SESID sesid, JET_sesparam sesparamid, byte[] data,
        //    int dataSize)
        //{
        //    Tracing.TraceFunctionCall("JetSetSessionParameter");
        //    this.Capabilities.CheckSupportsWindows8Features("JetSetSessionParameter");
        //    Helpers.CheckNotNegative(dataSize, "dataSize");
        //    Helpers.CheckDataSize(data, dataSize, "dataSize");
        //    int err = NativeMethods.JetSetSessionParameter(sesid.Value, (uint)sesparamid, data, dataSize);
        //    return Tracing.TraceResult(err);
        //}

        #endregion

        #region prereading
        /// <summary>
        /// If the records with the specified key rangess are not in the buffer
        /// cache, then start asynchronous reads to bring the records into the
        /// database buffer cache.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to issue the prereads against.</param>
        /// <param name="indexRanges">The key rangess to preread.</param>
        /// <param name="rangeIndex">The index of the first key range in the array to read.</param>
        /// <param name="rangeCount">The maximum number of key ranges to preread.</param>
        /// <param name="rangesPreread">Returns the number of keys actually preread.</param>
        /// <param name="columnsPreread">List of column ids for long value columns to preread.</param>
        /// <param name="grbit">Preread options. Used to specify the direction of the preread.</param>
        /// <returns>
        /// An error if the call fails.
        /// </returns>
        public int JetPrereadIndexRanges(JET_SESID sesid, JET_TABLEID tableid, JET_INDEX_RANGE[] indexRanges,
            int rangeIndex, int rangeCount, out int rangesPreread, JET_COLUMNID[] columnsPreread,
            PrereadIndexRangesGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetPrereadIndexRanges");
            this._Capabilities.CheckSupportsWindows8Features("JetPrereadIndexRanges");
            Helpers.CheckNotNull(indexRanges, "indexRanges");
            Helpers.CheckDataSize(indexRanges, rangeIndex, "rangeIndex", rangeCount, "rangeCount");

            var handles = new GCHandleCollection();
            try
            {
                NATIVE_INDEX_RANGE[] nativeRanges = new NATIVE_INDEX_RANGE[rangeCount];
                for (int i = 0; i < rangeCount; i++)
                {
                    nativeRanges[i] = indexRanges[i + rangeIndex].GetNativeIndexRange(ref handles);
                }

                if (columnsPreread != null)
                {
                    var nativecolumnids = new uint[columnsPreread.Length];
                    for (int i = 0; i < columnsPreread.Length; i++)
                    {
                        nativecolumnids[i] = (uint)columnsPreread[i].Value;
                    }

                    return Tracing.TraceResult(NativeMethods.JetPrereadIndexRanges(sesid.Value, tableid.Value, nativeRanges, (uint)rangeCount, out rangesPreread, nativecolumnids, (uint)columnsPreread.Length, checked((uint)grbit)));
                }
                else
                {
                    return Tracing.TraceResult(NativeMethods.JetPrereadIndexRanges(sesid.Value, tableid.Value, nativeRanges, (uint)rangeCount, out rangesPreread, null, (uint)0, checked((uint)grbit)));
                }
            }
            finally { handles.Dispose(); }
        }

        /// <summary>
        /// If the records with the specified key ranges are not in the
        /// buffer cache then start asynchronous reads to bring the records
        /// into the database buffer cache.
        /// </summary>
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
        public int JetPrereadKeyRanges(JET_SESID sesid, JET_TABLEID tableid, byte[][] keysStart,
            int[] keyStartLengths, byte[][] keysEnd, int[] keyEndLengths, int rangeIndex, int rangeCount,
            out int rangesPreread, JET_COLUMNID[] columnsPreread, PrereadIndexRangesGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetPrereadKeyRanges");
            this._Capabilities.CheckSupportsWindows8Features("JetPrereadKeyRanges");
            Helpers.CheckDataSize(keysStart, rangeIndex, "rangeIndex", rangeCount, "rangeCount");
            Helpers.CheckDataSize(keyStartLengths, rangeIndex, "rangeIndex", rangeCount, "rangeCount");
            Helpers.CheckNotNull(keysStart, "keysStart");
            if (keysEnd != null)
            {
                Helpers.CheckNotNull(keyEndLengths, "keyEndLengths");
                Helpers.CheckDataSize(keysEnd, rangeIndex, "rangeIndex", rangeCount, "rangeCount");
            }

            if (keyEndLengths != null)
            {
                Helpers.CheckNotNull(keysEnd, "keysEnd");
                Helpers.CheckDataSize(keyEndLengths, rangeIndex, "rangeIndex", rangeCount, "rangeCount");
            }

            grbit = grbit | PrereadIndexRangesGrbit.NormalizedKey;

            using (var handles = new GCHandleCollection())
            {
                NATIVE_INDEX_COLUMN[] startColumn;
                NATIVE_INDEX_COLUMN[] endColumn;
                NATIVE_INDEX_RANGE[] ranges = new NATIVE_INDEX_RANGE[rangeCount];
                for (int i = 0; i < rangeCount; i++)
                {
                    startColumn = new NATIVE_INDEX_COLUMN[1];
                    startColumn[0].pvData = handles.Add(keysStart[i + rangeIndex]);
                    startColumn[0].cbData = (uint)keyStartLengths[i + rangeIndex];
                    ranges[i].rgStartColumns = handles.Add(startColumn);
                    ranges[i].cStartColumns = 1;
                    if (keysEnd != null)
                    {
                        endColumn = new NATIVE_INDEX_COLUMN[1];
                        endColumn[0].pvData = handles.Add(keysEnd[i + rangeIndex]);
                        endColumn[0].cbData = (uint)keyEndLengths[i + rangeIndex];
                        ranges[i].rgEndColumns = handles.Add(endColumn);
                        ranges[i].cEndColumns = 1;
                    }
                }

                if (columnsPreread != null)
                {
                    var nativecolumnids = new uint[columnsPreread.Length];
                    for (int i = 0; i < columnsPreread.Length; i++)
                    {
                        nativecolumnids[i] = (uint)columnsPreread[i].Value;
                    }

                    return Tracing.TraceResult(NativeMethods.JetPrereadIndexRanges(sesid.Value, tableid.Value, ranges, (uint)rangeCount, out rangesPreread, nativecolumnids, (uint)columnsPreread.Length, checked((uint)grbit)));
                }
                else
                {
                    return Tracing.TraceResult(NativeMethods.JetPrereadIndexRanges(sesid.Value, tableid.Value, ranges, (uint)rangeCount, out rangesPreread, null, (uint)0, checked((uint)grbit)));
                }
            }
        }

        /// <summary>Set an array of simple filters for <see cref="IJetTable.Move"/></summary>
        /// <param name="sesid">The session to use for the call.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <param name="filters">Simple record filters.</param>
        /// <param name="grbit">Move options.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetSetCursorFilter(JET_SESID sesid, JET_TABLEID tableid, JET_INDEX_COLUMN[] filters, CursorFilterGrbit grbit)
        {
            Tracing.TraceFunctionCall("JetSetCursorFilter");
            this._Capabilities.CheckSupportsWindows8Features("JetSetCursorFilter");

            if (filters == null || filters.Length == 0)
            {
                return Tracing.TraceResult(NativeMethods.JetSetCursorFilter(sesid.Value, tableid.Value, null, 0, checked((uint)grbit)));
            }

            var handles = new GCHandleCollection();
            try
            {
                NATIVE_INDEX_COLUMN[] nativeFilters = new NATIVE_INDEX_COLUMN[filters.Length];

                for (int i = 0; i < filters.Length; i++)
                {
                    nativeFilters[i] = filters[i].GetNativeIndexColumn(ref handles);
                }

                return Tracing.TraceResult(NativeMethods.JetSetCursorFilter(sesid.Value, tableid.Value, nativeFilters, (uint)filters.Length, checked((uint)grbit)));
            }
            finally
            {
                handles.Dispose();
            }
        }
        #endregion

        #region Private utility functions

        /// <summary>
        /// Creates a table, adds columns, and indices on that table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to which to add the new table.</param>
        /// <param name="tablecreate">Object describing the table to create.</param>
        /// <returns>An error if the call fails.</returns>
        private static int CreateTableColumnIndex4(
            JET_SESID sesid,
            JET_DBID dbid,
            JET_TABLECREATE tablecreate)
        {
            NATIVE_TABLECREATE4 nativeTableCreate = tablecreate.GetNativeTableCreate4();

            unsafe
            {
                var handles = new GCHandleCollection();
                try
                {
                    // Convert/pin the column definitions.
                    nativeTableCreate.rgcolumncreate = (NATIVE_COLUMNCREATE*)GetNativeColumnCreates(tablecreate.rgcolumncreate, true, ref handles);

                    // Convert/pin the index definitions.
                    throw new NotImplementedException(); // For compilation purpose
                    // NATIVE_INDEXCREATE3[] nativeIndexCreates = GetNativeIndexCreate3s(tablecreate.rgindexcreate, ref handles);
                    ////nativeTableCreate.rgindexcreate = handles.Add(nativeIndexCreates);

                    ////// Convert/pin the space hints.
                    ////if (tablecreate.pSeqSpacehints != null)
                    ////{
                    ////    NATIVE_SPACEHINTS nativeSpaceHints = tablecreate.pSeqSpacehints.GetNativeSpaceHints();
                    ////    nativeTableCreate.pSeqSpacehints = (NATIVE_SPACEHINTS*)handles.Add(nativeSpaceHints);
                    ////}

                    ////if (tablecreate.pLVSpacehints != null)
                    ////{
                    ////    NATIVE_SPACEHINTS nativeSpaceHints = tablecreate.pLVSpacehints.GetNativeSpaceHints();
                    ////    nativeTableCreate.pLVSpacehints = (NATIVE_SPACEHINTS*)handles.Add(nativeSpaceHints);
                    ////}

                    ////int err = NativeMethods.JetCreateTableColumnIndex4W(sesid.Value, dbid.Value, ref nativeTableCreate);

                    ////// Modified fields.
                    ////tablecreate.tableid = new JET_TABLEID
                    ////{
                    ////    Value = nativeTableCreate.tableid
                    ////};

                    ////tablecreate.cCreated = checked((int)nativeTableCreate.cCreated);

                    ////if (tablecreate.rgcolumncreate != null)
                    ////{
                    ////    for (int i = 0; i < tablecreate.rgcolumncreate.Length; ++i)
                    ////    {
                    ////        tablecreate.rgcolumncreate[i].SetFromNativeColumnCreate(nativeTableCreate.rgcolumncreate[i]);
                    ////    }
                    ////}

                    ////if (tablecreate.rgindexcreate != null)
                    ////{
                    ////    for (int i = 0; i < tablecreate.rgindexcreate.Length; ++i)
                    ////    {
                    ////        tablecreate.rgindexcreate[i].SetFromNativeIndexCreate(nativeIndexCreates[i]);
                    ////    }
                    ////}

                    ////return Tracing.TraceResult(err);
                }
                finally
                {
                    handles.Dispose();
                }
            }
        }

        // Do not add new public functions here, go above private functions above ...
        #endregion

        // ---------- //
        // WINDOWS 10 //
        // ---------- //
        ///// <summary>Sets a parameter on the provided session state, used for the lifetime of this session or until reset.</summary>
        ///// <param name="sesid">The session to set the parameter on.</param>
        ///// <param name="sesparamid">The ID of the session parameter to set.</param>
        ///// <param name="operationContext">An operation context to set.</param>
        ///// <returns>An error code.</returns>
        //public int JetGetSessionParameter(JET_SESID sesid, JET_sesparam sesparamid,
        //    out JET_OPERATIONCONTEXT operationContext)
        //{
        //    Tracing.TraceFunctionCall("JetGetSessionParameter");
        //    _capabilities.CheckSupportsWindows10Features("JetGetSessionParameter");
        //    int err;
        //    int actualDataSize;

        //    NATIVE_OPERATIONCONTEXT nativeContext = new NATIVE_OPERATIONCONTEXT();
        //    int dataSize = Marshal.SizeOf(nativeContext);

        //    err = EsentLib.Implementation.NativeMethods.JetGetSessionParameter(sesid.Value,
        //        (uint)sesparamid, out nativeContext, dataSize, out actualDataSize);

        //    if (err >= (int)JET_err.Success) {
        //        if (actualDataSize != dataSize) {
        //            throw new ArgumentException(
        //                string.Format(CultureInfo.InvariantCulture,
        //                    "Bad return value. Unexpected data size returned. Expected {0}, but received {1}.",
        //                    dataSize, actualDataSize),
        //                "sesparamid");
        //        }
        //    }
        //    operationContext = new JET_OPERATIONCONTEXT(ref nativeContext);
        //    return Tracing.TraceResult(err);
        //}

        ///// <summary>
        ///// Sets a parameter on the provided session state, used for the lifetime of this session or until reset.
        ///// </summary>
        ///// <param name="sesid">The session to set the parameter on.</param>
        ///// <param name="sesparamid">The ID of the session parameter to set.</param>
        ///// <param name="operationContext">An operation context to set.</param>
        ///// <returns>An error code.</returns>
        //public int JetSetSessionParameter(JET_SESID sesid, JET_sesparam sesparamid,
        //    JET_OPERATIONCONTEXT operationContext)
        //{
        //    Tracing.TraceFunctionCall("JetSetSessionParameter");
        //    _capabilities.CheckSupportsWindows10Features("JetSetSessionParameter");
        //    NATIVE_OPERATIONCONTEXT nativeContext = operationContext.GetNativeOperationContext();
        //    int dataSize = Marshal.SizeOf(nativeContext);
        //    int err = NativeMethods.JetSetSessionParameter(sesid.Value, (uint)sesparamid, ref nativeContext,
        //        checked((int)dataSize));
        //    return Tracing.TraceResult(err);
        //}

        #region Sessions

        #endregion

        /// <summary>A dictionary of active sessions keyed by the identifier of the thread
        /// they were born from.</summary>
        private Dictionary<int, List<JetSession>> _activeSessions = new Dictionary<int, List<JetSession>>();
        /// <summary>Callback wrapper collection. This is used for long-running callbacks
        /// (callbacks which can be called after the API call returns). Create a wrapper
        /// here and occasionally clean them up.</summary>
        private readonly CallbackWrappers callbackWrappers = new CallbackWrappers();
        private JetCapabilities _capabilities;
        /// <summary>The native istance object.</summary>
        protected JET_INSTANCE _instance;
        private static Dictionary<IJetSession, JetSession> _perInterfaceSession =
            new Dictionary<IJetSession, JetSession>();
        private Dictionary<JetSession, int> _perSessionThreadId = new Dictionary<JetSession, int>();
        /// <summary>The true engine version as retrieved from the ESENT DLL. 0 until
        /// properly retrieved from the underlying engine.</summary>
        private uint _version = 0;
        /// <summary>The version of esent. If this is zero then it is looked up with
        /// JetGetVersion.</summary>
        private readonly uint versionOverride;
    }
}

// TODO : Have this code included in the above class. This is MANDATORY for safety.
//using System;
//using System.Diagnostics.CodeAnalysis;
//using System.Globalization;
//using System.Runtime.CompilerServices;
//using System.Security.Permissions;

//using EsentLib.Jet;
//using EsentLib.Jet.Types;
//using EsentLib.Platform.Vista;
//using Microsoft.Win32.SafeHandles;

//namespace EsentLib
//{
//    /// <summary>
//    /// A class that encapsulates a <see cref="JET_INSTANCE"/> in a disposable object. The
//    /// instance must be closed last and closing the instance releases all other
//    /// resources for the instance.
//    /// </summary>
//    public class Instance : SafeHandleZeroOrMinusOneIsInvalid
//    {
//        /// <summary>
//        /// Parameters for the instance.
//        /// </summary>
//        private readonly EsentLib.Platform.Windows8.InstanceParameters parameters;

//        /// <summary>
//        /// The name of the instance.
//        /// </summary>
//        private readonly string name;

//        /// <summary>The display name of the instance.</summary>
//        private readonly string displayName;

//        /// <summary>The TermGrbit to be used at JetTerm time.</summary>
//        private TermGrbit termGrbit;

//        /// <summary>
//        /// Initializes a new instance of the Instance class. The underlying
//        /// JET_INSTANCE is allocated, but not initialized.
//        /// </summary>
//        /// <param name="name">
//        /// The name of the instance. This string must be unique within a
//        /// given process hosting the database engine.
//        /// </param>
//        /// <param name="displayName">
//        /// A display name for the instance. This will be used in eventlog
//        /// entries.
//        /// </param>
//        /// <param name="termGrbit">
//        /// The TermGrbit to be used at JetTerm time.
//        /// </param>
//        [SecurityPermissionAttribute(SecurityAction.LinkDemand)]
//        public Instance(string name, string displayName, TermGrbit termGrbit) : base(true)
//        {
//            this.name = name;
//            this.displayName = displayName;
//            this.termGrbit = termGrbit;

//            JET_INSTANCE instance;
//            RuntimeHelpers.PrepareConstrainedRegions();
//            try { this.SetHandle(JET_INSTANCE.Nil.Value); }
//            finally
//            {
//                // This is the code that we want in a constrained execution region.
//                // We need to avoid the situation where JetCreateInstance2 is called
//                // but the handle isn't set, so the instance is never terminated.
//                // This would happen, for example, if there was a ThreadAbortException
//                // between the call to JetCreateInstance2 and the call to SetHandle.
//                // If an Esent exception is generated we do not want to call SetHandle
//                // because the instance isn't valid. On the other hand if a different 
//                // exception (out of memory or thread abort) is generated we still need
//                // to set the handle to avoid losing track of the instance. The call to
//                // JetCreateInstance2 is in the CER to make sure that the only exceptions
//                // which can be generated are from ESENT failures.
//                // JetCreateInstance2 is included in ::Create
//                Api.JetCreateInstance2(out instance, this.name, this.displayName, CreateInstanceGrbit.None);
//                this.SetHandle(instance.Value);
//            }
//            this.parameters = new EsentLib.Platform.Windows8.InstanceParameters(instance);
//        }

//        /// <summary>
//        /// Gets the JET_INSTANCE that this instance contains.
//        /// </summary>
//        public JET_INSTANCE JetInstance
//        {
//            [SecurityPermissionAttribute(SecurityAction.LinkDemand)]
//            get
//            {
//                this.CheckObjectIsNotDisposed();
//                return this.CreateInstanceFromHandle();
//            }
//        }

//        /// <summary>
//        /// Gets the InstanceParameters for this instance. 
//        /// </summary>
//        public EsentLib.Platform.Windows8.InstanceParameters Parameters
//        {
//            [SecurityPermissionAttribute(SecurityAction.LinkDemand)]
//            get
//            {
//                this.CheckObjectIsNotDisposed();
//                return this.parameters;
//            }
//        }

//        /// <summary>
//        /// Gets or sets the TermGrbit for this instance. 
//        /// </summary>
//        public TermGrbit TermGrbit
//        {
//            [SecurityPermissionAttribute(SecurityAction.LinkDemand)]
//            get
//            {
//                this.CheckObjectIsNotDisposed();
//                return this.termGrbit;
//            }

//            [SecurityPermissionAttribute(SecurityAction.LinkDemand)]
//            set
//            {
//                this.CheckObjectIsNotDisposed();
//                this.termGrbit = value;
//            }
//        }

//        /// <summary>
//        /// Provide implicit conversion of an Instance object to a JET_INSTANCE
//        /// structure. This is done so that an Instance can be used anywhere a
//        /// JET_INSTANCE is required.
//        /// </summary>
//        /// <param name="instance">The instance to convert.</param>
//        /// <returns>The JET_INSTANCE wrapped by the instance.</returns>
//        [SecurityPermissionAttribute(SecurityAction.LinkDemand)]
//        public static implicit operator JET_INSTANCE(Instance instance)
//        {
//            return instance.JetInstance;
//        }

//        /// <summary>
//        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="Instance"/>.
//        /// </summary>
//        /// <returns>
//        /// A <see cref="T:System.String"/> that represents the current <see cref="Instance"/>.
//        /// </returns>
//        public override string ToString()
//        {
//            return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", this.displayName, this.name);
//        }

//        /// <summary>
//        /// Initialize the JET_INSTANCE.
//        /// </summary>
//        [SecurityPermissionAttribute(SecurityAction.LinkDemand)]
//        public void Init()
//        {
//            this.Init(InitGrbit.None);
//        }

//        /// <summary>
//        /// Initialize the JET_INSTANCE.
//        /// </summary>
//        /// <param name="grbit">
//        /// Initialization options.
//        /// </param>
//        [SecurityPermissionAttribute(SecurityAction.LinkDemand)]
//        public void Init(InitGrbit grbit)
//        {
//            this.CheckObjectIsNotDisposed();
//            JET_INSTANCE instance = this.JetInstance;

//            // Use a constrained region so that the handle is
//            // always set after JetInit2 is called.
//            RuntimeHelpers.PrepareConstrainedRegions();
//            try
//            {
//                // Remember that a failure in JetInit can zero the handle
//                // and that JetTerm should not be called in that case.
//                Api.JetInit2(ref instance, grbit);
//            }
//            finally
//            {
//                this.SetHandle(instance.Value);
//            }
//        }

//        /// <summary>
//        /// Initialize the JET_INSTANCE. This API requires at least the
//        /// Vista version of ESENT.
//        /// </summary>
//        /// <param name="recoveryOptions">
//        /// Additional recovery parameters for remapping databases during
//        /// recovery, position where to stop recovery at, or recovery status.
//        /// </param>
//        /// <param name="grbit">
//        /// Initialization options.
//        /// </param>
//        [SecurityPermissionAttribute(SecurityAction.LinkDemand)]
//        public void Init(JET_RSTINFO recoveryOptions, InitGrbit grbit)
//        {
//            this.CheckObjectIsNotDisposed();
//            JET_INSTANCE instance = this.JetInstance;

//            // Use a constrained region so that the handle is
//            // always set after JetInit3 is called.
//            RuntimeHelpers.PrepareConstrainedRegions();
//            try
//            {
//                // Remember that a failure in JetInit can zero the handle
//                // and that JetTerm should not be called in that case.
//                // Replaced by Iitialize
//                VistaApi.JetInit3(ref instance, recoveryOptions, grbit);
//            }
//            finally
//            {
//                this.SetHandle(instance.Value);
//            }
//        }

//        /// <summary>
//        /// Terminate the JET_INSTANCE.
//        /// </summary>
//        [SuppressMessage(
//            "Microsoft.StyleCop.CSharp.MaintainabilityRules",
//            "SA1409:RemoveUnnecessaryCode",
//            Justification = "CER code belongs in the finally block, so the try clause is empty")]
//        [SecurityPermissionAttribute(SecurityAction.LinkDemand)]
//        public void Term()
//        {
//            // Use a constrained region so that the handle is
//            // always set as invalid after JetTerm is called.
//            RuntimeHelpers.PrepareConstrainedRegions();
//            try
//            {
//                // This try block deliberately left blank.
//            }
//            finally
//            {
//                // This is the code that we want in a constrained execution region.
//                // We need to avoid the situation where JetTerm is called
//                // but the handle isn't invalidated, so the instance is terminated again.
//                // This would happen, for example, if there was a ThreadAbortException
//                // between the call to JetTerm and the call to SetHandle.
//                //
//                // If an Esent exception is generated we do not want to invalidate the handle
//                // because the instance isn't necessarily terminated. On the other hand if a 
//                // different exception (out of memory or thread abort) is generated we still need
//                // to invalidate the handle.
//                try
//                {
//                    Api.JetTerm2(this.JetInstance, this.termGrbit);
//                }
//                catch (EsentDirtyShutdownException)
//                {
//                    this.SetHandleAsInvalid();
//                    throw;
//                }

//                this.SetHandleAsInvalid();
//            }
//        }

//        /// <summary>
//        /// Release the handle for this instance.
//        /// </summary>
//        /// <returns>True if the handle could be released.</returns>
//        protected override bool ReleaseHandle()
//        {
//            // The object is already marked as invalid so don't check
//            var instance = this.CreateInstanceFromHandle();
//            return (int)JET_err.Success == Api.Impl.JetTerm2(instance, this.termGrbit);
//        }

//        /// <summary>
//        /// Create a JET_INSTANCE from the internal handle value.
//        /// </summary>
//        /// <returns>A JET_INSTANCE containing the internal handle.</returns>
//        private JET_INSTANCE CreateInstanceFromHandle()
//        {
//            return new JET_INSTANCE { Value = this.handle };
//        }

//        /// <summary>
//        /// Check to see if this instance is invalid or closed.
//        /// </summary>
//        [SecurityPermissionAttribute(SecurityAction.LinkDemand)]
//        private void CheckObjectIsNotDisposed()
//        {
//            if (this.IsInvalid || this.IsClosed)
//            {
//                throw new ObjectDisposedException("Instance");
//            }
//        }
//    }
//}
