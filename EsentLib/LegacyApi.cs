//-----------------------------------------------------------------------
// <copyright file="Api.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------
// For design goals initial version from MS see Documentation\MSInitialDesignGoals.txt
// For design goals in this modified version see Documentation\OurDesignGoals.txt

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;

using EsentLib.Api;
using EsentLib.Implementation;
using EsentLib.Jet;
using EsentLib.Jet.Types;

namespace EsentLib
{
    /// <summary>Managed versions of the ESENT Api. This class contains static methods
    /// corresponding with the unmanaged ESENT Api. These methods throw exceptions when
    /// errors are returned.</summary>
    public static partial class LegacyApi
    {
        /// <summary>Initializes static members of the Api class.</summary>
        static LegacyApi()
        {
            // Api.Impl = new JetInstance();
        }
        
        /// <summary>Gets or sets the IJetApi this is called for all functions.</summary>
        internal static IJetInstance Impl { get; set; }

        #region DDL
        // NOT IMPLEMENTED : Resort to JetCreateIndex4
        ///// <summary>Creates indexes over data in an ESE database.</summary>
        ///// <remarks>When creating multiple indexes (i.e. with numIndexCreates
        ///// greater than 1) this method MUST be called
        ///// outside of any transactions and with exclusive access to the
        ///// table. The JET_TABLEID returned by "IJetDatabase.CreateTable"
        ///// will have exlusive access or the table can be opened for
        ///// exclusive access by passing <see cref="OpenTableGrbit.DenyRead"/>
        ///// to <see cref="IJetDatabase.OpenTable"/>.<para>
        ///// <see cref="Api.JetCreateIndex2"/> and <see cref="EsentLib.Platform.Windows8.Windows8Api.JetCreateIndex4"/>
        ///// are very similar, and appear to take the same arguments. The difference is in the
        ///// implementation. JetCreateIndex2 uses LCIDs for Unicode indices (e.g. 1033), while
        ///// JetCreateIndex4 uses Locale Names (e.g. "en-US" or "de-DE". LCIDs are older, and not as well
        ///// supported in newer version of windows.
        ///// </para>
        ///// </remarks>
        ///// <param name="sesid">The session to use.</param>
        ///// <param name="tableid">The table to create the index on.</param>
        ///// <param name="indexcreates">Array of objects describing the indexes to be created.</param>
        ///// <param name="numIndexCreates">Number of index description objects.</param>
        ///// <seealso cref="EsentLib.Platform.Windows8.Windows8Api.JetCreateIndex4"/>
        //public static void JetCreateIndex2(JET_SESID sesid, JET_TABLEID tableid, JET_INDEXCREATE[] indexcreates,
        //    int numIndexCreates)
        //{
        //    EsentExceptionHelper.Check(Impl.JetCreateIndex2(sesid, tableid, indexcreates, numIndexCreates));            
        //}

        // NOT IMPLEMENTED
        ///// <summary>
        ///// Creates a table, adds columns, and indices on that table.
        ///// </summary>
        ///// <param name="sesid">The session to use.</param>
        ///// <param name="dbid">The database to which to add the new table.</param>
        ///// <param name="tablecreate">Object describing the table to create.</param>
        ///// <seealso cref="EsentLib.Platform.Windows8.Windows8Api.JetCreateTableColumnIndex4"/>
        //public static void JetCreateTableColumnIndex3(
        //    JET_SESID sesid,
        //    JET_DBID dbid,
        //    JET_TABLECREATE tablecreate)
        //{
        //    EsentExceptionHelper.Check(Impl.JetCreateTableColumnIndex3(sesid, dbid, tablecreate));
        //}

        #region JetGetTableColumnInfo overloads

        /// <summary>Retrieves information about a table column.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columndef">Filled in with information about the column.</param>
        public static void JetGetTableColumnInfo(JET_SESID sesid, JET_TABLEID tableid,
            string columnName, out JET_COLUMNDEF columndef)
        {
            EsentExceptionHelper.Check(Impl.JetGetTableColumnInfo(sesid, tableid, columnName, out columndef));
        }

        /// <summary>Retrieves information about a table column.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="columnid">The columnid of the column.</param>
        /// <param name="columndef">Filled in with information about the column.</param>
        public static void JetGetTableColumnInfo(JET_SESID sesid, JET_TABLEID tableid,
            JET_COLUMNID columnid, out JET_COLUMNDEF columndef)
        {
            EsentExceptionHelper.Check(Impl.JetGetTableColumnInfo(sesid, tableid, columnid, out columndef));
        }

        /// <summary>Retrieves information about a table column.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columnbase">Filled in with information about the column.</param>
        public static void JetGetTableColumnInfo(JET_SESID sesid, JET_TABLEID tableid,
            string columnName, out JET_COLUMNBASE columnbase)
        {
            EsentExceptionHelper.Check(Impl.JetGetTableColumnInfo(sesid, tableid, columnName, out columnbase));
        }

        #endregion

        #region JetGetColumnInfo overloads

        /// <summary>Retrieves information about a table column.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database that contains the table.</param>
        /// <param name="tablename">The name of the table containing the column.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columndef">Filled in with information about the column.</param>
        public static void JetGetColumnInfo(JET_SESID sesid, JET_DBID dbid, string tablename,
            string columnName, out JET_COLUMNDEF columndef)
        {
            EsentExceptionHelper.Check(Impl.JetGetColumnInfo(sesid, dbid, tablename, columnName, out columndef));
        }

        /// <summary>Retrieves information about all columns in a table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database that contains the table.</param>
        /// <param name="tablename">The name of the table containing the column.</param>
        /// <param name="columnName">This parameter is ignored.</param>
        /// <param name="columnlist">Filled in with information about the columns in the table.</param>
        public static void JetGetColumnInfo(JET_SESID sesid, JET_DBID dbid, string tablename,
            string columnName, out JET_COLUMNLIST columnlist)
        {
            throw new NotImplementedException();
            // EsentExceptionHelper.Check(Impl.JetGetColumnInfo(sesid, dbid, tablename, columnName, out columnlist));
        }

        /// <summary>Retrieves information about a column in a table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database that contains the table.</param>
        /// <param name="tablename">The name of the table containing the column.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columnbase">Filled in with information about the columns in the table.</param>
        public static void JetGetColumnInfo(JET_SESID sesid, JET_DBID dbid, string tablename,
            string columnName, out JET_COLUMNBASE columnbase)
        {
            EsentExceptionHelper.Check(Impl.JetGetColumnInfo(sesid, dbid, tablename, columnName, out columnbase));
        }

        #endregion

        /// <summary>Retrieves information about database objects.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to use.</param>
        /// <param name="objtyp">The type of the object.</param>
        /// <param name="objectName">The object name about which to retrieve information.</param>
        /// <param name="objectinfo">Filled in with information about the objects in the database.</param>
        public static void JetGetObjectInfo(JET_SESID sesid, JET_DBID dbid, JET_objtyp objtyp,
            string objectName, out JET_OBJECTINFO objectinfo)
        {
            EsentExceptionHelper.Check(Impl.JetGetObjectInfo(sesid, dbid, objtyp, objectName, out objectinfo));
        }

        #region JetGetTableInfo overloads

