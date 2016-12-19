using System;
using System.Globalization;
using System.Runtime.InteropServices;

using EsentLib.Jet;

namespace EsentLib.Implementation
{
    /// <summary>A JET session bound to a JET engine.</summary>
    [CLSCompliant(false)]
    public class JetSession : IDisposable, IJetSession
    {
        internal JetSession(JetInstance owner, JET_SESID hSession)
        {
            if (null == owner) { throw new ArgumentNullException("owner"); }
            _hSession = hSession;
            _owner = owner;
        }

        #region PROPERTIES
        internal JetCapabilities Capabilities
        {
            get { return _owner._Capabilities; }
        }

        /// <summary>A 32-bit integer ID that is logged in traces and can be used by clients to
        /// correlate ESE actions with their activity.</summary>
        /// <returns>The corrlation identifer of the specified database session.</returns>
        public int CorrelationID
        {
            get { return GetInt32Parameter(JET_sesparam.CorrelationID); }
            set { SetParameter(JET_sesparam.CorrelationID, value); }
        }

        /// <summary>Get session identifier.</summary>
        public IntPtr Id
        {
            get { return _hSession.Value; }
        }

        /// <summary>A client context of type <see cref="JET_OPERATIONCONTEXT"/> that the engine
        /// uses to track and trace operations (such as IOs).</summary>
        /// <returns>The operation context of the specified database session.</returns>
        public JET_OPERATIONCONTEXT OperationContext
        {
            get { return GetOperationContextParameter(); }
            set
            {
                Tracing.TraceFunctionCall("Set-OperationContext");
                this.Capabilities.CheckSupportsWindows10Features("Set-OperationContext");
                NATIVE_OPERATIONCONTEXT nativeContext = value.GetNativeOperationContext();
                int dataSize = Marshal.SizeOf(nativeContext);
                int returnCode = NativeMethods.JetSetSessionParameter(Id,
                    (uint)JET_sesparam.OperationContext, ref nativeContext, checked((int)dataSize));
                Tracing.TraceResult(returnCode);
                EsentExceptionHelper.Check(returnCode);
            }
        }

        /// <summary>Get the instance owning this session.</summary>
        public IJetInstance Owner { get; private set; }

        /// <summary>Gets the current number of nested levels of transactions begun. A value
        /// of zero indicates that the session is not currently in a transaction. This
        /// parameter is read-only. Requires Windows 10.</summary>
        /// <returns>The current transaction level of the specified database session.</returns>
        public int TransactionLevel
        {
            get { return GetInt32Parameter(JET_sesparam.TransactionLevel); }
        }
        #endregion

        /// <summary>Attaches a database file for use with a database instance. In order to
        /// use the database, it will need to be subsequently opened with
        /// <see cref="IJetSession.OpenDatabase"/>.</summary>
        /// <param name="database">The database to attach.</param>
        /// <param name="grbit">Attach options.</param>
        /// <param name="maxPages">The maximum size, in database pages, of the database.
        /// Passing 0 means there is no enforced maximum.</param>
        /// <returns>An error or warning.</returns>
        [CLSCompliant(false)]
        public void AttachDatabase(string database, AttachDatabaseGrbit grbit, uint maxPages = 0)
        {
            Tracing.TraceFunctionCall("AttachDatabase");
            Helpers.CheckNotNull(database, "database");
            int returnCode = (0 == maxPages)
                ? NativeMethods.JetAttachDatabaseW(Id, database, (uint)grbit)
                : NativeMethods.JetAttachDatabase2W(Id, database, maxPages, (uint)grbit);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return;
        }

        /// <summary>Causes a session to enter a transaction or create a new save point in an
        /// existing transaction.</summary>
        /// <returns></returns>
        public IJetTransaction BeginTransaction(BeginTransactionGrbit grbit = BeginTransactionGrbit.None)
        {
            return new JetTransaction(this, grbit);
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
            int returnCode = NativeMethods.JetEndSession(Id, (uint)grbit);
            Tracing.TraceResult(returnCode);
            if (throwOnError) { EsentExceptionHelper.Check(returnCode); }
            JetInstance.NotifySessionClosed(this);
        }

