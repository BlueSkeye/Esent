//-----------------------------------------------------------------------
// <copyright file="JetCapabilities.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace EsentLib.Implementation
{
    /// <summary>Describes the functionality exposed by an object which implements IJetApi.</summary>
    public sealed class JetCapabilities : IJetCapabilities
    {
        /// <summary>Gets or sets a value indicating whether Windows Server 2003 features
        /// (in the Interop.Server2003 namespace) are supported.</summary>
        public bool SupportsServer2003Features { get; internal set; }

        /// <summary>Gets or sets a value indicating whether Vista features (in the
        /// Interop.Vista namespace) are supported.</summary>
        public bool SupportsVistaFeatures { get; internal set; }

        /// <summary>Gets or sets a value indicating whether Win7 features (in the
        /// Interop.Windows7 namespace) are supported.</summary>
        public bool SupportsWindows7Features { get; internal set; }

        /// <summary>Gets or sets a value indicating whether Win8 features (in the
        /// Interop.Windows8 namespace) are supported.</summary>
        public bool SupportsWindows8Features { get; internal set; }

        /// <summary>Gets or sets a value indicating whether Win8.1 features (in the
        /// Interop.Windows81 namespace) are supported.</summary>
        public bool SupportsWindows81Features { get; internal set; }

        /// <summary>Gets or sets a value indicating whether Win10 features (in the
        /// Interop.Windows10 namespace) are supported.</summary>
        public bool SupportsWindows10Features { get; internal set; }

        /// <summary>Gets or sets a value indicating whether unicode file paths are
        /// supported.</summary>
        public bool SupportsUnicodePaths { get; internal set; }

        /// <summary>Gets or sets a value indicating whether large (> 255 byte) keys are
        /// supported. The key size for an index can be specified in the
        /// <see cref="EsentLib.Jet.JET_INDEXCREATE"/> object.</summary>
        public bool SupportsLargeKeys { get; internal set; }

        /// <summary>Gets or sets the maximum number of components in a sort or index key.</summary>
        public int ColumnsKeyMost { get; internal set; }


        /// <summary>Check that ESENT supports Server 2003 features. Throws an exception if
        /// Server 2003 features aren't supported.</summary>
        /// <param name="api">The API that is being called.</param>
        internal void CheckSupportsServer2003Features(string api)
        {
            if (!SupportsServer2003Features) {
                throw JetEnvironment.UnsupportedApiException(api);
            }
        }

        /// <summary>Check that ESENT supports Vista features. Throws an exception if Vista
        /// features aren't supported.</summary>
        /// <param name="api">The API that is being called.</param>
        internal void CheckSupportsVistaFeatures(string api)
        {
            if (!SupportsVistaFeatures) {
                throw JetEnvironment.UnsupportedApiException(api);
            }
        }

        /// <summary>Check that ESENT supports Windows7 features. Throws an exception if
        /// Windows7 features aren't supported.</summary>
        /// <param name="api">The API that is being called.</param>
        internal void CheckSupportsWindows7Features(string api)
        {
            if (!SupportsWindows7Features) {
                throw JetEnvironment.UnsupportedApiException(api);
            }
        }

        /// <summary>Check that ESENT supports Windows8 features. Throws an exception if
        /// Windows8 features aren't supported.</summary>
        /// <param name="api">The API that is being called.</param>
        internal void CheckSupportsWindows8Features(string api)
        {
            if (!SupportsWindows8Features) {
                throw JetEnvironment.UnsupportedApiException(api);
            }
        }

        /// <summary>Check that ESENT supports Windows10 features. Throws an exception if
        /// Windows10 features aren't supported.</summary>
        /// <param name="api">The API that is being called.</param>
        internal void CheckSupportsWindows10Features(string api)
        {
            if (!SupportsWindows10Features) {
                throw JetEnvironment.UnsupportedApiException(api);
            }
        }
    }
}