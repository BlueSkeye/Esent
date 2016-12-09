using System;
using System.Diagnostics;
using System.Globalization;

namespace EsentLib.Jet.Types
{
    /// <summary>A JET_INSTANCE contains a handle to the instance of the database to use
    /// for calls to the JET Api.</summary>
    public struct JET_INSTANCE : IEquatable<JET_INSTANCE>, IFormattable
    {
        /// <summary>The native value.</summary>
        internal IntPtr Value;

        /// <summary>Gets a null JET_INSTANCE.</summary>
        public static JET_INSTANCE Nil
        {
            [DebuggerStepThrough]
            get { return new JET_INSTANCE(); }
        }

        /// <summary>Gets a value indicating whether the <see cref="T:JET_INSTANCE"/> is
        /// valid (checks against 0 and -1).</summary>
        public bool IsInvalid
        {
            get { return this.IsNil || (new IntPtr(~0) == this.Value); }
        }

        /// <summary>Gets a value indicating if the <see cref="T:JET_INSTANCE"/> is
        /// a null handle.</summary>
        public bool IsNil
        {
            get { return (IntPtr.Zero == this.Value); }
        }

        /// <summary>Determines whether two specified instances of JET_INSTANCE are equal.
        /// </summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the two instances are equal.</returns>
        public static bool operator ==(JET_INSTANCE lhs, JET_INSTANCE rhs)
        {
            return lhs.Value == rhs.Value;
        }

        /// <summary>Determines whether two specified instances of JET_INSTANCE are not
        /// equal.</summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the two instances are not equal.</returns>
        public static bool operator !=(JET_INSTANCE lhs, JET_INSTANCE rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>Generate a string representation of the structure.</summary>
        /// <returns>The structure as a string.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "JET_INSTANCE(0x{0:x})",
                this.Value.ToInt64());
        }

        /// <summary>Formats the value of the current instance using the specified format.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> containing the value of the current
        /// instance in the specified format.</returns>
        /// <param name="format">The <see cref="T:System.String"/> specifying the format
        /// to use. -or- null to use the default format defined for the type of the
        /// <see cref="T:System.IFormattable"/> implementation. </param>
        /// <param name="formatProvider">The <see cref="T:System.IFormatProvider"/> to use
        /// to format the value. -or-  null to obtain the numeric format information from
        /// the current locale setting of the operating system. </param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return (string.IsNullOrEmpty(format) || ("G" == format))
                ? this.ToString()
                : this.Value.ToInt64().ToString(format, formatProvider);
        }

        /// <summary>Returns a value indicating whether this instance is equal to another
        /// instance.</summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public override bool Equals(object obj)
        {
            return ((null != obj) && (GetType() != obj.GetType()) && this.Equals((JET_INSTANCE)obj));
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
        public bool Equals(JET_INSTANCE other)
        {
            return this.Value.Equals(other.Value);
        }
    }
}
