﻿using System;

namespace EsentLib
{
    /// <summary></summary>
    [CLSCompliant(false)]
    public interface IJetSession
    {
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

        /// <summary>Opens a database previously attached with <see cref="AttachDatabase"/>,
        /// for use with a database session. This function can be called multiple times for
        /// the same database.</summary>
        /// <param name="database">The database to open.</param>
        /// <param name="connect">Reserved for future use.</param>
        /// <param name="grbit">Open database options.</param>
        /// <returns>An error or warning.</returns>
        IJetDatabase OpenDatabase(string database, string connect, OpenDatabaseGrbit grbit);

        /// <summary>Disassociates a session from the current thread. This should be
        /// used in conjunction with <see cref="IJetSession.SetContext"/>.</summary>
        void ResetContext();

        /// <summary>Associates a session with the current thread using the given context
        /// handle. This association overrides the default engine requirement that a
        /// transaction for a given session must occur entirely on the same thread.
        /// Use <see cref="ResetContext"/> to remove the association.</summary>
        /// <param name="context">The context to set.</param>
        void SetContext(IntPtr context);
    }
}
