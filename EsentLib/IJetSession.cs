
namespace EsentLib
{
    /// <summary></summary>
    public interface IJetSession
    {
        /// <summary>Attaches a database file for use with a database instance. In order to
        /// use the database, it will need to be subsequently opened with
        /// <see cref="IJetSession.OpenDatabase"/>.</summary>
        /// <param name="database">The database to attach.</param>
        /// <param name="grbit">Attach options.</param>
        /// <returns>An error or warning.</returns>
        void AttachDatabase(string database, AttachDatabaseGrbit grbit);

        /// <summary>Ends a session.</summary>
        /// <param name="grbit">This parameter is not used.</param>
        /// <returns>An error if the call fails.</returns>
        void Close(EndSessionGrbit grbit);

        /// <summary>Creates and attaches a database file.</summary>
        /// <param name="database">The path to the database file to create.</param>
        /// <param name="connect">The parameter is not used.</param>
        /// <param name="grbit">Database creation options.</param>
        /// <returns>An error or warning.</returns>
        JetDatabase CreateDatabase(string database, string connect, CreateDatabaseGrbit grbit);

        /// <summary>Opens a database previously attached with <see cref="AttachDatabase"/>,
        /// for use with a database session. This function can be called multiple times for
        /// the same database.</summary>
        /// <param name="database">The database to open.</param>
        /// <param name="connect">Reserved for future use.</param>
        /// <param name="grbit">Open database options.</param>
        /// <returns>An error or warning.</returns>
        JetDatabase OpenDatabase(string database, string connect, OpenDatabaseGrbit grbit);
    }
}
