//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

using EsentLib.Jet;
using EsentLib.Jet.Vista;
using EsentLib.Jet.Windows8;

namespace EsentLib.Implementation
{
    /// <summary>Native interop for functions in esent.dll.</summary>
    [SuppressUnmanagedCodeSecurity]
    [BestFitMapping(false, ThrowOnUnmappableChar = true)]
    internal static partial class NativeMethods
    {
        #region Configuration Constants

        /// <summary>The CharSet for the methods in the DLL.</summary>
        private const CharSet EsentCharSet = CharSet.Ansi;

        /// <summary>Initializes static members of the NativeMethods class.</summary>
        static NativeMethods()
        {
            // This must be changed when the CharSet is changed.
            NativeMethods.Encoding = LibraryHelpers.EncodingASCII;
        }

        /// <summary>Gets encoding to be used when converting data to/from byte arrays.
        /// This should match the CharSet above.</summary>
        public static Encoding Encoding { get; private set; }

        #endregion Configuration Constants

        #region init/term
        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetCreateInstance(out IntPtr instance, string szInstanceName);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetCreateInstanceW(out IntPtr instance, string szInstanceName);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetCreateInstance2(out IntPtr instance, string szInstanceName,
            string szDisplayName, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetCreateInstance2W(out IntPtr instance, string szInstanceName,
            string szDisplayName, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetInit(ref IntPtr instance);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetInit2(ref IntPtr instance, uint grbit);

        // JetInit3 was introduced in Vista, so therefore we'll only support the Unicode version.
        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetInit3W(ref IntPtr instance, ref NATIVE_RSTINFO prstinfo,
            uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetInit3W(ref IntPtr instance, IntPtr prstinfo, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern unsafe int JetGetInstanceInfo(out uint pcInstanceInfo,
            out NATIVE_INSTANCE_INFO* prgInstanceInfo);

        // Returns unicode strings in the NATIVE_INSTANCE_INFO.
        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern unsafe int JetGetInstanceInfoW(out uint pcInstanceInfo,
            out NATIVE_INSTANCE_INFO* prgInstanceInfo);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGetInstanceMiscInfo(IntPtr instance, ref NATIVE_SIGNATURE pvResult,
            uint cbMax, uint infoLevel);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetStopBackupInstance(IntPtr instance);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetStopServiceInstance(IntPtr instance);


        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetStopServiceInstance2(IntPtr instance, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetTerm(IntPtr instance);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetTerm2(IntPtr instance, uint grbit);
        
        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static unsafe extern int JetSetSystemParameter(IntPtr* pinstance, IntPtr sesid,
            uint paramid, IntPtr lParam, string szParam);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static unsafe extern int JetSetSystemParameterW(IntPtr* pinstance, IntPtr sesid,
            uint paramid, IntPtr lParam, string szParam);

        // The param is ref because it is an 'in' parameter when getting error text
        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetSystemParameter(IntPtr instance, IntPtr sesid, uint paramid,
            ref IntPtr plParam, [Out] StringBuilder szParam, uint cbMax);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetSystemParameterW(IntPtr instance, IntPtr sesid, uint paramid,
            ref IntPtr plParam, [Out] StringBuilder szParam, uint cbMax);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGetVersion(IntPtr sesid, out uint dwVersion);

        #endregion

        #region Databases

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetCreateDatabase(IntPtr sesid, string szFilename, string szConnect, out uint dbid, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetCreateDatabaseW(IntPtr sesid, string szFilename, string szConnect, out uint dbid, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetCreateDatabase2(IntPtr sesid, string szFilename, uint cpgDatabaseSizeMax, out uint dbid, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetCreateDatabase2W(IntPtr sesid, string szFilename, uint cpgDatabaseSizeMax, out uint dbid, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetAttachDatabaseW(IntPtr sesid, string szFilename, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetAttachDatabase2(IntPtr sesid, string szFilename, uint cpgDatabaseSizeMax, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetAttachDatabase2W(IntPtr sesid, string szFilename, uint cpgDatabaseSizeMax, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetDetachDatabase(IntPtr sesid, string szFilename);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetDetachDatabase2(IntPtr sesid, string szFilename, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetDetachDatabase2W(IntPtr sesid, string szFilename, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetDetachDatabaseW(IntPtr sesid, string szFilename);

        //[DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        //public static extern int JetOpenDatabase(IntPtr sesid, string database, string szConnect, out uint dbid, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetOpenDatabaseW(IntPtr sesid, string database, string szConnect,
            out uint dbid, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetCloseDatabase(IntPtr sesid, uint dbid, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetCompact(IntPtr sesid, string szDatabaseSrc, string szDatabaseDest,
            IntPtr pfnStatus, IntPtr pconvert, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetCompactW(IntPtr sesid, string szDatabaseSrc, string szDatabaseDest,
            IntPtr pfnStatus, IntPtr pconvert, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGrowDatabase(IntPtr sesid, uint dbid, uint cpg, out uint pcpgReal);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetSetDatabaseSize(IntPtr sesid, string szDatabaseName, uint cpg, out uint pcpgReal);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetSetDatabaseSizeW(IntPtr sesid, string szDatabaseName, uint cpg, out uint pcpgReal);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetDatabaseInfo(IntPtr sesid, uint dbid, out int intValue, uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetDatabaseInfo(IntPtr sesid, uint dbid, out NATIVE_DBINFOMISC dbinfomisc, uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetDatabaseInfo(IntPtr sesid, uint dbid, out NATIVE_DBINFOMISC4 dbinfomisc, uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetDatabaseInfo(IntPtr sesid, uint dbid, [Out] StringBuilder stringValue, uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetDatabaseInfoW(IntPtr sesid, uint dbid, out int intValue, uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetDatabaseInfoW(IntPtr sesid, uint dbid, out NATIVE_DBINFOMISC dbinfomisc, uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetDatabaseInfoW(IntPtr sesid, uint dbid, out NATIVE_DBINFOMISC4 dbinfomisc, uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetDatabaseInfoW(IntPtr sesid, uint dbid, [Out] StringBuilder stringValue, uint cbMax, uint InfoLevel);

        #endregion

        #region JetGetDatabaseFileInfo

        // Unicode, int
        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetDatabaseFileInfoW(string szFilename, out int intValue, uint cbMax, uint InfoLevel);

        // ASCII, int
        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetDatabaseFileInfo(string szFilename, out int intValue, uint cbMax, uint InfoLevel);

        // Unicode, long
        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetDatabaseFileInfoW(string szFilename, out long intValue, uint cbMax, uint InfoLevel);

        // ASCII, long
        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetDatabaseFileInfo(string szFilename, out long intValue, uint cbMax, uint InfoLevel);

        // Unicode, JET_DBINFOMISC4
        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetDatabaseFileInfoW(string szFilename, out NATIVE_DBINFOMISC4 dbinfomisc, uint cbMax, uint InfoLevel);

        // ASCII, JET_DBINFOMISC
        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetDatabaseFileInfo(string szFilename, out NATIVE_DBINFOMISC dbinfomisc, uint cbMax, uint InfoLevel);

        // Unicode, JET_DBINFOMISC
        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetDatabaseFileInfoW(string szFilename, out NATIVE_DBINFOMISC dbinfomisc, uint cbMax, uint InfoLevel);

        #endregion

        #region Backup/Restore
        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetBackupInstanceW(
            IntPtr instance, string szBackupPath, uint grbit, IntPtr pfnStatus);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetRestoreInstance(IntPtr instance, string sz, string szDest, IntPtr pfn);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetRestoreInstanceW(IntPtr instance, string sz, string szDest, IntPtr pfn);

        #endregion

        #region Snapshot Backup

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetOSSnapshotPrepare(out IntPtr snapId, uint grbit);

        // Introduced in Windows Vista
        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetOSSnapshotPrepareInstance(IntPtr snapId, IntPtr instance, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern unsafe int JetOSSnapshotFreeze(IntPtr snapId, out uint pcInstanceInfo,
            out NATIVE_INSTANCE_INFO* prgInstanceInfo, uint grbit);

        // Returns unicode strings in the NATIVE_INSTANCE_INFO.
        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern unsafe int JetOSSnapshotFreezeW(IntPtr snapId, out uint pcInstanceInfo,
            out NATIVE_INSTANCE_INFO* prgInstanceInfo, uint grbit);

        // Introduced in Windows Vista
        // Returns unicode strings in the NATIVE_INSTANCE_INFO.
        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern unsafe int JetOSSnapshotGetFreezeInfoW(IntPtr snapId,
            out uint pcInstanceInfo, out NATIVE_INSTANCE_INFO* prgInstanceInfo, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetOSSnapshotThaw(IntPtr snapId, uint grbit);

        // Introduced in Windows Vista
        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetOSSnapshotTruncateLog(IntPtr snapId, uint grbit);

        // Introduced in Windows Vista
        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetOSSnapshotTruncateLogInstance(IntPtr snapId, IntPtr instance, uint grbit);

        // Introduced in Windows Vista
        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetOSSnapshotEnd(IntPtr snapId, uint grbit);

        // Introduced in Windows Server 2003
        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetOSSnapshotAbort(IntPtr snapId, uint grbit);
        #endregion

        #region Snapshot Backup/Restore

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetBeginExternalBackupInstance(IntPtr instance, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetCloseFileInstance(IntPtr instance, IntPtr handle);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetEndExternalBackupInstance(IntPtr instance);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetEndExternalBackupInstance2(IntPtr instance, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetAttachInfoInstance(IntPtr instance, [Out] byte[] szz, uint cbMax, out uint pcbActual);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetAttachInfoInstanceW(IntPtr instance, [Out] byte[] szz, uint cbMax, out uint pcbActual);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetLogInfoInstance(IntPtr instance, [Out] byte[] szz, uint cbMax, out uint pcbActual);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetLogInfoInstanceW(IntPtr instance, [Out] byte[] szz, uint cbMax, out uint pcbActual);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetTruncateLogInfoInstance(IntPtr instance, [Out] byte[] szz, uint cbMax, out uint pcbActual);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetTruncateLogInfoInstanceW(IntPtr instance, [Out] byte[] szz, uint cbMax, out uint pcbActual);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetOpenFileInstance(IntPtr instance, string szFileName,
            out IntPtr phfFile, out uint pulFileSizeLow, out uint pulFileSizeHigh);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetOpenFileInstanceW(IntPtr instance, string szFileName,
            out IntPtr phfFile, out uint pulFileSizeLow, out uint pulFileSizeHigh);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetReadFileInstance(IntPtr instance, IntPtr handle,
            IntPtr pv, uint cb, out uint pcbActual);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetTruncateLogInstance(IntPtr instance);
        #endregion

        #region sessions

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetBeginSession(IntPtr instance, out IntPtr session, string username, string password);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetSetSessionContext(IntPtr session, IntPtr context);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetResetSessionContext(IntPtr session);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetEndSession(IntPtr sesid, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetDupSession(IntPtr sesid, out IntPtr newSesid);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static unsafe extern int JetGetThreadStats(JET_THREADSTATS* pvResult, uint cbMax);

        #endregion

        #region tables

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetOpenTable(IntPtr sesid, uint dbid, string tablename, byte[] pvParameters, uint cbParameters, uint grbit, out IntPtr tableid);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetOpenTableW(IntPtr sesid, uint dbid, string tablename, byte[] pvParameters, uint cbParameters, uint grbit, out IntPtr tableid);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetCloseTable(IntPtr sesid, IntPtr tableid);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetDupCursor(IntPtr sesid, IntPtr tableid, out IntPtr tableidNew, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetComputeStats(IntPtr sesid, IntPtr tableid);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetSetLS(IntPtr sesid, IntPtr tableid, IntPtr ls, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGetLS(IntPtr sesid, IntPtr tableid, out IntPtr pls, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGetCursorInfo(IntPtr sesid, IntPtr tableid, IntPtr pvResult, uint cbMax, uint infoLevel);
        #endregion

        #region transactions

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetBeginTransaction(IntPtr sesid);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetBeginTransaction2(IntPtr sesid, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetBeginTransaction3(IntPtr sesid, long trxid, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetCommitTransaction(IntPtr sesid, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetRollback(IntPtr sesid, uint grbit);

        #endregion

        #region DDL

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetCreateTable(IntPtr sesid, uint dbid, string szTableName, int pages, int density, out IntPtr tableid);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetAddColumn(IntPtr sesid, IntPtr tableid, string szColumnName, [In] ref NATIVE_COLUMNDEF columndef, [In] byte[] pvDefault, uint cbDefault, out uint columnid);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetDeleteColumn(IntPtr sesid, IntPtr tableid, string szColumnName);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetDeleteColumn2(IntPtr sesid, IntPtr tableid, string szColumnName, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetDeleteIndex(IntPtr sesid, IntPtr tableid, string szIndexName);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetDeleteTable(IntPtr sesid, uint dbid, string szTableName);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetCreateIndex(IntPtr sesid, IntPtr tableid, string szIndexName, uint grbit, string szKey, uint cbKey, uint lDensity);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetCreateIndex2(IntPtr sesid, IntPtr tableid,
            [In] JET_INDEXCREATE.NATIVE_INDEXCREATE[] pindexcreate, uint cIndexCreate);

        // Introduced in Windows Vista, this version takes the larger NATIVE_INDEXCREATE1 structure.
        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetCreateIndex2W(
            IntPtr sesid, IntPtr tableid, [In] JET_INDEXCREATE.NATIVE_INDEXCREATE1[] pindexcreate, uint cIndexCreate);

        // Introduced in Windows 7, this version takes the larger NATIVE_INDEXCREATE2 structure, supporting
        // space hints.
        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetCreateIndex3W(IntPtr sesid, IntPtr tableid,
            [In] JET_INDEXCREATE.NATIVE_INDEXCREATE2[] pindexcreate, uint cIndexCreate);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetOpenTempTable(IntPtr sesid, [In] NATIVE_COLUMNDEF[] rgcolumndef,
            uint ccolumn, uint grbit, out IntPtr ptableid, [Out] uint[] rgcolumnid);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetOpenTempTable2(IntPtr sesid, [In] NATIVE_COLUMNDEF[] rgcolumndef,
            uint ccolumn, uint lcid, uint grbit, out IntPtr ptableid, [Out] uint[] rgcolumnid);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetOpenTempTable3(IntPtr sesid, [In] NATIVE_COLUMNDEF[] rgcolumndef,
            uint ccolumn, [In] ref NATIVE_UNICODEINDEX pidxunicode, uint grbit, out IntPtr ptableid,
            [Out] uint[] rgcolumnid);

        // Introduced in Windows Vista
        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetOpenTemporaryTable(IntPtr sesid, [In, Out] ref NATIVE_OPENTEMPORARYTABLE popentemporarytable);

        // OMMITED : workaroud method
        //[DllImport(Constants.EsentDll, ExactSpelling = true)]
        //public static extern int JetOpenTemporaryTable2(IntPtr sesid, [In, Out] ref NATIVE_OPENTEMPORARYTABLE2 popentemporarytable);

        // Overload to allow for null pidxunicode
        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetOpenTempTable3(IntPtr sesid, [In] NATIVE_COLUMNDEF[] rgcolumndef,
            uint ccolumn, IntPtr pidxunicode, uint grbit, out IntPtr ptableid, [Out] uint[] rgcolumnid);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetCreateTableColumnIndex2(IntPtr sesid, uint dbid, ref JET_TABLECREATE.NATIVE_TABLECREATE2 tablecreate3);

        // Introduced in Vista.
        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetCreateTableColumnIndex2W(IntPtr sesid, uint dbid, ref JET_TABLECREATE.NATIVE_TABLECREATE2 tablecreate3);

        // Introduced in Windows 7
        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetCreateTableColumnIndex3W(IntPtr sesid, uint dbid, ref JET_TABLECREATE.NATIVE_TABLECREATE3 tablecreate3);

        #region JetGetTableColumnInfo overlaods.
        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetTableColumnInfoW(IntPtr sesid, IntPtr tableid,
            IntPtr searchKey, ref object columndef, int cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetTableColumnInfoW(IntPtr sesid, IntPtr tableid,
            string szColumnName, ref NATIVE_COLUMNDEF columndef, uint cbMax,
            uint InfoLevel);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGetTableColumnInfoW(IntPtr sesid, IntPtr tableid,
            ref uint pcolumnid, ref NATIVE_COLUMNDEF columndef, uint cbMax,
            uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetTableColumnInfoW(IntPtr sesid, IntPtr tableid,
            string szColumnName, ref NATIVE_COLUMNBASE_WIDE columnbase, uint cbMax,
            uint InfoLevel);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGetTableColumnInfoW(IntPtr sesid, IntPtr tableid,
            ref uint pcolumnid, ref NATIVE_COLUMNBASE_WIDE columnbase, uint cbMax,
            uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetTableColumnInfoW(IntPtr sesid, IntPtr tableid,
            string szIgnored, ref NATIVE_COLUMNLIST columnlist, uint cbMax,
            uint InfoLevel);
        #endregion

        #region JetGetColumnInfo overlaods.
        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetColumnInfo(IntPtr sesid, uint dbid, string szTableName, string szColumnName, ref NATIVE_COLUMNDEF columndef, uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetColumnInfo(IntPtr sesid, uint dbid, string szTableName, string szColumnName, ref NATIVE_COLUMNLIST columnlist, uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetColumnInfo(IntPtr sesid, uint dbid, string szTableName, string szColumnName, ref NATIVE_COLUMNBASE columnbase, uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetColumnInfo(IntPtr sesid, uint dbid, string szTableName, ref uint pcolumnid, ref NATIVE_COLUMNBASE columnbase, uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetColumnInfoW(IntPtr sesid, uint dbid, string szTableName, string szColumnName, ref NATIVE_COLUMNDEF columndef, uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetColumnInfoW(IntPtr sesid, uint dbid, string szTableName, string szColumnName, ref NATIVE_COLUMNLIST columnlist, uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetColumnInfoW(IntPtr sesid, uint dbid, string szTableName, string szColumnName, ref NATIVE_COLUMNBASE_WIDE columnbase, uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetColumnInfoW(IntPtr sesid, uint dbid, string szTableName, ref uint pcolumnid, ref NATIVE_COLUMNBASE_WIDE columnbase, uint cbMax, uint InfoLevel);
        #endregion

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetObjectInfo(IntPtr sesid, uint dbid, uint objtyp,
            string szContainerName, string szObjectName, [In, Out] ref NATIVE_OBJECTLIST objectlist,
            uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetObjectInfoW(IntPtr sesid, uint dbid, uint objtyp,
            string szContainerName, string szObjectName, [In, Out] ref NATIVE_OBJECTLIST objectlist,
            uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetObjectInfo(IntPtr sesid, uint dbid, uint objtyp,
            string szContainerName, string szObjectName, [In, Out] ref NATIVE_OBJECTINFO objectinfo,
            uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetObjectInfoW(IntPtr sesid, uint dbid, uint objtyp,
            string szContainerName, string szObjectName, [In, Out] ref NATIVE_OBJECTINFO objectinfo,
            uint cbMax, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetCurrentIndex(IntPtr sesid, IntPtr tableid,
            [Out] StringBuilder szIndexName, uint cchIndexName);

        #region JetGetTableInfo overloads

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetTableInfo(IntPtr sesid, IntPtr tableid, [Out] out NATIVE_OBJECTINFO pvResult,
            uint cbMax, uint infoLevel);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetTableInfo(IntPtr sesid, IntPtr tableid, [Out] out uint pvResult,
            uint cbMax, uint infoLevel);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetTableInfo(IntPtr sesid, IntPtr tableid, [Out] int[] pvResult,
            uint cbMax, uint infoLevel);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetTableInfo(IntPtr sesid, IntPtr tableid,
            [Out] StringBuilder pvResult, uint cbMax, uint infoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetTableInfoW(IntPtr sesid, IntPtr tableid,
            [Out] out NATIVE_OBJECTINFO pvResult, uint cbMax, uint infoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetTableInfoW(IntPtr sesid, IntPtr tableid, [Out] out uint pvResult,
            uint cbMax, uint infoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetTableInfoW(IntPtr sesid, IntPtr tableid, [Out] int[] pvResult,
            uint cbMax, uint infoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetTableInfoW(IntPtr sesid, IntPtr tableid, [Out] StringBuilder pvResult,
            uint cbMax, uint infoLevel);

        #endregion

        #region JetGetIndexInfo overloads

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetIndexInfo(IntPtr sesid, uint dbid, string szTableName,
            string szIndexName, [Out] out ushort result, uint cbResult, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetIndexInfo(IntPtr sesid, uint dbid, string szTableName,
            string szIndexName, [Out] out uint result, uint cbResult, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetIndexInfo(IntPtr sesid, uint dbid, string szTableName,
            string szIndexName, [Out] out JET_INDEXID result, uint cbResult, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetIndexInfo(IntPtr sesid, uint dbid, string szTableName,
            string szIndexName, [In] [Out] ref NATIVE_INDEXLIST result, uint cbResult, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetIndexInfoW(IntPtr sesid, uint dbid, string szTableName,
            string szIndexName, [Out] out ushort result, uint cbResult, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetIndexInfoW(IntPtr sesid, uint dbid, string szTableName,
            string szIndexName, [Out] out uint result, uint cbResult, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetIndexInfoW(IntPtr sesid, uint dbid, string szTableName,
            string szIndexName, [Out] out JET_INDEXID result, uint cbResult, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetIndexInfoW(IntPtr sesid, uint dbid, string szTableName,
            string szIndexName, [In] [Out] ref NATIVE_INDEXLIST result, uint cbResult, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetIndexInfoW(IntPtr sesid, uint dbid, string szTableName,
            string szIndexName, [Out] StringBuilder result, uint cbResult, uint InfoLevel);

        #endregion

        #region JetGetTableIndexInfo overloads

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetTableIndexInfo(IntPtr sesid, IntPtr tableid, string szIndexName,
            [Out] out ushort result, uint cbResult, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetTableIndexInfo(IntPtr sesid, IntPtr tableid, string szIndexName,
            [Out] out uint result, uint cbResult, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetTableIndexInfo(IntPtr sesid, IntPtr tableid, string szIndexName,
            [Out] out JET_INDEXID result, uint cbResult, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetTableIndexInfo(IntPtr sesid, IntPtr tableid, string szIndexName,
            [In] [Out] ref NATIVE_INDEXLIST result, uint cbResult, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetTableIndexInfoW(IntPtr sesid, IntPtr tableid, string szIndexName,
            [Out] out ushort result, uint cbResult, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetTableIndexInfoW(IntPtr sesid, IntPtr tableid, string szIndexName,
            [Out] out uint result, uint cbResult, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetTableIndexInfoW(IntPtr sesid, IntPtr tableid, string szIndexName,
            [Out] out JET_INDEXID result, uint cbResult, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetTableIndexInfoW(IntPtr sesid, IntPtr tableid, string szIndexName,
            [In] [Out] ref NATIVE_INDEXLIST result, uint cbResult, uint InfoLevel);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetTableIndexInfoW(IntPtr sesid, IntPtr tableid, string szIndexName,
            [Out] StringBuilder result, uint cbResult, uint InfoLevel);

        #endregion

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetRenameTable(IntPtr sesid, uint dbid, string szName, string szNameNew);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetRenameColumn(IntPtr sesid, IntPtr tableid, string szName, string szNameNew, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetSetColumnDefaultValue(
            IntPtr sesid, uint tableid, [MarshalAs(UnmanagedType.LPStr)] string szTableName, [MarshalAs(UnmanagedType.LPStr)] string szColumnName, byte[] pvData, uint cbData, uint grbit);

        #endregion

        #region Navigation

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGotoBookmark(IntPtr sesid, IntPtr tableid, [In] byte[] pvBookmark,
            uint cbBookmark);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGotoSecondaryIndexBookmark(IntPtr sesid, IntPtr tableid,
            [In] byte[] pvSecondaryKey, uint cbSecondaryKey, [In] byte[] pvPrimaryBookmark,
            uint cbPrimaryBookmark, uint grbit);

        // This has IntPtr and NATIVE_RETINFO versions because the parameter can be null
        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetMove(IntPtr sesid, IntPtr tableid, int cRow, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetMakeKey(IntPtr sesid, IntPtr tableid, IntPtr pvData, uint cbData, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetSeek(IntPtr sesid, IntPtr tableid, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetSetIndexRange(IntPtr sesid, IntPtr tableid, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetIntersectIndexes(IntPtr sesid, [In] NATIVE_INDEXRANGE[] rgindexrange,
            uint cindexrange, [In] [Out] ref NATIVE_RECORDLIST recordlist, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetSetCurrentIndex(IntPtr sesid, IntPtr tableid, string szIndexName);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetSetCurrentIndex2(IntPtr sesid, IntPtr tableid, string szIndexName,
            uint grbit);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetSetCurrentIndex3(IntPtr sesid, IntPtr tableid, string szIndexName,
            uint grbit, uint itagSequence);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetSetCurrentIndex4(IntPtr sesid, IntPtr tableid, string szIndexName,
            [In] ref JET_INDEXID indexid, uint grbit, uint itagSequence);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetIndexRecordCount(IntPtr sesid, IntPtr tableid, out uint crec, uint crecMax);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetSetTableSequential(IntPtr sesid, IntPtr tableid, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetResetTableSequential(IntPtr sesid, IntPtr tableid, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGetRecordPosition(IntPtr sesid, IntPtr tableid, out NATIVE_RECPOS precpos, uint cbRecpos);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGotoPosition(IntPtr sesid, IntPtr tableid, [In] ref NATIVE_RECPOS precpos);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static unsafe extern int JetPrereadKeys(
            IntPtr sesid, IntPtr tableid, void** rgpvKeys, uint* rgcbKeys, int ckeys, out int pckeysPreread, uint grbit);

        #endregion

        #region Data Retrieval

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGetBookmark(IntPtr sesid, IntPtr tableid, [Out] byte[] pvBookmark, uint cbMax, out uint cbActual);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGetSecondaryIndexBookmark(IntPtr sesid, IntPtr tableid,
            [Out] byte[] secondaryKey, uint secondaryKeySize, out uint actualSecondaryKeySize,
            [Out] byte[] primaryKey, uint primaryKeySize, out uint actualPrimaryKeySize, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetRetrieveColumn(IntPtr sesid, IntPtr tableid, uint columnid, IntPtr pvData, uint cbData, out uint cbActual, uint grbit, IntPtr pretinfo);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetRetrieveColumn(IntPtr sesid, IntPtr tableid, uint columnid,
            IntPtr pvData, uint cbData, out uint cbActual, uint grbit,
            [In] [Out] ref NATIVE_RETINFO pretinfo);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static unsafe extern int JetRetrieveColumns(
            IntPtr sesid, IntPtr tableid, [In] [Out] NATIVE_RETRIEVECOLUMN* psetcolumn, uint csetcolumn);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetRetrieveKey(IntPtr sesid, IntPtr tableid, [Out] byte[] pvData, uint cbMax, out uint cbActual, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern unsafe int JetEnumerateColumns(IntPtr sesid, IntPtr tableid,
            uint cEnumColumnId, NATIVE_ENUMCOLUMNID* rgEnumColumnId, out uint pcEnumColumn,
            out NATIVE_ENUMCOLUMN* prgEnumColumn, JET_PFNREALLOC pfnRealloc, IntPtr pvReallocContext,
            uint cbDataMost, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGetRecordSize(
            IntPtr sesid, IntPtr tableid, ref NATIVE_RECSIZE precsize, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGetRecordSize2(
            IntPtr sesid, IntPtr tableid, ref NATIVE_RECSIZE2 precsize, uint grbit);

        #endregion

        #region DML

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetDelete(IntPtr sesid, IntPtr tableid);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetPrepareUpdate(IntPtr sesid, IntPtr tableid, uint prep);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetUpdate(IntPtr sesid, IntPtr tableid, [Out] byte[] pvBookmark,
            uint cbBookmark, out uint cbActual);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetUpdate2(IntPtr sesid, IntPtr tableid, [Out] byte[] pvBookmark,
            uint cbBookmark, out uint cbActual, uint grbit);

        // This has IntPtr and NATIVE_SETINFO versions because the parameter can be null
        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetSetColumn(IntPtr sesid, IntPtr tableid, uint columnid, IntPtr pvData, uint cbData, uint grbit, IntPtr psetinfo);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetSetColumn(IntPtr sesid, IntPtr tableid, uint columnid, IntPtr pvData, uint cbData, uint grbit, [In] ref NATIVE_SETINFO psetinfo);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static unsafe extern int JetSetColumns(
            IntPtr sesid, IntPtr tableid, [In] [Out] NATIVE_SETCOLUMN* psetcolumn, uint csetcolumn);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGetLock(IntPtr sesid, IntPtr tableid, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetEscrowUpdate(IntPtr sesid, IntPtr tableid, uint columnid,
            [In] byte[] pv, uint cbMax, [Out] byte[] pvOld, uint cbOldMax, out uint cbOldActual,
            uint grbit);

        #endregion

        #region Callbacks

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetRegisterCallback(
            IntPtr sesid, IntPtr tableid, uint cbtyp, NATIVE_CALLBACK callback, IntPtr pvContext, out IntPtr pCallbackId);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetUnregisterCallback(IntPtr sesid, IntPtr tableid, uint cbtyp, IntPtr hCallbackId);

        #endregion

        #region Online Maintenance

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetDefragment(
            IntPtr sesid, uint dbid, string szTableName, ref uint pcPasses, ref uint pcSeconds, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetDefragment(
            IntPtr sesid, uint dbid, string szTableName, IntPtr pcPasses, IntPtr pcSeconds, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetDefragment2(
            IntPtr sesid, uint dbid, string szTableName, ref uint pcPasses, ref uint pcSeconds, IntPtr callback, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetDefragment2(
            IntPtr sesid, uint dbid, string szTableName, IntPtr pcPasses, IntPtr pcSeconds, IntPtr callback, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetDefragment2W(
            IntPtr sesid, uint dbid, string szTableName, ref uint pcPasses, ref uint pcSeconds, IntPtr callback, uint grbit);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetDefragment2W(
            IntPtr sesid, uint dbid, string szTableName, IntPtr pcPasses, IntPtr pcSeconds, IntPtr callback, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetIdle(IntPtr sesid, uint grbit);

        #endregion

        #region Misc

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetConfigureProcessForCrashDump(uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetFreeBuffer(IntPtr pbBuf);

        #endregion

        // ---------- //
        // WINDOWS 8  //
        // ---------- //
        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGetErrorInfoW(ref int error, [In, Out] ref NATIVE_ERRINFOBASIC pvResult,
            uint cbMax, uint InfoLevel, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetResizeDatabase(IntPtr sesid, uint dbid, uint cpg,
            out uint pcpgActual, uint grbit);

        #region DDL
        // OMMITED : workaround method
        //[DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        //public static extern int JetCreateIndex4W(
        //    IntPtr sesid, IntPtr tableid, [In] NATIVE_INDEXCREATE3[] pindexcreate, uint cIndexCreate);

        [DllImport(Constants.EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetCreateTableColumnIndex4W(IntPtr sesid, uint dbid, ref NATIVE_TABLECREATE4 tablecreate3);
        #endregion

        #region Session Parameters
        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGetSessionParameter(IntPtr sesid, uint sesparamid, out int data,
            int dataSize, out int actualDataSize);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGetSessionParameter(IntPtr sesid, uint sesparamid, [Out] byte[] data,
            int dataSize, out int actualDataSize);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetSetSessionParameter(IntPtr sesid, uint sesparamid, byte[] data,
            int dataSize);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetSetSessionParameter(IntPtr sesid, uint sesparamid, ref int data,
            int dataSize);

        #endregion

        #region Misc
        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetCommitTransaction2(IntPtr sesid, uint grbit, uint cmsecDurableCommit,
            ref NATIVE_COMMIT_ID pCommitId);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetPrereadIndexRanges(IntPtr sesid, IntPtr tableid, [In] NATIVE_INDEX_RANGE[] pIndexRanges, uint cIndexRanges, out int pcRangesPreread, uint[] rgcolumnidPreread, uint ccolumnidPreread, uint grbit);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetSetCursorFilter(IntPtr sesid, IntPtr tableid, [In] NATIVE_INDEX_COLUMN[] pFilters, uint cFilters, uint grbit);
        #endregion

        // ---------- //
        // WINDOWS 10 //
        // ---------- //
        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetSetSessionParameter(IntPtr sesid, uint sesparamid,
            ref NATIVE_OPERATIONCONTEXT data, int dataSize);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGetSessionParameter(IntPtr sesid, uint sesparamid,
            out NATIVE_OPERATIONCONTEXT data, int dataSize, out int actualDataSize);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static unsafe extern int JetGetThreadStats(JET_THREADSTATS2* pvResult, uint cbMax);
    }
}