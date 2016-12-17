using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsentLib
{
    /// <summary></summary>
    public interface IJetCapabilities
    {
        /// <summary>Gets or sets a value indicating whether Windows Server 2003 features
        /// (in the Interop.Server2003 namespace) are supported.</summary>
        bool SupportsServer2003Features { get; }

        /// <summary>Gets or sets a value indicating whether Vista features (in the
        /// Interop.Vista namespace) are supported.</summary>
        bool SupportsVistaFeatures { get; }

        /// <summary>Gets or sets a value indicating whether Win7 features (in the
        /// Interop.Windows7 namespace) are supported.</summary>
        bool SupportsWindows7Features { get; }

        /// <summary>Gets or sets a value indicating whether Win8 features (in the
        /// Interop.Windows8 namespace) are supported.</summary>
        bool SupportsWindows8Features { get; }

        /// <summary>Gets or sets a value indicating whether Win8.1 features (in the
        /// Interop.Windows81 namespace) are supported.</summary>
        bool SupportsWindows81Features { get; }

        /// <summary>Gets or sets a value indicating whether Win10 features (in the
        /// Interop.Windows10 namespace) are supported.</summary>
        bool SupportsWindows10Features { get; }

        /// <summary>Gets or sets a value indicating whether unicode file paths are
        /// supported.</summary>
        bool SupportsUnicodePaths { get; }

        /// <summary>Gets or sets a value indicating whether large (> 255 byte) keys are
        /// supported. The key size for an index can be specified in the
        /// <see cref="EsentLib.Jet.JET_INDEXCREATE"/> object.</summary>
        bool SupportsLargeKeys { get; }

        /// <summary>Gets or sets the maximum number of components in a sort or index key.</summary>
        int ColumnsKeyMost { get; }
    }
}