        /// <summary>Retrieves various pieces of information about a table in a database.</summary>
        /// <remarks>This overload is used with <see cref="JET_TblInfo.Default"/>.</remarks>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve information about.</param>
        /// <param name="result">Retrieved information.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>       
        public static void JetGetTableInfo(JET_SESID sesid, JET_TABLEID tableid, out JET_OBJECTINFO result, JET_TblInfo infoLevel)
        {
            EsentExceptionHelper.Check(Impl.JetGetTableInfo(sesid, tableid, out result, infoLevel));
        }

        /// <summary>Retrieves various pieces of information about a table in a database.</summary>
        /// <remarks>This overload is used with <see cref="JET_TblInfo.Name"/> and
        /// <see cref="JET_TblInfo.TemplateTableName"/>.</remarks>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve information about.</param>
        /// <param name="result">Retrieved information.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        public static void JetGetTableInfo(JET_SESID sesid, JET_TABLEID tableid, out string result, JET_TblInfo infoLevel)
        {
            EsentExceptionHelper.Check(Impl.JetGetTableInfo(sesid, tableid, out result, infoLevel));
        }

        /// <summary>Retrieves various pieces of information about a table in a database.</summary>
        /// <remarks>This overload is used with <see cref="JET_TblInfo.Dbid"/>.</remarks>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve information about.</param>
        /// <param name="result">Retrieved information.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        public static void JetGetTableInfo(JET_SESID sesid, JET_TABLEID tableid, out JET_DBID result, JET_TblInfo infoLevel)
        {
            EsentExceptionHelper.Check(Impl.JetGetTableInfo(sesid, tableid, out result, infoLevel));
        }

        /// <summary>Retrieves various pieces of information about a table in a database.</summary>
        /// <remarks>This overload is used with <see cref="JET_TblInfo.SpaceUsage"/> and
        /// <see cref="JET_TblInfo.SpaceAlloc"/>.</remarks>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve information about.</param>
        /// <param name="result">Retrieved information.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        public static void JetGetTableInfo(JET_SESID sesid, JET_TABLEID tableid, int[] result, JET_TblInfo infoLevel)
        {
            EsentExceptionHelper.Check(Impl.JetGetTableInfo(sesid, tableid, result, infoLevel));
        }

        /// <summary>Retrieves various pieces of information about a table in a database.</summary>
        /// <remarks>This overload is used with <see cref="JET_TblInfo.SpaceOwned"/> and
        /// <see cref="JET_TblInfo.SpaceAvailable"/>.</remarks>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve information about.</param>
        /// <param name="result">Retrieved information.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        public static void JetGetTableInfo(JET_SESID sesid, JET_TABLEID tableid, out int result, JET_TblInfo infoLevel)
        {
            EsentExceptionHelper.Check(Impl.JetGetTableInfo(sesid, tableid, out result, infoLevel));
        }

        #endregion

