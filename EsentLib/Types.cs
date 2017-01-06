//-----------------------------------------------------------------------
// <copyright file="types.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

using EsentLib.Api;
using EsentLib.Implementation;

namespace EsentLib
{
    /// <summary>
    /// A JET_TABLEID contains a handle to the database cursor to use for a call to the JET Api.
    /// A cursor can only be used with the session that was used to open that cursor.
    /// </summary>
    public struct JET_TABLEID : IEquatable<JET_TABLEID>, IFormattable
    {
        /// <summary>
        /// The native value.
        /// </summary>
        internal IntPtr Value;

        /// <summary>
        /// Gets a null JET_TABLEID.
        /// </summary>
        public static JET_TABLEID Nil
        {
            [DebuggerStepThrough]
            get { return new JET_TABLEID(); }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="JET_TABLEID"/> is valid (checks against 0 and -1).
        /// </summary>
        public bool IsInvalid
        {
            get { return this.Value == IntPtr.Zero || this.Value == new IntPtr(~0); }
        }
   
        /// <summary>
        /// Determines whether two specified instances of JET_TABLEID
        /// are equal.
        /// </summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the two instances are equal.</returns>
        public static bool operator ==(JET_TABLEID lhs, JET_TABLEID rhs)
        {
            return lhs.Value == rhs.Value;
        }

        /// <summary>
        /// Determines whether two specified instances of JET_TABLEID
        /// are not equal.
        /// </summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the two instances are not equal.</returns>
        public static bool operator !=(JET_TABLEID lhs, JET_TABLEID rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Generate a string representation of the structure.
        /// </summary>
        /// <returns>The structure as a string.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "JET_TABLEID(0x{0:x})", this.Value.ToInt64());
        }

        /// <summary>
        /// Formats the value of the current instance using the specified format.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing the value of the current instance in the specified format.
        /// </returns>
        /// <param name="format">The <see cref="T:System.String"/> specifying the format to use.
        /// -or- 
        /// null to use the default format defined for the type of the <see cref="T:System.IFormattable"/> implementation. 
        /// </param>
        /// <param name="formatProvider">The <see cref="T:System.IFormatProvider"/> to use to format the value.
        /// -or- 
        /// null to obtain the numeric format information from the current locale setting of the operating system. 
        /// </param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.IsNullOrEmpty(format) || "G" == format ? this.ToString() : this.Value.ToInt64().ToString(format, formatProvider);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal
        /// to another instance.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals((JET_TABLEID)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal
        /// to another instance.
        /// </summary>
        /// <param name="other">An instance to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public bool Equals(JET_TABLEID other)
        {
            return this.Value.Equals(other.Value);
        }
    }

    /// <summary>A JET_COLUMNID identifies a column within a table.</summary>
    public struct JET_COLUMNID : IEquatable<JET_COLUMNID>, IComparable<JET_COLUMNID>, IFormattable
    {
        /// <summary>Creates a new instance of a <see cref="JET_COLUMNID"/> using the specified
        /// value as the underlying value.</summary>
        /// <param name="nativeValue">The native ESE JET_COLUMNID.</param>
        /// <returns>An initialized <see cref="JET_COLUMNID"/> structure.</returns>
        /// <remarks>Use of this function should be exceedingly rare.</remarks>
        internal JET_COLUMNID(uint nativeValue)
        {
            Value = nativeValue;
        }

        /// <summary>Gets a null JET_COLUMNID.</summary>
        public static JET_COLUMNID Nil
        {
            [DebuggerStepThrough]
            get { return new JET_COLUMNID(); }
        }

        /// <summary>Gets a value indicating whether the <see cref="JET_COLUMNID"/> is valid (checks against 0 and -1).</summary>
        public bool IsInvalid
        {
            get { return (0 == Value) || (uint.MaxValue == Value); }
        }

        /// <summary>Determines whether two specified instances of JET_COLUMNID are equal.</summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the two instances are equal.</returns>
        public static bool operator ==(JET_COLUMNID lhs, JET_COLUMNID rhs)
        {
            return lhs.Value == rhs.Value;
        }

