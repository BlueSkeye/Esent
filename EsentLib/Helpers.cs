using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsentLib
{
    internal static class Helpers
    {
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
