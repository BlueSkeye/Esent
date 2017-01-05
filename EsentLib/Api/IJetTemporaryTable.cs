using System;
using System.Collections.Generic;

namespace EsentLib.Api
{
    /// <summary></summary>
    /// <typeparam name="T"></typeparam>
    public interface IJetTemporaryTable<T> : IEnumerable<T>, IDisposable
    {
    }
}
