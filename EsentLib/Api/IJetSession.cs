﻿using System;

using EsentLib.Api.Flags;
using EsentLib.Jet;

namespace EsentLib.Api
{
    /// <summary></summary>
    [CLSCompliant(false)]
    public interface IJetSession : IDisposable
    {
        /// <summary>Get session id.</summary>
        IntPtr Id { get; }

        /// <summary>Get instance this session is bound to.</summary>
        IJetInstance Owner { get; }

        /// <summary>Gets the current transaction level of the <see cref="IJetTransaction"/>.
        /// Requires Win10.</summary>
        int TransactionLevel { get; }

        /// <summary>Attaches a database file for use with a database instance. In order to
        /// use the database, it will need to be subsequently opened with
        /// <see cref="IJetSession.OpenDatabase"/>.</summary>
        /// <param name="database">The database to attach.</param>
        /// <param name="grbit">Attach options.</param>
        /// <param name="maxPages">The maximum size, in database pages, of the database.
        /// Passing 0 means there is no enforced maximum.</param>
        /// <returns>An error or warning.</returns>
        [CLSCompliant(false)]
        void AttachDatabase(string database, AttachDatabaseGrbit grbit, uint maxPages = 0);

        /// <summary>Causes a session to enter a transaction or create a new save point in an
        /// existing transaction.</summary>
        /// <returns></returns>
        IJetTransaction BeginTransaction(BeginTransactionGrbit grbit = BeginTransactionGrbit.None);

        /// <summary>Ends a session.</summary>
        /// <param name="grbit">This parameter is not used.</param>
        /// <returns>An error if the call fails.</returns>
        void Close(EndSessionGrbit grbit);

        /// <summary>Creates and attaches a database file.</summary>
        /// <param name="database">The path to the database file to create.</param>
        /// <param name="grbit">Database creation options.</param>
        /// <param name="maxPages">The maximum size, in database pages, of the database.
        /// Passing 0 means there is no enforced maximum.</param>
        /// <returns>A database instance.</returns>
        [CLSCompliant(false)]
        IJetDatabase CreateDatabase(string database, CreateDatabaseGrbit grbit,
            uint maxPages = 0);

        /// <summary>Initialize a new ESE session in the same instance as the given
        /// sesid.</summary>
        /// <returns>An error if the call fails.</returns>
        IJetSession Duplicate();

        /// <summary></summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        JET_COMMIT_ID FlushTransactions(TransactionFlushKind kind);

        /// <summary>Opens a database previously attached with <see cref="AttachDatabase"/>,
        /// for use with a database session. This function can be called multiple times for
        /// the same database.</summary>
        /// <param name="database">The database to open.</param>
        /// <param name="connect">Reserved for future use.</param>
        /// <param name="grbit">Open database options.</param>
        /// <returns>An error or warning.</returns>
        IJetDatabase OpenDatabase(string database, string connect, OpenDatabaseGrbit grbit);

        /// <summary>Creates a temporary table with a single index. A temporary table stores and
        /// retrieves records just like an ordinary table created using JetCreateTableColumnIndex.
        /// However, temporary tables are much faster than ordinary tables due to their volatile
        /// nature. They can also be used to very quickly sort and perform duplicate removal on
        /// record sets when accessed in a purely sequential manner. Also see
        /// <seealso cref="LegacyApi.JetOpenTempTable3"/>.
        /// <seealso cref="LegacyApi.JetOpenTemporaryTable"/>.</summary>
        /// <param name="columns">Column definitions for the columns created in the temporary table.
        /// </param>
        /// <param name="grbit">Table creation options.</param>
        /// <param name="columnids">The output buffer that receives the array of column IDs generated
        /// during the creation of the temporary table. The column IDs in this array will exactly
        /// correspond to the input array of column definitions. As a result, the size of this buffer
        /// must correspond to the size of the input array.</param>
        IJetTable OpenTemporaryTable(JET_COLUMNDEF[] columns, TempTableGrbit grbit, JET_COLUMNID[] columnids);

        /// <summary>Disassociates a session from the current thread. This should be
        /// used in conjunction with <see cref="IJetSession.SetContext"/>.</summary>
        void ResetContext();

        /// <summary>Force ew log files creation.</summary>
        /// <returns>The commit ID assoiated with the commit record on Windows 8 and later
        /// versions or a null reference for other platforms.</returns>
        JET_COMMIT_ID RotateTransactionLogs();

        /// <summary>Associates a session with the current thread using the given context
        /// handle. This association overrides the default engine requirement that a
        /// transaction for a given session must occur entirely on the same thread.
        /// Use <see cref="ResetContext"/> to remove the association.</summary>
        /// <param name="context">The context to set.</param>
        void SetContext(IntPtr context);
    }

}