using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using EsentLib.Api;
using EsentLib.Api.Flags;
using EsentLib.Jet;
using EsentLib.Jet.Types;

namespace EsentLib.Implementation
{
    /// <summary></summary>
    [CLSCompliant(false)]
    public class JetDatabase : IJetDatabase, IDisposable
    {
        /// <summary></summary>
        /// <param name="owner">The session that has been used to open or create this
        /// database. Every operation on the database must be performed inside this session.
        /// </param>
        /// <param name="dbid"></param>
        /// <param name="name">Database name.</param>
        internal JetDatabase(JetSession owner, JET_DBID dbid, string name)
        {
            if (null == owner) { throw new ArgumentNullException("owner"); }
            if (string.IsNullOrEmpty(name)) { throw new ArgumentNullException("name"); }
            _dbid = dbid;
            _name = name;
            _owner = owner;
        }

        /// <summary></summary>
        ~JetDatabase()
        {
            Dispose(false);
        }

        internal JetSession Session
        {
            get { return _owner; }
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

        /// <summary>Clean-up resources.</summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        /// <summary>Clean-up resources.</summary>
        protected void Dispose(bool disposing)
        {
            Close(CloseDatabaseGrbit.None);
            return;
        }

        /// <summary>Enumerate the name of the tables in this database, optionally including
        /// system tables.</summary>
        /// <param name="includeSystemTables">true if system tables should be included.</param>
        /// <returns>An enumerable object.</returns>
        public IJetTemporaryTable<string> EnumerateTableNames(bool includeSystemTables = false)
        {
            JET_OBJECTLIST tables = GetDatabaseTables();
            JetTable resultTable = new JetTable(this, tables.tableid);
            return new JetTemporaryTable<string>(resultTable,
                resultTable.Enumerate<string>(
                    includeSystemTables
                    ? (JetTable.FilterDelegate)null // Accept every record.
                    : (JetTable.FilterDelegate)delegate () {
                        // Determine if the current entry in the table being enumerated should be
                        // skipped (not returned). Here we are skipping system tables.
                        // Returns true if the current entry should be skipped.
                        int flags = (int)resultTable.RetrieveColumnAsInt32(tables.columnidflags);
                        return (ObjectInfoFlags.System == ((ObjectInfoFlags)flags & ObjectInfoFlags.System));
                    },
                    delegate () {
                        string name = resultTable.RetrieveColumnAsString(tables.columnidobjectname);
                        return StringCache.TryToIntern(name);
                    }));
        }

        //internal string RetrieveTableName()
        //{
        //    string name = RetrieveColumnAsString(_owner.Session.Handle, this.TableidToEnumerate,
        //        this.objectlist.columnidobjectname, Encoding.Unicode, RetrieveColumnGrbit.None);
        //    return StringCache.TryToIntern(name);
        //}

        /// <summary>Retrieves information about database tables. This is the only kind of
        /// database objects that are supported for information retrieval by the underlying
        /// native API.</summary>
        /// <returns>An object list. One of the member of the returned structure is the identifier
        /// of the underlying temporary table. THe caller is responsible for closing this
        /// temporary table.</returns>
        public JET_OBJECTLIST GetDatabaseTables()
        {
            Tracing.TraceFunctionCall("GetDatabaseTables");
            JET_OBJECTLIST result = new JET_OBJECTLIST();
            NATIVE_OBJECTLIST nativeObjectlist = NATIVE_OBJECTLIST.Create();
            bool succeeded = false;
            int returnCode = NativeMethods.JetGetObjectInfoW(_owner.Id, _dbid.Value,
                (uint)JET_objtyp.Table, null, null, ref nativeObjectlist, nativeObjectlist.cbStruct,
                (uint)JET_ObjInfo.ListNoStats);
            try {
                Tracing.TraceResult(returnCode);
                EsentExceptionHelper.Check(returnCode);
                result.SetFromNativeObjectlist(_owner, nativeObjectlist);
                succeeded = true;
                return result;
            }
            finally { if (!succeeded) { NativeMethods.JetCloseTable(_owner.Id, result.tableid.Value); } }
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
        /// <param name="grbit">Table open options.</param>
        /// <returns>An ESENT warning.</returns>
        [CLSCompliant(false)]
        public IJetTable OpenTable(string tablename, OpenTableGrbit grbit = OpenTableGrbit.None)
        {
            Tracing.TraceFunctionCall("OpenTable");
            JET_TABLEID tableid = JET_TABLEID.Nil;
            Helpers.CheckNotNull(tablename, "tablename");
            int returnCode = NativeMethods.JetOpenTable(_owner.Id, _dbid.Value, tablename,
                null, 0, (uint)grbit, out tableid.Value);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return new JetTable(this, tableid);
        }

        /// <summary>Resizes a currently open database. Windows 8: Only supports growing a
        /// database file. Windows 8.1: When <see cref="JET_param.EnableShrinkDatabase"/> is set to
        /// <see cref="ShrinkDatabaseGrbit.On"/>, and if the file system supports Sparse
        /// Files, then space may be freed up in the middle of the file.</summary>
        /// <remarks>Many APIs return the logical size of the file, not how many bytes it
        /// takes up on disk. Win32's GetCompressedFileSize returns the correct on-disk size.
        /// <see cref="IJetDatabase.GetInfo(JET_DbInfo)"/> returns the on-disk size when
        /// used with <see cref="JET_DbInfo.FilesizeOnDisk"/></remarks>
        /// <param name="desiredPages">The desired size of the database, in pages.</param>
        /// <param name="grbit">Resize options.</param>
        public int Resize(int desiredPages, ResizeDatabaseGrbit grbit = ResizeDatabaseGrbit.None)
        {
            // LegacyApi.Impl.JetResizeDatabase(sesid, dbid, desiredPages, out actualPages, grbit)
            Tracing.TraceFunctionCall("Resize");
            Helpers.CheckNotNegative(desiredPages, "desiredPages");
            uint actualPagesNative = 0;
            int returnCode;
            if (ResizeDatabaseGrbit.None != grbit) {
                this._owner.Capabilities.CheckSupportsWindows8Features("Resize");
                returnCode = NativeMethods.JetResizeDatabase(_owner.Id, this._dbid.Value,
                    checked((uint)desiredPages), out actualPagesNative, (uint)grbit);
            }
            else {
                Helpers.CheckNotNull(_name, "database");
                returnCode = NativeMethods.JetSetDatabaseSizeW(_owner.Id, _name,
                    checked((uint)desiredPages), out actualPagesNative);
            }
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return checked((int)actualPagesNative);
        }

        private JET_DBID _dbid;
        private string _name;
        private JetSession _owner;
    }
}
