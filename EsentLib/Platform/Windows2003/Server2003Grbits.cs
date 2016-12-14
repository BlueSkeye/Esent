//-----------------------------------------------------------------------
// <copyright file="Server2003Grbits.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;

using EsentLib.Implementation;

namespace EsentLib.Platform
{
    /// <summary>Options for <see cref="IJetSnapshot.Abort"/>.</summary>
    [Flags]
    public enum SnapshotAbortGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,    
    }

    /// <summary>Options for <see cref="IJetInstance.JetUpdate2"/>.</summary>
    [Flags]
    public enum UpdateGrbit
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>This flag causes the update to return an error if the update would
        /// not have been possible in the Windows 2000 version of ESE, which
        /// enforced a smaller maximum number of multi-valued column instances
        /// in each record than later versions of ESE. This is important only
        /// for applications that wish to replicate data between applications
        /// hosted on Windows 2000 and applications hosted on Windows 
        /// 2003, or later versions of ESE. It should not be necessary for most
        /// applications.</summary>
        [Obsolete("Only needed for legacy replication applications.")]
        CheckESE97Compatibility = 0x1,
    }
}