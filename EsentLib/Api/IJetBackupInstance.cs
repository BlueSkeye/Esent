using System;

namespace EsentLib.Api
{
    /// <summary></summary>
    [CLSCompliant(false)]
    public interface IJetBackupInstance : IJetInstance
    {
        /// <summary>Opens an attached database, database patch file, or transaction log file
        /// of an active instance for the purpose of performing a streaming fuzzy backup. The
        /// data from these files can subsequently be read through the returned handle using
        /// JET_HANDLE.Read. The returned handle must be closed using JET_HANDLE.Close. An
        /// external backup of the instance must have been previously initiated using
        /// <see cref="IJetInstance.PrepareBackup"/>.</summary>
        /// <param name="file">The file to open.</param>
        /// <param name="fileSize">Returns the file size.</param>
        ///<returns>Handle tà ely opened file.</returns>
        JET_HANDLE OpenFile(string file, out long fileSize);
    }
}
