using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EsentLib.Jet;

namespace EsentLib.Implementation
{
    internal class JetTable : IJetTable
    {
        internal JetTable(JetSession session, IJetDatabase owner, JET_TABLEID id)
        {
            _session = session;
            _owner = owner;
            _id = id;
        }

        /// <summary>Add a new column to an existing table.</summary>
        /// <param name="column">The name of the column.</param>
        /// <param name="columndef">The definition of the column.</param>
        /// <param name="defaultValue">The default value of the column.</param>
        /// <param name="defaultValueSize">The size of the default value.</param>
        public IJetColumn AddColumn(string column, JET_COLUMNDEF columndef, byte[] defaultValue, int defaultValueSize)
        {
            Tracing.TraceFunctionCall("JetAddColumn");
            JET_COLUMNID columnid = JET_COLUMNID.Nil;
            Helpers.CheckNotNull(column, "column");
            Helpers.CheckNotNull(columndef, "columndef");
            Helpers.CheckDataSize(defaultValue, defaultValueSize, "defaultValueSize");

            NATIVE_COLUMNDEF nativeColumndef = columndef.GetNativeColumndef();
            int returnCode = Tracing.TraceResult(NativeMethods.JetAddColumn(_session.Id, _id.Value,
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
            int returnCode = NativeMethods.JetCloseTable(_session.Id, _id.Value);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Walks each index of a table to exactly compute the number
        /// of entries in an index, and the number of distinct keys in an index.
        /// This information, together with the number of database pages allocated
        /// for an index and the current time of the computation is stored in index
        /// metadata in the database. This data can be subsequently retrieved with
        /// information operations.</summary>
        public void ComputeStatistics()
        {
            Tracing.TraceFunctionCall("ComputeStatistics");
            int returnCode = NativeMethods.JetComputeStats(_session.Id, _id.Value);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Deletes a column from a database table.</summary>
        /// <param name="column">The name of the column to be deleted.</param>
        /// <param name="grbit">Optional deletion flag.</param>
        public void DeleteColumn(string column, DeleteColumnGrbit grbit = DeleteColumnGrbit.None)
        {
            Tracing.TraceFunctionCall("DeleteColumn");
            Helpers.CheckNotNull(column, "column");
            int returnCode = (DeleteColumnGrbit.None == grbit)
                ? NativeMethods.JetDeleteColumn(_session.Id, _id.Value, column)
                : NativeMethods.JetDeleteColumn2(_session.Id, _id.Value, column, (uint)grbit);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return;
        }

        /// <summary>Deletes an index from a database table.</summary>
        /// <param name="index">The name of the index to be deleted.</param>
        public void DeleteIndex(string index)
        {
            Tracing.TraceFunctionCall("DeleteIndex");
            Helpers.CheckNotNull(index, "index");
            int returnCode = NativeMethods.JetDeleteIndex(_session.Id, _id.Value, index);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        private JET_TABLEID _id;
        private IJetDatabase _owner;
        private JetSession _session;
    }
}
