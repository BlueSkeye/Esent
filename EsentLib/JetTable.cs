using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using EsentLib.Api;
using EsentLib.Jet;

namespace EsentLib.Implementation
{
    internal class JetTable : IJetTable
    {
        internal JetTable(JetDatabase owner, JET_TABLEID id)
        {
            _owner = owner;
            _id = id;
        }

        /// <summary>Gets the database this table belongs to.</summary>
        public IJetDatabase Database
        {
            get { return _owner; }
        }

        internal IntPtr Id
        {
            get { return _id.Value; }
        }

        /// <summary>Add a new column to an existing table.</summary>
        /// <param name="session">Session to use.</param>
        /// <param name="column">The name of the column.</param>
        /// <param name="columndef">The definition of the column.</param>
        /// <param name="defaultValue">The default value of the column.</param>
        /// <param name="defaultValueSize">The size of the default value.</param>
        public IJetColumn AddColumn(IJetSession session, string column, JET_COLUMNDEF columndef,
            byte[] defaultValue, int defaultValueSize)
        {
            Tracing.TraceFunctionCall("JetAddColumn");
            JET_COLUMNID columnid = JET_COLUMNID.Nil;
            Helpers.CheckNotNull(column, "column");
            Helpers.CheckNotNull(columndef, "columndef");
            Helpers.CheckDataSize(defaultValue, defaultValueSize, "defaultValueSize");

            NATIVE_COLUMNDEF nativeColumndef = columndef.GetNativeColumndef();
            int returnCode = Tracing.TraceResult(NativeMethods.JetAddColumn(session.Id, _id.Value,
                column, ref nativeColumndef, defaultValue, checked((uint)defaultValueSize),
                out columnid.Value));
            // esent doesn't actually set the columnid member of the passed in JET_COLUMNDEF, but
            // we will do that here for completeness.
            columndef.columnid = new JET_COLUMNID { Value = columnid.Value };
            EsentExceptionHelper.Check(returnCode);
            return new JetColumn(this, columnid);
        }

