using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

using EsentLib.Implementation;
using EsentLib.Jet;
using EsentLib.Jet.Types;

namespace EsentLib
{
    /// <summary></summary>
    [CLSCompliant(false)]
    public class JetEnvironment : IJetEnvironment
    {
        private JetEnvironment()
        {
            return;
        }

        #region PROPERTIES
        /// <summary>Gets the maximum size of a bookmark. <seealso cref="Api.JetGetBookmark"/>.</summary>
        public int BookmarkMost
        {
            // This correctly returns 256 on pre-Vista systems
            get { return KeyMost + 1; }
        }

        /// <summary>Gets or sets the size of the database cache in pages. By default the
        /// database cache will automatically tune its size, setting this property to a non-zero
        /// value will cause the cache to adjust itself to the target size. </summary>
        public int CacheSize
        {
            get { return NativeHelpers.GetInt32Parameter(JET_param.CacheSize); }
            set { SetParameter(JET_param.CacheSize, value); }
        }

        /// <summary>Gets or sets the maximum size of the database page cache. The size is in
        /// database pages. If this parameter is left to its default value, then the maximum
        /// size of the cache will be set to the size of physical memory when JetInit is called.
        /// </summary>
        public int CacheSizeMax
        {
            get { return NativeHelpers.GetInt32Parameter(JET_param.CacheSizeMax); }
            set { SetParameter(JET_param.CacheSizeMax, value); }
        }

        /// <summary>Gets or sets the minimum size of the database page cache, in database pages.
        /// </summary>
        public int CacheSizeMin
        {
            get { return NativeHelpers.GetInt32Parameter(JET_param.CacheSizeMin); }
            set { SetParameter(JET_param.CacheSizeMin, value); }
        }

        /// <summary>Gets the maximum number of components in a sort or index key.</summary>
        public int ColumnsKeyMost
        {
            get { return Api.Impl.Capabilities.ColumnsKeyMost; }
        }

        /// <summary>Gets or sets a value specifying the default values for the entire set
        /// of system parameters. When this parameter is set to a specific configuration, all
        /// system parameter values are reset to their default values for that configuration.
        /// If the configuration is set for a specific instance then global system parameters
        /// will not be reset to their default values. Small Configuration (0): The database
        /// engine is optimized for memory use.  Legacy Configuration (1): The database engine
        /// has its traditional defaults.
        /// <para>Supported on Windows Vista and up. Ignored on Windows XP and Windows Server
        /// 2003.</para>
        /// </summary>
        public int Configuration
        {
            get
            {
                return EsentVersion.SupportsVistaFeatures
                    ? NativeHelpers.GetInt32Parameter(JET_param.Configuration)
                    : 1;
            }
            set
            {
                if (EsentVersion.SupportsVistaFeatures) {
                    JetEnvironment.SetParameter(JET_param.Configuration, value);
                }
            }
        }

        /// <summary>Get the instance that will get you access to the JET environment.</summary>
        [CLSCompliant(false)]
        public static IJetEnvironment Current
        {
            get { return _singleton; }
        }

        /// <summary>Gets or sets the size of the database pages, in bytes.</summary>
        public int DatabasePageSize
        {
            get { return NativeHelpers.GetInt32Parameter(JET_param.DatabasePageSize); }
            set { SetParameter(JET_param.DatabasePageSize, value); }
        }

        /// <summary>Gets the true capabilities of this implementation of ESENT.</summary>
        public JetCapabilities DefaultCapabilities
        {
            get
            {
                if (null == _defaultCapabilities) {
                    // JetGetVersion isn't available in new Windows UI, so we'll pretend it's always Win8:
                    _defaultCapabilities = DetermineCapabilities(8250 << 8);
                }
                return _defaultCapabilities;
            }
        }

        /// <summary>Gets or sets the detail level of eventlog messages that are emitted to the
        /// eventlog by the database engine. Higher numbers will result in more detailed eventlog
        /// messages.</summary>
        public int EventLoggingLevel
        {
            get { return NativeHelpers.GetInt32Parameter(JET_param.EventLoggingLevel); }
            set { SetParameter(JET_param.EventLoggingLevel, value); }
        }

