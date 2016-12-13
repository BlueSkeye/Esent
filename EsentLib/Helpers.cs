using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsentLib
{
    internal static class Helpers
    {
        /// <summary>Make sure the data and dataSize arguments match.</summary>
        /// <param name="data">The data buffer.</param>
        /// <param name="dataSize">The size of the data.</param>
        /// <param name="argumentName">The name of the size argument.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        internal static void CheckDataSize<T>(ICollection<T> data, int dataSize, string argumentName)
        {
            CheckDataSize(data, 0, string.Empty, dataSize, argumentName);
        }

        /// <summary>Make sure the data, dataOffset and dataSize arguments match.</summary>
        /// <param name="data">The data buffer.</param>
        /// <param name="dataOffset">The offset into the data.</param>
        /// <param name="offsetArgumentName">The name of the offset argument.</param>
        /// <param name="dataSize">The size of the data.</param>
        /// <param name="sizeArgumentName">The name of the size argument.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        internal static void CheckDataSize<T>(ICollection<T> data, int dataSize,
            string sizeArgumentName, int dataOffset = 0, string offsetArgumentName = "")
        {
            Helpers.CheckNotNegative(dataSize, sizeArgumentName);
            Helpers.CheckNotNegative(dataOffset, offsetArgumentName);
            if ((null == data && 0 != dataOffset) || (null != data && dataOffset > data.Count)) {
                Tracing.TraceErrorLine("CheckDataSize failed");
                throw new ArgumentOutOfRangeException(offsetArgumentName, dataOffset,
                    "cannot be greater than the length of the buffer");
            }
            if ((null == data && 0 != dataSize) || (null != data && dataSize > data.Count - dataOffset)) {
                Tracing.TraceErrorLine("CheckDataSize failed");
                throw new ArgumentOutOfRangeException(sizeArgumentName, dataSize,
                    "cannot be greater than the length of the buffer");
            }
        }

        /// <summary>Make sure the given integer isn't negative. If it is then throw an
        /// ArgumentOutOfRangeException.</summary>
        /// <param name="i">The integer to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        internal static void CheckNotNegative(int i, string paramName)
        {
            if (0 <= i) { return; }
            Tracing.TraceErrorLine("CheckNotNegative failed");
            throw new ArgumentOutOfRangeException(paramName, i, "cannot be negative");
        }

        /// <summary>Make sure the given object isn't null. If it is then throw an ArgumentNullException.</summary>
        /// <param name="o">The object to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        internal static void CheckNotNull(object o, string paramName)
        {
            if (null == o) {
                Tracing.TraceErrorLine(string.Format("CheckNotNull failed for '{0}'", paramName));
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
