﻿using System;
using System.Globalization;
using System.Runtime.InteropServices;

using EsentLib.Api;
using EsentLib.Api.Flags;
using EsentLib.Jet;
using EsentLib.Jet.Types;

namespace EsentLib.Implementation
{
    /// <summary>A JET session bound to a JET engine.</summary>
    [CLSCompliant(false)]
    public class JetSession : IDisposable, IJetSession
    {
        static JetSession()
        {
            NullSession = new JetSession(JET_SESID.Nil);
        }

        /// <summary>This constructor is required for <see cref="NullSession"/> instanciation
        /// because no owner.</summary>
        /// <param name="hSession"></param>
        private JetSession(JET_SESID hSession)
        {
            _hSession = hSession;
        }

        internal JetSession(JetInstance owner, JET_SESID hSession)
            : this(hSession)
        {
            if (null == owner) { throw new ArgumentNullException("owner"); }
            _owner = owner;
        }

        #region PROPERTIES
        internal JetCapabilities Capabilities
        {
            get {
                AssertNotNullSession();
                return _owner._Capabilities;
            }
        }

        /// <summary>A 32-bit integer ID that is logged in traces and can be used by clients to
        /// correlate ESE actions with their activity.</summary>
        /// <returns>The corrlation identifer of the specified database session.</returns>
        public int CorrelationID
        {
            get { return GetInt32Parameter(JET_sesparam.CorrelationID); }
            set { SetParameter(JET_sesparam.CorrelationID, value); }
        }