        /// <summary>Close an open table.</summary>
        public void Close()
        {
            Tracing.TraceFunctionCall("Close");
            int returnCode = NativeMethods.JetCloseTable(_owner.Session.Id, _id.Value);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Walks each index of a table to exactly compute the number
        /// of entries in an index, and the number of distinct keys in an index.
        /// This information, together with the number of database pages allocated
        /// for an index and the current time of the computation is stored in index
        /// metadata in the database. This data can be subsequently retrieved with
        /// information operations.</summary>
        /// <param name="session">Session to use.</param>
        public void ComputeStatistics(IJetSession session)
        {
            Tracing.TraceFunctionCall("ComputeStatistics");
            int returnCode = NativeMethods.JetComputeStats(session.Id, _id.Value);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Creates an index over data in an ESE database. An index can be used to
        /// locate specific data quickly.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="indexName">Pointer to a null-terminated string that specifies the
        /// name of the index to create. </param>
        /// <param name="grbit">Index creation options.</param>
        /// <param name="keyDescription">Pointer to a double null-terminated string of null
        /// delimited tokens.</param>
        /// <param name="keyDescriptionLength">The length, in characters, of szKey including
        /// the two terminating nulls.</param>
        /// <param name="density">Initial B+ tree density.</param>
        /// <seealso cref="IJetTable.CreateIndex(IJetSession, JET_INDEXCREATE[], int)"/>
        public void CreateIndex(IJetSession session, string indexName, CreateIndexGrbit grbit,
            string keyDescription, int keyDescriptionLength, int density)
        {
            Tracing.TraceFunctionCall("CreateIndex");
            Helpers.CheckNotNull(indexName, "indexName");
            Helpers.CheckNotNegative(keyDescriptionLength, "keyDescriptionLength");
            Helpers.CheckNotNegative(density, "density");
            if (keyDescriptionLength > checked(keyDescription.Length + 1)) {
                throw new ArgumentOutOfRangeException(
                    "keyDescriptionLength", keyDescriptionLength, "cannot be greater than keyDescription.Length");
            }
            int returnCode = NativeMethods.JetCreateIndex(session.Id, _id.Value, indexName, (uint)grbit,
                keyDescription, checked((uint)keyDescriptionLength), checked((uint)density));
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        // OMITTED : workaround method.
        /// <summary>Creates indexes over data in an ESE database.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="indexcreates">Array of objects describing the indexes to be created.</param>
        /// <param name="numIndexCreates">Number of index description objects.</param>
        /// <returns>An error code.</returns>
        /// <seealso cref="IJetTable.CreateIndex(IJetSession, string, CreateIndexGrbit, string, int, int)"/>
        public void CreateIndex(IJetSession session, JET_INDEXCREATE[] indexcreates, int numIndexCreates)
        {
            Tracing.TraceFunctionCall("CreateIndex");
            JetSession _session = JetInstance.GetSession(session);
            _session.Capabilities.CheckSupportsWindows8Features("JetCreateIndex4");
            Helpers.CheckNotNull(indexcreates, "indexcreates");
            Helpers.CheckNotNegative(numIndexCreates, "numIndexCreates");
            if (numIndexCreates > indexcreates.Length) {
                throw new ArgumentOutOfRangeException(
                    "numIndexCreates", numIndexCreates, "numIndexCreates is larger than the number of indexes passed in");
            }
            // pin the memory
            var handles = new GCHandleCollection();
            try {
                throw new NotImplementedException();
                //NATIVE_INDEXCREATE3[] nativeIndexcreates = GetNativeIndexCreate3s(indexcreates, ref handles);
                //int returnCode = NativeMethods.JetCreateIndex4W(session.Id, _id.Value, nativeIndexcreates,
                //    checked((uint)numIndexCreates));
                //Tracing.TraceResult(returnCode);
                //EsentExceptionHelper.Check(returnCode);
            }
            finally { handles.Dispose(); }
        }

        /// <summary>Deletes a column from a database table.</summary>
        /// <param name="session">Session to use.</param>
        /// <param name="column">The name of the column to be deleted.</param>
        /// <param name="grbit">Optional deletion flag.</param>
        public void DeleteColumn(IJetSession session, string column,
            DeleteColumnGrbit grbit = DeleteColumnGrbit.None)
        {
            Tracing.TraceFunctionCall("DeleteColumn");
            Helpers.CheckNotNull(column, "column");
            int returnCode = (DeleteColumnGrbit.None == grbit)
                ? NativeMethods.JetDeleteColumn(session.Id, _id.Value, column)
                : NativeMethods.JetDeleteColumn2(session.Id, _id.Value, column, (uint)grbit);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return;
        }

        /// <summary>Deletes an index from a database table.</summary>
        /// <param name="session">Session to use.</param>
        /// <param name="index">The name of the index to be deleted.</param>
        public void DeleteIndex(IJetSession session, string index)
        {
            Tracing.TraceFunctionCall("DeleteIndex");
            Helpers.CheckNotNull(index, "index");
            int returnCode = NativeMethods.JetDeleteIndex(session.Id, _id.Value, index);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            Close();
        }

        /// <summary>Enumerate the name and identifier of the columns in this table.</summary>
        /// <returns>An enumerable object.</returns>
        public IJetTemporaryTable<BasicColumnDesriptor> EnumerateColumns()
        {
            Tracing.TraceFunctionCall("EnumerateColumns");
            JET_COLUMNLIST columns = GetTableColumns();
            JetCursor cursor = new JetCursor(_owner.Session, columns.tableid);
            return new JetTemporaryTable<BasicColumnDesriptor>(cursor,
                cursor.Enumerate<BasicColumnDesriptor>(null, // Accept every record.
                    delegate () {
                        return new BasicColumnDesriptor(
                            cursor.RetrieveColumnAsString(columns.columnidcolumnname),
                            new JET_COLUMNID(cursor.RetrieveColumnAsUInt32(columns.columnidcolumnid).Value)); 
                    }));
        }

        /// <summary>Retrieves information about all columns in the table.</summary>
        /// <param name="grbit">Additional options for JetGetTableColumnInfo.</param>
        /// <returns>Filled in with information about the columns in the table.</returns>
        public ICollection<IJetColumn> GetColumns(ColInfoGrbit grbit = ColInfoGrbit.None)
        {
            Tracing.TraceFunctionCall("GetColumns");
            JET_COLUMNLIST columns = GetTableColumns();
            List<IJetColumn> result = new List<IJetColumn>();
            JetCursor cursor = new JetCursor(_owner.Session, columns.tableid);
            using (JetTemporaryTable<IJetColumn> input =
                new JetTemporaryTable<IJetColumn>(cursor,
                    cursor.Enumerate<IJetColumn>(null, // Accept every record.
                        delegate () { return JetColumn.FromColumnList(this, columns, cursor); })))
            {
                result.AddRange(input);
            }
            return result;
        }

        ///// <summary>Returns a <see cref="JET_INDEXLIST"/> structure with information about
        ///// the index.</summary>
        //Default = 0,

        ///// <summary>Returns a <see cref="JET_INDEXLIST"/> structure with information about
        ///// the index.</summary>
        //List = 1,

        ///// <summary>Returns a <see cref="JET_INDEXID"/> identifying the index.</summary>
        //IndexId = 9,

        //// --------- //
        //// WINDOWS 7 //
        //// --------- //
        ///// <summary>Introduced in Windows 7. Returns a <see cref="JET_INDEXCREATE"/> structure
        ///// suitable for use by see cref="Api.JetCreateIndex2".</summary>
        ///// <remarks>Not currently implemented in this layer.</remarks>
        //CreateIndex = 11,

        ///// <summary>Introduced in Windows 7. Returns a JET_INDEXCREATE2 structure (similar to
        ///// <see cref="JET_INDEXCREATE"/> structure, but it contains a <see cref="JET_SPACEHINTS"/>
        ///// member called pSpacehints). This structure is suitable for use by
        ///// see cref="Api.JetCreateIndex2".</summary>
        ///// <remarks>Not currently implemented in this layer.</remarks>
        //CreateIndex2 = 12,

        //// --------- //
        //// WINDOWS 8 //
        //// --------- //
        ///// <summary>Introduced in Windows 8. Returns a JET_INDEXCREATE3 structure. This is
        ///// similar to <see cref="JET_INDEXCREATE"/> structure,  but it contains some additional
        ///// members, and it also uses a locale name in the <see cref="JET_UNICODEINDEX"/> index
        ///// definition, not an LCID. The returned object is suitable for use by
        ///// <see cref="IJetTable.CreateIndex(IJetSession, JET_INDEXCREATE[], int)"/>.</summary>
        ///// <remarks>Not currently implemented in this layer, but provided for completeness to
        ///// match the underlying API layer.</remarks>
        //InfoCreateIndex3 = 13,

        ///// <summary>Introduced in Windows 8. Returns the locale name of the given index.</summary>
        //LocaleName = 14,


        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <returns>An error if the call fails.</returns>
        public uint GetIndexAllocatedSpace(IJetSession session, string indexname)
        {
            Tracing.TraceFunctionCall("GetIndexMaxColumnLength");
            return _GetUInt32IndexInfo(session, indexname, JET_IdxInfo.SpaceAlloc);
        }

        /// <summary>Retrieves information about all indexes on the table.</summary>
        /// <returns>Filled in with information about the columns in the table.</returns>
        public ICollection<IJetIndex> GetIndexes()
        {
            Tracing.TraceFunctionCall("GetIndexes");
            //JET_COLUMNLIST columns = GetTableColumns();

            NATIVE_INDEXLIST nativeIndexlist = new NATIVE_INDEXLIST();
            nativeIndexlist.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_INDEXLIST)));
            // Technically, this should have worked in Vista. But there was a bug, and
            // it was fixed after Windows 7.
            int returnCode = NativeMethods.JetGetTableIndexInfoW(_owner.Session.Id, _id.Value, null,
                ref nativeIndexlist, nativeIndexlist.cbStruct, (uint)JET_IdxInfo.List);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            JET_INDEXLIST indexes = new JET_INDEXLIST().SetFromNativeIndexlist(nativeIndexlist);

            List<IJetIndex> result = new List<IJetIndex>();
            JetCursor inputTable = new JetCursor(_owner.Session, indexes.tableid);
            JetIndex currentIndex = null;
            uint columnsCount = 0;
            using (JetTemporaryTable<IJetIndex> input =
                new JetTemporaryTable<IJetIndex>(inputTable,
                    inputTable.Enumerate<IJetIndex>(null, // Accept every record.
                        // We return null most of the time. When we reach the last column of the
                        // current index, we return it.
                        delegate () {
                            if (null == currentIndex) {
                                // We are on the first column of a new index. This is time to create
                                // the index.
                                currentIndex = JetIndex.FromIndexList(this, indexes, inputTable,
                                    out columnsCount);
                            }
                            JetIndex.Column.FromIndexList(currentIndex, indexes, inputTable);
                            if (currentIndex.Columns.Count == columnsCount) {
                                try { return currentIndex; }
                                // Reset in preparation of next index.
                                finally {
                                    currentIndex = null;
                                    columnsCount = 0;
                                }
                            }
                            return null;
                        })))
            {
                foreach(IJetIndex index in input) {
                    if (null != index) { result.Add(index); }
                }
            }
            return result;
        }

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <returns>An error if the call fails.</returns>
        public uint GetIndexesCount(IJetSession session, string indexname)
        {
            Tracing.TraceFunctionCall("GetIndexMaxColumnLength");
            return _GetUInt32IndexInfo(session, indexname, JET_IdxInfo.Count);
        }

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <returns>An error if the call fails.</returns>
        public uint GetIndexLocaleID(IJetSession session, string indexname)
        {
            Tracing.TraceFunctionCall("GetIndexMaxColumnLength");
            return _GetUInt32IndexInfo(session, indexname, JET_IdxInfo.LCID);
        }

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <returns>An error if the call fails.</returns>
        public ushort GetIndexMaxColumnLength(IJetSession session, string indexname)
        {
            Tracing.TraceFunctionCall("GetIndexMaxColumnLength");
            return _GetUInt16IndexInfo(session, indexname, JET_IdxInfo.VarSegMac);
        }

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <returns>An error if the call fails.</returns>
        public ushort GetIndexMaxKeyLength(IJetSession session, string indexname)
        {
            Tracing.TraceFunctionCall("GetIndexMaxKeyLength");
            return _GetUInt16IndexInfo(session, indexname, JET_IdxInfo.KeyMost);
        }

