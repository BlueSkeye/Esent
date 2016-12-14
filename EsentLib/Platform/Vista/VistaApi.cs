//-----------------------------------------------------------------------
// <copyright file="VistaApi.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using EsentLib.Jet;
using EsentLib.Jet.Types;
using EsentLib.Jet.Vista;

namespace EsentLib.Platform.Vista
{
    /// <summary>
    /// ESENT APIs that were first supported in Windows Vista.
    /// </summary>
    public static class VistaApi
    {
        /// <summary>
        /// Retrieves information about a column in a table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database that contains the table.</param>
        /// <param name="tablename">The name of the table containing the column.</param>
        /// <param name="columnid">The ID of the column.</param>
        /// <param name="columnbase">Filled in with information about the columns in the table.</param>
        public static void JetGetColumnInfo(
                JET_SESID sesid,
                JET_DBID dbid,
                string tablename,
                JET_COLUMNID columnid,
                out JET_COLUMNBASE columnbase)
        {
            EsentExceptionHelper.Check(Api.Impl.JetGetColumnInfo(sesid, dbid, tablename, columnid, out columnbase));
        }

        #region JetGetTableColumnInfo overloads
        /// <summary>
        /// Retrieves information about a table column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="columnid">The columnid of the column.</param>
        /// <param name="columnbase">Filled in with information about the column.</param>
        public static void JetGetTableColumnInfo(
            JET_SESID sesid,
            JET_TABLEID tableid,
            JET_COLUMNID columnid,
            out JET_COLUMNBASE columnbase)
        {
            EsentExceptionHelper.Check(Api.Impl.JetGetTableColumnInfo(sesid, tableid, columnid, out columnbase));
        }
        #endregion

        /// <summary>
        /// Creates a temporary table with a single index. A temporary table
        /// stores and retrieves records just like an ordinary table created
        /// using JetCreateTableColumnIndex. However, temporary tables are
        /// much faster than ordinary tables due to their volatile nature.
        /// They can also be used to very quickly sort and perform duplicate
        /// removal on record sets when accessed in a purely sequential manner.
        /// Also see
        /// <seealso cref="Api.JetOpenTempTable"/>,
        /// <seealso cref="Api.JetOpenTempTable3"/>.
        /// </summary>
        /// <remarks>
        /// Introduced in Windows Vista. Use <see cref="Api.JetOpenTempTable3"/>
        /// for earlier versions of Esent.
        /// </remarks>
        /// <param name="sesid">The session to use.</param>
        /// <param name="temporarytable">
        /// Description of the temporary table to create on input. After a
        /// successful call, the structure contains the handle to the temporary
        /// table and column identifications. Use <see cref="Api.JetCloseTable"/>
        /// to free the temporary table when finished.
        /// </param>
        public static void JetOpenTemporaryTable(JET_SESID sesid, JET_OPENTEMPORARYTABLE temporarytable)
        {
            EsentExceptionHelper.Check(Api.Impl.JetOpenTemporaryTable(sesid, temporarytable));
        }

        /// <summary>
        /// Retrieves record size information from the desired location.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">
        /// The cursor that will be used for the API call. The cursor must be
        /// positioned on a record, or have an update prepared.
        /// </param>
        /// <param name="recsize">Returns the size of the record.</param>
        /// <param name="grbit">Call options.</param>
        public static void JetGetRecordSize(JET_SESID sesid, JET_TABLEID tableid, ref JET_RECSIZE recsize, GetRecordSizeGrbit grbit)
        {
            EsentExceptionHelper.Check(Api.Impl.JetGetRecordSize(sesid, tableid, ref recsize, grbit));
        }
    }
}