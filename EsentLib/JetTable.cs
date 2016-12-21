using System;
using System.Collections.Generic;

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
        /// <param name="session">Session to use.</param>
        public void Close(IJetSession session)
        {
            Tracing.TraceFunctionCall("Close");
            int returnCode = NativeMethods.JetCloseTable(session.Id, _id.Value);
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
                NATIVE_INDEXCREATE3[] nativeIndexcreates = GetNativeIndexCreate3s(indexcreates, ref handles);
                int returnCode = NativeMethods.JetCreateIndex4W(session.Id, _id.Value, nativeIndexcreates,
                    checked((uint)numIndexCreates));
                Tracing.TraceResult(returnCode);
                EsentExceptionHelper.Check(returnCode);
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

        /// <summary>
        /// Make native conditionalcolumn structures from the managed ones.
        /// </summary>
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

        /// <summary>Make native indexcreate structures from the managed ones.</summary>
        /// <param name="managedIndexCreates">Index create structures to convert.</param>
        /// <param name="handles">The handle collection used to pin the data.</param>
        /// <returns>Pinned native versions of the index creates.</returns>
        private static unsafe NATIVE_INDEXCREATE3[] GetNativeIndexCreate3s(
            IList<JET_INDEXCREATE> managedIndexCreates, ref GCHandleCollection handles)
        {
            if ((null == managedIndexCreates) || (0 >= managedIndexCreates.Count)) {
                return null;
            }
            NATIVE_INDEXCREATE3[] result = new NATIVE_INDEXCREATE3[managedIndexCreates.Count];
            for (int index = 0; index < managedIndexCreates.Count; ++index) {
                result[index] = managedIndexCreates[index].GetNativeIndexcreate3();
                if (null != managedIndexCreates[index].pidxUnicode) {
                    NATIVE_UNICODEINDEX2 unicode = managedIndexCreates[index].pidxUnicode.GetNativeUnicodeIndex2();
                    unicode.szLocaleName = handles.Add(Util.ConvertToNullTerminatedUnicodeByteArray(managedIndexCreates[index].pidxUnicode.GetEffectiveLocaleName()));
                    result[index].pidxUnicode = (NATIVE_UNICODEINDEX2*)handles.Add(unicode);
                    result[index].grbit |= (uint)CreateIndexGrbit.IndexUnicode;
                }
                result[index].szKey = handles.Add(Util.ConvertToNullTerminatedUnicodeByteArray(managedIndexCreates[index].szKey));
                result[index].szIndexName = handles.Add(Util.ConvertToNullTerminatedUnicodeByteArray(managedIndexCreates[index].szIndexName));
                result[index].rgconditionalcolumn = GetNativeConditionalColumns(managedIndexCreates[index].rgconditionalcolumn, true, ref handles);
                // Convert pSpaceHints.
                if (managedIndexCreates[index].pSpaceHints != null) {
                    result[index].pSpaceHints = handles.Add(
                        managedIndexCreates[index].pSpaceHints.GetNativeSpaceHints());
                }
            }
            return result;
        }

        private JET_TABLEID _id;
        private JetDatabase _owner;
    }
}
