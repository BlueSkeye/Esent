using System;

using EsentLib.Api;
using EsentLib.Implementation;

namespace EsentLib.Api.Flags
{
    /// <summary>Options for <see cref="JetInstance.EnableShrinkDatabase"/>.</summary>
    [Flags]
    public enum ShrinkDatabaseGrbit
    {
        /// <summary>Does not reduce the size of the database during normal operations.</summary>
        Off = 0x0,

        /// <summary>Turns on the database shrinking functionality. If this parameter is not
        /// set, then <see cref="IJetDatabase.Resize"/> will be unable to reclaim space to
        /// the file system. Uses the file system's Sparse Files feature to release space
        /// in the middle of a file. When enough rows or tables get free up by
        /// the Version Store Cleanup task, and space is reclaimed, the database
        /// engine will attempt to return it to the file system, via sparse files.
        /// Sparse files are currently only available on NTFS and ReFS file systems.</summary>
        On = 0x1,

        /// <summary>After space is release from a table to a the root Available Extent, the database
        /// engine will attempt to release the space back to the file system. This parameter
        /// requires that <see cref="On"/> is also specified.</summary>
        Realtime = 0x2,
    }
}