        /// <summary>Determines whether two specified instances of JET_COLUMNID are not equal.</summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the two instances are not equal.</returns>
        public static bool operator !=(JET_COLUMNID lhs, JET_COLUMNID rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Determine whether one columnid is before another columnid.
        /// </summary>
        /// <param name="lhs">The first columnid to compare.</param>
        /// <param name="rhs">The second columnid to compare.</param>
        /// <returns>True if lhs comes before rhs.</returns>
        public static bool operator <(JET_COLUMNID lhs, JET_COLUMNID rhs)
        {
            return lhs.Value < rhs.Value;
        }

        /// <summary>
        /// Determine whether one columnid is after another columnid.
        /// </summary>
        /// <param name="lhs">The first columnid to compare.</param>
        /// <param name="rhs">The second columnid to compare.</param>
        /// <returns>True if lhs comes after rhs.</returns>
        public static bool operator >(JET_COLUMNID lhs, JET_COLUMNID rhs)
        {
            return lhs.Value > rhs.Value;
        }

        /// <summary>
        /// Determine whether one columnid is before or equal to
        /// another columnid.
        /// </summary>
        /// <param name="lhs">The first columnid to compare.</param>
        /// <param name="rhs">The second columnid to compare.</param>
        /// <returns>True if lhs comes before or is equal to rhs.</returns>
        public static bool operator <=(JET_COLUMNID lhs, JET_COLUMNID rhs)
        {
            return lhs.Value <= rhs.Value;
        }

        /// <summary>
        /// Determine whether one columnid is after or equal to
        /// another columnid.
        /// </summary>
        /// <param name="lhs">The first columnid to compare.</param>
        /// <param name="rhs">The second columnid to compare.</param>
        /// <returns>True if lhs comes after or is equal to rhs.</returns>
        public static bool operator >=(JET_COLUMNID lhs, JET_COLUMNID rhs)
        {
            return lhs.Value >= rhs.Value;
        }

        /// <summary>
        /// Generate a string representation of the structure.
        /// </summary>
        /// <returns>The structure as a string.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "JET_COLUMNID(0x{0:x})", this.Value);
        }

        /// <summary>
        /// Formats the value of the current instance using the specified format.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing the value of the current instance in the specified format.
        /// </returns>
        /// <param name="format">The <see cref="T:System.String"/> specifying the format to use.
        /// -or- 
        /// null to use the default format defined for the type of the <see cref="T:System.IFormattable"/> implementation. 
        /// </param>
        /// <param name="formatProvider">The <see cref="T:System.IFormatProvider"/> to use to format the value.
        /// -or- 
        /// null to obtain the numeric format information from the current locale setting of the operating system. 
        /// </param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.IsNullOrEmpty(format) || "G" == format ? this.ToString() : this.Value.ToString(format, formatProvider);
        }

        /// <summary>Returns a value indicating whether this instance is equal to another instance.</summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public override bool Equals(object obj)
        {
            return ((null != obj) && (GetType() == obj.GetType()) && this.Equals((JET_COLUMNID)obj));
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        /// <summary>Returns a value indicating whether this instance is equal to another
        /// instance.</summary>
        /// <param name="other">An instance to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public bool Equals(JET_COLUMNID other)
        {
            return this.Value.Equals(other.Value);
        }

        /// <summary>Compares this columnid to another columnid and determines whether this
        /// instance is before, the same as or after the other instance.</summary>
        /// <param name="other">The columnid to compare to the current instance.</param>
        /// <returns>A signed number indicating the relative positions of this instance and the
        /// value parameter.</returns>
        public int CompareTo(JET_COLUMNID other)
        {
            return this.Value.CompareTo(other.Value);
        }

        /// <summary>Creates a new instance of a <see cref="JET_COLUMNID"/> using the specified
        /// value as the underlying value.</summary>
        /// <param name="nativeValue">The native ESE JET_COLUMNID.</param>
        /// <returns>An initialized <see cref="JET_COLUMNID"/> structure.</returns>
        /// <remarks>Use of this function should be exceedingly rare.</remarks>
        internal static JET_COLUMNID CreateColumnidFromNativeValue(int nativeValue)
        {
            return new JET_COLUMNID() { Value = unchecked((uint)nativeValue) };
        }

        /// <summary>The native value.</summary>
        internal uint Value;
    }

    /// <summary>
    /// A JET_OSSNAPID contains a handle to a snapshot of a database.
    /// </summary>
    public struct JET_OSSNAPID : IEquatable<JET_OSSNAPID>, IFormattable
    {
        /// <summary>
        /// The native value.
        /// </summary>
        internal IntPtr Value;

