using System;

namespace EsentLib.Api
{
    /// <summary></summary>
    [CLSCompliant(false)]
    public interface IJetIndexColumn
    {
        /// <summary></summary>
        JET_COLUMNID Id { get; }

        /// <summary></summary>
        string Name { get; }
    }
}
