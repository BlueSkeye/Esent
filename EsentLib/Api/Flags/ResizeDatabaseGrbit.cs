using System;

namespace EsentLib.Api.Flags
{
    /// <summary>Options for <see cref="IJetDatabase.Resize"/>.</summary>
    [Flags]
    public enum ResizeDatabaseGrbit
    {
        /// <summary>No option.</summary>
        None = 0,

        /// <summary>Only grow the database. If the resize call would shrink the database, do nothing.</summary>
        OnlyGrow = 0x1,

        // ----------- //
        // WINDOWS 8.1 //
        // ----------- //
        /// <summary>Only shrink the database to the desired size, but keeping an 
        /// empty extent at the end. If the resize call would grow the database, do nothing.
        /// In order to use this functionality, <see cref="EsentLib.Implementation.JetInstance.EnableShrinkDatabase"/>
        /// must be set to <see cref="ShrinkDatabaseGrbit.On"/>. Otherwise, an exception may
        /// be thrown.</summary>
        OnlyShrink = 0x2,
    }
}
