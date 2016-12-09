//-----------------------------------------------------------------------
// <copyright file="Windows10NativeMethods.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

using EsentLib.Jet;
using EsentLib.Jet.Windows10;

namespace EsentLib.Implementation
{
    /// <summary>
    /// Native interop for Windows10 functions in ese.dll.
    /// </summary>
    internal static partial class NativeMethods
    {
        #region Sessions

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetSetSessionParameter(
            IntPtr sesid,
            uint sesparamid,
            ref NATIVE_OPERATIONCONTEXT data,
            int dataSize);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static extern int JetGetSessionParameter(
            IntPtr sesid,
            uint sesparamid,
            out NATIVE_OPERATIONCONTEXT data,
            int dataSize,
            out int actualDataSize);

        [DllImport(Constants.EsentDll, ExactSpelling = true)]
        public static unsafe extern int JetGetThreadStats(JET_THREADSTATS2* pvResult, uint cbMax);

        #endregion
    }
}
