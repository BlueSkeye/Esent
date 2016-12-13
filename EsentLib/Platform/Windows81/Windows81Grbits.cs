//-----------------------------------------------------------------------
// <copyright file="Windows81Grbits.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;

using EsentLib.Implementation;
using EsentLib.Platform.Windows8;

namespace EsentLib.Platform
{

    /// <summary>Options that have been introduced in Windows 8.1.</summary>
    public static class Windows81Grbits
    {
        /// <summary>Only shrink the database to the desired size, but keeping an 
        /// empty extent at the end. If the resize call would grow the database, do nothing.
        /// In order to use this functionality, <see cref="JetInstance.EnableShrinkDatabase"/>
        /// must be set to <see cref="Enums.ShrinkDatabaseGrbit.On"/>. Otherwise, an exception may
        /// be thrown.</summary>
        public const ResizeDatabaseGrbit OnlyShrink = (ResizeDatabaseGrbit)0x2;
    }
}
