namespace EsentLib.Api.Flags
{
    /// <summary>An enum for use by <see cref="IJetSession.FlushTransactions(TransactionFlushKind)"/></summary>
    public enum TransactionFlushKind
    {
        /// <summary>All transactions previously committed by any session that have not yet been
        /// flushed to the transaction log file will be flushed immediately. </summary>
        AllPending = 1,
        /// <summary>If the session has previously committed any transactions and they have not
        /// yet been flushed to the transaction log file, they should be flushed immediately.</summary>
        SessionPending = 2
    }
}