        /// <summary>
        /// Gets a null JET_OSSNAPID.
        /// </summary>
        public static JET_OSSNAPID Nil
        {
            [DebuggerStepThrough]
            get { return new JET_OSSNAPID(); }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="JET_OSSNAPID"/> is valid (checks against 0 and -1).
        /// </summary>
        public bool IsInvalid
        {
            get { return this.Value == IntPtr.Zero || this.Value == new IntPtr(~0); }
        }

        /// <summary>
        /// Determines whether two specified instances of JET_OSSNAPID
        /// are equal.
        /// </summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the two instances are equal.</returns>
        public static bool operator ==(JET_OSSNAPID lhs, JET_OSSNAPID rhs)
        {
            return lhs.Value == rhs.Value;
        }

        /// <summary>
        /// Determines whether two specified instances of JET_OSSNAPID
        /// are not equal.
        /// </summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the two instances are not equal.</returns>
        public static bool operator !=(JET_OSSNAPID lhs, JET_OSSNAPID rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Generate a string representation of the structure.
        /// </summary>
        /// <returns>The structure as a string.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "JET_OSSNAPID(0x{0:x})", this.Value.ToInt64());
        }

        /// <summary>
        /// Formats the value of the current instance using the specified format.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing the value of the current instance in the specified format.
        /// </returns>
        /// <param name="format">The <see cref="T:System.String"/> specifying the format to use.
        /// -or- 
        /// null to use the default format defined for the type of the <see cref="T:System.IFormattable"/> implementation. 
        /// </param>
        /// <param name="formatProvider">The <see cref="T:System.IFormatProvider"/> to use to format the value.
        /// -or- 
        /// null to obtain the numeric format information from the current locale setting of the operating system. 
        /// </param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.IsNullOrEmpty(format) || "G" == format ? this.ToString() : this.Value.ToInt64().ToString(format, formatProvider);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal
        /// to another instance.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals((JET_OSSNAPID)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal
        /// to another instance.
        /// </summary>
        /// <param name="other">An instance to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public bool Equals(JET_OSSNAPID other)
        {
            return this.Value.Equals(other.Value);
        }
    }

    /// <summary>A JET_HANDLE contains a generic handle.</summary>
    public class JET_HANDLE : IEquatable<JET_HANDLE>, IFormattable
    {
        private JET_HANDLE(IJetInstance owner)
        {
            _owner = owner;
            return;
        }
        
        /// <summary>Gets a value indicating whether the <see cref="JET_HANDLE"/> is valid (checks
        /// against 0 and -1).</summary>
        public bool IsInvalid
        {
            get { return this._nativeHandle == IntPtr.Zero || this._nativeHandle == new IntPtr(~0); }
        }

        /// <summary>Gets a null JET_HANDLE.</summary>
        public static JET_HANDLE Nil
        {
            [DebuggerStepThrough]
            get { return new JET_HANDLE(null); }
        }

        /// <summary>Determines whether two specified instances of JET_HANDLE are equal.</summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the two instances are equal.</returns>
        public static bool operator ==(JET_HANDLE lhs, JET_HANDLE rhs)
        {
            return lhs._nativeHandle == rhs._nativeHandle;
        }

        /// <summary>Determines whether two specified instances of JET_HANDLE are not equal.</summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the two instances are not equal.</returns>
        public static bool operator !=(JET_HANDLE lhs, JET_HANDLE rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary></summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public static JET_HANDLE Create(IJetInstance owner)
        {
            return new JET_HANDLE(owner);
        }

        /// <summary>Closes a file that was opened with IJetInstance.OpenFile after the
        /// data from that file has been extracted using JetReadFileInstance.</summary>
        public void Close()
        {
            Tracing.TraceFunctionCall("Close");
            int returnCode = NativeMethods.JetCloseFileInstance(_owner.Id, _nativeHandle);
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
        }

        /// <summary>Returns a value indicating whether this instance is equal to another
        /// instance.</summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) { return false; }
            return this.Equals((JET_HANDLE)obj);
        }

        /// <summary>Returns a value indicating whether this instance is equal to another
        /// instance.</summary>
        /// <param name="other">An instance to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public bool Equals(JET_HANDLE other)
        {
            return _nativeHandle.Equals(other._nativeHandle);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return _nativeHandle.GetHashCode();
        }

