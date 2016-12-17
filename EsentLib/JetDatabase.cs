using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using EsentLib.Jet;

namespace EsentLib.Implementation
{
    /// <summary></summary>
    public class JetDatabase : IJetDatabase
    {
        internal JetDatabase(JetSession owner, JET_DBID dbid, string name)
        {
            if (null == owner) { throw new ArgumentNullException("owner"); }
            if (string.IsNullOrEmpty(name)) { throw new ArgumentNullException("name"); }
            _dbid = dbid;
            _name = name;
            _owner = owner;
        }

        /// <summary>Closes a database file that was previously opened with
        /// <see cref="IJetSession.OpenDatabase"/> or created with
        /// <see cref="IJetSession.CreateDatabase"/>.</summary>
        /// <param name="grbit">Close options.</param>
        public void Close(CloseDatabaseGrbit grbit)
        {
            Tracing.TraceFunctionCall("Close");
            int returnCode = NativeMethods.JetCloseDatabase(_owner.Id, _dbid.Value, (uint)grbit);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return;
        }

        /// <summary>Makes a copy of an existing database. The copy is compacted to a state
        /// optimal for usage. Data in the copied data will be packed according to the measures
        /// chosen for the indexes at index create. In this way, compacted data may be
        /// stored as densely as possible. Alternatively, compacted data may reserve space
        /// for subsequent record growth or index insertions.</summary>
        /// <param name="destinationDatabase">The name to use for the compacted database.</param>
        /// <param name="statusCallback">A callback function that can be called periodically
        /// through the database compact operation to report progress.</param>
        /// <param name="grbit">Compact options.</param>
        public void CompactDatabase(string destinationDatabase, JET_PFNSTATUS statusCallback,
            CompactGrbit grbit)
        {
            // int returnCode = Impl.JetCompact(sesid, sourceDatabase, destinationDatabase, statusCallback, ignored, grbit);
            Tracing.TraceFunctionCall("CompactDatabase");
            Helpers.CheckNotNull(destinationDatabase, "destinationDatabase");
            StatusCallbackWrapper callbackWrapper = new StatusCallbackWrapper(statusCallback);
            IntPtr functionPointer = (null == statusCallback)
                ? IntPtr.Zero
                : Marshal.GetFunctionPointerForDelegate(callbackWrapper.NativeCallback);
#if DEBUG
            GC.Collect();
#endif
            int returnCode = NativeMethods.JetCompactW(_owner.Id, _name, destinationDatabase,
                functionPointer, IntPtr.Zero, (uint)grbit);
            Tracing.TraceResult(returnCode);
            callbackWrapper.ThrowSavedException();
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Create an empty table. The newly created table is opened exclusively.</summary>
        /// <param name="table">The name of the table to create.</param>
        /// <param name="pages">Initial number of pages in the table.</param>
        /// <param name="density">
        /// The default density of the table. This is used when doing sequential inserts.
        /// </param>
        [CLSCompliant(false)]
        public IJetTable CreateTable(string table, int pages, int density)
        {
            Tracing.TraceFunctionCall("CreateTable");
            Helpers.CheckNotNull(table, "table");
            JET_TABLEID tableid = JET_TABLEID.Nil;
            int returnCode = NativeMethods.JetCreateTable(_owner.Id, _dbid.Value, table,
                pages, density, out tableid.Value);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return new JetTable(this, tableid);
        }

        /// <summary>Deletes a table from a database.</summary>
        /// <param name="table">The name of the table to delete.</param>
        public void DeleteTable(string table)
        {
            Tracing.TraceFunctionCall("DeleteTable");
            Helpers.CheckNotNull(table, "table");

            int returnCode = NativeMethods.JetDeleteTable(_owner.Id, _dbid.Value, table);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Releases a database file that was previously attached to a database
        /// session.</summary>
        /// <param name="grbit">Detach options.</param>
        public void Detach(DetachDatabaseGrbit grbit = 0)
        {
            Tracing.TraceFunctionCall("Detach");
            int returnCode = (0 == grbit)
                ? NativeMethods.JetDetachDatabaseW(_owner.Id, _name)
                : NativeMethods.JetDetachDatabase2W(_owner.Id, _name, (uint)grbit);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Retrieves certain information about the given database.</summary>
        /// <param name="infoLevel">The specific data to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        public JET_DBINFOMISC GetFileInfo(JET_DbInfo infoLevel)
        {
            Tracing.TraceFunctionCall("GetFileInfo");
            int returnCode = (int)JET_err.Success;
            JET_DBINFOMISC result = null;
            if (_owner.Capabilities.SupportsWindows7Features) {
                // Windows7 -> Unicode path support
                NATIVE_DBINFOMISC4 native;
                returnCode = NativeMethods.JetGetDatabaseFileInfoW(_name, out native,
                    (uint)Marshal.SizeOf(typeof(NATIVE_DBINFOMISC4)), (uint)infoLevel);
                Tracing.TraceResult(returnCode);
                result = new JET_DBINFOMISC();
                result.SetFromNativeDbinfoMisc(ref native);
            }
            else {
                NATIVE_DBINFOMISC native;
                returnCode = NativeMethods.JetGetDatabaseFileInfoW(_name, out native,
                    (uint)Marshal.SizeOf(typeof(NATIVE_DBINFOMISC)), (uint)infoLevel);
                Tracing.TraceResult(returnCode);
                result = new JET_DBINFOMISC();
                result.SetFromNativeDbinfoMisc(ref native);
            }
            EsentExceptionHelper.Check(returnCode);
            return result;
        }

        /// <summary>Retrieves certain information about the given database.</summary>
        /// <param name="infoLevel">The specific data to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        public JET_DBINFOMISC GetInfo(JET_DbInfo infoLevel)
        {
            JET_DBINFOMISC result = null;

            Tracing.TraceFunctionCall("GetInfo");
            int returnCode = (int)JET_err.Success;
            if (_owner.Capabilities.SupportsWindows7Features) {
                NATIVE_DBINFOMISC4 native;
                returnCode = NativeMethods.JetGetDatabaseInfoW(_owner.Id, _dbid.Value, out native,
                    (uint)Marshal.SizeOf(typeof(NATIVE_DBINFOMISC4)), (uint)infoLevel);
                Tracing.TraceResult(returnCode);
                result = new JET_DBINFOMISC();
                result.SetFromNativeDbinfoMisc(ref native);
            }
            else {
                NATIVE_DBINFOMISC native;
                returnCode = NativeMethods.JetGetDatabaseInfo(_owner.Id, _dbid.Value, out native,
                    (uint)Marshal.SizeOf(typeof(NATIVE_DBINFOMISC)), (uint)infoLevel);
                Tracing.TraceResult(returnCode);
                result = new JET_DBINFOMISC();
                result.SetFromNativeDbinfoMisc(ref native);
            }
            EsentExceptionHelper.Check(returnCode);
            return result;
        }

        /// <summary>Retrieves certain information about the given database.</summary>
        /// <param name="infoLevel">The specific data to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        public int GetInt32FileInfo(JET_DbInfo infoLevel)
        {
            int result;
            Tracing.TraceFunctionCall("GetInt32FileInfo");
            int returnCode = NativeMethods.JetGetDatabaseFileInfoW(_name, out result, sizeof(int),
                (uint)infoLevel);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return result;
        }

        /// <summary>Retrieves certain information about the given database.</summary>
        /// <param name="infoLevel">The specific data to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        public int GetInt32Info(JET_DbInfo infoLevel)
        {
            Tracing.TraceFunctionCall("GetInt32Info");
            int result;
            int returnCode = NativeMethods.JetGetDatabaseInfo(_owner.Id, _dbid.Value,
                out result, sizeof(int), (uint)infoLevel);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return result;
        }

        /// <summary>Retrieves certain information about the given database.</summary>
        /// <param name="infoLevel">The specific data to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        public long GetInt64FileInfo(JET_DbInfo infoLevel)
        {
            Tracing.TraceFunctionCall("GetInt64FileInfo");
            long result;
            int returnCode = NativeMethods.JetGetDatabaseFileInfoW(_name, out result,
                sizeof(long), (uint)infoLevel);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return result;
        }

        /// <summary>Retrieves certain information about the given database.</summary>
        /// <param name="infoLevel">The specific data to retrieve.</param>
        /// <returns>An error if the call fails.</returns>
        public string GetStringInfo(JET_DbInfo infoLevel)
        {
            Tracing.TraceFunctionCall("GetStringInfo");
            const int MaxCharacters = 1024;
            StringBuilder sb = new StringBuilder(MaxCharacters);
            int returnCode = NativeMethods.JetGetDatabaseInfoW(_owner.Id, _dbid.Value, sb,
                MaxCharacters, (uint)infoLevel);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return sb.ToString();
        }

        /// <summary>Extends the size of a database that is currently open.</summary>
        /// <param name="desiredPages">The desired size of the database, in pages.</param>
        /// <param name="actualPages">The size of the database, in pages, after the call.</param>
        public void Grow(int desiredPages, out int actualPages)
        {
            Tracing.TraceFunctionCall("Grow");
            Helpers.CheckNotNegative(desiredPages, "desiredPages");

            uint actualPagesNative = 0;
            int returnCode = NativeMethods.JetGrowDatabase(_owner.Id, _dbid.Value,
                checked((uint)desiredPages), out actualPagesNative);
            Tracing.TraceResult(returnCode);
            actualPages = checked((int)actualPagesNative);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Opens a cursor on a previously created table.</summary>
        /// <param name="tablename">The name of the table to open.</param>
        /// <param name="parameters">The parameter is not used.</param>
        /// <param name="grbit">Table open options.</param>
        /// <returns>An ESENT warning.</returns>
        [CLSCompliant(false)]
        public IJetTable OpenTable(string tablename, byte[] parameters,
            OpenTableGrbit grbit)
        {
            Tracing.TraceFunctionCall("OpenTable");
            JET_TABLEID tableid = JET_TABLEID.Nil;
            Helpers.CheckNotNull(tablename, "tablename");
            int returnCode = NativeMethods.JetOpenTable(_owner.Id, this._dbid.Value,
                tablename, parameters, checked((uint)parameters.Length),
                (uint)grbit, out tableid.Value);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return new JetTable(this, tableid);
        }

        /// <summary>Extends the size of a database that is currently open.</summary>
        /// <param name="desiredPages">The desired size of the database, in pages.</param>
        /// <param name="actualPages">The size of the database, in pages, after the call.</param>
        /// <returns>An error if the call fails.</returns>
        public int SetSize(int desiredPages, out int actualPages)
        {
            Tracing.TraceFunctionCall("SetSize");
            Helpers.CheckNotNegative(desiredPages, "desiredPages");
            Helpers.CheckNotNull(_name, "database");
            uint actualPagesNative = 0;
            int err = NativeMethods.JetSetDatabaseSizeW(_owner.Id, _name, checked((uint)desiredPages),
                out actualPagesNative);
            Tracing.TraceResult(err);
            actualPages = checked((int)actualPagesNative);
            return err;
        }

        private JET_DBID _dbid;
        private string _name;
        private JetSession _owner;
    }
}