        internal JET_SESID Handle
        {
            get { return _hSession; }
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
        public IJetInstance Owner
        {
            get
            {
                if (object.ReferenceEquals(this, NullSession)) {
                    throw new InvalidOperationException("NullSession has no owner.");
                }
                return _owner;
            }
        }

        /// <summary>Gets the current number of nested levels of transactions begun. A value
        /// of zero indicates that the session is not currently in a transaction. This
        /// parameter is read-only. Requires Windows 10.</summary>
        /// <returns>The current transaction level of the specified database session.</returns>
        public int TransactionLevel
        {
            get { return GetInt32Parameter(JET_sesparam.TransactionLevel); }
        }
        #endregion

        private void AssertNotNullSession()
        {
            if (object.ReferenceEquals(this, NullSession)) {
                throw new InvalidOperationException(
                    "Operation not available on NullSession instance.");
            }
        }

        /// <summary>Attaches a database file for use with a database instance. In order to
        /// use the database, it will need to be subsequently opened with
        /// <see cref="IJetSession.OpenDatabase"/>.</summary>
        /// <param name="database">The database to attach.</param>
        /// <param name="grbit">Attach options.</param>
        /// <param name="maxPages">The maximum size, in database pages, of the database. Passing
        /// 0 means there is no enforced maximum.</param>
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
        /// <param name="readOnly">true if transaction is readonly.</param>
        /// <param name="userTransactionId">An optional identifier supplied by the user for
        /// identifying the transaction.</param>
        /// <returns>The newly created transaction.</returns>
        public IJetTransaction BeginTransaction(bool readOnly = false, long? userTransactionId = null)
        {
            return new JetTransaction(this, readOnly, userTransactionId);
        }

        /// <summary>Ends a session.</summary>
        /// <param name="grbit">This parameter is not used.</param>
        public void Close(EndSessionGrbit grbit)
        {
            Close(grbit, true);
        }

        private void Close(EndSessionGrbit grbit, bool throwOnError)
        {
            AssertNotNullSession();
            Tracing.TraceFunctionCall("Close");
            int returnCode = NativeMethods.JetEndSession(Id, (uint)grbit);
            Tracing.TraceResult(returnCode);
            if (throwOnError) { EsentExceptionHelper.Check(returnCode); }
            _owner.NotifySessionClosed(this);
            return;
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
            AssertNotNullSession();
            Tracing.TraceFunctionCall("Duplicate");
            JET_SESID newSesid = JET_SESID.Nil;
            int returnCode = NativeMethods.JetDupSession(Id, out newSesid.Value);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return new JetSession(this._owner, newSesid);
        }

        /// <summary></summary>
        /// <param name="kind"></param>
        /// <returns>The commit ID assoiated with the commit record on Windows 8 and later
        /// versions or a null reference for other platforms.</returns>
        public JET_COMMIT_ID FlushTransactions(TransactionFlushKind kind)
        {
            switch(kind) {
                case TransactionFlushKind.SessionPending:
                    return ManageTransactions(CommitTransactionGrbit.WaitLastLevel0Commit);
                case TransactionFlushKind.AllPending:
                    if (!this.Capabilities.SupportsServer2003Features) {
                        throw new NotSupportedException(
                            "TransactionFlushKind.AllPending not supported on this platform.");
                    }
                    return ManageTransactions(CommitTransactionGrbit.WaitAllLevel0Commit);
                default:
                    throw new ArgumentException("kind");
            }
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

        private JET_COMMIT_ID ManageTransactions(CommitTransactionGrbit grbit)
        {
            // TODO
            // this.CheckObjectIsNotDisposed();
            Tracing.TraceFunctionCall("FlushTransactions");
            bool windows8FeaturesEnabled = this.Capabilities.SupportsWindows8Features;
            NATIVE_COMMIT_ID nativeCommitId = new NATIVE_COMMIT_ID();
            unsafe {
                int err = (windows8FeaturesEnabled)
                    ? NativeMethods.JetCommitTransaction2(this.Id, unchecked((uint)grbit), 0,
                        ref nativeCommitId)
                    : NativeMethods.JetCommitTransaction(this.Id, unchecked((uint)grbit));
                Tracing.TraceResult(err);
                EsentExceptionHelper.Check(err);
            }
            return (windows8FeaturesEnabled)
                ? new JET_COMMIT_ID(nativeCommitId)
                : null;
        }

        /// <summary>Opens a database previously attached with <see cref="IJetSession.AttachDatabase"/>,
        /// for use with a database session. This function can be called multiple times for the same
        /// database.</summary>
        /// <param name="database">The database to open.</param>
        /// <param name="grbit">Open database options.</param>
        /// <returns>A database instance.</returns>
        [CLSCompliant(false)]
        public IJetDatabase OpenDatabase(string database, OpenDatabaseGrbit grbit = OpenDatabaseGrbit.None)
        {
            Tracing.TraceFunctionCall("OpenDatabase");
            Helpers.CheckNotNull(database, "database");
            JET_DBID dbid = JET_DBID.Nil;
            int returnCode = NativeMethods.JetOpenDatabaseW(Id, database, null, out dbid.Value,
                (uint)grbit);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return new JetDatabase(this, dbid, database);
        }

        // OMITTED : workaround method
        ///// <summary>Creates a temporary table with a single index. A temporary table stores and
        ///// retrieves records just like an ordinary table created using JetCreateTableColumnIndex.
        ///// However, temporary tables are much faster than ordinary tables due to their volatile
        ///// nature. They can also be used to very quickly sort and perform duplicate removal on
        ///// record sets when accessed in a purely sequential manner.</summary>
        ///// <param name="temporarytable">Description of the temporary table to create on input.
        ///// After a successful call, the structure contains the handle to the temporary table and
        ///// column identifications.</param>
        ///// <returns>Returns the tableid of the temporary table. Closing this tableid with
        ///// <see cref="IJetTable.Close"/> frees the resources associated with the temporary table.</returns>
        //public IJetTable OpenTemporaryTable(JET_OPENTEMPORARYTABLE temporarytable)
        //{
        //    AssertNotNullSession();
        //    Tracing.TraceFunctionCall("OpenTemporaryTable");
        //    Helpers.CheckNotNull(temporarytable, "temporarytable");
        //    _owner._Capabilities.CheckSupportsWindows8Features("OpenTemporaryTable");
        //    NATIVE_OPENTEMPORARYTABLE2 nativetemporarytable = temporarytable.GetNativeOpenTemporaryTable2();
        //    uint[] nativecolumnids = new uint[nativetemporarytable.ccolumn];
        //    NATIVE_COLUMNDEF[] nativecolumndefs = temporarytable.prgcolumndef.GetNativecolumndefs();
        //    unsafe {
        //        using (var gchandlecollection = new GCHandleCollection()) {
        //            // Pin memory
        //            nativetemporarytable.prgcolumndef = (NATIVE_COLUMNDEF*)gchandlecollection.Add(nativecolumndefs);
        //            nativetemporarytable.rgcolumnid = (uint*)gchandlecollection.Add(nativecolumnids);
        //            if (null != temporarytable.pidxunicode) {
        //                NATIVE_UNICODEINDEX2 unicode = temporarytable.pidxunicode.GetNativeUnicodeIndex2();
        //                unicode.szLocaleName = gchandlecollection.Add(
        //                    Util.ConvertToNullTerminatedUnicodeByteArray(
        //                        temporarytable.pidxunicode.GetEffectiveLocaleName()));
        //                nativetemporarytable.pidxunicode = (NATIVE_UNICODEINDEX2*)gchandlecollection.Add(unicode);
        //            }
        //            // Call the interop method
        //            int returnCode = NativeMethods.JetOpenTemporaryTable2(Id, ref nativetemporarytable);
        //            Tracing.TraceResult(returnCode);
        //            EsentExceptionHelper.Check(returnCode);
        //            // Convert the return values
        //            temporarytable.prgcolumndef.SetColumnids(temporarytable.prgcolumnid, nativecolumnids);
        //            return new JetTable(null, new JET_TABLEID { Value = nativetemporarytable.tableid });
        //        }
        //    }
        //}

        /// <summary>Creates a temporary table with a single index. A temporary table stores and
        /// retrieves records just like an ordinary table created using JetCreateTableColumnIndex.
        /// However, temporary tables are much faster than ordinary tables due to their volatile
        /// nature. They can also be used to very quickly sort and perform duplicate removal on
        /// record sets when accessed in a purely sequential manner.</summary>
        /// <param name="columns">Column definitions for the columns created in the temporary table.
        /// </param>
        /// <param name="grbit">Table creation options.</param>
        /// <param name="columnids">The output buffer that receives the array of column IDs generated
        /// during the creation of the temporary table. The column IDs in this array will exactly
        /// correspond to the input array of column definitions. As a result, the size of this buffer
        /// must correspond to the size of the input array.</param>
        /// <param name="lcid">The locale ID to use to compare any Unicode key column data in
        /// the temporary table. Any locale may be used as long as the appropriate language pack
        /// has been installed on the machine. </param>
        /// <param name="unicodeindex">The Locale ID and normalization flags that will be used
        /// to compare any Unicode key column data in the temporary table. When this is not
        /// present then the default options are used. </param>
        /// <returns>Returns the tableid of the temporary table. Closing this tableid with
        /// <see cref="IJetTable.Close"/> frees the resources associated with the temporary table.</returns>
        public IJetCursor OpenTemporaryTable(JET_COLUMNDEF[] columns, TemporaryTableCreationFlags grbit,
            JET_COLUMNID[] columnids, int lcid /* JetOpenTempTable2*/, JET_UNICODEINDEX unicodeindex /* JetOpenTempTable3 */)
        {
            Tracing.TraceFunctionCall("OpenTemporaryTable");
            Helpers.CheckNotNull(columns, "columnns");
            Helpers.CheckNotNull(columnids, "columnids");
            JET_TABLEID tableid = JET_TABLEID.Nil;
            NATIVE_COLUMNDEF[] nativecolumndefs = columns.GetNativecolumndefs();
            uint[] nativecolumnids = new uint[columns.Length];
            int returnCode;
            if (null != unicodeindex) {
                if (0 != lcid) {
                    throw new NotSupportedException("lcid and unicodeindex are exclusive.");
                }
                NATIVE_UNICODEINDEX nativeunicodeindex = unicodeindex.GetNativeUnicodeIndex();
                returnCode = NativeMethods.JetOpenTempTable3(this.Id, nativecolumndefs,
                    checked((uint)columns.Length), ref nativeunicodeindex, (uint)grbit,
                    out tableid.Value, nativecolumnids);
            }
            else {
                if (0 != lcid) {
                    returnCode = NativeMethods.JetOpenTempTable2(this.Id, nativecolumndefs,
                        checked((uint)columns.Length), (uint)lcid, (uint)grbit,
                        out tableid.Value, nativecolumnids);
                }
                else {
                    returnCode = NativeMethods.JetOpenTempTable(this.Id, nativecolumndefs,
                        checked((uint)columns.Length), (uint)grbit, out tableid.Value,
                        nativecolumnids);
                }
            }
            Tracing.TraceResult(returnCode);
            columns.SetColumnids(columnids, nativecolumnids);
            EsentExceptionHelper.Check(returnCode);
            throw new NotImplementedException();
            // return new JetCursor(this, tableid);
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

        /// <summary>Force ew log files creation.</summary>
        /// <returns>The commit ID assoiated with the commit record on Windows 8 and later
        /// versions or a null reference for other platforms.</returns>
        public JET_COMMIT_ID RotateTransactionLogs()
        {
            if (!this.Capabilities.SupportsWindows7Features) {
                throw new NotSupportedException();
            }
            return ManageTransactions(CommitTransactionGrbit.ForceNewLog);
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

        internal static readonly JetSession NullSession;
        private JET_SESID _hSession;
        private JetInstance _owner;
    }
}