        /// <summary>Gets or sets the value encoding what to do with exceptions generated
        /// within JET.</summary>
        public JET_ExceptionAction ExceptionAction
        {
            get { return (JET_ExceptionAction)NativeHelpers.GetInt32Parameter(JET_param.ExceptionAction); }
            set { SetParameter(JET_param.ExceptionAction, (int)value); }
        }

        /// <summary>Gets or sets the set of actions to be taken on IOs that appear hung.</summary>
        public int HungIOActions
        {
            get { return NativeHelpers.GetInt32Parameter(JET_param.HungIOActions); }
            set { JetEnvironment.SetParameter(JET_param.HungIOActions, value); }
        }

        /// <summary>Gets or sets the threshold for what is considered a hung IO that should
        /// be acted upon.</summary>
        public int HungIOThreshold
        {
            get { return NativeHelpers.GetInt32Parameter(JET_param.HungIOThreshold); }
            set { SetParameter(JET_param.HungIOThreshold, value); }
        }

        /// <summary>Gets the maximum key size. This depends on the Esent version and database
        /// page size.</summary>
        public int KeyMost
        {
            get
            {
                return EsentVersion.SupportsVistaFeatures
                    ? NativeHelpers.GetInt32Parameter(JET_param.KeyMost)
                    : 255;
            }
        }

        /// <summary>Gets or sets backwards compatibility with the file naming conventions of earlier
        /// releases of the database engine.</summary>
        public int LegacyFileNames
        {
            get
            {
                return (EsentVersion.SupportsVistaFeatures)
                    ? NativeHelpers.GetInt32Parameter(JET_param.LegacyFileNames)
                    : 1;
            }
            set
            {
                if (EsentVersion.SupportsVistaFeatures) {
                    JetEnvironment.SetParameter(JET_param.LegacyFileNames, value);
                }
            }
        }

        /// <summary>Gets the lv chunks size. This depends on the database page size.</summary>
        public int LVChunkSizeMost
        {
            get
            {
                if (EsentVersion.SupportsWindows7Features) {
                    return NativeHelpers.GetInt32Parameter(JET_param.LVChunkSizeMost);
                }
                // Can't retrieve the size directly, determine it from the database page size
                const int ColumnLvPageOverhead = 82;
                return NativeHelpers.GetInt32Parameter(JET_param.DatabasePageSize) - ColumnLvPageOverhead;
            }
        }

        /// <summary>Gets or sets the maximum number of instances that can be created.</summary>
        public int MaxInstances
        {
            get { return NativeHelpers.GetInt32Parameter(JET_param.MaxInstances); }
            set { SetParameter(JET_param.MaxInstances, value); }
        }

        /// <summary>Gets or sets the smallest amount of data that should be compressed with xpress
        /// compression.</summary>
        public int MinDataForXpress
        {
            get { return NativeHelpers.GetInt32Parameter(JET_param.MinDataForXpress); }
            set { JetEnvironment.SetParameter(JET_param.MinDataForXpress, value); }
        }

        /// <summary>Gets or sets how many database file I/Os can be queued per-disk in the
        /// host operating system at one time. A larger value for this parameter can significantly
        /// help the performance of a large database application. </summary>
        public int OutstandingIOMax
        {
            get { return NativeHelpers.GetInt32Parameter(JET_param.OutstandingIOMax); }
            set { SetParameter(JET_param.OutstandingIOMax, value); }
        }

        /// <summary>Gets or sets the friendly name for this instance of the process.</summary>
        public string ProcessFriendlyName
        {
            get { return NativeHelpers.GetStringParameter(JET_param.ProcessFriendlyName); }
            set { SetParameter(JET_param.ProcessFriendlyName, value); }
        }

