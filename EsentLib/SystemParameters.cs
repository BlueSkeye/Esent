//-----------------------------------------------------------------------
// <copyright file="SystemParameters.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using EsentLib.Jet;
using EsentLib.Implementation;
using EsentLib.Platform.Vista;
using EsentLib.Platform.Windows7;

namespace EsentLib
{
    /// <summary>This class provides static properties to set and get global ESENT system
    /// parameters.</summary>
    public static partial class SystemParameters
    {

        /// <summary>
        /// Get a system parameter which is a string.
        /// </summary>
        /// <param name="param">The parameter to get.</param>
        /// <returns>The value of the parameter.</returns>
        private static string GetStringParameter(JET_param param)
        {
            int ignored = 0;
            string value;
            Api.JetGetSystemParameter(EsentLib.Jet.Types.JET_INSTANCE.Nil, JET_SESID.Nil, param, ref ignored, out value, 1024);
            return value;
        }

        /// <summary>
        /// Get a system parameter which is an integer.
        /// </summary>
        /// <param name="param">The parameter to get.</param>
        /// <returns>The value of the parameter.</returns>
        private static int GetIntegerParameter(JET_param param)
        {
            int value = 0;
            string ignored;
            Api.JetGetSystemParameter(EsentLib.Jet.Types.JET_INSTANCE.Nil, JET_SESID.Nil, param, ref value, out ignored, 0);
            return value;
        }

        /// <summary>
        /// Get a system parameter which is a boolean.
        /// </summary>
        /// <param name="param">The parameter to get.</param>
        /// <returns>The value of the parameter.</returns>
        private static bool GetBoolParameter(JET_param param)
        {
            int value = 0;
            string ignored;
            Api.JetGetSystemParameter(EsentLib.Jet.Types.JET_INSTANCE.Nil, JET_SESID.Nil, param, ref value, out ignored, 0);
            return value != 0;
        }
    }
}