        /// <summary>Retrieves the contents of a file opened with
        /// <see cref="IJetBackupInstance.OpenFile"/>.</summary>
        /// <param name="buffer">The buffer to read into.</param>
        /// <param name="bufferSize">The size of the buffer.</param>
        /// <returns>Number of bytes read.</returns>
        public int Read(byte[] buffer, int bufferSize)
        {
            Tracing.TraceFunctionCall("Read");
            Helpers.CheckNotNull(buffer, "buffer");
            Helpers.CheckDataSize(buffer, bufferSize, "bufferSize");

            // ESENT requires that the buffer be aligned on a page allocation boundary.
            // VirtualAlloc is the API used to do that, so we use P/Invoke to call it.
            IntPtr alignedBuffer = VirtualAlloc(IntPtr.Zero, (UIntPtr)bufferSize,
                (uint)(AllocationType.MEM_COMMIT | AllocationType.MEM_RESERVE),
                (uint)MemoryProtection.PAGE_READWRITE);
            ThrowExceptionOnNull(alignedBuffer, "VirtualAlloc");
            try {
                uint nativeBytesRead = 0;
                int returnCode = NativeMethods.JetReadFileInstance(_owner.Id, _nativeHandle, alignedBuffer,
                    checked((uint)bufferSize), out nativeBytesRead);
                Tracing.TraceResult(returnCode);
                int bytesRead = checked((int)nativeBytesRead);
                // Copy the memory out of the aligned buffer into the user buffer.
                Marshal.Copy(alignedBuffer, buffer, 0, bytesRead);
                EsentExceptionHelper.Check(returnCode);
                return bytesRead;
            }
            finally {
                ThrowExceptionOnFailure(VirtualFree(alignedBuffer, UIntPtr.Zero,
                    (uint)FreeType.MEM_RELEASE), "VirtualFree");
            }
        }

        /// <summary>Throw an exception if the success code is not true.</summary>
        /// <param name="success">The success code.</param>
        /// <param name="message">The message for the exception.</param>
        private static void ThrowExceptionOnFailure(bool success, string message)
        {
            if (success) { return; }
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), message);
        }

