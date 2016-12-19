using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EsentLib.Jet;

using EsentLib.Implementation;
using EsentLib.Platform;

namespace EsentLib
{
    /// <summary></summary>
    [CLSCompliant(false)]
    public interface IJetSnapshot
    {
        /// <summary>Notifies the engine that it can resume normal IO operations after a
        /// freeze period ended with a failed snapshot.</summary>
        /// <param name="grbit">Options for this call.</param>
        /// <returns>An error code if the call fails.</returns>
        void Abort(SnapshotAbortGrbit grbit);

        /// <summary>Selects a specific instance to be part of the snapshot session.</summary>
        /// <param name="instance">The instance to add to the snapshot.</param>
        /// <param name="grbit">Options for this call.</param>
        void BindInstance(IJetInstance instance,
            SnapshotPrepareInstanceGrbit grbit = SnapshotPrepareInstanceGrbit.None);

        /// <summary>Notifies the engine that the snapshot session finished.</summary>
        /// <param name="grbit">Snapshot end options.</param>
        /// <returns>An error code if the call fails.</returns>
        void End(SnapshotEndGrbit grbit);

        /// <summary>Retrieves the list of instances and databases that are part of the snapshot
        /// session at any given moment.</summary>
        /// <param name="grbit">Options for this call.</param>
        /// <returns>An error code if the call fails.</returns>
        JET_INSTANCE_INFO[] GetInfo(SnapshotGetFreezeInfoGrbit grbit = SnapshotGetFreezeInfoGrbit.None);

        /// <summary>Starts a snapshot. While the snapshot is in progress, no write-to-disk
        /// activity by the engine can take place.</summary>
        /// <param name="grbit">Snapshot freeze options.</param>
        /// <returns></returns>
        JET_INSTANCE_INFO[] Start(SnapshotFreezeGrbit grbit = SnapshotFreezeGrbit.None);

        /// <summary>Notifies the engine that it can resume normal IO operations after a
        /// freeze period and a successful snapshot.</summary>
        /// <param name="grbit">Thaw options.</param>
        void Thaw(SnapshotThawGrbit grbit = SnapshotThawGrbit.None);

        /// <summary>Enables log truncation for all instances that are part of the snapshot
        /// session.</summary>
        /// <remarks>This function should be called only if the snapshot was created with the
        /// <see cref="SnapshotPrepareGrbit.ContinueAfterThaw"/> option. Otherwise, the snapshot session
        /// ends after the call to <see cref="IJetSnapshot.Thaw"/>.</remarks>
        /// <param name="grbit">Options for this call.</param>
        /// <returns>An error code if the call fails.</returns>
        void TruncateLog(SnapshotTruncateLogGrbit grbit);

        /// <summary>Truncates the log for a specified instance during a snapshot session.
        /// </summary>
        /// <remarks>This function should be called only if the snapshot was created with the
        /// <see cref="SnapshotPrepareGrbit.ContinueAfterThaw"/> option. Otherwise, the snapshot session
        /// ends after the call to <see cref="IJetSnapshot.Thaw"/>.</remarks>
        /// <param name="instance">The instance to truncat the log for.</param>
        /// <param name="grbit">Options for this call.</param>
        /// <returns>An error code if the call fails.</returns>
        void TruncateInstanceLog(IJetInstance instance, SnapshotTruncateLogGrbit grbit);
    }
}