        /// <summary>Gets or sets the threshold at which the database page cache begins evicting
        /// pages from the cache to make room for pages that are not cached. When the number of
        /// page buffers in the cache drops below this threshold then a background process will
        /// be started to replenish that pool of available buffers. This threshold is always
        /// relative to the maximum cache size as set by JET_paramCacheSizeMax. This threshold
        /// must also always be less than the stop threshold as set by JET_paramStopFlushThreshold.
        /// <para> The distance height of the start threshold will determine the response time
        /// that the database page cache must have to produce available buffers before the
        /// application needs them. A high start threshold will give the background process more
        /// time to react. However, a high start threshold implies a higher stop threshold and
        /// that will reduce the effective size of the database page cache.</para>
        /// </summary>
        public int StartFlushThreshold
        {
            get { return NativeHelpers.GetInt32Parameter(JET_param.StartFlushThreshold); }
            set { SetParameter(JET_param.StartFlushThreshold, value); }
        }

        /// <summary>Gets or sets the threshold at which the database page cache ends evicting pages
        /// from the cache to make room for pages that are not cached. When the number of page buffers
        /// in the cache rises above this threshold then the background process that was started to
        /// replenish that pool of available buffers is stopped. This threshold is always relative 
        /// the maximum cache size as set by JET_paramCacheSizeMax. This threshold must also always
        /// be greater than the start threshold as set by JET_paramStartFlushThreshold.
        /// <para>The distance between the start threshold and the stop threshold affects the efficiency
        /// with which database pages are flushed by the background process. A larger gap will make it
        /// more likely that writes to neighboring pages may be combined. However, a high stop threshold
        /// will reduce the effective size of the database page cache.</para>
        /// </summary>
        public int StopFlushThreshold
        {
            get { return NativeHelpers.GetInt32Parameter(JET_param.StopFlushThreshold); }
            set { SetParameter(JET_param.StopFlushThreshold, value); }
        }

        /// <summary>Gets the true capabilities of this implementation of ESENT.</summary>
        public JetCapabilities TrueCapabilities
        {
            get
            {
                if (null == _trueCapabilities) { DetermineCapabilities(); }
                return _trueCapabilities;
            }
        }
        #endregion

        /// <summary>Calculates the capabilities of the current Esent version.</summary>
        internal static JetCapabilities DetermineCapabilities(uint versionOverride = 0)
        {
            const int Server2003BuildNumber = 2700;
            const int VistaBuildNumber = 6000;
            const int Windows7BuildNumber = 7000; // includes beta as well as RTM (RTM is 7600)
            const int Windows8BuildNumber = 8000; // includes beta as well as RTM (RTM is 9200)
            const int Windows81BuildNumber = 9300; // includes beta as well as RTM (RTM is 9600)
            const int Windows10BuildNumber = 9900; // includes beta as well as RTM (RTM is TBD)

            // Create new capabilities, set as all false. This will allow us to call into Esent.
            JetCapabilities result = new JetCapabilities { ColumnsKeyMost = 12 };

            uint version = versionOverride;
            if (0 == version) {
                version = GetInstalledVersion();
                _defaultCapabilities = result;
            }
            int buildNumber = (int)((version & 0xFFFFFF) >> 8);
            Tracing.TraceVerboseLine(string.Format(CultureInfo.InvariantCulture, "Version = {0}, BuildNumber = {1}", version, buildNumber));
            if (buildNumber >= Server2003BuildNumber) {
                Tracing.TraceVerboseLine("Supports Server 2003 features");
                result.SupportsServer2003Features = true;
            }
            if (buildNumber >= VistaBuildNumber) {
                Tracing.TraceVerboseLine("Supports Vista features");
                result.SupportsVistaFeatures = true;
                Tracing.TraceVerboseLine("Supports Unicode paths");
                result.SupportsUnicodePaths = true;
                Tracing.TraceVerboseLine("Supports large keys");
                result.SupportsLargeKeys = true;
                Tracing.TraceVerboseLine("Supports 16-column keys");
                result.ColumnsKeyMost = 16;
            }
            if (Windows7BuildNumber <= buildNumber) {
                Tracing.TraceVerboseLine("Supports Windows 7 features");
                result.SupportsWindows7Features = true;
            }
            if (Windows8BuildNumber <= buildNumber) {
                Tracing.TraceVerboseLine("Supports Windows 8 features");
                result.SupportsWindows8Features = true;
            }
            if (Windows81BuildNumber <= buildNumber) {
                Tracing.TraceVerboseLine("Supports Windows 8.1 features");
                result.SupportsWindows81Features = true;
            }
            if (Windows10BuildNumber <= buildNumber) {
                Tracing.TraceVerboseLine("Supports Windows 10 features");
                result.SupportsWindows10Features = true;
            }
            return result;
        }