        private ushort _GetUInt16IndexInfo(IJetSession session, string indexname, JET_IdxInfo info)
        {
            ushort result;
            int returnCode = NativeMethods.JetGetTableIndexInfoW(session.Id, _id.Value, indexname,
                out result, sizeof(ushort), (uint)info);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return result;
        }

        private uint _GetUInt32IndexInfo(IJetSession session, string indexname, JET_IdxInfo info)
        {
            uint result;
            int returnCode = NativeMethods.JetGetTableIndexInfoW(session.Id, _id.Value, indexname,
                out result, sizeof(uint), (uint)info);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return result;
        }

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        public int GetIndexInfo(IJetSession session, string indexname, out JET_INDEXID result,
            JET_IdxInfo infoLevel)
        {
            Tracing.TraceFunctionCall("GetIndexInfo");
            int err = NativeMethods.JetGetTableIndexInfoW(session.Id, _id.Value, indexname,
                out result, JET_INDEXID.SizeOfIndexId, (uint)infoLevel);
            Tracing.TraceResult(err);
            EsentExceptionHelper.Check(err);
            return err;
        }

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        public int GetIndexInfo(IJetSession session, string indexname, out JET_INDEXLIST result,
            JET_IdxInfo infoLevel)
        {
            Tracing.TraceFunctionCall("GetIndexInfo");
            var nativeIndexlist = new NATIVE_INDEXLIST();
            nativeIndexlist.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_INDEXLIST)));
            int returnCode = NativeMethods.JetGetTableIndexInfoW(session.Id, _id.Value, indexname,
                ref nativeIndexlist, nativeIndexlist.cbStruct, (uint)infoLevel);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            result = new JET_INDEXLIST();
            result.SetFromNativeIndexlist(nativeIndexlist);
            return returnCode;
        }

        /// <summary>Retrieves information about indexes on a table.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="indexname">The name of the index.</param>
        /// <param name="result">Filled in with information about indexes on the table.</param>
        /// <param name="infoLevel">The type of information to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        public int GetIndexInfo(IJetSession session, string indexname, out string result,
            JET_IdxInfo infoLevel)
        {
            Tracing.TraceFunctionCall("GetIndexInfo");
            // Will need to check for Windows 8 Features.
            // Currently only JET_IdxInfo.LocaleName is supported.
            uint bytesMax = checked((uint)Constants.LocaleNameMaxLength * sizeof(char));
            var stringBuilder = new StringBuilder(Constants.LocaleNameMaxLength);
            int returnCode = NativeMethods.JetGetTableIndexInfoW(session.Id, _id.Value, indexname,
                stringBuilder, bytesMax, (uint)infoLevel);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            result = stringBuilder.ToString();
            result = StringCache.TryToIntern(result);
            return returnCode;
        }

        /// <summary>Retrieve a list of colums for this table. This is used by both
        /// <see cref="EnumerateColumns"/> and <see cref="GetColumns"/></summary>
        /// <returns></returns>
        private JET_COLUMNLIST GetTableColumns()
        {
            var nativeColumnlist = new NATIVE_COLUMNLIST();
            nativeColumnlist.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_COLUMNLIST)));
            // Technically, this should have worked in Vista. But there was a bug, and
            // it was fixed after Windows 7.
            int returnCode = NativeMethods.JetGetTableColumnInfoW(_owner.Session.Id, _id.Value, null,
                ref nativeColumnlist, nativeColumnlist.cbStruct, (uint)JET_ColInfo.List);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return new JET_COLUMNLIST().SetFromNativeColumnlist(nativeColumnlist);
        }

        ///// <summary>Retrieves information about all columns in a table.</summary>
        ///// <param name="sesid">The session to use.</param>
        ///// <param name="dbid">The database that contains the table.</param>
        ///// <param name="tablename">The name of the table containing the column.</param>
        ///// <param name="ignored">This parameter is ignored.</param>
        ///// <param name="columnlist">Filled in with information about the columns in the table.</param>
        ///// <returns>An error if the call fails.</returns>
        //public int JetGetColumnInfo(JET_SESID sesid, JET_DBID dbid, string tablename,
        //    string ignored, out JET_COLUMNLIST columnlist)
        //{
        //    int err;
        //    var nativeColumnlist = new NATIVE_COLUMNLIST();
        //    nativeColumnlist.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_COLUMNLIST)));

        //    // Technically, this should have worked in Vista. But there was a bug, and
        //    // it was fixed after Windows 7.
        //    if (_capabilities.SupportsWindows8Features)
        //    {
        //        err = Tracing.TraceResult(NativeMethods.JetGetColumnInfoW(sesid.Value, dbid.Value,
        //            tablename, ignored, ref nativeColumnlist, nativeColumnlist.cbStruct,
        //            (uint)JET_ColInfo.List));
        //    }
        //    else
        //    {
        //        err = Tracing.TraceResult(NativeMethods.JetGetColumnInfo(sesid.Value, dbid.Value,
        //            tablename, ignored, ref nativeColumnlist, nativeColumnlist.cbStruct,
        //            (uint)JET_ColInfo.List));
        //    }
        //    columnlist.SetFromNativeColumnlist(nativeColumnlist);
        //    return err;
        //}

        /// <summary>Retrieves information about a table column, given its <see cref="JET_TABLEID"/> and name.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <returns>A olumn definition.</returns>
        public JET_COLUMNBASE GetTableColumn(IJetSession session, string columnName)
        {
            uint nativeSize = checked((uint)Marshal.SizeOf(typeof(NATIVE_COLUMNBASE_WIDE)));
            object nativeColumnbase = new NATIVE_COLUMNBASE_WIDE() { cbStruct = nativeSize };
            IntPtr searchKey = Marshal.StringToHGlobalUni(columnName);
            try {
                return new JET_COLUMNBASE((NATIVE_COLUMNBASE_WIDE)_GetTableColumn(session, searchKey,
                    ref nativeColumnbase, (int)nativeSize, JET_ColInfo.Base));
            }
            finally { Marshal.FreeHGlobal(searchKey); }
        }

        /// <summary>Retrieves information about a table column, given its <see cref="JET_TABLEID"/> and name.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="searchKey">The native search argument.</param>
        /// <param name="nativeArgument">The native argument.</param>
        /// <param name="nativeArgumentSize">Native argument size.</param>
        /// <param name="flags">Query flags.</param>
        /// <returns>An error if the call fails.</returns>
        private object _GetTableColumn(IJetSession session, IntPtr searchKey,
            ref object nativeArgument, int nativeArgumentSize, JET_ColInfo flags)
        {
            Helpers.CheckNotNull(nativeArgument, "nativeArgument");
            int returnCode = NativeMethods.JetGetTableColumnInfoW(session.Id, _id.Value,
                searchKey, ref nativeArgument, nativeArgumentSize, (uint)flags);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return nativeArgument;
        }

        /// <summary>Retrieves information about all columns in the table.</summary>
        /// <param name="grbit">Additional options for JetGetTableColumnInfo.</param>
        /// <returns>Filled in with information about the columns in the table.</returns>
        public ICollection<IJetColumn> GetTableColumns(ColInfoGrbit grbit = ColInfoGrbit.None)
        {
            Tracing.TraceFunctionCall("GetTableColumns");
            JET_COLUMNLIST columnlist = new JET_COLUMNLIST();
            NATIVE_COLUMNLIST nativeColumnlist = new NATIVE_COLUMNLIST();
            nativeColumnlist.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_COLUMNLIST)));
            // Technically, this should have worked in Vista. But there was a bug, and
            // it was fixed after Windows 7.
            int returnCode = NativeMethods.JetGetTableColumnInfoW(_owner.Session.Id, this._id.Value,
                null, ref nativeColumnlist, nativeColumnlist.cbStruct,
                (uint)grbit | (uint)JET_ColInfo.List);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            List<IJetColumn> result = new List<IJetColumn>();
            columnlist.SetFromNativeColumnlist(nativeColumnlist);
            return result;
        }

        /// <summary>Explicitly reserve the ability to update a row, write lock, or to explicitly
        /// prevent a row from being updated by any other session, read lock. Normally, row write
        /// locks are acquired implicitly as a result of updating rows. Read locks are usually not
        /// required because of record versioning. However, in some cases a transaction may desire
        /// to explicitly lock a row to enforce serialization, or to ensure that a subsequent
        /// operation will succeed.</summary>
        /// <param name="session">The session to use.</param>
        /// <param name="readLock">Acquire a read lock on the current record. Read locks are
        /// incompatible with write locks already held by other sessions but are compatible with
        /// read locks held by other sessions.</param>
        /// <param name="writeLock">Acquire a write lock on the current record. Write locks are not
        /// compatible with write or read locks held by other sessions but are compatible with read
        /// locks held by the same session.</param>
        public void GetLock(IJetSession session, bool readLock, bool writeLock)
        {
            Tracing.TraceFunctionCall("GetLock");
            JET_err result = _TryGetLock(session, readLock, writeLock);
            EsentExceptionHelper.Check((int)result);
            return;
        }

        /// <summary>Make native conditionalcolumn structures from the managed ones.</summary>
        /// <param name="conditionalColumns">The conditional columns to convert.</param>
        /// <param name="useUnicodeData">Wehether to convert the strings with UTF-16.</param>
        /// <param name="handles">The handle collection used to pin the data.</param>
        /// <returns>Pinned native versions of the conditional columns.</returns>
        private static IntPtr GetNativeConditionalColumns(IList<JET_CONDITIONALCOLUMN> conditionalColumns,
            bool useUnicodeData, ref GCHandleCollection handles)
        {
            if (null == conditionalColumns) { return IntPtr.Zero; }
            NATIVE_CONDITIONALCOLUMN[] nativeConditionalcolumns =
                new NATIVE_CONDITIONALCOLUMN[conditionalColumns.Count];
            for (int i = 0; i < conditionalColumns.Count; ++i) {
                nativeConditionalcolumns[i] = conditionalColumns[i].GetNativeConditionalColumn();
                nativeConditionalcolumns[i].szColumnName = (useUnicodeData)
                    ?  handles.Add(Util.ConvertToNullTerminatedUnicodeByteArray(conditionalColumns[i].szColumnName))
                    : handles.Add(Util.ConvertToNullTerminatedAsciiByteArray(conditionalColumns[i].szColumnName));
            }
            return handles.Add(nativeConditionalcolumns);
        }

        /// <summary>Determine whether an update of the current record of a cursor will result
        /// in a write conflict, based on the current update status of the record. It is possible
        /// that a write conflict will ultimately be returned even if IsWriteConflictExpected
        /// returns successfully. because another session may update the record before the current
        /// session is able to update the same record.</summary>
        /// <param name="session">The session to use.</param>
        /// <returns>An error if the call fails.</returns>
        public bool IsWriteConflictExpected(IJetSession session)
        {
            Tracing.TraceFunctionCall("IsWriteConflictExpected");
            int returnCode = NativeMethods.JetGetCursorInfo(session.Id, _id.Value, IntPtr.Zero, 0, 0);
            Tracing.TraceResult(returnCode);
            switch ((JET_err)returnCode) {
                case JET_err.Success:
                    return false;
                case JET_err.WriteConflict:
                    return true;
                default:
                    EsentExceptionHelper.Check(returnCode);
                    // Should never be reached. Exception thrown above.
                    return true;
            }
        }

        // OMITTED : workaround method.
        ///// <summary>Make native indexcreate structures from the managed ones.</summary>
        ///// <param name="managedIndexCreates">Index create structures to convert.</param>
        ///// <param name="handles">The handle collection used to pin the data.</param>
        ///// <returns>Pinned native versions of the index creates.</returns>
        //private static unsafe NATIVE_INDEXCREATE3[] GetNativeIndexCreate3s(
        //    IList<JET_INDEXCREATE> managedIndexCreates, ref GCHandleCollection handles)
        //{
        //    if ((null == managedIndexCreates) || (0 >= managedIndexCreates.Count)) {
        //        return null;
        //    }
        //    NATIVE_INDEXCREATE3[] result = new NATIVE_INDEXCREATE3[managedIndexCreates.Count];
        //    for (int index = 0; index < managedIndexCreates.Count; ++index) {
        //        result[index] = managedIndexCreates[index].GetNativeIndexcreate3();
        //        if (null != managedIndexCreates[index].pidxUnicode) {
        //            NATIVE_UNICODEINDEX2 unicode = managedIndexCreates[index].pidxUnicode.GetNativeUnicodeIndex2();
        //            unicode.szLocaleName = handles.Add(Util.ConvertToNullTerminatedUnicodeByteArray(managedIndexCreates[index].pidxUnicode.GetEffectiveLocaleName()));
        //            result[index].pidxUnicode = (NATIVE_UNICODEINDEX2*)handles.Add(unicode);
        //            result[index].grbit |= (uint)CreateIndexGrbit.IndexUnicode;
        //        }
        //        result[index].szKey = handles.Add(Util.ConvertToNullTerminatedUnicodeByteArray(managedIndexCreates[index].szKey));
        //        result[index].szIndexName = handles.Add(Util.ConvertToNullTerminatedUnicodeByteArray(managedIndexCreates[index].szIndexName));
        //        result[index].rgconditionalcolumn = GetNativeConditionalColumns(managedIndexCreates[index].rgconditionalcolumn, true, ref handles);
        //        // Convert pSpaceHints.
        //        if (managedIndexCreates[index].pSpaceHints != null) {
        //            result[index].pSpaceHints = handles.Add(
        //                managedIndexCreates[index].pSpaceHints.GetNativeSpaceHints());
        //        }
        //    }
        //    return result;
        //}

        /// <summary>Prepare a cursor for update.</summary>
        /// <param name="session">The session which is starting the update.</param>
        /// <param name="prep">The type of update to prepare.</param>
        /// <returns>An instance of the ongoing update.</returns>
        public IJetCursor PrepareUpdate(IJetSession session, JET_prep prep)
        {
            Tracing.TraceFunctionCall("PrepareUpdate");
            JET_TABLEID cursorId = new JET_TABLEID();
            int returnCode = NativeMethods.JetPrepareUpdate(session.Id, cursorId.Value,
                unchecked((uint)prep));
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return new JetCursor(session, cursorId, prep);
        }

        /// <summary>Explicitly reserve the ability to update a row, write lock, or to explicitly
        /// prevent a row from being updated by any other session, read lock. Normally, row write
        /// locks are acquired implicitly as a result of updating rows. Read locks are usually not
        /// required because of record versioning. However, in some cases a transaction may desire
        /// to explicitly lock a row to enforce serialization, or to ensure that a subsequent
        /// operation will succeed. </summary>
        /// <param name="session">The session to use.</param>
        /// <param name="readLock">Acquire a read lock on the current record. Read locks are
        /// incompatible with write locks already held by other sessions but are compatible with
        /// read locks held by other sessions.</param>
        /// <param name="writeLock">Acquire a write lock on the current record. Write locks are not
        /// compatible with write or read locks held by other sessions but are compatible with read
        /// locks held by the same session.</param>
        /// <returns>True if the lock was obtained, false otherwise. An exception is thrown if an
        /// unexpected error is encountered.</returns>
        public bool TryGetLock(IJetSession session, bool readLock, bool writeLock)
        {
            Tracing.TraceFunctionCall("TryGetLock");
            JET_err err = _TryGetLock(session, readLock, writeLock);
            if (JET_err.WriteConflict == err) { return false;   }
            Debug.Assert(err >= JET_err.Success, "Exception should have been thrown in case of error");
            return true;
        }

        private JET_err _TryGetLock(IJetSession session, bool readLock, bool writeLock)
        {
            GetLockGrbit flags = 0;
            if (readLock) { flags &= GetLockGrbit.Read; }
            if (writeLock) { flags &= GetLockGrbit.Write; }
            if (0 == flags) {
                throw new InvalidOperationException("At least of of readLok or writeLock must be true.");
            }
            int result = NativeMethods.JetGetLock(session.Id, _id.Value, (uint)flags);
            Tracing.TraceResult(result);
            return (JET_err)result;
        }

        private JET_TABLEID _id;
        private JetDatabase _owner;

        /// <summary>Options for JetGetLock.</summary>
        [Flags]
        private enum GetLockGrbit : uint
        {
            /// <summary>Acquire a read lock on the current record. Read locks are incompatible
            /// with write locks already held by other sessions but are compatible with read locks
            /// held by other sessions.</summary>
            Read = 0x1,
            /// <summary>Acquire a write lock on the current record. Write locks are not compatible
            /// with write or read locks held by other sessions but are compatible with read locks
            /// held by the same session.</summary>
            Write = 0x2,
        }
    }
}
