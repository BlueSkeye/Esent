//-----------------------------------------------------------------------
// <copyright file="Windows10JetApi.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using EsentLib.Jet;
using EsentLib.Jet.Windows10;
using EsentLib.Jet.Windows8;

namespace EsentLib.Implementation
{
    /// <summary>Windows10 calls to the ESENT interop layer. These calls take the managed
    /// types (e.g. JET_SESID) and return errors.</summary>
    internal sealed partial class JetInstance : IJetApi
    {
        #region Session Parameters
        /// <summary>
        /// Sets a parameter on the provided session state, used for the lifetime of this session or until reset.
        /// </summary>
        /// <param name="sesid">The session to set the parameter on.</param>
        /// <param name="sesparamid">The ID of the session parameter to set.</param>
        /// <param name="operationContext">An operation context to set.</param>
        /// <returns>An error code.</returns>
        public int JetGetSessionParameter(
            JET_SESID sesid,
            JET_sesparam sesparamid,
            out JET_OPERATIONCONTEXT operationContext)
        {
            TraceFunctionCall("JetGetSessionParameter");
            this.CheckSupportsWindows10Features("JetGetSessionParameter");
            int err;
            int actualDataSize;

            NATIVE_OPERATIONCONTEXT nativeContext = new NATIVE_OPERATIONCONTEXT();
            int dataSize = Marshal.SizeOf(nativeContext);

            err = EsentLib.Implementation.NativeMethods.JetGetSessionParameter(
                sesid.Value,
                (uint)sesparamid,
                out nativeContext,
                dataSize,
                out actualDataSize);

            if (err >= (int)JET_err.Success)
            {
                if (actualDataSize != dataSize)
                {
                    throw new ArgumentException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Bad return value. Unexpected data size returned. Expected {0}, but received {1}.",
                            dataSize,
                            actualDataSize),
                        "sesparamid");
                }
            }

            operationContext = new JET_OPERATIONCONTEXT(ref nativeContext);

            return TraceResult(err);
        }

        /// <summary>
        /// Sets a parameter on the provided session state, used for the lifetime of this session or until reset.
        /// </summary>
        /// <param name="sesid">The session to set the parameter on.</param>
        /// <param name="sesparamid">The ID of the session parameter to set.</param>
        /// <param name="operationContext">An operation context to set.</param>
        /// <returns>An error code.</returns>
        public int JetSetSessionParameter(
            JET_SESID sesid,
            JET_sesparam sesparamid,
            JET_OPERATIONCONTEXT operationContext)
        {
            TraceFunctionCall("JetSetSessionParameter");
            this.CheckSupportsWindows10Features("JetSetSessionParameter");
            int err;

            NATIVE_OPERATIONCONTEXT nativeContext = operationContext.GetNativeOperationContext();
            int dataSize = Marshal.SizeOf(nativeContext);

            err = EsentLib.Implementation.NativeMethods.JetSetSessionParameter(
                sesid.Value,
                (uint)sesparamid,
                ref nativeContext,
                checked((int)dataSize));

            return TraceResult(err);
        }
        #endregion

        #region Sessions

        /// <summary>
        /// Retrieves performance information from the database engine for the
        /// current thread. Multiple calls can be used to collect statistics
        /// that reflect the activity of the database engine on this thread
        /// between those calls. 
        /// </summary>
        /// <param name="threadstats">
        /// Returns the thread statistics..
        /// </param>
        /// <returns>An error code if the operation fails.</returns>
        public int JetGetThreadStats(out JET_THREADSTATS2 threadstats)
        {
            TraceFunctionCall("JetGetThreadStats");
            this.CheckSupportsVistaFeatures("JetGetThreadStats");

            // To speed up the interop we use unsafe code to avoid initializing
            // the out parameter. We just call the interop code.
            unsafe
            {
                fixed (JET_THREADSTATS2* rawJetThreadstats = &threadstats)
                {
                    return TraceResult(NativeMethods.JetGetThreadStats(rawJetThreadstats, checked((uint)JET_THREADSTATS2.Size)));
                }
            }
        }

        #endregion
    }
}
