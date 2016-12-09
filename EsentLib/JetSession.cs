using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace EsentLib.Implementation
{
    /// <summary>A JET session bound to a JET engine.</summary>
    public class JetSession : IDisposable
    {
        internal JetSession(JetEngine owner, JET_SESID hSession)
        {
            if (null == owner) { throw new ArgumentNullException("owner"); }
            _hSession = hSession;
            _owner = owner;
        }

        /// <summary>Ends a session.</summary>
        /// <param name="grbit">This parameter is not used.</param>
        public void Close(EndSessionGrbit grbit)
        {
            Close(grbit, true);
        }

        private void Close(EndSessionGrbit grbit, bool throwOnError)
        {
            JetEngine.TraceFunctionCall("Close");
            int returnCode = NativeMethods.JetEndSession(_hSession.Value, (uint)grbit);
            JetEngine.TraceResult(returnCode);
            if (throwOnError) { EsentExceptionHelper.Check(returnCode); }
        }

        /// <summary></summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary></summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                GC.SuppressFinalize(this);
                this.Close(EndSessionGrbit.None);
            }
            else {
                // This one is guaranteed not to throw.
                this.Close(EndSessionGrbit.None, false);
            }
        }

        /// <summary>Retrieves the version of the database engine.</summary>
        internal uint GetVersion()
        {
            JetEngine.TraceFunctionCall("GetVersion");
            if (0 != _owner.OverridenVersion) {
                // We have an explicitly set version
                Trace.WriteLineIf(_owner.Tracer.TraceVerbose,
                    string.Format(CultureInfo.InvariantCulture,
                        "GetVersion overridden with 0x{0:X}", _owner.OverridenVersion));
                return _owner.OverridenVersion;
            }
            // Get the version from Esent
            uint nativeVersion;
            int returnCode = NativeMethods.JetGetVersion(_hSession.Value, out nativeVersion);
            JetEngine.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return nativeVersion;
        }

        private JET_SESID _hSession;
        private JetEngine _owner;
    }
}
