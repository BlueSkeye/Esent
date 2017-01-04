//-----------------------------------------------------------------------
// <copyright file="ColumnValueOfStruct.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;

using EsentLib.Jet;

namespace EsentLib.Api.Data
{
    /// <summary>Set a column of a struct type (e.g. Int32/Guid).</summary>
    /// <typeparam name="T">Type to set.</typeparam>
    public abstract class ColumnValueOfStruct<T> : ColumnValue where T : struct, IEquatable<T>
    {
        /// <summary>Gets the byte length of a column value, which is zero if column is null,
        /// otherwise it matches the Size for this fixed-size column.</summary>
        public override int Length
        {
            get { return this.Value.HasValue ? this.Size : 0; }
        }

        /// <summary>Gets or sets the value in the struct.</summary>
        public T? Value
        {
            get { return this.internalValue; }
            set
            {
                this.internalValue = value;
                this.Error = (null == value) ? JET_wrn.ColumnNull : JET_wrn.Success;
            }
        }

        /// <summary>Gets the last set or retrieved value of the column. The value is returned
        /// as a generic object.</summary>
        public override object ValueAsObject
        {
            get
            {
                // Merged in the BoxedValueCache<T>::GetBoxedValue implementation and removed
                // the class.
                if (!this.Value.HasValue) { return null; }
                T valueToBox = this.Value.Value;
                int index = (valueToBox.GetHashCode() & 0x7fffffff) % NumCachedBoxedValues;
                object result = BoxedValues[index];
                if ((null == result) || !((T)result).Equals(valueToBox)) {
                    result = valueToBox;
                    BoxedValues[index] = result;
                }
                return result;
            }
        }

        /// <summary>Make sure the retrieved data is exactly the size needed for the structure.
        /// An exception is thrown if there is a mismatch.</summary>
        /// <param name="count">The size of the retrieved data.</param>
        protected void CheckDataCount(int count)
        {
            if (this.Size != count) { throw new EsentInvalidColumnException(); }
        }

        /// <summary>Gets a string representation of this object.</summary>
        /// <returns>A string representation of this object.</returns>
        public override string ToString()
        {
            return this.Value.ToString();
        }

        /// <summary>Cached boxed values.</summary>
        private static readonly object[] BoxedValues = new object[NumCachedBoxedValues];
        /// <summary>Number of boxed values to cache.</summary>
        private const int NumCachedBoxedValues = 257;
        /// <summary>Internal value.</summary>
        private T? internalValue;
    }
}