        /// <summary>Throw an exception if the given pointer is null (IntPtr.Zero).</summary>
        /// <param name="ptr">The pointer to check.</param>
        /// <param name="message">The message for the exception.</param>
        private static void ThrowExceptionOnNull(IntPtr ptr, string message)
        {
            if (IntPtr.Zero != ptr) { return; }
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), message);
        }

        /// <summary>Generate a string representation of the structure.</summary>
        /// <returns>The structure as a string.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "JET_HANDLE(0x{0:x})", this._nativeHandle.ToInt64());
        }

        /// <summary>Formats the value of the current instance using the specified format.</summary>
        /// <returns>A <see cref="T:System.String"/> containing the value of the current instance in the
        /// specified format.</returns>
        /// <param name="format">The <see cref="T:System.String"/> specifying the format to use.
        /// -or- null to use the default format defined for the type of the <see cref="T:System.IFormattable"/>
        /// implementation. </param>
        /// <param name="formatProvider">The <see cref="T:System.IFormatProvider"/> to use to format
        /// the value. -or-  null to obtain the numeric format information from the current locale
        /// setting of the operating system. </param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.IsNullOrEmpty(format) || "G" == format ? this.ToString() : this._nativeHandle.ToInt64().ToString(format, formatProvider);
        }

        /// <summary>The native value.</summary>
        internal IntPtr _nativeHandle;
        private IJetInstance _owner;

        /// <summary>The name of the DLL that holds the Core Memory API set.</summary>
        private const string WinCoreMemoryDll = "kernel32.dll";

        [DllImport(WinCoreMemoryDll, SetLastError = true)]
        private static extern IntPtr VirtualAlloc(IntPtr plAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport(WinCoreMemoryDll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

        /// <summary>Allocation type options for <see cref="VirtualAlloc"/>.</summary>
        [Flags]
        internal enum AllocationType : uint
        {
            /// <summary>Commit the memory.</summary>
            MEM_COMMIT = 0x1000,
            /// <summary>Reserve the memory.</summary>
            MEM_RESERVE = 0x2000,
        }

        /// <summary>Memory protection options for <see cref="VirtualAlloc"/>.</summary>
        internal enum MemoryProtection : uint
        {
            /// <summary>Read/write access to the pages.</summary>
            PAGE_READWRITE = 0x04,
        }

        /// <summary>Options for <see cref="VirtualFree"/>.</summary>
        internal enum FreeType : uint
        {
            /// <summary>Release the memory. The pages will be in the free state.</summary>
            MEM_RELEASE = 0x8000,
        }
    }

    /// <summary>
    /// Local storage for an ESENT handle. Used by <see cref="LegacyApi.JetGetLS"/>
    /// and <see cref="LegacyApi.JetSetLS"/>.
    /// </summary>
    public struct JET_LS : IEquatable<JET_LS>, IFormattable
    {
        /// <summary>
        /// The null handle.
        /// </summary>
        public static readonly JET_LS Nil = new JET_LS { Value = new IntPtr(~0) };

        /// <summary>
        /// Gets a value indicating whether the <see cref="JET_LS"/> is valid (checks against 0 and -1).
        /// </summary>
        public bool IsInvalid
        {
            get { return this.Value == IntPtr.Zero || this.Value == new IntPtr(~0); }
        }

        /// <summary>
        /// Gets or sets the value of the handle.
        /// </summary>
        public IntPtr Value { get; set; }

        /// <summary>
        /// Determines whether two specified instances of JET_LS
        /// are equal.
        /// </summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the two instances are equal.</returns>
        public static bool operator ==(JET_LS lhs, JET_LS rhs)
        {
            return lhs.Value == rhs.Value;
        }

        /// <summary>
        /// Determines whether two specified instances of JET_LS
        /// are not equal.
        /// </summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the two instances are not equal.</returns>
        public static bool operator !=(JET_LS lhs, JET_LS rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Generate a string representation of the structure.
        /// </summary>
        /// <returns>The structure as a string.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "JET_LS(0x{0:x})", this.Value.ToInt64());
        }

        /// <summary>
        /// Formats the value of the current instance using the specified format.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing the value of the current instance in the specified format.
        /// </returns>
        /// <param name="format">The <see cref="T:System.String"/> specifying the format to use.
        /// -or- 
        /// null to use the default format defined for the type of the <see cref="T:System.IFormattable"/> implementation. 
        /// </param>
        /// <param name="formatProvider">The <see cref="T:System.IFormatProvider"/> to use to format the value.
        /// -or- 
        /// null to obtain the numeric format information from the current locale setting of the operating system. 
        /// </param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.IsNullOrEmpty(format) || "G" == format ? this.ToString() : this.Value.ToInt64().ToString(format, formatProvider);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal
        /// to another instance.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals((JET_LS)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal
        /// to another instance.
        /// </summary>
        /// <param name="other">An instance to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public bool Equals(JET_LS other)
        {
            return this.Value.Equals(other.Value);
        }
    }

    /// <summary>
    /// Holds an index ID. An index ID is a hint that is used to accelerate the
    /// selection of the current index using JetSetCurrentIndex. It is most
    /// useful when there is a very large number of indexes over a table. The
    /// index ID can be retrieved using JetGetIndexInfo or JetGetTableIndexInfo.
    /// </summary>
    /// <remarks>
    /// The Pack attribute is necessary because the C++ version is defined as
    /// a byte array. If the C# compiler inserts the usual padding between the uint cbStruct
    ///  and the IntPtr, then the structure ends up too large.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct JET_INDEXID : IEquatable<JET_INDEXID>
    {
        /// <summary>
        /// Size of the structure.
        /// </summary>
        internal uint CbStruct;

        /// <summary>
        /// Internal use only.
        /// </summary>
        internal IntPtr IndexId1;

        /// <summary>
        /// Internal use only.
        /// </summary>
        internal uint IndexId2;

        /// <summary>
        /// Internal use only.
        /// </summary>
        internal uint IndexId3;

        /// <summary>
        /// The size of a JET_INDEXID structure.
        /// </summary>
        private static readonly uint TheSizeOfIndexId = (uint)Marshal.SizeOf(typeof(JET_INDEXID));

        /// <summary>
        /// Gets the size of a JET_INDEXINDEXID structure.
        /// </summary>
        internal static uint SizeOfIndexId
        {
            [DebuggerStepThrough]
            get { return TheSizeOfIndexId; }
        }

        /// <summary>
        /// Determines whether two specified instances of JET_INDEXID
        /// are equal.
        /// </summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the two instances are equal.</returns>
        public static bool operator ==(JET_INDEXID lhs, JET_INDEXID rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Determines whether two specified instances of JET_INDEXID
        /// are not equal.
        /// </summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the two instances are not equal.</returns>
        public static bool operator !=(JET_INDEXID lhs, JET_INDEXID rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal
        /// to another instance.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals((JET_INDEXID)obj);
        }

        /// <summary>
        /// Generate a string representation of the structure.
        /// </summary>
        /// <returns>The structure as a string.</returns>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "JET_INDEXID(0x{0:x}:0x{1:x}:0x{2:x})",
                this.IndexId1,
                this.IndexId2,
                this.IndexId3);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return this.CbStruct.GetHashCode()
                   ^ this.IndexId1.GetHashCode()
                   ^ this.IndexId2.GetHashCode()
                   ^ this.IndexId3.GetHashCode();
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal
        /// to another instance.
        /// </summary>
        /// <param name="other">An instance to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public bool Equals(JET_INDEXID other)
        {
            return this.CbStruct == other.CbStruct
                   && this.IndexId1 == other.IndexId1
                   && this.IndexId2 == other.IndexId2
                   && this.IndexId3 == other.IndexId3;
        }
    }
}
