using System;
using System.Globalization;

namespace EsentLib.Implementation
{
    /// <summary>A JET session bound to a JET engine.</summary>
    public class JetSession : IDisposable, IJetSession
    {
        internal JetSession(JetInstance owner, JET_SESID hSession)
        {
            if (null == owner) { throw new ArgumentNullException("owner"); }
            _hSession = hSession;
            _owner = owner;
        }

        /// <summary>Gets the current number of nested levels of transactions begun. A value
        /// of zero indicates that the session is not currently in a transaction. This
        /// parameter is read-only.</summary>
        /// <returns>The current transaction level of the specified database session.</returns>
        public int TransactionLevel
        {
            get
            {
                return GetParameter(EsentLib.Jet.JET_sesparam.TransactionLevel);
            }
        }

        /// <summary>Attaches a database file for use with a database instance. In order to
        /// use the database, it will need to be subsequently opened with
        /// <see cref="IJetSession.OpenDatabase"/>.</summary>
        /// <param name="database">The database to attach.</param>
        /// <param name="grbit">Attach options.</param>
        /// <returns>An error or warning.</returns>
        public void AttachDatabase(string database, AttachDatabaseGrbit grbit)
        {
            Tracing.TraceFunctionCall("AttachDatabase");
            Helpers.CheckNotNull(database, "database");
            int returnCode = NativeMethods.JetAttachDatabaseW(_hSession.Value, database, (uint)grbit);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return;
        }

        /// <summary>Ends a session.</summary>
        /// <param name="grbit">This parameter is not used.</param>
        public void Close(EndSessionGrbit grbit)
        {
            Close(grbit, true);
        }

        private void Close(EndSessionGrbit grbit, bool throwOnError)
        {
            Tracing.TraceFunctionCall("Close");
            int returnCode = NativeMethods.JetEndSession(_hSession.Value, (uint)grbit);
            Tracing.TraceResult(returnCode);
            if (throwOnError) { EsentExceptionHelper.Check(returnCode); }
        }

        /// <summary>Creates and attaches a database file.</summary>
        /// <param name="database">The path to the database file to create.</param>
        /// <param name="connect">The parameter is not used.</param>
        /// <param name="grbit">Database creation options.</param>
        /// <returns>An error or warning.</returns>
        public JetDatabase CreateDatabase(string database, string connect, CreateDatabaseGrbit grbit)
        {
            Tracing.TraceFunctionCall("CreateDatabase");
            Helpers.CheckNotNull(database, "database");
            JET_DBID dbid = JET_DBID.Nil;
            int returnCode = NativeMethods.JetCreateDatabaseW(this._hSession.Value,
                database, connect, out dbid.Value, (uint)grbit);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return new JetDatabase(dbid);
        }

        /// <summary></summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary></summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            this.Close(EndSessionGrbit.None, disposing);
        }

        /// <summary>Gets a parameter on the provided session state, used for the lifetime of this
        /// session or until reset.</summary>
        /// <param name="sesparamid">The ID of the session parameter to set, see
        /// <returns>The requested parameter value.</returns>
        public int GetParameter(EsentLib.Jet.JET_sesparam sesparamid)
        {
            int result;
            int returnCode = Api.Impl.JetGetSessionParameter(this._hSession, sesparamid, out result);
            // TODO : Trace call
            EsentExceptionHelper.Check(returnCode);
            return result;
        }

        /// <summary>Gets a parameter on the provided session state, used for the lifetime of
        /// this session or until reset.</summary>
        /// <param name="sesparamid">The ID of the session parameter to set, see
        /// <see cref="JET_sesparam"/> and <see cref="EsentLib.Jet.JET_sesparam"/>.</param>
        /// <returns>The 32-bit retrieved value.</returns>
        public int GetParameter(JET_sesparam sesparamid)
        {
            Tracing.TraceFunctionCall("GetParameter");
            this.CheckSupportsWindows8Features("GetParameter");
            int actualDataSize;
            int result;
            int err = NativeMethods.JetGetSessionParameter(_hSession.Value, (uint)sesparamid,
                out result, sizeof(int), out actualDataSize);

            if (err >= (int)JET_err.Success) {
                if (actualDataSize != sizeof(int)) {
                    throw new ArgumentException(
                        string.Format(CultureInfo.InvariantCulture,
                            "Bad return value. Unexpected data size returned. Expected {0}, but received {1}.",
                            sizeof(int), actualDataSize), "sesparamid");
                }
            }
            Tracing.TraceResult(err);
            return result;
        }

        /// <summary>Retrieves the version of the database engine.</summary>
        internal uint GetVersion()
        {
            Tracing.TraceFunctionCall("GetVersion");
            if ((null != _owner) && (0 != _owner.OverridenVersion)) {
                // We have an explicitly set version
                Tracing.TraceVerboseLine(string.Format(CultureInfo.InvariantCulture,
                    "GetVersion overridden with 0x{0:X}", _owner.OverridenVersion));
                return _owner.OverridenVersion;
            }
            // Get the version from Esent
            uint nativeVersion;
            int returnCode = NativeMethods.JetGetVersion(_hSession.Value, out nativeVersion);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return nativeVersion;
        }

        /// <summary>Opens a database previously attached with <see cref="IJetSession.AttachDatabase"/>,
        /// for use with a database session. This function can be called multiple times for the same
        /// database.</summary>
        /// <param name="database">The database to open.</param>
        /// <param name="connect">Reserved for future use.</param>
        /// <param name="grbit">Open database options.</param>
        /// <returns>A database instance.</returns>
        public JetDatabase OpenDatabase(string database, string connect, OpenDatabaseGrbit grbit)
        {
            Tracing.TraceFunctionCall("OpenDatabase");
            Helpers.CheckNotNull(database, "database");
            JET_DBID dbid = JET_DBID.Nil;
            int returnCode = NativeMethods.JetOpenDatabaseW(this._hSession.Value, database, connect,
                out dbid.Value, (uint)grbit);
            Tracing.TraceResult(returnCode);
            return new JetDatabase(dbid);
        }

        /// <summary>Returns a <see cref="T:System.String"/> that represents the current
        /// <see cref="JetSession"/>.</summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="JetSession"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Session (0x{0:x})",
                this._hSession.Value);
        }

        internal static readonly JetSession NullSession = new JetSession(null, JET_SESID.Nil);
        private JET_SESID _hSession;
        private JetInstance _owner;
    }
}
