using System.Collections;
using System.Collections.Generic;

using EsentLib.Api;

namespace EsentLib
{
    /// <summary></summary>
    /// <typeparam name="T"></typeparam>
    public class JetTemporaryTable<T> : IJetTemporaryTable<T>
    {
        internal JetTemporaryTable(IJetCursor cursor, IEnumerable<T> enumerable)
        {
            _cursor = cursor;
            _enumerable = enumerable;
        }

        /// <summary></summary>
        public void Dispose()
        {
            _cursor.Close();
        }

        /// <summary></summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }

        /// <summary></summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private IEnumerable<T> _enumerable;
        private IJetCursor _cursor;
    }
}
