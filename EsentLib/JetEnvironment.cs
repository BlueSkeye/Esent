using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

using EsentLib.Implementation;
using EsentLib.Jet;
using EsentLib.Jet.Types;

namespace EsentLib
{
    /// <summary></summary>
    public static class JetEnvironment
    {
        /// <summary>Gets the true capabilities of this implementation of ESENT.</summary>
        public static JetCapabilities DefaultCapabilities
        {
            get
            {
                if (null == _defaultCapabilities) {
                    // JetGetVersion isn't available in new Windows UI, so we'll pretend it's always Win8:
                    _defaultCapabilities = DetermineCapabilities(8250 << 8);
                }
                return _defaultCapabilities;
            }
        }

        /// <summary>Gets the true capabilities of this implementation of ESENT.</summary>
        public static JetCapabilities TrueCapabilities
        {
            get
            {
                if (null == _trueCapabilities) { DetermineCapabilities(); }
                return _trueCapabilities;
            }
        }

        /// <summary>Calculates the capabilities of the current Esent version.</summary>
        internal static JetCapabilities DetermineCapabilities(uint versionOverride = 0)
        {
            const int Server2003BuildNumber = 2700;
            const int VistaBuildNumber = 6000;
            const int Windows7BuildNumber = 7000; // includes beta as well as RTM (RTM is 7600)
            const int Windows8BuildNumber = 8000; // includes beta as well as RTM (RTM is 9200)
            const int Windows81BuildNumber = 9300; // includes beta as well as RTM (RTM is 9600)
            const int Windows10BuildNumber = 9900; // includes beta as well as RTM (RTM is TBD)

            // Create new capabilities, set as all false. This will allow us to call into Esent.
            JetCapabilities result = new JetCapabilities { ColumnsKeyMost = 12 };

            uint version = versionOverride;
            if (0 == version) {
                version = GetInstalledVersion();
                _defaultCapabilities = result;
            }
            int buildNumber = (int)((version & 0xFFFFFF) >> 8);
            Tracing.TraceVerboseLine(string.Format(CultureInfo.InvariantCulture, "Version = {0}, BuildNumber = {1}", version, buildNumber));
            if (buildNumber >= Server2003BuildNumber) {
                Tracing.TraceVerboseLine("Supports Server 2003 features");
                result.SupportsServer2003Features = true;
            }
            if (buildNumber >= VistaBuildNumber) {
                Tracing.TraceVerboseLine("Supports Vista features");
                result.SupportsVistaFeatures = true;
                Tracing.TraceVerboseLine("Supports Unicode paths");
                result.SupportsUnicodePaths = true;
                Tracing.TraceVerboseLine("Supports large keys");
                result.SupportsLargeKeys = true;
                Tracing.TraceVerboseLine("Supports 16-column keys");
                result.ColumnsKeyMost = 16;
            }
            if (Windows7BuildNumber <= buildNumber) {
                Tracing.TraceVerboseLine("Supports Windows 7 features");
                result.SupportsWindows7Features = true;
            }
            if (Windows8BuildNumber <= buildNumber) {
                Tracing.TraceVerboseLine("Supports Windows 8 features");
                result.SupportsWindows8Features = true;
            }
            if (Windows81BuildNumber <= buildNumber) {
                Tracing.TraceVerboseLine("Supports Windows 8.1 features");
                result.SupportsWindows81Features = true;
            }
            if (Windows10BuildNumber <= buildNumber) {
                Tracing.TraceVerboseLine("Supports Windows 10 features");
                result.SupportsWindows10Features = true;
            }
            return result;
        }

        /// <summary>Frees memory that was allocated by a database engine call.</summary>
        /// <param name="buffer">The buffer allocated by a call to the database engine.
        /// <see cref="IntPtr.Zero"/> is acceptable, and will be ignored.</param>
        /// <returns>An error code.</returns>
        internal static void FreeJetBuffer(IntPtr buffer)
        {
            Tracing.TraceFunctionCall("FreeJetBuffer");
            int returnCode = NativeMethods.JetFreeBuffer(buffer);
            Tracing.TraceResult(returnCode);
        }

