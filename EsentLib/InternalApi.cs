//-----------------------------------------------------------------------
// <copyright file="InternalApi.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;

using EsentLib.Jet;

namespace EsentLib
{
    /// <summary>Internal-only methods of the Api.</summary>
    public static partial class InternalApi
    {
        /// <summary>The JetSetColumn function modifies a single column value in a modified
        /// record to be inserted or to update the current record. It can overwrite an
        /// existing value, add a new value to a sequence of values in a multi-valued column,
        /// remove a value from a sequence of values in a multi-valued column, or update all
        /// or part of a long value (a column of type <see cref="JET_coltyp.LongText"/> or
        /// <see cref="JET_coltyp.LongBinary"/>). </summary>
        /// <remarks>This is an internal-only version of the API that takes a data buffer and
        /// an offset into the buffer.</remarks>
        /// <param name="sesid">The session which is performing the update.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        /// <param name="dataSize">The size of data to set.</param>
        /// <param name="dataOffset">The offset in the data buffer to set data from.</param>
        /// <param name="grbit">SetColumn options.</param>
        /// <param name="setinfo">Used to specify itag or long-value offset.</param>
        /// <returns>A warning value.</returns>
        public static JET_wrn JetSetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid,
            byte[] data, int dataSize, int dataOffset, SetColumnGrbit grbit, JET_SETINFO setinfo)
        {
            if (dataOffset < 0
                || (null != data && 0 != dataSize && dataOffset >= data.Length)
                || (null == data && dataOffset != 0))
            {
                throw new ArgumentOutOfRangeException("dataOffset", dataOffset, "must be inside the data buffer");                    
            }
            if (null != data && dataSize > checked(data.Length - dataOffset) && (SetColumnGrbit.SizeLV != (grbit & SetColumnGrbit.SizeLV)))
            {
                throw new ArgumentOutOfRangeException("dataSize", dataSize,
                    "cannot be greater than the length of the data (unless the SizeLV option is used)");
            }
            unsafe {
                fixed (byte* pointer = data) {
                    return InternalApi.JetSetColumn(sesid, tableid, columnid,
                        new IntPtr(pointer + dataOffset), dataSize, grbit, setinfo);
                }
            }
        }

        /// <summary>The JetSetColumn function modifies a single column value in a modified
        /// record to be inserted or to update the current record. It can overwrite an
        /// existing value, add a new value to a sequence of values in a multi-valued column,
        /// remove a value from a sequence of values in a multi-valued column, or update all
        /// or part of a long value, a column of type JET_coltyp.LongText or JET_coltyp.LongBinary.
        /// </summary>
        /// <remarks>This method takes an IntPtr and is intended for internal use only.</remarks>
        /// <param name="sesid">The session which is performing the update.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        /// <param name="dataSize">The size of data to set.</param>
        /// <param name="grbit">SetColumn options.</param>
        /// <param name="setinfo">Used to specify itag or long-value offset.</param>
        /// <returns>A warning value.</returns>
        internal static JET_wrn JetSetColumn(JET_SESID sesid, JET_TABLEID tableid,
            JET_COLUMNID columnid, IntPtr data, int dataSize, SetColumnGrbit grbit,
            JET_SETINFO setinfo)
        {
            return EsentExceptionHelper.Check(EsentLib.LegacyApi.Impl.JetSetColumn(sesid, tableid, columnid, data,
                dataSize, grbit, setinfo));
        }
    }
}