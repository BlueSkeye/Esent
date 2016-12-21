//-----------------------------------------------------------------------
// <copyright file="OnlineMaintenanceHelpers.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using EsentLib.Implementation;
using EsentLib.Jet;

namespace EsentLib
{
    /// <summary>Helper methods for the ESENT API. These methods deal with database meta-data.
    /// </summary>
    public static partial class LegacyApi
    {
        /// <summary>Starts and stops database defragmentation tasks that improves data
        /// organization within a database.</summary>
        /// <param name="sesid">The session to use for the call.</param>
        /// <param name="dbid">The database to be defragmented.</param>
        /// <param name="tableName">
        /// Under some options defragmentation is performed for the entire database described
        /// by the given  database ID, and other options require the name of the table to
        /// defragment.</param>
        /// <param name="grbit">Defragmentation options.</param>
        /// <returns>A warning code.</returns>
        /// <seealso cref="LegacyApi.JetDefragment"/>
        public static JET_wrn Defragment(JET_SESID sesid, JET_DBID dbid, string tableName,
            DefragGrbit grbit)
        {
            return EsentExceptionHelper.Check(Impl.Defragment(sesid, dbid, tableName, grbit));
        }
    }
}
