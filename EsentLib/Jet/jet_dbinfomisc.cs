//-----------------------------------------------------------------------
// <copyright file="jet_dbinfomisc.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;

namespace EsentLib.Jet
{
    /// <summary>The native version of the JET_DBINFOMISC structure.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules",
        "SA1305:FieldNamesMustNotUseHungarianNotation",
        Justification = "This should match the unmanaged API, which isn't capitalized.")]
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.NamingRules",
        "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter",
        Justification = "This should match the unmanaged API, which isn't capitalized.")]
    internal struct NATIVE_DBINFOMISC
    {
        /// <summary>Version of Esent that created the database.</summary>
        public uint ulVersion;
        /// <summary>Incremental version of Esent that created the database.</summary>
        public uint ulUpdate;
        /// <summary>Database signature.</summary>
        public NATIVE_SIGNATURE signDb;
        /// <summary>Consistent/inconsistent state.</summary>
        public uint dbstate;
        /// <summary>Null if in inconsistent state.</summary>
        public JET_LGPOS lgposConsistent;
        /// <summary>Null if in inconsistent state.</summary>
        public JET_LOGTIME logtimeConsistent;
        /// <summary>Last attach time.</summary>
        public JET_LOGTIME logtimeAttach;
        /// <summary>Lgpos at last attach.</summary>
        public JET_LGPOS lgposAttach;
        /// <summary>Last detach time.</summary>
        public JET_LOGTIME logtimeDetach;
        /// <summary>Lgpos at last detach.</summary>
        public JET_LGPOS lgposDetach;
        /// <summary>Logfile signature.</summary>
        public NATIVE_SIGNATURE signLog;
        /// <summary>Last successful full backup.</summary>
        public JET_BKINFO bkinfoFullPrev;
        /// <summary>Last successful incremental backup. Reset when <see cref="bkinfoFullPrev"/>
        /// is set.</summary>
        public JET_BKINFO bkinfoIncPrev;
        /// <summary>Current backup.</summary>
        public JET_BKINFO bkinfoFullCur;
        /// <summary>Internal use only.</summary>
        public uint fShadowingDisabled;
        /// <summary>Internal use only.</summary>
        public uint fUpgradeDb;
        /// <summary>OS major version.</summary>
        public uint dwMajorVersion;
        /// <summary>OS minor version.</summary>
        public uint dwMinorVersion;
        /// <summary>OS build number.</summary>
        public uint dwBuildNumber;
        /// <summary>OS Service Pack number.</summary>
        public uint lSPNumber;
        /// <summary>Database page size (0 = 4Kb page).</summary>
        public uint cbPageSize;
    }

    /// <summary>Native version of the JET_DBINFOMISC structure. Adds support for fields that
    /// we added in Windows 7.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules",
        "SA1305:FieldNamesMustNotUseHungarianNotation",
        Justification = "This should match the unmanaged API, which isn't capitalized.")]
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.NamingRules",
        "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter",
        Justification = "This should match the unmanaged API, which isn't capitalized.")]
    internal struct NATIVE_DBINFOMISC4
    {
        /// <summary>The core dbinfo structure.</summary>
        public NATIVE_DBINFOMISC dbinfo;
        // Fields added in JET_DBINFOMISC2
        /// <summary>The minimum log generation required for replaying the logs. Typically the
        /// checkpoint generation.</summary>
        public uint genMinRequired;
        /// <summary>The maximum log generation required for replaying the logs.</summary>
        public uint genMaxRequired;
        /// <summary>Creation time of the <see cref="genMaxRequired"/> logfile.</summary>
        public JET_LOGTIME logtimeGenMaxCreate;
        /// <summary>Number of times repair has been called on this database.</summary>
        public uint ulRepairCount;
        /// <summary>The last time that repair was run against this database.</summary>
        public JET_LOGTIME logtimeRepair;
        /// <summary>Number of times this database was repaired before the last defrag.</summary>
        public uint ulRepairCountOld;
        /// <summary>Number of times a one bit error was successfully fixed.</summary>
        public uint ulECCFixSuccess;
        /// <summary>The last time a one bit error was successfully fixed.</summary>
        public JET_LOGTIME logtimeECCFixSuccess;
        /// <summary>The number of times a one bit error was successfully fixed before the
        /// last repair.</summary>
        public uint ulECCFixSuccessOld;
        /// <summary>Number of times an uncorrectable one bit error was encountered.</summary>
        public uint ulECCFixFail;
        /// <summary>The last time an uncorrectable one bit error was encountered.</summary>
        public JET_LOGTIME logtimeECCFixFail;
        /// <summary>The number of times an uncorrectable one bit error was encountered.</summary>
        public uint ulECCFixFailOld;
        /// <summary>Number of times a non-correctable checksum error was found.</summary>
        public uint ulBadChecksum;
        /// <summary>The last time a non-correctable checksum error was found.</summary>
        public JET_LOGTIME logtimeBadChecksum;
        /// <summary>The number of times a non-correctable checksum error was found before the
        /// last repair.</summary>
        public uint ulBadChecksumOld;
        // Fields added in JET_DBINFOMISC3
        /// <summary>The maximum log generation committed to the database. Typically the current
        /// log generation.</summary>
        public uint genCommitted;
        // Fields added in JET_DBINFOMISC4
        /// <summary>Last successful copy backup.</summary>
        public JET_BKINFO bkinfoCopyPrev;
        /// <summary>Last successful differential backup. Reset when bkinfoFullPrev is set.</summary>
        public JET_BKINFO bkinfoDiffPrev;
    }

    /// <summary>Holds miscellaneous information about a database. This is the information that
    /// is contained in the database header.</summary>
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.NamingRules",
        "SA1300:ElementMustBeginWithUpperCaseLetter",
        Justification = "This should match the unmanaged API, which isn't capitalized.")]
    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules",
        "SA1305:FieldNamesMustNotUseHungarianNotation",
        Justification = "Need to avoid clash between members and properties.")]
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.NamingRules",
        "SA1309:FieldNamesMustNotBeginWithUnderscore",
        Justification = "Need to avoid clash between members and properties.")]
    [Serializable]
    public sealed partial class JET_DBINFOMISC : IEquatable<JET_DBINFOMISC>
    {
        /// <summary>Gets the version of Esent that created the database.</summary>
        public int ulVersion { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the incremental version of Esent that created the database.</summary>
        public int ulUpdate { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the database signature.</summary>
        public JET_SIGNATURE signDb { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the consistent/inconsistent state of the database.</summary>
        public JET_dbstate dbstate { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the lgpos when the database was made consistent. This value is null
        /// if the database is inconsistent.</summary>
        public JET_LGPOS lgposConsistent { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the time when the database was made consistent. This value is null if
        /// the database is inconsistent.</summary>
        public JET_LOGTIME logtimeConsistent { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the time when the database was attached.</summary>
        public JET_LOGTIME logtimeAttach { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the lgpos of the last attach.</summary>
        public JET_LGPOS lgposAttach { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the time of the last detach.</summary>
        public JET_LOGTIME logtimeDetach { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the lgpos of the last detach.</summary>
        public JET_LGPOS lgposDetach { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the logfile signature of logs used to modify the database.</summary>
        public JET_SIGNATURE signLog { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets information about the last successful full backup.</summary>
        public JET_BKINFO bkinfoFullPrev { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets information about the last successful incremental backup. This value
        /// is reset when <see cref="bkinfoFullPrev"/> is set.
        /// </summary>
        public JET_BKINFO bkinfoIncPrev { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets information about the current backup.</summary>
        public JET_BKINFO bkinfoFullCur { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets a value indicating whether catalog shadowing is enabled. This value
        /// is for internal use only.</summary>
        public bool fShadowingDisabled { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets a value indicating whether the database is being upgraded. This
        /// value is for internal use only.</summary>
        public bool fUpgradeDb { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the OS major version from the last attach.</summary>
        public int dwMajorVersion { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the OS minor version from the last attach.</summary>
        public int dwMinorVersion { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the OS build number from the last attach.</summary>
        public int dwBuildNumber { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the OS Service Pack number from the last attach.</summary>
        public int lSPNumber { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the database page size. A value of 0 means 4Kb pages.</summary>
        public int cbPageSize { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the minimum log generation required for replaying the logs. Typically
        /// the checkpoint generation.</summary>
        public int genMinRequired { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the maximum log generation required for replaying the logs.</summary>
        public int genMaxRequired { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the creation time of the <see cref="genMaxRequired"/> logfile.</summary>
        public JET_LOGTIME logtimeGenMaxCreate { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the number of times repair has been called on this database.</summary>
        public int ulRepairCount { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the last time that repair was run against this database.</summary>
        public JET_LOGTIME logtimeRepair { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the number of times this database was repaired before the last
        /// defrag.</summary>
        public int ulRepairCountOld { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the number of times a one bit error was successfully fixed.</summary>
        public int ulECCFixSuccess { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the last time a one bit error was successfully fixed.</summary>
        public JET_LOGTIME logtimeECCFixSuccess { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the number of times a one bit error was successfully fixed before
        /// the last repair.</summary>
        public int ulECCFixSuccessOld { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the number of times an uncorrectable one bit error was encountered.
        /// </summary>
        public int ulECCFixFail { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the last time an uncorrectable one bit error was encountered.</summary>
        public JET_LOGTIME logtimeECCFixFail { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the number of times an uncorrectable one bit error was encountered.
        /// </summary>
        public int ulECCFixFailOld { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the number of times a non-correctable checksum error was found.</summary>
        public int ulBadChecksum { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the last time a non-correctable checksum error was found.</summary>
        public JET_LOGTIME logtimeBadChecksum { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the number of times a non-correctable checksum error was found
        /// before the last repair.</summary>
        public int ulBadChecksumOld { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets the maximum log generation committed to the database. Typically the
        /// current log generation.</summary>
        public int genCommitted { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets information about the last successful copy backup.</summary>
        public JET_BKINFO bkinfoCopyPrev { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets information about the last successful differential backup. Reset
        /// when  <see cref="bkinfoFullPrev"/> is set.</summary>
        public JET_BKINFO bkinfoDiffPrev { [DebuggerStepThrough] get; internal set; }

        /// <summary>Gets a string representation of this object.</summary>
        /// <returns>A string representation of this object.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "JET_DBINFOMISC({0})", this.signDb);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            var hashes = new[] {
                this.ulVersion,
                this.ulUpdate,
                this.signDb.GetHashCode(),
                this.dbstate.GetHashCode(),
                this.lgposConsistent.GetHashCode(),
                this.logtimeConsistent.GetHashCode(),
                this.logtimeAttach.GetHashCode(),
                this.lgposAttach.GetHashCode(),
                this.logtimeDetach.GetHashCode(),
                this.lgposDetach.GetHashCode(),
                this.signLog.GetHashCode(),
                this.bkinfoFullPrev.GetHashCode(),
                this.bkinfoIncPrev.GetHashCode(),
                this.bkinfoFullCur.GetHashCode(),
                this.fShadowingDisabled.GetHashCode(),
                this.fUpgradeDb.GetHashCode(),
                this.dwMajorVersion,
                this.dwMinorVersion,
                this.dwBuildNumber,
                this.lSPNumber,
                this.cbPageSize,
                this.genMinRequired,
                this.genMaxRequired,
                this.logtimeGenMaxCreate.GetHashCode(),
                this.ulRepairCount,
                this.logtimeRepair.GetHashCode(),
                this.ulRepairCountOld,
                this.ulECCFixSuccess,
                this.logtimeECCFixSuccess.GetHashCode(),
                this.ulECCFixSuccessOld,
                this.ulECCFixFail,
                this.logtimeECCFixFail.GetHashCode(),
                this.ulECCFixFailOld,
                this.ulBadChecksum,
                this.logtimeBadChecksum.GetHashCode(),
                this.ulBadChecksumOld,
                this.genCommitted,
                this.bkinfoCopyPrev.GetHashCode(),
                this.bkinfoDiffPrev.GetHashCode(),
            };
            List<int> listHashes = new List<int>(hashes);
            this.AddNotYetPublishedHashCodes(listHashes);
            return Util.CalculateHashCode(listHashes);
        }

        /// <summary>Determines whether the specified <see cref="T:System.Object"/> is equal to
        /// the current <see cref="T:System.Object"/>.</summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current
        /// <see cref="T:System.Object"/>.</param><returns>
        /// True if the specified <see cref="T:System.Object"/> is equal to the current
        /// <see cref="T:System.Object"/>; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return ((null != obj) && (this.GetType() == obj.GetType())
                && this.Equals((JET_DBINFOMISC)obj));
        }

        /// <summary>whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>True if the current object is equal to the <paramref name="other"/>
        /// parameter; otherwise, false.</returns>
        public bool Equals(JET_DBINFOMISC other)
        {
            if (null == other) { return false; }
            bool notYetPublishedEquals = true;
            this.NotYetPublishedEquals(other, ref notYetPublishedEquals);
            return notYetPublishedEquals
                   && this.ulVersion == other.ulVersion
                   && this.ulUpdate == other.ulUpdate
                   && this.signDb == other.signDb
                   && this.dbstate == other.dbstate
                   && this.lgposConsistent == other.lgposConsistent
                   && this.logtimeConsistent == other.logtimeConsistent
                   && this.logtimeAttach == other.logtimeAttach
                   && this.lgposAttach == other.lgposAttach
                   && this.logtimeDetach == other.logtimeDetach
                   && this.lgposDetach == other.lgposDetach
                   && this.signLog == other.signLog
                   && this.bkinfoFullPrev == other.bkinfoFullPrev
                   && this.bkinfoIncPrev == other.bkinfoIncPrev
                   && this.bkinfoFullCur == other.bkinfoFullCur
                   && this.fShadowingDisabled == other.fShadowingDisabled
                   && this.fUpgradeDb == other.fUpgradeDb
                   && this.dwMajorVersion == other.dwMajorVersion
                   && this.dwMinorVersion == other.dwMinorVersion
                   && this.dwBuildNumber == other.dwBuildNumber
                   && this.lSPNumber == other.lSPNumber
                   && this.cbPageSize == other.cbPageSize
                   && this.genMinRequired == other.genMinRequired
                   && this.genMaxRequired == other.genMaxRequired
                   && this.logtimeGenMaxCreate == other.logtimeGenMaxCreate
                   && this.ulRepairCount == other.ulRepairCount
                   && this.logtimeRepair == other.logtimeRepair
                   && this.ulRepairCountOld == other.ulRepairCountOld
                   && this.ulECCFixSuccess == other.ulECCFixSuccess
                   && this.logtimeECCFixSuccess == other.logtimeECCFixSuccess
                   && this.ulECCFixSuccessOld == other.ulECCFixSuccessOld
                   && this.ulECCFixFail == other.ulECCFixFail
                   && this.logtimeECCFixFail == other.logtimeECCFixFail
                   && this.ulECCFixFailOld == other.ulECCFixFailOld
                   && this.ulBadChecksum == other.ulBadChecksum
                   && this.logtimeBadChecksum == other.logtimeBadChecksum
                   && this.ulBadChecksumOld == other.ulBadChecksumOld
                   && this.genCommitted == other.genCommitted
                   && this.bkinfoCopyPrev == other.bkinfoCopyPrev
                   && this.bkinfoDiffPrev == other.bkinfoDiffPrev;
        }

        /// <summary>Sets the members of this object from a native object.</summary>
        /// <param name="native">The native object.</param>
        internal void SetFromNativeDbinfoMisc(ref NATIVE_DBINFOMISC native)
        {
            unchecked {
                this.ulVersion = (int)native.ulVersion;
                this.ulUpdate = (int)native.ulUpdate;
                this.signDb = new JET_SIGNATURE(native.signDb);
                this.dbstate = (JET_dbstate)native.dbstate;
                this.lgposConsistent = native.lgposConsistent;
                this.logtimeConsistent = native.logtimeConsistent;
                this.logtimeAttach = native.logtimeAttach;
                this.lgposAttach = native.lgposAttach;
                this.logtimeDetach = native.logtimeDetach;
                this.lgposDetach = native.lgposDetach;
                this.signLog = new JET_SIGNATURE(native.signLog);
                this.bkinfoFullPrev = native.bkinfoFullPrev;
                this.bkinfoIncPrev = native.bkinfoIncPrev;
                this.bkinfoFullCur = native.bkinfoFullCur;
                this.fShadowingDisabled = 0 != native.fShadowingDisabled;
                this.fUpgradeDb = 0 != native.fUpgradeDb;
                this.dwMajorVersion = (int)native.dwMajorVersion;
                this.dwMinorVersion = (int)native.dwMinorVersion;
                this.dwBuildNumber = (int)native.dwBuildNumber;
                this.lSPNumber = (int)native.lSPNumber;
                this.cbPageSize = (int)native.cbPageSize;
            }
        }

        /// <summary>Sets the members of this object from a native object.</summary>
        /// <param name="native">The native object.</param>
        internal void SetFromNativeDbinfoMisc(ref NATIVE_DBINFOMISC4 native)
        {
            this.SetFromNativeDbinfoMisc(ref native.dbinfo);

            unchecked {
                this.genMinRequired = (int)native.genMinRequired;
                this.genMaxRequired = (int)native.genMaxRequired;
                this.logtimeGenMaxCreate = native.logtimeGenMaxCreate;
                this.ulRepairCount = (int)native.ulRepairCount;
                this.logtimeRepair = native.logtimeRepair;
                this.ulRepairCountOld = (int)native.ulRepairCountOld;
                this.ulECCFixSuccess = (int)native.ulECCFixSuccess;
                this.logtimeECCFixSuccess = native.logtimeECCFixSuccess;
                this.ulECCFixSuccessOld = (int)native.ulECCFixSuccessOld;
                this.ulECCFixFail = (int)native.ulECCFixFail;
                this.logtimeECCFixFail = native.logtimeECCFixFail;
                this.ulECCFixFailOld = (int)native.ulECCFixFailOld;
                this.ulBadChecksum = (int)native.ulBadChecksum;
                this.logtimeBadChecksum = native.logtimeBadChecksum;
                this.ulBadChecksumOld = (int)native.ulBadChecksumOld;
                this.genCommitted = (int)native.genCommitted;
                this.bkinfoCopyPrev = native.bkinfoCopyPrev;
                this.bkinfoDiffPrev = native.bkinfoDiffPrev;
            }
        }

        /// <summary>Calculates the native version of the structure.</summary>
        /// <returns>The native version of the structure.</returns>
        internal NATIVE_DBINFOMISC GetNativeDbinfomisc()
        {
            NATIVE_DBINFOMISC native = new NATIVE_DBINFOMISC();

            unchecked {
                native.ulVersion = (uint)this.ulVersion;
                native.ulUpdate = (uint)this.ulUpdate;
                native.signDb = this.signDb.GetNativeSignature();
                native.dbstate = (uint)this.dbstate;
                native.lgposConsistent = this.lgposConsistent;
                native.logtimeConsistent = this.logtimeConsistent;
                native.logtimeAttach = this.logtimeAttach;
                native.lgposAttach = this.lgposAttach;
                native.logtimeDetach = this.logtimeDetach;
                native.lgposDetach = this.lgposDetach;
                native.signLog = this.signLog.GetNativeSignature();
                native.bkinfoFullPrev = this.bkinfoFullPrev;
                native.bkinfoIncPrev = this.bkinfoIncPrev;
                native.bkinfoFullCur = this.bkinfoFullCur;
                native.fShadowingDisabled = this.fShadowingDisabled ? 1u : 0u;
                native.fUpgradeDb = this.fUpgradeDb ? 1u : 0u;
                native.dwMajorVersion = (uint)this.dwMajorVersion;
                native.dwMinorVersion = (uint)this.dwMinorVersion;
                native.dwBuildNumber = (uint)this.dwBuildNumber;
                native.lSPNumber = (uint)this.lSPNumber;
                native.cbPageSize = (uint)this.cbPageSize;
            }
            return native;
        }

        /// <summary>Calculates the native version of the structure.</summary>
        /// <returns>The native version of the structure.</returns>
        internal NATIVE_DBINFOMISC4 GetNativeDbinfomisc4()
        {
            NATIVE_DBINFOMISC4 native = new NATIVE_DBINFOMISC4();
            native.dbinfo = this.GetNativeDbinfomisc();
            unchecked {
                native.genMinRequired = (uint)this.genMinRequired;
                native.genMaxRequired = (uint)this.genMaxRequired;
                native.logtimeGenMaxCreate = this.logtimeGenMaxCreate;
                native.ulRepairCount = (uint)this.ulRepairCount;
                native.logtimeRepair = this.logtimeRepair;
                native.ulRepairCountOld = (uint)this.ulRepairCountOld;
                native.ulECCFixSuccess = (uint)this.ulECCFixSuccess;
                native.logtimeECCFixSuccess = this.logtimeECCFixSuccess;
                native.ulECCFixSuccessOld = (uint)this.ulECCFixSuccessOld;
                native.ulECCFixFail = (uint)this.ulECCFixFail;
                native.logtimeECCFixFail = this.logtimeECCFixFail;
                native.ulECCFixFailOld = (uint)this.ulECCFixFailOld;
                native.ulBadChecksum = (uint)this.ulBadChecksum;
                native.logtimeBadChecksum = this.logtimeBadChecksum;
                native.ulBadChecksumOld = (uint)this.ulBadChecksumOld;
                native.genCommitted = (uint)this.genCommitted;
                native.bkinfoCopyPrev = this.bkinfoCopyPrev;
                native.bkinfoDiffPrev = this.bkinfoDiffPrev;
            }
            return native;
        }

        /// <summary>Provides a hook to allow comparison of additional fields in a different
        /// file. These additonal fields are not yet published on MSDN.</summary>
        /// <param name="other">The structure to compare with.</param>
        /// <param name="notYetPublishedEquals">Whether the additional fields in <paramref name="other"/>
        /// are the same as this.</param>
        partial void NotYetPublishedEquals(JET_DBINFOMISC other, ref bool notYetPublishedEquals);

        /// <summary>Provides a hook to allow additional fields to be calculated in the hashcode.
        /// These additonal fields are not yet published on MSDN.</summary>
        /// <param name="hashCodes">The list of hashcodes to add to.</param>
        partial void AddNotYetPublishedHashCodes(IList<int> hashCodes);
    }
}