        /// <summary>Creates and attaches a database file.</summary>
        /// <param name="database">The path to the database file to create.</param>
        /// <param name="grbit">Database creation options.</param>
        /// <param name="maxPages">The maximum size, in database pages, of the database.
        /// Passing 0 means there is no enforced maximum.</param>
        /// <returns>An error or warning.</returns>
        [CLSCompliant(false)]
        public IJetDatabase CreateDatabase(string database, CreateDatabaseGrbit grbit,
            uint maxPages = 0)
        {
            Tracing.TraceFunctionCall("CreateDatabase");
            Helpers.CheckNotNull(database, "database");
            JET_DBID dbid = JET_DBID.Nil;
            int returnCode = (0 == maxPages)
                ? NativeMethods.JetCreateDatabaseW(Id, database, null, out dbid.Value, (uint)grbit)
                : NativeMethods.JetCreateDatabase2W(Id, database, maxPages, out dbid.Value, (uint)grbit);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return new JetDatabase(this, dbid, database);
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

        /// <summary>Initialize a new ESE session in the same instance as the given sesid.
        /// </summary>
        [CLSCompliant(false)]
        public IJetSession Duplicate()
        {
            Tracing.TraceFunctionCall("Duplicate");
            JET_SESID newSesid = JET_SESID.Nil;
            int returnCode = NativeMethods.JetDupSession(Id, out newSesid.Value);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return new JetSession(this._owner, newSesid);
        }

        /// <summary>Gets a parameter on the provided session state, used for the lifetime of
        /// this session or until reset.</summary>
        /// <param name="sesparamid">The ID of the session parameter to set, see
        /// <see cref="JET_sesparam"/> and <see cref="EsentLib.Jet.JET_sesparam"/>.</param>
        /// <returns>The 32-bit retrieved value.</returns>
        private int GetInt32Parameter(JET_sesparam sesparamid)
        {
            Tracing.TraceFunctionCall("GetParameter");
            this.Capabilities.CheckSupportsWindows8Features("GetParameter");
            int actualDataSize;
            int result;
            int err = NativeMethods.JetGetSessionParameter(Id, (uint)sesparamid, out result,
                sizeof(int), out actualDataSize);

            if (err >= (int)JET_err.Success) {
                if (actualDataSize != sizeof(int)) {
                    throw new ArgumentException(
                        string.Format(CultureInfo.InvariantCulture,
                            "Bad return value. Unexpected data size returned. Expected {0}, but received {1}.",
                            sizeof(int), actualDataSize), "sesparamid");
                }
            }
            Tracing.TraceResult(err);
            EsentExceptionHelper.Check(err);
            return result;
        }

        private JET_OPERATIONCONTEXT GetOperationContextParameter()
        {
            Tracing.TraceFunctionCall("JetGetSessionParameter");
            this.Capabilities.CheckSupportsWindows10Features("JetGetSessionParameter");
            NATIVE_OPERATIONCONTEXT nativeContext = new NATIVE_OPERATIONCONTEXT();
            int dataSize = Marshal.SizeOf(nativeContext);
            int actualDataSize;
            int err = NativeMethods.JetGetSessionParameter(Id, (uint)JET_sesparam.OperationContext,
                out nativeContext, dataSize, out actualDataSize);
            if ((int)JET_err.Success <= err) {
                if (actualDataSize != dataSize) {
                    throw new ArgumentException(
                        string.Format(CultureInfo.InvariantCulture,
                            "Bad return value. Unexpected data size returned. Expected {0}, but received {1}.",
                            dataSize, actualDataSize),
                        "sesparamid");
                }
            }
            Tracing.TraceResult(err);
            return new JET_OPERATIONCONTEXT(ref nativeContext);
        }

        /// <summary>Opens a database previously attached with <see cref="IJetSession.AttachDatabase"/>,
        /// for use with a database session. This function can be called multiple times for the same
        /// database.</summary>
        /// <param name="database">The database to open.</param>
        /// <param name="connect">Reserved for future use.</param>
        /// <param name="grbit">Open database options.</param>
        /// <returns>A database instance.</returns>
        [CLSCompliant(false)]
        public IJetDatabase OpenDatabase(string database, string connect, OpenDatabaseGrbit grbit)
        {
            Tracing.TraceFunctionCall("OpenDatabase");
            Helpers.CheckNotNull(database, "database");
            JET_DBID dbid = JET_DBID.Nil;
            int returnCode = NativeMethods.JetOpenDatabaseW(Id, database, connect, out dbid.Value,
                (uint)grbit);
            Tracing.TraceResult(returnCode);
            return new JetDatabase(this, dbid, database);
        }

        /// <summary>Creates a temporary table with a single index. A temporary table stores and
        /// retrieves records just like an ordinary table created using JetCreateTableColumnIndex.
        /// However, temporary tables are much faster than ordinary tables due to their volatile
        /// nature. They can also be used to very quickly sort and perform duplicate removal on
        /// record sets when accessed in a purely sequential manner. Also see
        /// <seealso cref="Api.JetOpenTempTable3"/>.
        /// <seealso cref="Api.JetOpenTemporaryTable"/>.</summary>
        /// <param name="columns">Column definitions for the columns created in the temporary table.
        /// </param>
        /// <param name="grbit">Table creation options.</param>
        /// <param name="columnids">The output buffer that receives the array of column IDs generated
        /// during the creation of the temporary table. The column IDs in this array will exactly
        /// correspond to the input array of column definitions. As a result, the size of this buffer
        /// must correspond to the size of the input array.</param>
        /// <returns>Returns the tableid of the temporary table. Closing this tableid with
        /// <see cref="IJetTable.Close"/> frees the resources associated with the temporary table.</returns>
        public IJetTable OpenTemporaryTable(JET_COLUMNDEF[] columns, TempTableGrbit grbit,
            JET_COLUMNID[] columnids)
        {
            Tracing.TraceFunctionCall("OpenTemporaryTable");
            Helpers.CheckNotNull(columns, "columnns");
            Helpers.CheckNotNull(columnids, "columnids");
            JET_TABLEID tableid = JET_TABLEID.Nil;
            NATIVE_COLUMNDEF[] nativecolumndefs = columns.GetNativecolumndefs();
            uint[] nativecolumnids = new uint[columns.Length];
            int returnCode = NativeMethods.JetOpenTempTable(Id, nativecolumndefs,
                checked((uint)columns.Length), (uint)grbit, out tableid.Value,
                nativecolumnids);
            Tracing.TraceResult(returnCode);
            columns.SetColumnids(columnids, nativecolumnids);
            EsentExceptionHelper.Check(returnCode);
            throw new NotImplementedException();
        }

        /// <summary>Disassociates a session from the current thread. This should be
        /// used in conjunction with <see cref="IJetSession.SetContext"/>.</summary>
        public void ResetContext()
        {
            Tracing.TraceFunctionCall("ResetContext");
            int returnCode = NativeMethods.JetResetSessionContext(Id);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Associates a session with the current thread using the given context
        /// handle. This association overrides the default engine requirement that a
        /// transaction for a given session must occur entirely on the same thread.
        /// Use <see cref="ResetContext"/> to remove the association.</summary>
        /// <param name="context">The context to set.</param>
        public void SetContext(IntPtr context)
        {
            Tracing.TraceFunctionCall("SetContext");
            int returnCode = NativeMethods.JetSetSessionContext(Id, context);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Sets a parameter on the provided session state, used for the lifetime of
        /// this session or until reset.</summary>
        /// <param name="sesparamid">The ID of the session parameter to set.</param>
        /// <param name="valueToSet">A 32-bit integer to set.</param>
        /// <returns>An error if the call fails.</returns>
        private int SetParameter(JET_sesparam sesparamid, int valueToSet)
        {
            Tracing.TraceFunctionCall("SetParameter");
            this.Capabilities.CheckSupportsWindows8Features("SetParameter");
            int err = NativeMethods.JetSetSessionParameter(Id, (uint)sesparamid, ref valueToSet, sizeof(int));
            return Tracing.TraceResult(err);
        }

        /// <summary>Returns a <see cref="T:System.String"/> that represents the current
        /// <see cref="JetSession"/>.</summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="JetSession"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Session (0x{0:x})", Id);
        }

        internal static readonly JetSession NullSession = new JetSession(null, JET_SESID.Nil);
        private JET_SESID _hSession;
        private JetInstance _owner;
    }
}
