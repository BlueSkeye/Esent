using System;

using EsentLib.Jet;

namespace EsentLib
{
    /// <summary></summary>
    public interface IJetTransaction
    {
        /// <summary>Gets the current transaction level of the <see cref="IJetTransaction"/>.
        /// Requires Win10.</summary>
        int TransactionLevel { get; }

        /// <summary>Commit a transaction. This object should be in a transaction.
        /// Commits the changes made to the state of the database during the current save point
        /// and migrates them to the previous save point. If the outermost save point is committed
        /// then the changes made during that save point will be committed to the state of the
        /// database and the session will exit the transaction.</summary>
        JET_COMMIT_ID Commit();

        /// <summary>Lasily xommit a transaction. This object should be in a transaction.
        /// Commits the changes made to the state of the database during the current save point
        /// and migrates them to the previous save point. If the outermost save point is committed
        /// then the changes made during that save point will be committed to the state of the
        /// database and the session will exit the transaction.</summary>
        /// <returns>Commit-id for this commit record on Windows 8 and later otherise a null
        /// reference..</returns>
        JET_COMMIT_ID Commit(TimeSpan durableCommit);

        /// <summary>Undoes the changes made to the state of the database and returns to the
        /// last save point. JetRollback will also close any cursors opened during the save
        /// point. If the outermost save point is undone, the session will exit the transaction.</summary>
        /// <param name="grbit">Rollback options.</param>
        /// <returns>An error if the call fails.</returns>
        void Rollback(RollbackTransactionGrbit grbit = RollbackTransactionGrbit.None);
    }
}