        #region JetGetIndexInfo overloads

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to use.</param>
        /// <param name="tablename">The name of the table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index to retrieve information about.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        [CLSCompliant(false)]
        public static void JetGetIndexInfo(JET_SESID sesid, JET_DBID dbid, string tablename,
            string indexname, out ushort result, JET_IdxInfo infoLevel)
        {
            EsentExceptionHelper.Check(Impl.JetGetIndexInfo(sesid, dbid, tablename, indexname, out result, infoLevel));
        }

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to use.</param>
        /// <param name="tablename">The name of the table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index to retrieve information about.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        public static void JetGetIndexInfo(JET_SESID sesid, JET_DBID dbid, string tablename,
            string indexname, out int result, JET_IdxInfo infoLevel)
        {
            EsentExceptionHelper.Check(Impl.JetGetIndexInfo(sesid, dbid, tablename, indexname, out result, infoLevel));
        }

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to use.</param>
        /// <param name="tablename">The name of the table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index to retrieve information about.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        public static void JetGetIndexInfo(JET_SESID sesid, JET_DBID dbid, string tablename,
            string indexname, out JET_INDEXID result, JET_IdxInfo infoLevel)
        {
            EsentExceptionHelper.Check(Impl.JetGetIndexInfo(sesid, dbid, tablename, indexname, out result, infoLevel));
        }

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to use.</param>
        /// <param name="tablename">The name of the table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index to retrieve information about.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        public static void JetGetIndexInfo(JET_SESID sesid, JET_DBID dbid, string tablename,
            string indexname, out JET_INDEXLIST result, JET_IdxInfo infoLevel)
        {
            EsentExceptionHelper.Check(Impl.JetGetIndexInfo(sesid, dbid, tablename, indexname, out result, infoLevel));
        }

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to use.</param>
        /// <param name="tablename">The name of the table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index to retrieve information about.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        public static void JetGetIndexInfo(JET_SESID sesid, JET_DBID dbid, string tablename,
            string indexname, out string result, JET_IdxInfo infoLevel)
        {
            EsentExceptionHelper.Check(Impl.JetGetIndexInfo(sesid, dbid, tablename, indexname, out result, infoLevel));
        }

        #endregion

        #region JetGetTableIndexInfo overloads

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        [CLSCompliant(false)]
        public static void JetGetTableIndexInfo(JET_SESID sesid, JET_TABLEID tableid,
            string indexname, out ushort result, JET_IdxInfo infoLevel)
        {
            EsentExceptionHelper.Check(Impl.JetGetTableIndexInfo(sesid, tableid, indexname, out result, infoLevel));
        }

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        public static void JetGetTableIndexInfo(JET_SESID sesid, JET_TABLEID tableid,
            string indexname, out int result, JET_IdxInfo infoLevel)
        {
            EsentExceptionHelper.Check(Impl.JetGetTableIndexInfo(sesid, tableid, indexname, out result, infoLevel));
        }

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        public static void JetGetTableIndexInfo(JET_SESID sesid, JET_TABLEID tableid,
            string indexname, out JET_INDEXID result, JET_IdxInfo infoLevel)
        {
            EsentExceptionHelper.Check(Impl.JetGetTableIndexInfo(sesid, tableid, indexname, out result, infoLevel));
        }

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        public static void JetGetTableIndexInfo(JET_SESID sesid, JET_TABLEID tableid,
            string indexname, out JET_INDEXLIST result, JET_IdxInfo infoLevel)
        {
            EsentExceptionHelper.Check(Impl.JetGetTableIndexInfo(sesid, tableid, indexname, out result, infoLevel));
        }

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve index information about.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        public static void JetGetTableIndexInfo(JET_SESID sesid, JET_TABLEID tableid,
            string indexname, out string result, JET_IdxInfo infoLevel)
        {
            EsentExceptionHelper.Check(Impl.JetGetTableIndexInfo(sesid, tableid, indexname, out result, infoLevel));
        }

        #endregion

        /// <summary>Changes the name of an existing table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database containing the table.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="newTableName">The new name of the table.</param>
        public static void JetRenameTable(JET_SESID sesid, JET_DBID dbid, string tableName, string newTableName)
        {
            EsentExceptionHelper.Check(Impl.JetRenameTable(sesid, dbid, tableName, newTableName));
        }

        /// <summary>Changes the name of an existing column.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="name">The name of the column.</param>
        /// <param name="newName">The new name of the column.</param>
        /// <param name="grbit">Column rename options.</param>
        public static void JetRenameColumn(JET_SESID sesid, JET_TABLEID tableid, string name,
            string newName, RenameColumnGrbit grbit)
        {
            EsentExceptionHelper.Check(Impl.JetRenameColumn(sesid, tableid, name, newName, grbit));
        }

        /// <summary>Changes the default value of an existing column.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database containing the column.</param>
        /// <param name="tableName">The name of the table containing the column.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="data">The new default value.</param>
        /// <param name="dataSize">Size of the new default value.</param>
        /// <param name="grbit">Column default value options.</param>
        public static void JetSetColumnDefaultValue(JET_SESID sesid, JET_DBID dbid, string tableName,
            string columnName, byte[] data, int dataSize, SetColumnDefaultValueGrbit grbit)
        {
            EsentExceptionHelper.Check(Impl.JetSetColumnDefaultValue(sesid, dbid, tableName,
                columnName, data, dataSize, grbit));
        }

        #endregion

        #region Navigation

        /// <summary>Positions a cursor to an index entry that is associated with the specified
        /// secondary index bookmark. The secondary index bookmark must be used with the same
        /// index over the same table from which it was originally retrieved. The secondary
        /// index bookmark for an index entry can be retrieved using
        /// <see cref="JetGotoSecondaryIndexBookmark"/>.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table cursor to position.</param>
        /// <param name="secondaryKey">The buffer that contains the secondary key.</param>
        /// <param name="secondaryKeySize">The size of the secondary key.</param>
        /// <param name="primaryKey">The buffer that contains the primary key.</param>
        /// <param name="primaryKeySize">The size of the primary key.</param>
        /// <param name="grbit">Options for positioning the bookmark.</param>
        public static void JetGotoSecondaryIndexBookmark(JET_SESID sesid, JET_TABLEID tableid,
            byte[] secondaryKey, int secondaryKeySize, byte[] primaryKey, int primaryKeySize,
            GotoSecondaryIndexBookmarkGrbit grbit)
        {
            EsentExceptionHelper.Check(
                Impl.JetGotoSecondaryIndexBookmark(
                    sesid, tableid, secondaryKey, secondaryKeySize, primaryKey, primaryKeySize, grbit));
        }

        // <seealso cref="TryMoveFirst"/>, <seealso cref="TryMoveLast"/>,
        // <seealso cref="TryMoveNext"/>, <seealso cref="TryMovePrevious"/>.
        /// <summary>
        /// Navigate through an index. The cursor can be positioned at the start or
        /// end of the index and moved backwards and forwards by a specified number
        /// of index entries. Also see
        /// </summary>
        /// <param name="sesid">The session to use for the call.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <param name="numRows">An offset which indicates how far to move the cursor.</param>
        /// <param name="grbit">Move options.</param>
        public static void JetMove(JET_SESID sesid, JET_TABLEID tableid, int numRows, MoveGrbit grbit)
        {
            throw new NotImplementedException();
            // EsentExceptionHelper.Check(Impl.JetMove(sesid, tableid, numRows, grbit));
        }

        // <seealso cref="TryMoveFirst"/>, <seealso cref="TryMoveLast"/>,
        // <seealso cref="TryMoveNext"/>, <seealso cref="TryMovePrevious"/>.
        /// <summary>
        /// Navigate through an index. The cursor can be positioned at the start or
        /// end of the index and moved backwards and forwards by a specified number
        /// of index entries. Also see
        /// </summary>
        /// <param name="sesid">The session to use for the call.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <param name="numRows">An offset which indicates how far to move the cursor.</param>
        /// <param name="grbit">Move options.</param>
        public static void JetMove(JET_SESID sesid, JET_TABLEID tableid, JET_Move numRows, MoveGrbit grbit)
        {
            throw new NotImplementedException();
            //EsentExceptionHelper.Check(Impl.JetMove(sesid, tableid, (int)numRows, grbit));
        }

        // Also see <seealso cref="TrySeek"/>.
        /// <summary>Efficiently positions a cursor to an index entry that matches the search
        /// criteria specified by the search key in that cursor and the specified inequality.
        /// A search key must have been previously constructed using IJetTable.MakeKey.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <param name="grbit">Seek options.</param>
        /// <returns>An ESENT warning.</returns>
        public static JET_wrn JetSeek(JET_SESID sesid, JET_TABLEID tableid, SeekGrbit grbit)
        {
            return EsentExceptionHelper.Check(Impl.JetSeek(sesid, tableid, grbit));
        }

        // Also see <seealso cref="TrySetIndexRange"/>.
        /// <summary>
        /// Temporarily limits the set of index entries that the cursor can walk using
        /// <see cref="JetMove(JET_SESID,JET_TABLEID,int,MoveGrbit)"/> to those starting
        /// from the current index entry and ending at the index entry that matches the
        /// search criteria specified by the search key in that cursor and the specified
        /// bound criteria. A search key must have been previously constructed using
        /// IJetTable.MakeKey.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to set the index range on.</param>
        /// <param name="grbit">Index range options.</param>
        public static void JetSetIndexRange(JET_SESID sesid, JET_TABLEID tableid, SetIndexRangeGrbit grbit)
        {
            EsentExceptionHelper.Check(Impl.JetSetIndexRange(sesid, tableid, grbit));
        }

        // <seealso cref="IntersectIndexes"/>.
        /// <summary>
        /// Computes the intersection between multiple sets of index entries from different secondary
        /// indices over the same table. This operation is useful for finding the set of records in a
        /// table that match two or more criteria that can be expressed using index ranges. Also see
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="ranges">
        /// An the index ranges to intersect. The tableids in the ranges
        /// must have index ranges set on them. Use <see cref="JetSetIndexRange"/>
        /// to create an index range.
        /// </param>
        /// <param name="numRanges">
        /// The number of index ranges.
        /// </param>
        /// <param name="recordlist">
        /// Returns information about the temporary table containing the intersection results.
        /// </param>
        /// <param name="grbit">Intersection options.</param>
        public static void JetIntersectIndexes(JET_SESID sesid, JET_INDEXRANGE[] ranges,
            int numRanges, out JET_RECORDLIST recordlist, IntersectIndexesGrbit grbit)
        {
            EsentExceptionHelper.Check(Impl.JetIntersectIndexes(sesid, ranges, numRanges, out recordlist, grbit));
        }

        /// <summary>
        /// Counts the number of entries in the current index from the current position forward.
        /// The current position is included in the count. The count can be greater than the
        /// total number of records in the table if the current index is over a multi-valued
        /// column and instances of the column have multiple-values. If the table is empty,
        /// then 0 will be returned for the count. 
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to count the records in.</param>
        /// <param name="numRecords">Returns the number of records.</param>
        /// <param name="maxRecordsToCount">
        /// The maximum number of records to count. A value of 0 indicates that the count
        /// is unlimited.
        /// </param>
        public static void JetIndexRecordCount(JET_SESID sesid, JET_TABLEID tableid, out int numRecords, int maxRecordsToCount)
        {
            if (0 == maxRecordsToCount)
            {
                // BUG: Older versions of esent (e.g. Windows XP) don't use 0 as an unlimited count,
                // instead they simply count zero records (which isn't very useful). To make
                // sure this API works as advertised we will increase the maximum record count.
                maxRecordsToCount = int.MaxValue;
            }

            EsentExceptionHelper.Check(Impl.JetIndexRecordCount(sesid, tableid, out numRecords, maxRecordsToCount));
        }

        /// <summary>
        /// Notifies the database engine that the application is scanning the entire
        /// index that the cursor is positioned on. Consequently, the methods that
        /// are used to access the index data will be tuned to make this scenario as
        /// fast as possible. 
        /// Also see <seealso cref="JetResetTableSequential"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor that will be accessing the data.</param>
        /// <param name="grbit">Reserved for future use.</param>
        public static void JetSetTableSequential(JET_SESID sesid, JET_TABLEID tableid, SetTableSequentialGrbit grbit)
        {
            EsentExceptionHelper.Check(Impl.JetSetTableSequential(sesid, tableid, grbit));
        }

        /// <summary>
        /// Notifies the database engine that the application is no longer scanning the
        /// entire index the cursor is positioned on. This call reverses a notification
        /// sent by <see cref="JetSetTableSequential"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor that was accessing the data.</param>
        /// <param name="grbit">Reserved for future use.</param>
        public static void JetResetTableSequential(JET_SESID sesid, JET_TABLEID tableid, ResetTableSequentialGrbit grbit)
        {
            EsentExceptionHelper.Check(Impl.JetResetTableSequential(sesid, tableid, grbit));
        }

        /// <summary>
        /// Returns the fractional position of the current record in the current index
        /// in the form of a <see cref="JET_RECPOS"/> structure.
        /// Also see <seealso cref="JetGotoPosition"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor positioned on the record.</param>
        /// <param name="recpos">Returns the approximate fractional position of the record.</param>
        public static void JetGetRecordPosition(JET_SESID sesid, JET_TABLEID tableid, out JET_RECPOS recpos)
        {
            EsentExceptionHelper.Check(Impl.JetGetRecordPosition(sesid, tableid, out recpos));
        }

        /// <summary>
        /// Moves a cursor to a new location that is a fraction of the way through
        /// the current index. 
        /// Also see <seealso cref="JetGetRecordPosition"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <param name="recpos">The approximate position to move to.</param>
        public static void JetGotoPosition(JET_SESID sesid, JET_TABLEID tableid, JET_RECPOS recpos)
        {
            EsentExceptionHelper.Check(Impl.JetGotoPosition(sesid, tableid, recpos));
        }

        #endregion

        #region Data Retrieval

        /// <summary>
        /// Retrieves the bookmark for the record that is associated with the index entry
        /// at the current position of a cursor. This bookmark can then be used to
        /// reposition that cursor back to the same record using <see cref="ICursor.GotoBookmark"/>. 
        /// The bookmark will be no longer than <see cref="JetEnvironment.BookmarkMost"/>
        /// bytes.
        /// Also see <seealso cref="GetBookmark"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the bookmark from.</param>
        /// <param name="bookmark">Buffer to contain the bookmark.</param>
        /// <param name="bookmarkSize">Size of the bookmark buffer.</param>
        /// <param name="actualBookmarkSize">Returns the actual size of the bookmark.</param>
        public static void JetGetBookmark(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize, out int actualBookmarkSize)
        {
            EsentExceptionHelper.Check(Impl.JetGetBookmark(sesid, tableid, bookmark, bookmarkSize, out actualBookmarkSize));
        }

        /// <summary>
        /// Retrieves a special bookmark for the secondary index entry at the
        /// current position of a cursor. This bookmark can then be used to
        /// efficiently reposition that cursor back to the same index entry
        /// using JetGotoSecondaryIndexBookmark. This is most useful when
        /// repositioning on a secondary index that contains duplicate keys or
        /// that contains multiple index entries for the same record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the bookmark from.</param>
        /// <param name="secondaryKey">Output buffer for the secondary key.</param>
        /// <param name="secondaryKeySize">Size of the secondary key buffer.</param>
        /// <param name="actualSecondaryKeySize">Returns the size of the secondary key.</param>
        /// <param name="primaryKey">Output buffer for the primary key.</param>
        /// <param name="primaryKeySize">Size of the primary key buffer.</param>
        /// <param name="actualPrimaryKeySize">Returns the size of the primary key.</param>
        /// <param name="grbit">Options for the call.</param>
        public static void JetGetSecondaryIndexBookmark(JET_SESID sesid, JET_TABLEID tableid,
            byte[] secondaryKey, int secondaryKeySize, out int actualSecondaryKeySize,
            byte[] primaryKey, int primaryKeySize, out int actualPrimaryKeySize,
            GetSecondaryIndexBookmarkGrbit grbit)
        {
            EsentExceptionHelper.Check(
                Impl.JetGetSecondaryIndexBookmark(sesid, tableid, secondaryKey, secondaryKeySize,
                    out actualSecondaryKeySize, primaryKey, primaryKeySize, out actualPrimaryKeySize,
                    grbit));
        }

        /// <summary>
        /// Retrieves the key for the index entry at the current position of a cursor.
        /// Also see <seealso cref="RetrieveKey"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the key from.</param>
        /// <param name="data">The buffer to retrieve the key into.</param>
        /// <param name="dataSize">The size of the buffer.</param>
        /// <param name="actualDataSize">Returns the actual size of the data.</param>
        /// <param name="grbit">Retrieve key options.</param>
        public static void JetRetrieveKey(JET_SESID sesid, JET_TABLEID tableid, byte[] data, int dataSize, out int actualDataSize, RetrieveKeyGrbit grbit)
        {
            EsentExceptionHelper.Check(Impl.JetRetrieveKey(sesid, tableid, data, dataSize, out actualDataSize, grbit));
        }

        /// <summary>Retrieves multiple column values from the current record in a single
        /// operation. An array of JET_RETRIEVECOLUMN structures is used to describe the set
        /// of column values to be retrieved, and to describe output buffers for each column
        /// value to be retrieved.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the data from.</param>
        /// <param name="retrievecolumns">An array of one or more <see cref="JET_RETRIEVECOLUMN"/>
        /// objects describing the data to be retrieved.</param>
        /// <param name="numColumns">The number of entries in the columns array.</param>
        /// <returns>If any column retrieved is truncated due to an insufficient length buffer,
        /// then the API will return <see cref="JET_wrn.BufferTruncated"/>. However other errors
        /// JET_wrnColumnNull are returned only in the error field of the
        /// <see cref="JET_RETRIEVECOLUMN"/> object.</returns>
        public static JET_wrn JetRetrieveColumns(JET_SESID sesid, JET_TABLEID tableid,
            JET_RETRIEVECOLUMN[] retrievecolumns, int numColumns)
        {
            if (null == retrievecolumns) {
                throw new ArgumentNullException("retrievecolumns");
            }
            if (numColumns < 0 || numColumns > retrievecolumns.Length) {
                throw new ArgumentOutOfRangeException("numColumns", numColumns, "cannot be negative or greater than retrievecolumns.Length");
            }
            unsafe {
                NATIVE_RETRIEVECOLUMN* nativeretrievecolumns = stackalloc NATIVE_RETRIEVECOLUMN[numColumns];
                int err = LegacyApi.PinColumnsAndRetrieve(sesid, tableid, nativeretrievecolumns, retrievecolumns, numColumns, 0);
                for (int i = 0; i < numColumns; ++i) {
                    retrievecolumns[i].UpdateFromNativeRetrievecolumn(ref nativeretrievecolumns[i]);
                }
                return EsentExceptionHelper.Check(err);
            }
        }

        /// <summary>
        /// Efficiently retrieves a set of columns and their values from the
        /// current record of a cursor or the copy buffer of that cursor. The
        /// columns and values retrieved can be restricted by a list of
        /// column IDs, itagSequence numbers, and other characteristics. This
        /// column retrieval API is unique in that it returns information in
        /// dynamically allocated memory that is obtained using a
        /// user-provided realloc compatible callback. This new flexibility
        /// permits the efficient retrieval of column data with specific
        /// characteristics (such as size and multiplicity) that are unknown
        /// to the caller. This eliminates the need for the use of the discovery
        /// modes of JetRetrieveColumn to determine those
        /// characteristics in order to setup a final call to
        /// JetRetrieveColumn that will successfully retrieve
        /// the desired data.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve data from.</param>
        /// <param name="numColumnids">The numbers of JET_ENUMCOLUMNIDS.</param>
        /// <param name="columnids">
        /// An optional array of column IDs, each with an optional array of itagSequence
        /// numbers to enumerate.
        /// </param>
        /// <param name="numColumnValues">
        /// Returns the number of column values retrieved.
        /// </param>
        /// <param name="columnValues">
        /// Returns the enumerated column values.
        /// </param>
        /// <param name="allocator">
        /// Callback used to allocate memory.
        /// </param>
        /// <param name="allocatorContext">
        /// Context for the allocation callback.
        /// </param>
        /// <param name="maxDataSize">
        /// Sets a cap on the amount of data to return from a long text or long
        /// binary column. This parameter can be used to prevent the enumeration
        /// of an extremely large column value.
        /// </param>
        /// <param name="grbit">Retrieve options.</param>
        /// <returns>A warning or success.</returns>
        [CLSCompliant(false)]
        public static JET_wrn JetEnumerateColumns(
            JET_SESID sesid,
            JET_TABLEID tableid,
            int numColumnids,
            JET_ENUMCOLUMNID[] columnids,
            out int numColumnValues,
            out JET_ENUMCOLUMN[] columnValues,
            JET_PFNREALLOC allocator,
            IntPtr allocatorContext,
            int maxDataSize,
            EnumerateColumnsGrbit grbit)
        {
            return EsentExceptionHelper.Check(
                Impl.JetEnumerateColumns(
                    sesid,
                    tableid,
                    numColumnids,
                    columnids,
                    out numColumnValues,
                    out columnValues,
                    allocator,
                    allocatorContext,
                    maxDataSize,
                    grbit));
        }

        /// <summary>
        /// Efficiently retrieves a set of columns and their values from the
        /// current record of a cursor or the copy buffer of that cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve data from.</param>
        /// <param name="grbit">Enumerate options.</param>
        /// <param name="enumeratedColumns">The discovered columns and their values.</param>
        /// <returns>A warning or success.</returns>
        public static JET_wrn JetEnumerateColumns(
            JET_SESID sesid,
            JET_TABLEID tableid,
            EnumerateColumnsGrbit grbit,
            out IEnumerable<EnumeratedColumn> enumeratedColumns)
        {
            return EsentExceptionHelper.Check(
                Impl.JetEnumerateColumns(sesid, tableid, grbit, out enumeratedColumns));
        }

        #endregion

        #region DML

        /// <summary>
        /// Deletes the current record in a database table.
        /// </summary>
        /// <param name="sesid">The session that opened the cursor.</param>
        /// <param name="tableid">The cursor on a database table. The current row will be deleted.</param>
        public static void JetDelete(JET_SESID sesid, JET_TABLEID tableid)
        {
            EsentExceptionHelper.Check(Impl.JetDelete(sesid, tableid));
        }

        /// <summary>
        /// The JetSetColumn function modifies a single column value in a modified record to be inserted or to
        /// update the current record. It can overwrite an existing value, add a new value to a sequence of
        /// values in a multi-valued column, remove a value from a sequence of values in a multi-valued column,
        /// or update all or part of a long value (a column of type <see cref="JET_coltyp.LongText"/>
        /// or <see cref="JET_coltyp.LongBinary"/>). 
        /// </summary>
        /// <remarks>
        /// The SetColumn methods provide datatype-specific overrides which may be more efficient.
        /// </remarks>
        /// <param name="sesid">The session which is performing the update.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        /// <param name="dataSize">The size of data to set.</param>
        /// <param name="grbit">SetColumn options.</param>
        /// <param name="setinfo">Used to specify itag or long-value offset.</param>
        /// <returns>A warning code.</returns>
        public static JET_wrn JetSetColumn(JET_SESID sesid, JET_TABLEID tableid,
            JET_COLUMNID columnid, byte[] data, int dataSize, SetColumnGrbit grbit,
            JET_SETINFO setinfo)
        {
            return InternalApi.JetSetColumn(sesid, tableid, columnid, data, dataSize,
                0, grbit, setinfo);
        }

        /// <summary>
        /// Allows an application to set multiple column values in a single
        /// operation. An array of <see cref="JET_SETCOLUMN"/> structures is
        /// used to describe the set of column values to be set, and to describe
        /// input buffers for each column value to be set.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to set the columns on.</param>
        /// <param name="setcolumns">
        /// An array of <see cref="JET_SETCOLUMN"/> structures describing the
        /// data to set.
        /// </param>
        /// <param name="numColumns">
        /// Number of entries in the setcolumns parameter.
        /// </param>
        /// <returns>
        /// A warning. If the last column set has a warning, then
        /// this warning will be returned from JetSetColumns itself.
        /// </returns>
        [SecurityPermissionAttribute(SecurityAction.LinkDemand)]
        public static JET_wrn JetSetColumns(JET_SESID sesid, JET_TABLEID tableid, JET_SETCOLUMN[] setcolumns, int numColumns)
        {
            if (null == setcolumns)
            {
                throw new ArgumentNullException("setcolumns");
            }

            if (numColumns < 0 || numColumns > setcolumns.Length)
            {
                throw new ArgumentOutOfRangeException("numColumns", numColumns, "cannot be negative or greater than setcolumns.Length");
            }

            using (var gchandles = new GCHandleCollection())
            {
                unsafe
                {
                    NATIVE_SETCOLUMN* nativeSetColumns = stackalloc NATIVE_SETCOLUMN[numColumns];

                    // For performance, copy small amounts of data into a local buffer instead
                    // of pinning the data.
                    const int BufferSize = 128;
                    byte* buffer = stackalloc byte[BufferSize];
                    int bufferRemaining = BufferSize;

                    for (int i = 0; i < numColumns; ++i)
                    {
                        setcolumns[i].CheckDataSize();
                        nativeSetColumns[i] = setcolumns[i].GetNativeSetcolumn();
                        if (null == setcolumns[i].pvData)
                        {
                            nativeSetColumns[i].pvData = IntPtr.Zero;
                        }
                        else if (bufferRemaining >= setcolumns[i].cbData)
                        {
                            nativeSetColumns[i].pvData = new IntPtr(buffer);
                            Marshal.Copy(setcolumns[i].pvData, setcolumns[i].ibData, nativeSetColumns[i].pvData, setcolumns[i].cbData);
                            buffer += setcolumns[i].cbData;
                            bufferRemaining -= setcolumns[i].cbData;
                            Debug.Assert(bufferRemaining >= 0, "Buffer remaining is negative");
                        }
                        else
                        {
                            byte* pinnedBuffer = (byte*)gchandles.Add(setcolumns[i].pvData).ToPointer();
                            nativeSetColumns[i].pvData = new IntPtr(pinnedBuffer + setcolumns[i].ibData);
                        }
                    }

                    int err = Impl.JetSetColumns(sesid, tableid, nativeSetColumns, numColumns);
                    for (int i = 0; i < numColumns; ++i)
                    {
                        setcolumns[i].err = (JET_wrn)nativeSetColumns[i].err;
                    }

                    return EsentExceptionHelper.Check(err);
                }
            }
        }

        /// <summary>
        /// Performs an atomic addition operation on one column. This function allows
        /// multiple sessions to update the same record concurrently without conflicts.
        /// Also see <seealso cref="EscrowUpdate"/>.
        /// </summary>
        /// <param name="sesid">
        /// The session to use. The session must be in a transaction.
        /// </param>
        /// <param name="tableid">The cursor to update.</param>
        /// <param name="columnid">
        /// The column to update. This must be an escrow updatable column.
        /// </param>
        /// <param name="delta">The buffer containing the addend.</param>
        /// <param name="deltaSize">The size of the addend.</param>
        /// <param name="previousValue">
        /// An output buffer that will recieve the current value of the column. This buffer
        /// can be null.
        /// </param>
        /// <param name="previousValueLength">The size of the previousValue buffer.</param>
        /// <param name="actualPreviousValueLength">Returns the actual size of the previousValue.</param>
        /// <param name="grbit">Escrow update options.</param>
        public static void JetEscrowUpdate(
            JET_SESID sesid,
            JET_TABLEID tableid,
            JET_COLUMNID columnid,
            byte[] delta,
            int deltaSize,
            byte[] previousValue,
            int previousValueLength,
            out int actualPreviousValueLength,
            EscrowUpdateGrbit grbit)
        {
            EsentExceptionHelper.Check(Impl.JetEscrowUpdate(
                sesid,
                tableid,
                columnid,
                delta,
                deltaSize,
                previousValue,
                previousValueLength,
                out actualPreviousValueLength,
                grbit));
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// Allows the application to configure the database engine to issue
        /// notifications to the application for specific events. These
        /// notifications are associated with a specific table and remain in
        /// effect only until the instance containing the table is shut down
        /// using.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">
        /// A cursor opened on the table that the callback should be
        /// registered on.
        /// </param>
        /// <param name="cbtyp">
        /// The callback reasons for which the application wishes to receive notifications.
        /// </param>
        /// <param name="callback">The callback function.</param>
        /// <param name="context">A context that will be given to the callback.</param>
        /// <param name="callbackId">
        /// A handle that can later be used to cancel the registration of the given
        /// callback function using <see cref="JetUnregisterCallback"/>.
        /// </param>
        public static void JetRegisterCallback(
            JET_SESID sesid,
            JET_TABLEID tableid,
            JET_cbtyp cbtyp,
            JET_CALLBACK callback,
            IntPtr context,
            out JET_HANDLE callbackId)
        {
            EsentExceptionHelper.Check(Impl.JetRegisterCallback(sesid, tableid, cbtyp, callback, context, out callbackId));
        }

        /// <summary>
        /// Configures the database engine to stop issuing notifications to the
        /// application as previously requested through
        /// <see cref="JetRegisterCallback"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">
        /// A cursor opened on the table that the callback should be
        /// registered on.
        /// </param>
        /// <param name="cbtyp">
        /// The callback reasons for which the application no longer wishes to receive notifications.
        /// </param>
        /// <param name="callbackId">
        /// The handle of the registered callback that was returned by <see cref="JetRegisterCallback"/>.
        /// </param>
        public static void JetUnregisterCallback(JET_SESID sesid, JET_TABLEID tableid, JET_cbtyp cbtyp, JET_HANDLE callbackId)
        {
            EsentExceptionHelper.Check(Impl.JetUnregisterCallback(sesid, tableid, cbtyp, callbackId));
        }

        #endregion

        #region Online Maintenance

        /// <summary>Starts and stops database defragmentation tasks that improves data
        /// organization within a database.</summary>
        /// <param name="sesid">The session to use for the call.</param>
        /// <param name="dbid">The database to be defragmented.</param>
        /// <param name="tableName">Under some options defragmentation is performed for the
        /// entire database described by the given  database ID, and other options require
        /// the name of the table to defragment.</param>
        /// <param name="passes">When starting an online defragmentation task, this parameter
        /// sets the maximum number of defragmentation passes. When stopping an online
        /// defragmentation task, this parameter is set to the number of passes performed.
        /// This is not honored in all modes.</param>
        /// <param name="seconds">When starting an online defragmentation task, this parameter
        /// sets the maximum time for defragmentation. When stopping an online defragmentation
        /// task, this output buffer is set to the length of time used for defragmentation.
        /// This is not honored in all modes.</param>
        /// <param name="grbit">Defragmentation options.</param>
        /// <returns>A warning code.</returns>
        /// <seealso cref="LegacyApi.Defragment"/>
        public static JET_wrn JetDefragment(JET_SESID sesid, JET_DBID dbid, string tableName,
            ref int passes, ref int seconds, DefragGrbit grbit)
        {
            return EsentExceptionHelper.Check(Impl.JetDefragment(sesid, dbid, tableName, ref passes, ref seconds, grbit));
        }

        /// <summary>
        /// Starts and stops database defragmentation tasks that improves data
        /// organization within a database.
        /// </summary>
        /// <remarks>
        /// The callback passed to JetDefragment2 can be executed asynchronously.
        /// The GC doesn't know that the unmanaged code has a reference to the callback
        /// so it is important to make sure the callback isn't collected.
        /// </remarks>
        /// <param name="sesid">The session to use for the call.</param>
        /// <param name="dbid">The database to be defragmented.</param>
        /// <param name="tableName">
        /// Under some options defragmentation is performed for the entire database described by the given 
        /// database ID, and other options require the name of the table to defragment.
        /// </param>
        /// <param name="passes">
        /// When starting an online defragmentation task, this parameter sets the maximum number of defragmentation
        /// passes. When stopping an online defragmentation task, this parameter is set to the number of passes
        /// performed. This is not honored in all modes .</param>
        /// <param name="seconds">
        /// When starting an online defragmentation task, this parameter sets
        /// the maximum time for defragmentation. When stopping an online
        /// defragmentation task, this output buffer is set to the length of
        /// time used for defragmentation. This is not honored in all modes .</param>
        /// <param name="callback">Callback function that defrag uses to report progress.</param>
        /// <param name="grbit">Defragmentation options.</param>
        /// <returns>A warning code.</returns>
        public static JET_wrn JetDefragment2(
            JET_SESID sesid, 
            JET_DBID dbid, 
            string tableName, 
            ref int passes, 
            ref int seconds, 
            JET_CALLBACK callback, 
            DefragGrbit grbit)
        {
            return EsentExceptionHelper.Check(Impl.JetDefragment2(sesid, dbid, tableName, ref passes, ref seconds, callback, grbit));
        }

        /// <summary>
        /// Performs idle cleanup tasks or checks the version store status in ESE.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="grbit">A combination of JetIdleGrbit flags.</param>
        /// <returns>An error code if the operation fails.</returns>
        public static JET_wrn JetIdle(JET_SESID sesid, IdleGrbit grbit)
        {
            return EsentExceptionHelper.Check(Impl.JetIdle(sesid, grbit));
        }

        #endregion

        #region Misc

        #endregion

        /// <summary>Enables the application to associate a context handle known as Local Storage
        /// with a cursor or the table associated with that cursor. This context handle can be
        /// used by the application to store auxiliary data that is associated with a cursor or
        /// table. The application is later notified using a runtime callback when the context
        /// handle must be released. This makes it possible to associate dynamically  state with
        /// a cursor or table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to use.</param>
        /// <param name="ls">The context handle to be associated with the session or cursor.</param>
        /// <param name="grbit">Set options.</param>
        public static void JetSetLS(JET_SESID sesid, JET_TABLEID tableid, JET_LS ls, LsGrbit grbit)
        {
            EsentExceptionHelper.Check(Impl.JetSetLS(sesid, tableid, ls, grbit));
        }

        /// <summary>Enables the application to retrieve the context handle known as Local Storage
        /// that is associated with a cursor or the table associated with that cursor. This context
        /// handle must have been previously set using <see cref="JetSetLS"/>. JetGetLS can also
        /// be used to simultaneously fetch the current context handle for a cursor or table and
        /// reset that context handle.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to use.</param>
        /// <param name="ls">Returns the retrieved context handle.</param>
        /// <param name="grbit">Retrieve options.</param>
        public static void JetGetLS(JET_SESID sesid, JET_TABLEID tableid, out JET_LS ls, LsGrbit grbit)
        {
            EsentExceptionHelper.Check(Impl.JetGetLS(sesid, tableid, out ls, grbit));
        }

        /// <summary>Duplicates an open cursor and returns a handle to the duplicated cursor.
        /// If the cursor that was duplicated was a read-only cursor then the duplicated
        /// cursor is also a read-only cursor.Any state related to constructing a search key
        /// or updating a record isnot copied into the duplicated cursor. In addition, the
        /// location of the original cursor is not duplicated into the duplicated cursor. The
        /// duplicated cursor is always opened on the clustered index and its location is
        /// always on the first row of the table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to duplicate.</param>
        /// <param name="newTableid">The duplicated cursor.</param>
        /// <param name="grbit">Reserved for future use.</param>
        public static void JetDupCursor(JET_SESID sesid, JET_TABLEID tableid, out JET_TABLEID newTableid, DupCursorGrbit grbit)
        {
            EsentExceptionHelper.Check(Impl.JetDupCursor(sesid, tableid, out newTableid, grbit));
        }

        // ----- //
        // VISTA //
        // ----- //
        /// <summary>Retrieves information about a column in a table.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database that contains the table.</param>
        /// <param name="tablename">The name of the table containing the column.</param>
        /// <param name="columnid">The ID of the column.</param>
        /// <param name="columnbase">Filled in with information about the columns in the
        /// table.</param>
        public static void JetGetColumnInfo(JET_SESID sesid, JET_DBID dbid, string tablename,
                JET_COLUMNID columnid, out JET_COLUMNBASE columnbase)
        {
            EsentExceptionHelper.Check(LegacyApi.Impl.JetGetColumnInfo(sesid, dbid, tablename, columnid, out columnbase));
        }

        #region JetGetTableColumnInfo overloads
        /// <summary>Retrieves information about a table column.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="columnid">The columnid of the column.</param>
        /// <param name="columnbase">Filled in with information about the column.</param>
        public static void JetGetTableColumnInfo(JET_SESID sesid, JET_TABLEID tableid,
            JET_COLUMNID columnid, out JET_COLUMNBASE columnbase)
        {
            EsentExceptionHelper.Check(LegacyApi.Impl.JetGetTableColumnInfo(sesid, tableid, columnid, out columnbase));
        }
        #endregion

        /// <summary>Retrieves record size information from the desired location.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">
        /// The cursor that will be used for the API call. The cursor must be positioned on
        /// a record, or have an update prepared.</param>
        /// <param name="recsize">Returns the size of the record.</param>
        /// <param name="grbit">Call options.</param>
        public static void JetGetRecordSize(JET_SESID sesid, JET_TABLEID tableid, ref JET_RECSIZE recsize, GetRecordSizeGrbit grbit)
        {
            EsentExceptionHelper.Check(LegacyApi.Impl.JetGetRecordSize(sesid, tableid, ref recsize, grbit));
        }

        // --------- //
        // WINDOWS 7 //
        // --------- //
        /// <summary>Crash dump options for Watson.</summary>
        /// <param name="grbit">Crash dump options.</param>
        public static void JetConfigureProcessForCrashDump(CrashDumpGrbit grbit)
        {
            EsentExceptionHelper.Check(LegacyApi.Impl.JetConfigureProcessForCrashDump(grbit));
        }

        /// <summary>If the records with the specified keys are not in the buffer cache then
        /// start asynchronous reads to bring the records into the database buffer cache.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to issue the prereads against.</param>
        /// <param name="keys">The keys to preread. The keys must be sorted.</param>
        /// <param name="keyLengths">The lengths of the keys to preread.</param>
        /// <param name="keyIndex">The index of the first key in the keys array to read.</param>
        /// <param name="keyCount">The maximum number of keys to preread.</param>
        /// <param name="keysPreread">Returns the number of keys to actually preread.</param>
        /// <param name="grbit">Preread options. Used to specify the direction of the
        /// preread.</param>
        public static void JetPrereadKeys(JET_SESID sesid, JET_TABLEID tableid, byte[][] keys,
            int[] keyLengths, int keyIndex, int keyCount, out int keysPreread, PrereadKeysGrbit grbit)
        {
            EsentExceptionHelper.Check(LegacyApi.Impl.JetPrereadKeys(sesid, tableid, keys, keyLengths, keyIndex, keyCount, out keysPreread, grbit));
        }

        /// <summary>If the records with the specified keys are not in the buffer cache then
        /// start asynchronous reads to bring the records into the database buffer cache.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to issue the prereads against.</param>
        /// <param name="keys">The keys to preread. The keys must be sorted.</param>
        /// <param name="keyLengths">The lengths of the keys to preread.</param>
        /// <param name="keyCount">The maximum number of keys to preread.</param>
        /// <param name="keysPreread">Returns the number of keys to actually preread.</param>
        /// <param name="grbit">Preread options. Used to specify the direction of the preread.
        /// </param>
        public static void JetPrereadKeys(JET_SESID sesid, JET_TABLEID tableid, byte[][] keys,
            int[] keyLengths, int keyCount, out int keysPreread, PrereadKeysGrbit grbit)
        {
            JetPrereadKeys(sesid, tableid, keys, keyLengths, 0, keyCount, out keysPreread, grbit);
        }

        // --------- //
        // WINDOWS 8 //
        // --------- //
        #region Transactions

        ///// <summary>
        ///// Commits the changes made to the state of the database during the current save point
        ///// and migrates them to the previous save point. If the outermost save point is committed
        ///// then the changes made during that save point will be committed to the state of the
        ///// database and the session will exit the transaction.
        ///// </summary>
        ///// <param name="sesid">The session to commit the transaction for.</param>
        ///// <param name="grbit">Commit options.</param>
        ///// <param name="durableCommit">Duration to commit lazy transaction.</param>
        ///// <param name="commitId">Commit-id associated with this commit record.</param>
        //public static void JetCommitTransaction2(JET_SESID sesid, CommitTransactionGrbit grbit,
        //    TimeSpan durableCommit, out JET_COMMIT_ID commitId)
        //{
        //    EsentExceptionHelper.Check(Api.Impl.JetCommitTransaction2(sesid, grbit, durableCommit, out commitId));
        //}

        #endregion

        /// <summary>Gets extended information about an error.</summary>
        /// <param name="error">The error code about which to retrieve information.</param>
        /// <param name="errinfo">Information about the specified error code.</param>
        public static void JetGetErrorInfo(JET_err error, out JET_ERRINFOBASIC errinfo)
        {
            EsentExceptionHelper.Check(LegacyApi.Impl.JetGetErrorInfo(error, out errinfo));
        }

        #region DDL

        // OMITTED : workaround method
        ///// <summary>
        ///// Creates a temporary table with a single index. A temporary table
        ///// stores and retrieves records just like an ordinary table created
        ///// using JetCreateTableColumnIndex. However, temporary tables are
        ///// much faster than ordinary tables due to their volatile nature.
        ///// They can also be used to very quickly sort and perform duplicate
        ///// removal on record sets when accessed in a purely sequential manner.
        ///// Also see
        ///// <seealso cref="IJetSession.OpenTemporaryTable"/>.</summary>
        ///// <param name="sesid">The session to use.</param>
        ///// <param name="temporarytable">
        ///// Description of the temporary table to create on input. After a
        ///// successful call, the structure contains the handle to the temporary
        ///// table and column identifications. Use <see cref="IJetTable.Close"/>
        ///// to free the temporary table when finished.
        ///// </param>
        //public static void JetOpenTemporaryTable2(JET_SESID sesid, JET_OPENTEMPORARYTABLE temporarytable)
        //{
        //    EsentExceptionHelper.Check(LegacyApi.Impl.JetOpenTemporaryTable2(sesid, temporarytable));
        //}

        /// <summary>
        /// Creates a table, adds columns, and indices on that table.
        /// seealso cref="Api.JetCreateTableColumnIndex3"
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to which to add the new table.</param>
        /// <param name="tablecreate">Object describing the table to create.</param>
        /// seealso cref="Api.JetCreateTableColumnIndex3"
        public static void JetCreateTableColumnIndex4(JET_SESID sesid, JET_DBID dbid,
            JET_TABLECREATE tablecreate)
        {
            EsentExceptionHelper.Check(LegacyApi.Impl.JetCreateTableColumnIndex4(sesid, dbid, tablecreate));
        }
        #endregion

        #region Misc
        /// <summary>If the records with the specified key ranges are not in the buffer cache,
        /// then start asynchronous reads to bring the records into the database buffer cache.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to issue the prereads against.</param>
        /// <param name="indexRanges">The key ranges to preread.</param>
        /// <param name="rangeIndex">The index of the first key range in the array to read.</param>
        /// <param name="rangeCount">The maximum number of key ranges to preread.</param>
        /// <param name="rangesPreread">Returns the number of keys actually preread.</param>
        /// <param name="columnsPreread">List of column ids for long value columns to preread.</param>
        /// <param name="grbit">Preread options. Used to specify the direction of the preread.</param>
        /// <returns><c>true</c> if some preread done, <c>false</c> otherwise.</returns>
        public static bool JetTryPrereadIndexRanges(JET_SESID sesid, JET_TABLEID tableid,
            JET_INDEX_RANGE[] indexRanges, int rangeIndex, int rangeCount, out int rangesPreread,
            JET_COLUMNID[] columnsPreread, PrereadIndexRangesGrbit grbit)
        {
            JET_err err = (JET_err)LegacyApi.Impl.JetPrereadIndexRanges(sesid, tableid, indexRanges, rangeIndex, rangeCount, out rangesPreread, columnsPreread, grbit);
            return err >= JET_err.Success;
        }

        /// <summary>If the records with the specified key ranges are not in the buffer cache,
        /// then start asynchronous reads to bring the records into the database buffer cache.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to issue the prereads against.</param>
        /// <param name="indexRanges">The key ranges to preread.</param>
        /// <param name="rangeIndex">The index of the first key range in the array to read.</param>
        /// <param name="rangeCount">The maximum number of key ranges to preread.</param>
        /// <param name="rangesPreread">Returns the number of keys actually preread.</param>
        /// <param name="columnsPreread">List of column ids for long value columns to preread.</param>
        /// <param name="grbit">Preread options. Used to specify the direction of the preread.</param>
        public static void JetPrereadIndexRanges(JET_SESID sesid, JET_TABLEID tableid,
            JET_INDEX_RANGE[] indexRanges, int rangeIndex, int rangeCount, out int rangesPreread,
            JET_COLUMNID[] columnsPreread, PrereadIndexRangesGrbit grbit)
        {
            EsentExceptionHelper.Check(LegacyApi.Impl.JetPrereadIndexRanges(sesid, tableid, indexRanges, rangeIndex, rangeCount, out rangesPreread, columnsPreread, grbit));
        }

        /// <summary>If the records with the specified key ranges are not in the buffer cache
        /// then start asynchronous reads to bring the records into the database buffer cache.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to issue the prereads against.</param>
        /// <param name="keysStart">The start of key ranges to preread.</param>
        /// <param name="keyStartLengths">The lengths of the start keys to preread.</param>
        /// <param name="keysEnd">The end of key rangess to preread.</param>
        /// <param name="keyEndLengths">The lengths of the end keys to preread.</param>
        /// <param name="rangeIndex">The index of the first key range in the array to read.</param>
        /// <param name="rangeCount">The maximum number of key ranges to preread.</param>
        /// <param name="rangesPreread">Returns the number of keys actually preread.</param>
        /// <param name="columnsPreread">List of column ids for long value columns to preread.</param>
        /// <param name="grbit">Preread options. Used to specify the direction of the preread.</param>
        public static void PrereadKeyRanges(JET_SESID sesid, JET_TABLEID tableid, byte[][] keysStart,
            int[] keyStartLengths, byte[][] keysEnd, int[] keyEndLengths, int rangeIndex,
            int rangeCount, out int rangesPreread, JET_COLUMNID[] columnsPreread, PrereadIndexRangesGrbit grbit)
        {
            EsentExceptionHelper.Check(LegacyApi.Impl.JetPrereadKeyRanges(sesid, tableid, keysStart, keyStartLengths, keysEnd, keyEndLengths, rangeIndex, rangeCount, out rangesPreread, columnsPreread, grbit));
        }

        /// <summary>Set an array of simple filters for <see cref="LegacyApi.JetMove(JET_SESID,JET_TABLEID,int,MoveGrbit)"/>.</summary>
        /// <param name="sesid">The session to use for the call.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <param name="filters">Simple record filters.</param>
        /// <param name="grbit">Move options.</param>
        public static void JetSetCursorFilter(JET_SESID sesid, JET_TABLEID tableid, JET_INDEX_COLUMN[] filters, CursorFilterGrbit grbit)
        {
            EsentExceptionHelper.Check(LegacyApi.Impl.JetSetCursorFilter(sesid, tableid, filters, grbit));
        }

        #endregion
    }
}
