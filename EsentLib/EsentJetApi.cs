//-----------------------------------------------------------------------
// <copyright file="EsentJetApi.cs" company="Microsoft Corporation">
//  Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//  JetApi code that is specific to ESENT.
// </summary>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

using EsentLib.Jet;

namespace EsentLib.Implementation
{
    /// <summary>
    /// JetApi code that is specific to ESENT.
    /// </summary>
    internal sealed partial class JetEngine
    {
        /// <summary>
        /// Reports the exception to a central authority.
        /// </summary>
        /// <param name="exception">An unhandled exception.</param>
        /// <param name="description">A string description of the scenario.</param>
        internal static void ReportUnhandledException(
            Exception exception,
            string description)
        {
        }
    }
}
