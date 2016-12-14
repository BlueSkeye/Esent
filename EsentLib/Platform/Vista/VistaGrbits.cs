//-----------------------------------------------------------------------
// <copyright file="VistaGrbits.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace EsentLib.Platform
{
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
}