        /// <summary>Frees memory that was allocated by a database engine call.</summary>
        /// <param name="buffer">The buffer allocated by a call to the database engine.
        /// <see cref="IntPtr.Zero"/> is acceptable, and will be ignored.</param>
        /// <returns>An error code.</returns>
        internal static void FreeJetBuffer(IntPtr buffer)
        {
            Tracing.TraceFunctionCall("FreeJetBuffer");
            int returnCode = NativeMethods.JetFreeBuffer(buffer);
            Tracing.TraceResult(returnCode);
        }

        internal static string GetErrorText(ref IntPtr errorNumber)
        {
            return NativeHelpers.GetErrorText(ref errorNumber);
        }

        /// <summary>Create an instance and get the current version of Esent.</summary>
        /// <returns>The current version of Esent.</returns>
        private static uint GetInstalledVersion()
        {
            // Create a unique name so that multiple threads can call this simultaneously.
            // This can happen if there are multiple AppDomains.
            string instanceName = string.Format(CultureInfo.InvariantCulture,
                "GettingEsentVersion{0}", LibraryHelpers.GetCurrentManagedThreadId());
            JET_INSTANCE instance = JET_INSTANCE.Nil;
            RuntimeHelpers.PrepareConstrainedRegions();
            using (JetInstance engine = JetInstance.Create(instanceName)) {
                engine.SetSystemParameter(JET_SESID.Nil, JET_param.Recovery, new IntPtr(0), "off");
                engine.SetSystemParameter(JET_SESID.Nil, JET_param.NoInformationEvent, new IntPtr(1), null);
                engine.SetSystemParameter(JET_SESID.Nil, JET_param.MaxTemporaryTables, new IntPtr(0), null);
                engine.SetSystemParameter(JET_SESID.Nil, JET_param.MaxCursors, new IntPtr(16), null);
                engine.SetSystemParameter(JET_SESID.Nil, JET_param.MaxOpenTables, new IntPtr(16), null);
                engine.SetSystemParameter(JET_SESID.Nil, JET_param.MaxVerPages, new IntPtr(4), null);
                engine.SetSystemParameter(JET_SESID.Nil, JET_param.MaxSessions, new IntPtr(1), null);
                engine.Initialize();
                return engine.GetVersion();
            }
        }

        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="displayName"></param>
        /// <param name="grbit"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public IJetInstance GetInstance(string name, string displayName = null,
            CreateInstanceGrbit grbit = CreateInstanceGrbit.None)
        {
            return JetInstance.Create(name, displayName, grbit);
        }

        /// <summary>Retrieves information about the instances that are running.</summary>
        /// <returns>An error code if the call fails.</returns>
        public JET_INSTANCE_INFO[] GetRunningInstancesInfo()
        {
            Tracing.TraceFunctionCall("GetRunningInstancesInfo");
            unsafe {
                uint nativeNumInstance = 0;

                // Esent will allocate memory which will be freed by the ConvertInstanceInfos call.
                NATIVE_INSTANCE_INFO* nativeInstanceInfos = null;
                int returnCode = NativeMethods.JetGetInstanceInfoW(out nativeNumInstance, out nativeInstanceInfos);
                try { return JET_INSTANCE_INFO.FromNativeCollection(nativeNumInstance, nativeInstanceInfos); }
                finally { Tracing.TraceResult(returnCode); }
            }
        }