        /// <summary>Create an instance and get the current version of Esent.</summary>
        /// <returns>The current version of Esent.</returns>
        private static uint GetInstalledVersion()
        {
            // Create a unique name so that multiple threads can call this simultaneously.
            // This can happen if there are multiple AppDomains.
            string instanceName = string.Format(CultureInfo.InvariantCulture,
                "GettingEsentVersion{0}", LibraryHelpers.GetCurrentManagedThreadId());
            JET_INSTANCE instance = JET_INSTANCE.Nil;
            RuntimeHelpers.PrepareConstrainedRegions();
            using (JetInstance engine = JetInstance.Create(instanceName)) {
                engine.SetSystemParameter(JET_SESID.Nil, JET_param.Recovery, new IntPtr(0), "off");
                engine.SetSystemParameter(JET_SESID.Nil, JET_param.NoInformationEvent, new IntPtr(1), null);
                engine.SetSystemParameter(JET_SESID.Nil, JET_param.MaxTemporaryTables, new IntPtr(0), null);
                engine.SetSystemParameter(JET_SESID.Nil, JET_param.MaxCursors, new IntPtr(16), null);
                engine.SetSystemParameter(JET_SESID.Nil, JET_param.MaxOpenTables, new IntPtr(16), null);
                engine.SetSystemParameter(JET_SESID.Nil, JET_param.MaxVerPages, new IntPtr(4), null);
                engine.SetSystemParameter(JET_SESID.Nil, JET_param.MaxSessions, new IntPtr(1), null);
                engine.Initialize();
                using (JetSession session = engine.BeginSession(string.Empty, string.Empty)) {
                    return session.GetVersion();
                }
            }
        }

        /// <summary>Retrieves information about the instances that are running.</summary>
        /// <returns>An error code if the call fails.</returns>
        // TODO : Make this available through an interface in the JetEnironment
        public static JET_INSTANCE_INFO[] GetRunningInstancesInfo()
        {
            Tracing.TraceFunctionCall("GetRunningInstancesInfo");
            unsafe {
                uint nativeNumInstance = 0;

                // Esent will allocate memory which will be freed by the ConvertInstanceInfos call.
                NATIVE_INSTANCE_INFO* nativeInstanceInfos = null;
                int returnCode = NativeMethods.JetGetInstanceInfoW(out nativeNumInstance, out nativeInstanceInfos);
                try { return JET_INSTANCE_INFO.FromNativeCollection(nativeNumInstance, nativeInstanceInfos); }
                finally { Tracing.TraceResult(returnCode); }
            }
        }

        /// <summary>Set a system parameter which is a boolean.</summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="value">The value to set.</param>
        public static void SetParameter(JET_param param, bool value)
        {
            SetParameter(param, new IntPtr(value ? 1 : 0));
        }

        /// <summary>Set a system parameter which is a boolean.</summary>
        /// <param name="param">The parameter to set.</param>
        /// <param name="value">The value to set.</param>
        public static void SetParameter(JET_param param, int value)
        {
            SetParameter(param, new IntPtr(value));
        }

        /// <summary>Sets database configuration options.</summary>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="paramValue">The value of the parameter to set, if the parameter is an integer type.</param>
        /// <returns>An error or warning.</returns>
        public static void SetParameter(JET_param paramid, IntPtr paramValue)
        {
            NativeHelpers.SetParameter(JET_INSTANCE.Nil, JET_SESID.Nil, paramid, paramValue);
        }

        /// <summary>Sets database configuration options.</summary>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="paramString">The value of the parameter to set, if the parameter is a string type.</param>
        /// <returns>An error or warning.</returns>
        public static void SetParameter(JET_param paramid, string paramString)
        {
            NativeHelpers.SetParameter(JET_INSTANCE.Nil, JET_SESID.Nil, paramid, paramString);
        }

        /// <summary>Used when an unsupported API method is called. This logs an error and
        /// returns an InvalidOperationException.</summary>
        /// <param name="method">The name of the method.</param>
        /// <returns>The exception to throw.</returns>
        internal static Exception UnsupportedApiException(string method)
        {
            string error = string.Format(CultureInfo.InvariantCulture, "Method {0} is not supported by this version of ESENT", method);
            Tracing.TraceErrorLine(error);
            return new InvalidOperationException(error);
        }

        private static JetCapabilities _defaultCapabilities;
        private static JetCapabilities _trueCapabilities;
    }
}
