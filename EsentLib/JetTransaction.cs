//-----------------------------------------------------------------------
// <copyright file="Transaction.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Globalization;

using EsentLib.Jet;
using EsentLib.Platform.Windows8;
using EsentLib.Platform.Windows10;

namespace EsentLib.Implementation
{
    /// <summary>A class that encapsulates a transaction on a JET_SESID.</summary>
    public class JetTransaction : EsentResource, IJetTransaction
    {
        /// <summary>Initializes a new instance of the Transaction class. This automatically
        /// begins a transaction. The transaction will be rolled back if not explicitly committed.</summary>
        /// <param name="session">The session to start the transaction for.</param>
        /// <param name="grbit"></param>
        public JetTransaction(JetSession session, BeginTransactionGrbit grbit = BeginTransactionGrbit.None)
        {
            if (null == session) {
                throw new ArgumentNullException("session");
            }
            this._session = session;
            this.Begin(grbit);
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
        /// <returns>An error if the call fails.</returns>
        private void Begin(BeginTransactionGrbit grbit)
        {
            this.CheckObjectIsNotDisposed();
            if (this.IsInTransaction) {
                throw new InvalidOperationException("Already in a transaction");
            }
            int returnCode = (BeginTransactionGrbit.None == grbit)
                ? NativeMethods.JetBeginTransaction(SessionId)
                : NativeMethods.JetBeginTransaction2(SessionId, (uint)grbit);
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
        /// <param name="grbit">JetCommitTransaction options.</param>
        public void Commit(CommitTransactionGrbit grbit)
        {
            Tracing.TraceFunctionCall("Commit");
            this.CheckObjectIsNotDisposed();
            if (!this.IsInTransaction) {
                throw new InvalidOperationException("Not in a transaction");
            }
            int returnCode = NativeMethods.JetCommitTransaction(SessionId, unchecked((uint)grbit));
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            this.ResourceWasReleased();
            Debug.Assert(!this.IsInTransaction, "Commit finished, but object is still in a transaction");
        }

        /// <summary>Commit a transaction. This object should be in a transaction.
        /// Commits the changes made to the state of the database during the current save point
        /// and migrates them to the previous save point. If the outermost save point is committed
        /// then the changes made during that save point will be committed to the state of the
        /// database and the session will exit the transaction.</summary>
        /// <param name="grbit">JetCommitTransaction options.</param>
        /// <param name="durableCommit">Duration for committing lazy transactions.</param>
        /// <param name="commitId">Commit-id for this commit record.</param>
        public void Commit(CommitTransactionGrbit grbit, TimeSpan durableCommit, out JET_COMMIT_ID commitId)
        {
            this.CheckObjectIsNotDisposed();
            if (!this.IsInTransaction) {
                throw new InvalidOperationException("Not in a transaction");
            }
            Tracing.TraceFunctionCall("Commit");
            _session.Capabilities.CheckSupportsWindows8Features("JetCommitTransaction2");
            uint cmsecDurableCommit = (uint)durableCommit.TotalMilliseconds;
            NATIVE_COMMIT_ID nativeCommitId = new NATIVE_COMMIT_ID();
            unsafe {
                int err = NativeMethods.JetCommitTransaction2(SessionId, unchecked((uint)grbit),
                    cmsecDurableCommit, ref nativeCommitId);
                Tracing.TraceResult(err);
                EsentExceptionHelper.Check(err);
            }
            commitId = new JET_COMMIT_ID(nativeCommitId);
            this.ResourceWasReleased();
            Debug.Assert(!this.IsInTransaction, "Commit finished, but object is still in a transaction");
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
    }
}
