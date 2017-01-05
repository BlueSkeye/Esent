using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

using EsentLib.Api;
using EsentLib.Jet;
using EsentLib.Jet.Types;

namespace EsentLib.Implementation
{
    internal class JetTable : IJetTable
    {
        internal JetTable(JetDatabase owner, JET_TABLEID id)
        {
            _owner = owner;
            _id = id;
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

        /// <summary>Create the nullable return value.</summary>
        /// <typeparam name="T">The (struct) type to return.</typeparam>
        /// <param name="data">The data retrieved from the column.</param>
        /// <param name="dataSize">The size of the data.</param>
        /// <param name="wrn">The warning code from esent.</param>
        /// <param name="actualDataSize">The actual size of the data retireved fomr esent.</param>
        /// <returns>A nullable struct of type T.</returns>
        private static T? CreateReturnValue<T>(T data, int dataSize, JET_wrn wrn, int actualDataSize)
            where T : struct
        {
            if (JET_wrn.ColumnNull == wrn) { return new T?(); }
            Helpers.CheckDataSize(dataSize, actualDataSize);
            return data;
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

        /// <summary>A delegate that will filter out unwanted records.</summary>
        /// <returns></returns>
        internal delegate bool FilterDelegate();
        internal delegate T ItemRetriever<T>();

        /// <summary></summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="skipRecordFilter">This delegate, if not null, will be invoked on each
        /// record. It has to return true if the current record should be skipped and not be
        /// returned by the enumerator.</param>
        /// <param name="retriever"></param>
        /// <returns></returns>
        internal IEnumerable<T> Enumerate<T>(FilterDelegate skipRecordFilter, ItemRetriever<T> retriever)
        {
            if (!TryMoveFirst()) { yield break; }
            do {
                if ((null == skipRecordFilter) || !skipRecordFilter()) {
                    yield return retriever();
                }
            } while (TryMoveNext());
            yield break;
        }

        /// <summary>Retrieve a list of colums for this table.</summary>
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
            JET_COLUMNLIST result = new JET_COLUMNLIST();
            result.SetFromNativeColumnlist(nativeColumnlist);
            return result;
        }

        /// <summary>Enumerate the name of the columns in this table.</summary>
        /// <returns>An enumerable object.</returns>
        public IJetTemporaryTable<string> EnumerateColumnNames()
        {
            Tracing.TraceFunctionCall("EnumerateColumnNames");
            JET_COLUMNLIST columns = GetTableColumns();
            JetTable resultTable = new JetTable(_owner, columns.tableid);
            return new JetTemporaryTable<string>(resultTable,
                resultTable.Enumerate<string>((JetTable.FilterDelegate)null, // Accept every record.
                    delegate () {
                        string name = resultTable.RetrieveColumnAsString(columns.columnidcolumnname);
                        return StringCache.TryToIntern(name);
                    }));
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
        /// <param name="session">The session to use.</param>
        /// <param name="grbit">Additional options for JetGetTableColumnInfo.</param>
        /// <returns>Filled in with information about the columns in the table.</returns>
        public JET_COLUMNLIST GetTableColumns(IJetSession session, ColInfoGrbit grbit = ColInfoGrbit.None)
        {
            Tracing.TraceFunctionCall("GetTableColumns");
            JET_COLUMNLIST columnlist = new JET_COLUMNLIST();
            NATIVE_COLUMNLIST nativeColumnlist = new NATIVE_COLUMNLIST();
            nativeColumnlist.cbStruct = checked((uint)Marshal.SizeOf(typeof(NATIVE_COLUMNLIST)));
            // Technically, this should have worked in Vista. But there was a bug, and
            // it was fixed after Windows 7.
            int returnCode = NativeMethods.JetGetTableColumnInfoW(session.Id, this._id.Value,
                null, ref nativeColumnlist, nativeColumnlist.cbStruct,
                (uint)grbit | (uint)JET_ColInfo.List);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            columnlist.SetFromNativeColumnlist(nativeColumnlist);
            return columnlist;
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

        /// <summary>Constructs search keys that may then be used by JetSeek and JetSetIndexRange.
        /// </summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(byte[] data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            if (null == data) { MakeKey(null, 0, grbit); }
            else if (0 == data.Length) {
                MakeKey(data, data.Length, grbit | MakeKeyGrbit.KeyDataZeroLength);
            }
            else { MakeKey(data, data.Length, grbit); }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange.
        /// </summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(string data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            MakeKey(data, Encoding.Unicode, grbit);
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange.
        /// </summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="encoding">The encoding used to convert the string.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(string data, Encoding encoding, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            Helpers.CheckEncodingIsValid(encoding);
            if (null == data) { MakeKey(null, 0, grbit); }
            else if (0 == data.Length) {
                MakeKey(null, 0, grbit | MakeKeyGrbit.KeyDataZeroLength);
            }
            else if (Encoding.Unicode == encoding)
            {
                // Optimization for Unicode strings
                unsafe {
                    fixed (char* buffer = data) {
                        MakeKey(new IntPtr(buffer), checked(data.Length * sizeof(char)), grbit);
                    }
                }
            }
            else {
                // Convert the string using a cached column buffer. The column buffer is far larger
                // than the maximum key size, so any data truncation here won't matter.
                byte[] buffer = null;
                try {
                    buffer = MemoryCache.ColumnCache.Allocate();
                    int dataSize;
                    unsafe {
                        fixed (char* chars = data)
                        fixed (byte* bytes = buffer) {
                            dataSize = encoding.GetBytes(chars, data.Length, bytes, buffer.Length);
                        }
                    }
                    MakeKey(buffer, dataSize, grbit);
                }
                finally { if (buffer != null) { MemoryCache.ColumnCache.Free(ref buffer); } }
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange.
        /// </summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(bool data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            byte b = data ? (byte)0xff : (byte)0x0;
            MakeKey(b, grbit);
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange.</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(byte data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe {
                const int DataSize = sizeof(byte);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange.</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(short data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe {
                const int DataSize = sizeof(short);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(int data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe {
                const int DataSize = sizeof(int);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(long data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe {
                const int DataSize = sizeof(long);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(Guid data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe {
                const int DataSize = 16 /* sizeof(Guid) */;
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(DateTime data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            MakeKey(data.ToOADate(), grbit);
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(float data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe {
                const int DataSize = sizeof(float);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(double data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe {
                const int DataSize = sizeof(double);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        // [CLSCompliant(false)]
        public void MakeKey(ushort data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe {
                const int DataSize = sizeof(ushort);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        // [CLSCompliant(false)]
        public void MakeKey(uint data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe {
                const int DataSize = sizeof(uint);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs a search key that may then be used by JetSeek and JetSetIndexRange</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        // [CLSCompliant(false)]
        public void MakeKey(ulong data, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            unsafe {
                const int DataSize = sizeof(ulong);
                var pointer = new IntPtr(&data);
                MakeKey(pointer, DataSize, grbit);
            }
        }

        /// <summary>Constructs search keys that may then be used by JetSeek and JetSetIndexRange.</summary>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="dataSize">Size of the data.</param>
        /// <param name="grbit">Key options.</param>
        public void MakeKey(byte[] data, int dataSize, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            if (((null == data) && (0 != dataSize))
                || ((null != data) && (dataSize > data.Length)))
            {
                throw new ArgumentOutOfRangeException(string.Format(
                    "dataSize {0} cannot be greater than the length of the data", dataSize));
            }
            unsafe { fixed (byte* pointer = data) { MakeKey(data, dataSize, grbit); } }
        }

        /// <summary>Constructs search keys that may then be used by JetSeek and JetSetIndexRange.</summary>
        /// <param name="pointer">Column data for the current key column of the current index.</param>
        /// <param name="dataSize">Size of the data.</param>
        /// <param name="grbit">Key options.</param>
        private void MakeKey(IntPtr pointer, int dataSize, MakeKeyGrbit grbit = MakeKeyGrbit.None)
        {
            Tracing.TraceFunctionCall("MakeKey");
            Helpers.CheckNotNegative(dataSize, "dataSize");
            int returnCode = NativeMethods.JetMakeKey(_owner.Session.Id, this._id.Value,
                pointer, checked((uint)dataSize), unchecked((uint)grbit));
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Navigate through an index. The cursor can be positioned at the start or
        /// end of the index and moved backwards and forwards by a specified number of index
        /// entries.</summary>
        /// <param name="sesid">The session to use for the call.</param>
        /// <param name="numRows">An offset which indicates how far to move the cursor.</param>
        /// <param name="grbit">Move options.</param>
        /// <returns>An error if the call fails.</returns>
        public int Move(JET_SESID sesid, int numRows, MoveGrbit grbit = MoveGrbit.None)
        {
            Tracing.TraceFunctionCall("Move");
            int returnCode = NativeMethods.JetMove(sesid.Value, this._id.Value, numRows,
                unchecked((uint)grbit));
            return Tracing.TraceResult(returnCode);
        }

        /// <summary>Prepare a cursor for update.</summary>
        /// <param name="session">The session which is starting the update.</param>
        /// <param name="prep">The type of update to prepare.</param>
        /// <returns>An instance of the ongoing update.</returns>
        public ICursor PrepareUpdate(IJetSession session, JET_prep prep)
        {
            Tracing.TraceFunctionCall("PrepareUpdate");
            int returnCode = NativeMethods.JetPrepareUpdate(session.Id, this._id.Value,
                unchecked((uint)prep));
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return new JetCursor(session, this, prep);
        }

        /// <summary>Retrieves a single column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.
        /// Alternatively, this function can retrieve a column from a record being created in
        /// the cursor copy buffer. This function can also retrieve column data from an index
        /// entry that references the current record. In addition to retrieving the actual
        /// column value, JetRetrieveColumn can also be used to retrieve the size of a column,
        /// before retrieving the column data itself so that application buffers can be sized
        /// appropriately.</summary>
        /// <remarks>The RetrieveColumnAs functions provide datatype-specific retrieval
        /// functions.</remarks>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="data">The data buffer to be retrieved into.</param>
        /// <param name="dataSize">The size of the data buffer.</param>
        /// <param name="actualDataSize">Returns the actual size of the data buffer.</param>
        /// <param name="grbit">Retrieve column options.</param>
        /// <param name="retinfo">If pretinfo is give as NULL then the function behaves as
        /// though an itagSequence of 1 and an ibLongValue of 0 (zero) were given. This causes
        /// column retrieval to retrieve the first value of a multi-valued column, and to
        /// retrieve long data at offset 0 (zero).</param>
        /// <returns>An error or warning.</returns>
        public JET_wrn RetrieveColumn(JET_COLUMNID columnid, IntPtr data, int dataSize,
            out int actualDataSize, RetrieveColumnGrbit grbit, JET_RETINFO retinfo)
        {
            Tracing.TraceFunctionCall("RetrieveColumn");
            Helpers.CheckNotNegative(dataSize, "dataSize");
            int returnCode;
            uint bytesActual = 0;
            if (null != retinfo) {
                NATIVE_RETINFO nativeRetinfo = retinfo.GetNativeRetinfo();
                returnCode = NativeMethods.JetRetrieveColumn(_owner.Session.Id, this._id.Value,
                    columnid.Value, data, checked((uint)dataSize), out bytesActual,
                    unchecked((uint)grbit), ref nativeRetinfo);
                retinfo.SetFromNativeRetinfo(nativeRetinfo);
            }
            else {
                returnCode = NativeMethods.JetRetrieveColumn(_owner.Session.Id, this._id.Value,
                    columnid.Value, data, checked((uint)dataSize), out bytesActual,
                    unchecked((uint)grbit), IntPtr.Zero);
            }
            Tracing.TraceResult(returnCode);
            actualDataSize = checked((int)bytesActual);
            return EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Retrieves a single column value from the current record. The record
        /// is that record associated with the index entry at the current position of the
        /// cursor. Alternatively, this function can retrieve a column from a record being
        /// created in the cursor copy buffer. This function can also retrieve column data
        /// from an index entry that references the current record. In addition to retrieving
        /// the actual column value, JetRetrieveColumn can also be used to retrieve the size
        /// of a column, before retrieving the column data itself so that application buffers
        /// can be sized appropriately.</summary>
        /// <remarks>This is an internal method that takes a buffer offset as well as size.</remarks>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="data">The data buffer to be retrieved into.</param>
        /// <param name="dataOffset">Offset into the data buffer to read data into.</param>
        /// <param name="actualDataSize">Returns the actual size of the data buffer.</param>
        /// <param name="grbit">Retrieve column options.</param>
        /// <param name="retinfo">If pretinfo is give as NULL then the function behaves
        /// as though an itagSequence of 1 and an ibLongValue of 0 (zero) were given.
        /// This causes column retrieval to retrieve the first value of a multi-valued
        /// column, and to retrieve long data at offset 0 (zero).</param>
        /// <returns>An ESENT warning code.</returns>
        public JET_wrn RetrieveColumn(JET_COLUMNID columnid, byte[] data, int dataOffset,
            out int actualDataSize, RetrieveColumnGrbit grbit, JET_RETINFO retinfo)
        {
            return RetrieveColumn(columnid, data, 0, dataOffset, out actualDataSize,
                grbit, retinfo);
        }

        /// <summary>Retrieves a single column value from the current record. The record
        /// is that record associated with the index entry at the current position of the
        /// cursor. Alternatively, this function can retrieve a column from a record being
        /// created in the cursor copy buffer. This function can also retrieve column data
        /// from an index entry that references the current record. In addition to retrieving
        /// the actual column value, JetRetrieveColumn can also be used to retrieve the size
        /// of a column, before retrieving the column data itself so that application buffers
        /// can be sized appropriately.</summary>
        /// <remarks>This is an internal method that takes a buffer offset as well as size.</remarks>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="data">The data buffer to be retrieved into.</param>
        /// <param name="dataSize">The size of the data buffer.</param>
        /// <param name="dataOffset">Offset into the data buffer to read data into.</param>
        /// <param name="actualDataSize">Returns the actual size of the data buffer.</param>
        /// <param name="grbit">Retrieve column options.</param>
        /// <param name="retinfo">If pretinfo is give as NULL then the function behaves
        /// as though an itagSequence of 1 and an ibLongValue of 0 (zero) were given.
        /// This causes column retrieval to retrieve the first value of a multi-valued
        /// column, and to retrieve long data at offset 0 (zero).</param>
        /// <returns>An ESENT warning code.</returns>
        public JET_wrn RetrieveColumn(JET_COLUMNID columnid, byte[] data, int dataSize,
            int dataOffset, out int actualDataSize, RetrieveColumnGrbit grbit,
            JET_RETINFO retinfo)
        {
            if ((0 > dataOffset)
                || ((null != data) && (0 != dataSize) && (dataOffset >= data.Length))
                || ((null == data) && (0 != dataOffset)))
            {
                throw new ArgumentOutOfRangeException(
                    string.Format("dataOffset {0} must be inside the data buffer", dataOffset));
            }
            if ((null == data && dataSize > 0) || (null != data && dataSize > data.Length))
            {
                throw new ArgumentOutOfRangeException(
                    string.Format("dataSize {0} cannot be greater than the length of the data", dataSize));
            }
            unsafe {
                fixed (byte* pointer = data) {
                    return RetrieveColumn(columnid, new IntPtr(pointer + dataOffset),
                        dataSize, out actualDataSize, grbit, retinfo);
                }
            }
        }

        /// <summary>Retrieves a single column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.
        /// Alternatively, this function can retrieve a column from a record being created in
        /// the cursor copy buffer. This function can also retrieve column data from an index
        /// entry that references the current record. In addition to retrieving the 
        /// column value, JetRetrieveColumn can also be used to retrieve the size of a column,
        /// before retrieving the column data itself so that application buffers can be sized
        /// appropriately.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieve column options.</param>
        /// <param name="retinfo">If pretinfo is give as NULL then the function behaves as
        /// though an itagSequence of 1 and an ibLongValue of 0 (zero) were given. This causes
        /// column retrieval to retrieve the first value of a multi-valued column, and to
        /// retrieve long data at offset 0 (zero).</param>
        /// <returns>The data retrieved from the column. Null if the column is null.</returns>
        public byte[] RetrieveColumn(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None, JET_RETINFO retinfo = null)
        {
            // We expect most column values retrieved this way to be small (retrieving a 1GB LV as one
            // chunk is a bit extreme!). Allocate a cached buffer and use that, allocating a larger one
            // if needed.
            byte[] cache = null;
            byte[] data;

            try {
                cache = MemoryCache.ColumnCache.Allocate();
                data = cache;
                int dataSize;
                //JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid,
                // byte[] data, int dataSize, int dataOffset, out int actualDataSize,
                // RetrieveColumnGrbit grbit, JET_RETINFO retinfo

                JET_wrn wrn = RetrieveColumn(columnid, data, data.Length, out dataSize,
                    grbit, retinfo);
                if (JET_wrn.ColumnNull == wrn) {
                    // null column
                    data = null;
                }
                else if (JET_wrn.Success == wrn) {
                    data = MemoryCache.Duplicate(data, dataSize);
                }
                else {
                    // there is more data to retrieve
                    Debug.Assert(JET_wrn.BufferTruncated == wrn, "Unexpected warning: " + wrn.ToString());
                    data = new byte[dataSize];
                    wrn = RetrieveColumn(columnid, data, data.Length, out dataSize, grbit, retinfo);
                    if (JET_wrn.BufferTruncated == wrn) {
                        string error = string.Format(CultureInfo.CurrentCulture,
                            "Column size changed from {0} to {1}. The record was probably updated by another thread.",
                            data.Length, dataSize);
                        Trace.TraceError(error);
                        throw new InvalidOperationException(error);
                    }
                }
            }
            finally { if (cache != null) { MemoryCache.ColumnCache.Free(ref cache); } }
            return data;
        }

        /// <summary>Retrieves a boolean column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a boolean. Null if the column is null.</returns>
        public bool? RetrieveColumnAsBoolean(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            byte? b = RetrieveColumnAsByte(columnid, grbit);
            return (b.HasValue) ? (0 != b.Value) :new bool?();
        }

        /// <summary>Retrieves a byte column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a byte. Null if the column is null.</returns>
        public byte? RetrieveColumnAsByte(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) {  // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(byte);
                byte data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid, pointer, DataSize, out actualDataSize,
                    grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves a datetime column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a datetime. Null if the column is null.</returns>
        public DateTime? RetrieveColumnAsDateTime(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // Internally DateTime is stored in OLE Automation format
            double? oadate = RetrieveColumnAsDouble(columnid, grbit);
            return (oadate.HasValue)
                ? Conversions.ConvertDoubleToDateTime(oadate.Value)
                : new DateTime?();
        }

        /// <summary>Retrieves a double column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a double. Null if the column is null.</returns>
        public double? RetrieveColumnAsDouble(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) {  // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(double);
                double data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid, pointer, DataSize, out actualDataSize,
                    grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves a float column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a float. Null if the column is null.</returns>
        public float? RetrieveColumnAsFloat(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) {  // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(float);
                float data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid, pointer, DataSize,
                    out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves an int16 column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a short. Null if the column is null.</returns>
        public short? RetrieveColumnAsInt16(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) {  // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(short);
                short data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid, pointer, DataSize,
                    out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves a guid column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a guid. Null if the column is null.</returns>
        public Guid? RetrieveColumnAsGuid(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) {  // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = 16;
                Guid data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid, pointer, DataSize,
                    out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves an int32 column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an int. Null if the column is null.</returns>
        public int? RetrieveColumnAsInt32(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) {  // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(int);
                int data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid, pointer, DataSize,
                    out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves a single column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a long. Null if the column is null.</returns>
        public long? RetrieveColumnAsInt64(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) { // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(long);
                long data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid, pointer, DataSize,
                    out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves a string column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a string. Null if the column is null.</returns>
        public unsafe string RetrieveColumnAsString(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            Debug.Assert((grbit & (RetrieveColumnGrbit)0x00020000) == 0,
                "UnpublishedGrbits.RetrieveAsRefIfNotInRecord is not supported.");
            // 512 Unicode characters (1kb on stack)
            const int BufferSize = 512;
            char* buffer = stackalloc char[BufferSize];
            int actualDataSize;
            JET_wrn wrn = RetrieveColumn(columnid, new IntPtr(buffer), BufferSize * sizeof(char),
                out actualDataSize, grbit, null);
            if (JET_wrn.ColumnNull == wrn) { return null; }
            if (JET_wrn.Success == wrn) {
                ////return StringCache.GetString(buffer, 0, actualDataSize);
                return new string(buffer, 0, actualDataSize / sizeof(char));
            }
            Debug.Assert(JET_wrn.BufferTruncated == wrn, "Unexpected warning code");
            // Create a fake string of the appropriate size and then fill it in.
            var s = new string('\0', actualDataSize / sizeof(char));
            fixed (char* p = s) {
                int newDataSize;
                wrn = this.RetrieveColumn(columnid, new IntPtr(p),
                    actualDataSize, out newDataSize, grbit, null);
                if (JET_wrn.BufferTruncated != wrn) { return s; }
                string error = string.Format(CultureInfo.CurrentCulture,
                    "Column size changed from {0} to {1}. The record was probably updated by another thread.",
                    actualDataSize, newDataSize);
                Trace.TraceError(error);
                throw new InvalidOperationException(error);
            }
        }

        /// <summary>Retrieves a uint16 column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an UInt16. Null if the column is null.</returns>
        // [CLSCompliant(false)]
        public ushort? RetrieveColumnAsUInt16(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) { // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(ushort);
                ushort data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid, pointer, DataSize, out actualDataSize,
                    grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves a uint32 column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an UInt32. Null if the column is null.</returns>
        // [CLSCompliant(false)]
        public uint? RetrieveColumnAsUInt32(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) { // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(uint);
                uint data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid, pointer, DataSize, out actualDataSize,
                    grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves a uint64 column value from the current record. The record is
        /// that record associated with the index entry at the current position of the cursor.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an UInt64. Null if the column is null.</returns>
        // [CLSCompliant(false)]
        public ulong? RetrieveColumnAsUInt64(JET_COLUMNID columnid,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) { // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            unsafe {
                const int DataSize = sizeof(ulong);
                ulong data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = RetrieveColumn(columnid, pointer, DataSize, out actualDataSize,
                    grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>Retrieves the size of a single column value from the current record. The
        /// record is that record associated with the index entry at the current position of
        /// the cursor. Alternatively, this function can retrieve a column from a record being
        /// created in the cursor copy buffer. This function can also retrieve column data
        /// from an index entry that references the current record.</summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="itagSequence">The sequence number of value in a multi-valued column.
        /// The array of values is one-based. The first value is sequence 1, not 0. If the
        /// record column has only one value then 1 should be passed as the itagSequence.</param>
        /// <param name="grbit">Retrieve column options.</param>
        /// <returns>The size of the column. 0 if the column is null.</returns>
        public int? RetrieveColumnSize(JET_COLUMNID columnid, int itagSequence,
            RetrieveColumnGrbit grbit = RetrieveColumnGrbit.None)
        {
            // We cannot support this request when there is no way to indicate that a column reference is returned.
            if (0 != (grbit & (RetrieveColumnGrbit)0x00020000)) { // UnpublishedGrbits.RetrieveAsRefIfNotInRecord
                throw new EsentInvalidGrbitException();
            }
            var retinfo = new JET_RETINFO { itagSequence = itagSequence };
            int dataSize;
            JET_wrn wrn = RetrieveColumn(columnid, null, 0, out dataSize,
                grbit, retinfo);
            return (JET_wrn.ColumnNull == wrn) ? (int?)null : dataSize;
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

        /// <summary>Try to navigate through an index. If the navigation succeeds this method
        /// returns true. If there is no record to navigate to this method returns false; an
        /// exception will be thrown for other errors.</summary>
        /// <param name="move">The direction to move in.</param>
        /// <param name="grbit">Move options.</param>
        /// <returns>True if the move was successful.</returns>
        public bool TryMove(JET_Move move, MoveGrbit grbit = MoveGrbit.None)
        {
            JET_err err = (JET_err)Move(_owner.Session.Handle, (int)move, grbit);
            if (JET_err.NoCurrentRecord == err) { return false; }
            EsentExceptionHelper.Check((int)err);
            Debug.Assert((JET_err.Success <= err), "Exception should have been thrown in case of error");
            return true;
        }

        /// <summary>Try to move to the first record in the table. If the table is empty this
        /// returns false, if a different error is encountered an exception is thrown.</summary>
        /// <returns>True if the move was successful.</returns>
        internal bool TryMoveFirst()
        {
            return TryMove(JET_Move.First);
        }

        /// <summary>Try to move to the last record in the table. If the table is empty this
        /// returns false, if a different error is encountered an exception is thrown.</summary>
        /// <returns>True if the move was successful.</returns>
        public bool TryMoveLast()
        {
            return TryMove(JET_Move.Last);
        }

        /// <summary>Try to move to the next record in the table. If there is not a next
        /// record this returns false, if a different error is encountered an exception is
        /// thrown.</summary>
        /// <returns>True if the move was successful.</returns>
        public bool TryMoveNext()
        {
            return TryMove(JET_Move.Next, MoveGrbit.None);
        }

        /// <summary>The JetUpdate function performs an update operation including inserting
        /// a new row into a table or updating an existing row. Deleting a table row is performed
        /// by calling IJetApi.JetDelete</summary>
        /// <param name="session">The session which started the update.</param>
        /// <param name="bookmark">Returns the bookmark of the updated record. This can be null.</param>
        /// <param name="grbit">Update options.</param>
        /// <returns>Returns the actual size of the bookmark.</returns>
        /// <remarks>JetUpdate is the final step in performing an insert or an update. The update
        /// is begun by calling <see cref="IJetTable.PrepareUpdate"/> and then by calling JetSetColumn
        /// one or more times to set the record state. Finally, JetUpdate is called to complete
        /// the update operation. Indexes are updated only by JetUpdate or and not during
        /// JetSetColumn.</remarks>
        public int Update(IJetSession session, byte[] bookmark, UpdateGrbit grbit = UpdateGrbit.None)
        {
            Tracing.TraceFunctionCall("JetUpdate");
            int bookmarkSize = (null == bookmark) ? 0 : bookmark.Length;
            Helpers.CheckDataSize(bookmark, bookmarkSize, "bookmarkSize");
            uint bytesActual;
            int returnCode = (UpdateGrbit.None == grbit)
                ? NativeMethods.JetUpdate(session.Id, this._id.Value, bookmark,
                    checked((uint)bookmarkSize), out bytesActual)
                : NativeMethods.JetUpdate2(session.Id, this._id.Value, bookmark,
                    checked((uint)bookmarkSize), out bytesActual, (uint)grbit);
            EsentExceptionHelper.Check(returnCode);
            return Helpers.GetActualSize(bytesActual);
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
