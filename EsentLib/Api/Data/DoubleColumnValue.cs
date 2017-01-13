//-----------------------------------------------------------------------
// <copyright file="DoubleColumnValue.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;

using EsentLib.Jet;
using EsentLib.Jet.Types;

namespace EsentLib.Api.Data
{
    /// <summary>A <see cref="double"/> column value.</summary>
    public class DoubleColumnValue : ColumnValueOfStruct<double>
    {
        /// <summary>Gets the size of the value in the column. This returns 0 for variable
        /// sized columns (i.e. binary and string).</summary>
        protected override int Size
        {
            [DebuggerStepThrough]
            get { return sizeof(double); }
        }

        /// <summary>Recursive SetColumns method for data pinning. This populates the buffer
        /// and calls the inherited SetColumns method.</summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to set the columns in. An update should be prepared.</param>
        /// <param name="columnValues">Column values to set.</param>
        /// <param name="nativeColumns">Structures to put the pinned data in.</param>
        /// <param name="i">Offset of this object in the array.</param>
        /// <returns>An error code.</returns>
        internal override unsafe int SetColumns(JET_SESID sesid, JET_TABLEID tableid,
            ColumnValue[] columnValues, NATIVE_SETCOLUMN* nativeColumns, int i)
        {
            var data = this.Value.GetValueOrDefault();
            return this.SetColumns(sesid, tableid, columnValues, nativeColumns, i, &data,
                sizeof(double), this.Value.HasValue);
        }

        /// <summary>Given data retrieved from ESENT, decode the data and set the value in
        /// the ColumnValue object.</summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within the bytes.</param>
        /// <param name="count">The number of bytes to decode.</param>
        /// <param name="err">The error returned from ESENT.</param>
        protected override void GetValueFromBytes(byte[] value, int startIndex, int count, int err)
        {
            if (JET_wrn.ColumnNull == (JET_wrn)err) {
                this.Value = null;
                return;
            }
            this.CheckDataCount(count);
            this.Value = BitConverter.ToDouble(value, startIndex);
        }
    }
}