        /// <summary>Retrieves performance information from the database engine for the
        /// current thread. Multiple calls can be used to collect statistics that reflect
        /// the activity of the database engine on this thread between those calls.</summary>
        /// <returns>Returns the thread statistics.</returns>
        public EsentLib.Jet.Vista.JET_THREADSTATS GetThreadStatistics()
        {
            Tracing.TraceFunctionCall("GetThreadStatistics");
            TrueCapabilities.CheckSupportsVistaFeatures("GetThreadStatistics");

            // To speed up the interop we use unsafe code to avoid initializing
            // the out parameter. We just call the interop code.
            EsentLib.Jet.Vista.JET_THREADSTATS result = new EsentLib.Jet.Vista.JET_THREADSTATS();
            int returnCode;
            unsafe {
                returnCode = NativeMethods.JetGetThreadStats(&result, checked((uint)EsentLib.Jet.Vista.JET_THREADSTATS.Size));
            }
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return result;
        }

        /// <summary>Retrieves performance information from the database engine for the current
        /// thread. Multiple calls can be used to collect statistics that reflect the activity
        /// of the database engine on this thread between those calls.</summary>
        /// <returns>An error code if the operation fails.</returns>
        public JET_THREADSTATS2 GetThreadStatisticsEx()
        {
            Tracing.TraceFunctionCall("GetThreadStatisticsEx");
            TrueCapabilities.CheckSupportsVistaFeatures("JetGetThreadStats");
            // To speed up the interop we use unsafe code to avoid initializing
            // the out parameter. We just call the interop code.
            unsafe {
                JET_THREADSTATS2 result;
                int returnCode = NativeMethods.JetGetThreadStats(&result, checked((uint)JET_THREADSTATS2.Size));
                Tracing.TraceResult(returnCode);
                EsentExceptionHelper.Check(returnCode);
                return result;
            }
        }

        /// <summary>Begins the preparations for a snapshot session. A snapshot session
        /// is a short time interval in which the engine does not issue any write IOs to
        /// disk, so that the engine can participate in a volume snapshot session (when
        /// driven by a snapshot writer).</summary>
        /// <param name="grbit">Snapshot options.</param>
        /// <returns>A snapshot handler.</returns>
        public IJetSnapshot PrepareSnapshot(SnapshotPrepareGrbit grbit)
        {
            return JetSnapshot.Create(grbit);
        }

        /// <summary>Set a system parameter which is a boolean.</summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetParameter(JET_param param, bool value)
        {
            SetParameter(param, new IntPtr(value ? 1 : 0));
        }

        /// <summary>Set a system parameter which is a boolean.</summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="value">The value to set.</param>
        public static void SetParameter(JET_param param, int value)
        {
            SetParameter(param, new IntPtr(value));
        }

        /// <summary>Sets database configuration options.</summary>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="paramValue">The value of the parameter to set, if the parameter is an integer type.</param>
        /// <returns>An error or warning.</returns>
        public static void SetParameter(JET_param paramid, IntPtr paramValue)
        {
            NativeHelpers.SetParameter(JET_INSTANCE.Nil, JET_SESID.Nil, paramid, paramValue);
        }

        /// <summary>Sets database configuration options.</summary>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="paramString">The value of the parameter to set, if the parameter is a string type.</param>
        /// <returns>An error or warning.</returns>
        public static void SetParameter(JET_param paramid, string paramString)
        {
            NativeHelpers.SetParameter(JET_INSTANCE.Nil, JET_SESID.Nil, paramid, paramString);
        }

        /// <summary>Used when an unsupported API method is called. This logs an error and
        /// returns an InvalidOperationException.</summary>
        /// <param name="method">The name of the method.</param>
        /// <returns>The exception to throw.</returns>
        internal static Exception UnsupportedApiException(string method)
        {
            string error = string.Format(CultureInfo.InvariantCulture, "Method {0} is not supported by this version of ESENT", method);
            Tracing.TraceErrorLine(error);
            return new InvalidOperationException(error);
        }

        private static JetCapabilities _defaultCapabilities;
        private static JetEnvironment _singleton = new JetEnvironment();
        // TODO : MUST iitialize this static member.
        private static JetCapabilities _trueCapabilities = null;
    }
}
