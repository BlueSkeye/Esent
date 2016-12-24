using EsentLib.Api;

namespace EsentLib.Implementation
{
    /// <summary></summary>
    internal class JetBackupInstance : JetInstance , IJetBackupInstance
    {
        /// <summary>Opens an attached database, database patch file, or transaction log file
        /// of an active instance for the purpose of performing a streaming fuzzy backup. The
        /// data from these files can subsequently be read through the returned handle using
        /// JET_HANDLE.Read. The returned handle must be closed using JET_HANDLE.Close.
        /// An external backup of the instance must have been previously initiated using
        /// JetBeginExternalBackupInstance.</summary>
        /// <param name="file">The file to open.</param>
        /// <param name="fileSize">Returns the file size.</param>
        ///<returns>Handle tà ely opeed file.</returns>
        public JET_HANDLE OpenFile(string file, out long fileSize)
        {
            Tracing.TraceFunctionCall("OpenFile");
            Helpers.CheckNotNull(file, "file");
            JET_HANDLE handle = JET_HANDLE.Nil;
            uint nativeFileSizeLow;
            uint nativeFileSizeHigh;
            int returnCode = NativeMethods.JetOpenFileInstanceW(_instance.Value, file,
                out handle._nativeHandle, out nativeFileSizeLow, out nativeFileSizeHigh);
            Tracing.TraceResult(returnCode);
            fileSize = (nativeFileSizeLow + (nativeFileSizeHigh << 32));
            EsentExceptionHelper.Check(returnCode);
            return handle;
        }
    }
}
