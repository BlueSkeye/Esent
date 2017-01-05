using System;

using EsentLib.Jet;

namespace EsentLib.Api
{
    /// <summary></summary>
    [CLSCompliant(false)]
    public interface IJetEnvironment
    {
        #region PROPERTIES
        /// <summary>Gets the maximum size of a bookmark. <seealso cref="LegacyApi.JetGetBookmark"/>.</summary>
        int BookmarkMost { get; }

        /// <summary>Gets or sets the size of the database cache in pages. By default the
        /// database cache will automatically tune its size, setting this property to a non-zero
        /// value will cause the cache to adjust itself to the target size. </summary>
        int CacheSize { get; set; }

        /// <summary>Gets or sets the maximum size of the database page cache. The size is in
        /// database pages. If this parameter is left to its default value, then the maximum
        /// size of the cache will be set to the size of physical memory when JetInit is called.
        /// </summary>
        int CacheSizeMax { get; set; }

        /// <summary>Gets or sets the minimum size of the database page cache, in database pages.
        /// </summary>
        int CacheSizeMin { get; set; }

        /// <summary>Gets the maximum number of components in a sort or index key.</summary>
        int ColumnsKeyMost { get; }

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
        int Configuration { get; set; }

        /// <summary>Gets or sets the size of the database pages, in bytes.</summary>
        int DatabasePageSize { get; set; }

        /// <summary>Gets the true capabilities of this implementation of ESENT.</summary>
        EsentLib.Implementation.JetCapabilities DefaultCapabilities { get; }

        /// <summary>Gets or sets the detail level of eventlog messages that are emitted to the
        /// eventlog by the database engine. Higher numbers will result in more detailed eventlog
        /// messages.</summary>
        int EventLoggingLevel { get; set; }

        /// <summary>Gets or sets the value encoding what to do with exceptions generated
        /// within JET.</summary>
        EsentLib.Jet.JET_ExceptionAction ExceptionAction { get; set; }

        /// <summary>Gets or sets the set of actions to be taken on IOs that appear hung.</summary>
        int HungIOActions { get; set; }

        /// <summary>Gets or sets the threshold for what is considered a hung IO that should
        /// be acted upon.</summary>
        int HungIOThreshold { get; set; }

        /// <summary>Gets the maximum key size. This depends on the Esent version and database
        /// page size.</summary>
        int KeyMost { get; }

        /// <summary>Gets or sets backwards compatibility with the file naming conventions of earlier
        /// releases of the database engine.</summary>
        int LegacyFileNames { get; set; }

        /// <summary>Gets the lv chunks size. This depends on the database page size.</summary>
        int LVChunkSizeMost { get; }

        /// <summary>Gets or sets the maximum number of instances that can be created.</summary>
        int MaxInstances { get; set; }

        /// <summary>Gets or sets the smallest amount of data that should be compressed with xpress
        /// compression.</summary>
        int MinDataForXpress { get; set; }

        /// <summary>Gets or sets how many database file I/Os can be queued per-disk in the
        /// host operating system at one time. A larger value for this parameter can significantly
        /// help the performance of a large database application. </summary>
        int OutstandingIOMax { get; set; }

        /// <summary>Gets or sets the friendly name for this instance of the process.</summary>
        string ProcessFriendlyName { get; set; }

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
        int StartFlushThreshold { get; set; }

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
        int StopFlushThreshold { get; set; }

        /// <summary>Gets the true capabilities of this implementation of ESENT.</summary>
        EsentLib.Implementation.JetCapabilities TrueCapabilities { get; }
        #endregion

        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="displayName"></param>
        /// <param name="grbit"></param>
        /// <returns></returns>
        IJetInstance GetInstance(string name, string displayName = null,
            CreateInstanceGrbit grbit = CreateInstanceGrbit.None);

        /// <summary>Retrieves information about the instances that are running.</summary>
        /// <returns>An error code if the call fails.</returns>
        JET_INSTANCE_INFO[] GetRunningInstancesInfo();

        /// <summary>Retrieves performance information from the database engine for the
        /// current thread. Multiple calls can be used to collect statistics that reflect
        /// the activity of the database engine on this thread between those calls.</summary>
        /// <returns>Returns the thread statistics.</returns>
        EsentLib.Jet.Vista.JET_THREADSTATS GetThreadStatistics();

        /// <summary>Retrieves performance information from the database engine for the current
        /// thread. Multiple calls can be used to collect statistics that reflect the activity
        /// of the database engine on this thread between those calls.</summary>
        /// <returns>An error code if the operation fails.</returns>
        JET_THREADSTATS2 GetThreadStatisticsEx();

        /// <summary>Sets system wide configuration options.
        /// <seealso cref="IJetInstance.SetSystemParameter(JET_param, JET_CALLBACK, string)"/>.
        /// </summary>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="paramValue">The value of the parameter to set, if the parameter is a
        /// JET_CALLBACK.</param>
        /// <param name="paramString">The value of the parameter to set, if the parameter is a
        /// string type.</param>
        /// <returns>An ESENT warning code.</returns>
        void SetSystemParameter(JET_param paramid, JET_CALLBACK paramValue, string paramString);

        /// <summary>Sets system wide configuration options.
        /// <seealso cref="IJetInstance.SetSystemParameter(JET_param, IntPtr, string)"/></summary>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="paramValue">The value of the parameter to set, if the parameter is an
        /// integer type.</param>
        /// <param name="paramString">The value of the parameter to set, if the parameter is a
        /// string type.</param>
        /// <returns>An error or warning.</returns>
        void SetSystemParameter(JET_param paramid, IntPtr paramValue, string paramString);
    }
}
