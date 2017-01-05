using System;
using System.Globalization;

namespace EsentLib.Jet.Types
{
    /// <summary>A JET_DBID contains the handle to the database. A database handle is used to
    /// manage the schema of a database. It can also be used to manage the tables inside of
    /// that database.</summary>
    public struct JET_DBID : IEquatable<JET_DBID>, IFormattable
    {
        /// <summary>Gets a null JET_DBID.</summary>
        public static JET_DBID Nil
        {
            get { return new JET_DBID { Value = 0xffffffff }; }
        }

        /// <summary>Determines whether two specified instances of JET_DBID are equal.</summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the two instances are equal.</returns>
        public static bool operator ==(JET_DBID lhs, JET_DBID rhs)
        {
            return lhs.Value == rhs.Value;
        }

        /// <summary>Determines whether two specified instances of JET_DBID are not equal.</summary>
        /// <param name="lhs">The first instance to compare.</param>
        /// <param name="rhs">The second instance to compare.</param>
        /// <returns>True if the two instances are not equal.</returns>
        public static bool operator !=(JET_DBID lhs, JET_DBID rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>Generate a string representation of the structure.</summary>
        /// <returns>The structure as a string.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "JET_DBID({0})", this.Value);
        }

        /// <summary>Formats the value of the current instance using the specified format.</summary>
        /// <returns>A <see cref="T:System.String"/> containing the value of the current 
        /// instance in the specified format.</returns>
        /// <param name="format">The <see cref="T:System.String"/> specifying the format to
        /// use. -OR- null to use the default format defined for the type of the
        /// <see cref="T:System.IFormattable"/> implementation.</param>
        /// <param name="formatProvider">The <see cref="T:System.IFormatProvider"/> to use to
        /// format the value. -OR- null to obtain the numeric format information from the
        /// current locale setting of the operating system.</param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return (string.IsNullOrEmpty(format) || ("G" == format))
                ? this.ToString()
                : this.Value.ToString(format, formatProvider);
        }

        /// <summary>Returns a value indicating whether this instance is equal to another
        /// instance.</summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public override bool Equals(object obj)
        {
            return ((null != obj) && (GetType() == obj.GetType()) && this.Equals((JET_DBID)obj));
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
        public bool Equals(JET_DBID other)
        {
            return this.Value.Equals(other.Value);
        }

        /// <summary>The native value.</summary>
        internal uint Value;
    }
}
