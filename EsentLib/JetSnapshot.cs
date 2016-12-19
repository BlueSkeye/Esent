using System;

using EsentLib.Jet;

namespace EsentLib.Implementation
{
    internal class JetSnapshot : IJetSnapshot
    {
        private JetSnapshot()
        {
            return;
        }

        internal IntPtr Id
        {
            get { return _snapid.Value; }
        }

        /// <summary>Notifies the engine that it can resume normal IO operations after a freeze
        /// period ended with a failed snapshot.</summary>
        /// <param name="grbit">Options for this call.</param>
        /// <returns>An error code.</returns>
        public void Abort(SnapshotAbortGrbit grbit)
        {
            Tracing.TraceFunctionCall("Abort");
            
            JetEnvironment.Current.TrueCapabilities.CheckSupportsServer2003Features("Abort");
            int returnCode = NativeMethods.JetOSSnapshotAbort(Id, (uint)grbit);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Selects a specific instance to be part of the snapshot session.</summary>
        /// <param name="instance">The instance to add to the snapshot.</param>
        /// <param name="grbit">Options for this call.</param>
        public void BindInstance(IJetInstance instance,
            SnapshotPrepareInstanceGrbit grbit = SnapshotPrepareInstanceGrbit.None)
        {
            Tracing.TraceFunctionCall("BindInstance");
            JetEnvironment.Current.TrueCapabilities.CheckSupportsVistaFeatures("JetOSSnapshotPrepareInstance");
            int errorCode = NativeMethods.JetOSSnapshotPrepareInstance(Id, instance.Id, (uint)grbit);
            Tracing.TraceResult(errorCode);
            EsentExceptionHelper.Check(errorCode);
        }

        internal static JetSnapshot Create(SnapshotPrepareGrbit grbit)
        {
            Tracing.TraceFunctionCall("Create");
            JetSnapshot result = new JetSnapshot();
            int returnCode = NativeMethods.JetOSSnapshotPrepare(out result._snapid.Value, (uint)grbit);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return result;
        }

        /// <summary>Notifies the engine that the snapshot session finished.</summary>
        /// <param name="grbit">Snapshot end options.</param>
        /// <returns>An error code.</returns>
        public void End(SnapshotEndGrbit grbit)
        {
            Tracing.TraceFunctionCall("End");
            JetEnvironment.Current.TrueCapabilities.CheckSupportsVistaFeatures("JetOSSnapshotEnd");
            int returnCode = NativeMethods.JetOSSnapshotEnd(Id, (uint)grbit);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Retrieves the list of instances and databases that are part of the snapshot
        /// session at any given moment.</summary>
        /// <param name="grbit">Options for this call.</param>
        /// <returns>An error code if the call fails.</returns>
        public JET_INSTANCE_INFO[] GetInfo(SnapshotGetFreezeInfoGrbit grbit = SnapshotGetFreezeInfoGrbit.None)
        {
            Tracing.TraceFunctionCall("GetInfo");
            JetEnvironment.Current.TrueCapabilities.CheckSupportsVistaFeatures("JetOSSnapshotGetFreezeInfo");
            int returnCode;
            JET_INSTANCE_INFO[] result;
            unsafe {
                uint nativeNumInstance = 0;
                NATIVE_INSTANCE_INFO* nativeInstanceInfos = null;
                returnCode = NativeMethods.JetOSSnapshotGetFreezeInfoW(Id, out nativeNumInstance, out nativeInstanceInfos, (uint)grbit);
                result = JET_INSTANCE_INFO.FromNativeCollection(nativeNumInstance, nativeInstanceInfos);
                Tracing.TraceResult(returnCode);
            }
            EsentExceptionHelper.Check(returnCode);
            return result;
        }

        /// <summary>Starts a snapshot. While the snapshot is in progress, no write-to-disk
        /// activity by the engine can take place.</summary>
        /// <param name="grbit">Snapshot freeze options.</param>
        /// <returns></returns>
        public JET_INSTANCE_INFO[] Start(SnapshotFreezeGrbit grbit = SnapshotFreezeGrbit.None)
        {
            Tracing.TraceFunctionCall("Start");
            int returnCode;
            JET_INSTANCE_INFO[] result;
            unsafe {
                uint nativeNumInstance = 0;
                NATIVE_INSTANCE_INFO* nativeInstanceInfos = null;
                returnCode = NativeMethods.JetOSSnapshotFreezeW(Id, out nativeNumInstance,
                    out nativeInstanceInfos, (uint)grbit);
                Tracing.TraceResult(returnCode);
                result = JET_INSTANCE_INFO.FromNativeCollection(nativeNumInstance, nativeInstanceInfos);
            }
            EsentExceptionHelper.Check(returnCode);
            return result;
        }

        /// <summary>Notifies the engine that it can resume normal IO operations after a
        /// freeze period and a successful snapshot.</summary>
        /// <param name="grbit">Thaw options.</param>
        public void Thaw(SnapshotThawGrbit grbit = SnapshotThawGrbit.None)
        {
            Tracing.TraceFunctionCall("Thaw");
            int returnCode = NativeMethods.JetOSSnapshotThaw(Id, (uint)grbit);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Enables log truncation for all instances that are part of the snapshot
        /// session.</summary>
        /// <remarks>This function should be called only if the snapshot was created with the
        /// <see cref="SnapshotPrepareGrbit.ContinueAfterThaw"/> option. Otherwise, the snapshot session
        /// ends after the call to <see cref="IJetSnapshot.Thaw"/>.</remarks>
        /// <param name="grbit">Options for this call.</param>
        /// <returns>An error code if the call fails.</returns>
        public void TruncateLog(SnapshotTruncateLogGrbit grbit)
        {
            Tracing.TraceFunctionCall("TruncateLog");
            JetEnvironment.Current.TrueCapabilities.CheckSupportsVistaFeatures("JetOSSnapshotTruncateLog");
            int returnCode = NativeMethods.JetOSSnapshotTruncateLog(Id, (uint)grbit);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Truncates the log for a specified instance during a snapshot session.
        /// </summary>
        /// <remarks>This function should be called only if the snapshot was created with the
        /// <see cref="SnapshotPrepareGrbit.ContinueAfterThaw"/> option. Otherwise, the snapshot session
        /// ends after the call to <see cref="IJetSnapshot.Thaw"/>.</remarks>
        /// <param name="instance">The instance to truncat the log for.</param>
        /// <param name="grbit">Options for this call.</param>
        /// <returns>An error code if the call fails.</returns>
        public void TruncateInstanceLog(IJetInstance instance, SnapshotTruncateLogGrbit grbit)
        {
            Tracing.TraceFunctionCall("TruncateInstanceLog");
            JetEnvironment.Current.TrueCapabilities.CheckSupportsVistaFeatures("JetOSSnapshotTruncateLogInstance");
            int returnCode = NativeMethods.JetOSSnapshotTruncateLogInstance(Id, instance.Id, (uint)grbit);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        private JET_OSSNAPID _snapid = JET_OSSNAPID.Nil;
    }
}
