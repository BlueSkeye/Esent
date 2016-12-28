//-----------------------------------------------------------------------
// <copyright file="Transaction.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Globalization;

using EsentLib.Api;
using EsentLib.Jet;

namespace EsentLib.Implementation
{
    /// <summary>A class that encapsulates a transaction on a JET_SESID.</summary>
    public class JetTransaction : EsentResource, IJetTransaction
    {
        /// <summary>Initializes a new instance of the Transaction class. This automatically
        /// begins a transaction. The transaction will be rolled back if not explicitly committed.</summary>
        /// <param name="session">The session to start the transaction for.</param>
        /// <param name="readOnly">true if transaction is readonly.</param>
        /// <param name="userTransactionId">An optional identifier supplied by the user for
        /// identifying the transaction.</param>
        internal JetTransaction(JetSession session, bool readOnly = false, long? userTransactionId = null)
        {
            if (null == session) {
                throw new ArgumentNullException("session");
            }
            this._session = session;
            this.Begin(readOnly, userTransactionId);
        }

        /// <summary></summary>
        public IntPtr SessionId
        {
            get { return _session.Id; }
        }

        /// <summary>Gets the current transaction level of the <see cref="JetTransaction"/>.
        /// Requires Win10.</summary>
        public int TransactionLevel
        {
            get { return _session.TransactionLevel; }
        }

        /// <summary>Gets a value indicating whether this object is currently in a transaction.</summary>
        public bool IsInTransaction
        { 
            get
            {
                this.CheckObjectIsNotDisposed();
                return this.HasResource;
            }
        }

        /// <summary>Causes a session to enter a transaction or create a new save point in an
        /// existing transaction.</summary>
        /// <param name="readOnly">true if transaction is readonly.</param>
        /// <param name="userTransactionId">An optional identifier supplied by the user for
        /// identifying the transaction.</param>
        /// <returns>An error if the call fails.</returns>
        private void Begin(bool readOnly, long? userTransactionId = null)
        {
            this.CheckObjectIsNotDisposed();
            if (this.IsInTransaction) {
                throw new InvalidOperationException("Already in a transaction");
            }
            uint creationFlags = (readOnly) ? (uint)BeginTransactionGrbit.ReadOnly : 0;
            int returnCode = (userTransactionId.HasValue)
                ? NativeMethods.JetBeginTransaction3(SessionId, userTransactionId.Value,
                    creationFlags)
                : (readOnly)
                    ? NativeMethods.JetBeginTransaction2(SessionId, creationFlags)
                    : NativeMethods.JetBeginTransaction(SessionId);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            this.ResourceWasAllocated();
            Debug.Assert(this.IsInTransaction, "Begin finished, but object isn't in a transaction");
        }

        /// <summary>Commit a transaction. This object should be in a transaction.
        /// Commits the changes made to the state of the database during the current save point
        /// and migrates them to the previous save point. If the outermost save point is committed
        /// then the changes made during that save point will be committed to the state of the
        /// database and the session will exit the transaction.</summary>
        /// <returns>Commit-id for this commit record on Windows 8 and later otherise a null
        /// reference..</returns>
        public JET_COMMIT_ID Commit()
        {
            return _Commit(null);
        }

        /// <summary>Lasily xommit a transaction. This object should be in a transaction.
        /// Commits the changes made to the state of the database during the current save point
        /// and migrates them to the previous save point. If the outermost save point is committed
        /// then the changes made during that save point will be committed to the state of the
        /// database and the session will exit the transaction.</summary>
        /// <returns>Commit-id for this commit record on Windows 8 and later otherise a null
        /// reference..</returns>
        public JET_COMMIT_ID Commit(TimeSpan durableCommit)
        {
            return _Commit(durableCommit);
        }

        private JET_COMMIT_ID _Commit(TimeSpan? durableCommit)
        {
            Tracing.TraceFunctionCall("Commit");
            this.CheckObjectIsNotDisposed();
            if (!this.IsInTransaction) {
                throw new InvalidOperationException("Not in a transaction");
            }
            bool windows8FeaturesEnabled = _session.Capabilities.SupportsWindows8Features;
            NATIVE_COMMIT_ID nativeCommitId = new NATIVE_COMMIT_ID();
            uint bitmask = 0;
            uint cmsecDurableCommit = 0;
            if (durableCommit.HasValue) {
                bitmask = unchecked((uint)CommitTransactionGrbit.LazyFlush);
                cmsecDurableCommit = (uint)durableCommit.Value.TotalMilliseconds;
            }
            int returnCode = (windows8FeaturesEnabled)
                ? NativeMethods.JetCommitTransaction2(SessionId, bitmask,
                    cmsecDurableCommit, ref nativeCommitId)
                : NativeMethods.JetCommitTransaction(SessionId, bitmask);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            this.ResourceWasReleased();
            Debug.Assert(!this.IsInTransaction, "Commit finished, but object is still in a transaction");
            if (!windows8FeaturesEnabled) { return null; }
            JET_COMMIT_ID commitId = new JET_COMMIT_ID(nativeCommitId);
            return commitId;
        }

        /// <summary>Undoes the changes made to the state of the database and returns to the
        /// last save point. JetRollback will also close any cursors opened during the save
        /// point. If the outermost save point is undone, the session will exit the transaction.</summary>
        /// <param name="grbit">Rollback options.</param>
        /// <returns>An error if the call fails.</returns>
        public void Rollback(RollbackTransactionGrbit grbit = RollbackTransactionGrbit.None)
        {
            Tracing.TraceFunctionCall("Rollback");
            this.CheckObjectIsNotDisposed();
            if (!this.IsInTransaction) { throw new InvalidOperationException("Not in a transaction"); }
            int returnCode = NativeMethods.JetRollback(SessionId, unchecked((uint)grbit));
            Tracing.TraceResult(returnCode);
            this.ResourceWasReleased();
            Debug.Assert(!this.IsInTransaction, "Rollback finished, but object is still in a transaction");
            return;
        }

        /// <summary>Called when the transaction is being disposed while active. This should
        /// rollback the transaction.</summary>
        protected override void ReleaseResource()
        {
            this.Rollback();
        }

        /// <summary>Returns a <see cref="T:System.String"/> that represents the current
        /// <see cref="JetTransaction"/>.</summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current
        /// <see cref="JetTransaction"/>.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Transaction (0x{0:x})", SessionId);
        }

        /// <summary>The underlying session.</summary>
        private readonly JetSession _session;

        /// <summary>Options for <see cref="JetSession.BeginTransaction"/>.</summary>
        [Flags()]
        [CLSCompliant(false)]
        public enum BeginTransactionGrbit : uint
        {
            /// <summary>Default options.</summary>
            None = 0,

            /// <summary>The transaction will not modify the database. If an update is attempted,
            /// that operation will fail with <see cref="JET_err.TransReadOnly"/>. This option is
            /// ignored unless it is requested when the given session is not already in a transaction.
            /// </summary>
            ReadOnly = 0x1,
        }
    }
}
