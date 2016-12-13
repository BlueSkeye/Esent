//-----------------------------------------------------------------------
// <copyright file="InstanceParameters.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;

using EsentLib.Jet;
using EsentLib.Jet.Types;
using EsentLib.Platform.Vista;
using EsentLib.Platform.Windows7;
using EsentLib.Platform.Windows2003;

namespace EsentLib.Platform.Windows8
{
    /// <summary>This class provides properties to set and get system parameters on an ESENT
    /// instance.</summary>
    public partial class InstanceParameters
    {
        /// <summary>The instance to set parameters on.</summary>
        private readonly JET_INSTANCE instance;

        /// <summary>The session to set parameters with.</summary>
        private readonly JET_SESID sesid;

        /// <summary>Initializes a new instance of the InstanceParameters class.</summary>
        /// <param name="instance">The instance to set parameters on. If this is JET_INSTANCE.Nil,
        /// then the settings affect the default settings of future instances.</param>
        public InstanceParameters(JET_INSTANCE instance)
        {
            this.instance = instance;
            this.sesid = JET_SESID.Nil;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="InstanceParameters"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="InstanceParameters"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "InstanceParameters (0x{0:x})", this.instance.Value);
        }

        ///// <summary>
        ///// Set a system parameter which is a string.
        ///// </summary>
        ///// <param name="param">The parameter to set.</param>
        ///// <param name="value">The value to set.</param>
        //private void SetStringParameter(JET_param param, string value)
        //{
        //    Api.JetSetSystemParameter(this.instance, this.sesid, param, 0, value);
        //}

        ///// <summary>Get a system parameter which is a string.</summary>
        ///// <param name="param">The parameter to get.</param>
        ///// <returns>The value of the parameter.</returns>
        //private string GetStringParameter(JET_param param)
        //{
        //    int ignored = 0;
        //    string value;
        //    Api.JetGetSystemParameter(this.instance, this.sesid, param, ref ignored, out value, 1024);
        //    return value;
        //}

        ///// <summary>
        ///// Set a system parameter which is an integer.
        ///// </summary>
        ///// <param name="param">The parameter to set.</param>
        ///// <param name="value">The value to set.</param>
        //private void SetIntegerParameter(JET_param param, int value)
        //{
        //    Api.JetSetSystemParameter(this.instance, this.sesid, param, value, null);
        //}

        /// <summary>
        /// Get a system parameter which is an integer.
        /// </summary>
        /// <param name="param">The parameter to get.</param>
        /// <returns>The value of the parameter.</returns>
        private int GetIntegerParameter(JET_param param)
        {
            int value = 0;
            string ignored;
            Api.JetGetSystemParameter(this.instance, this.sesid, param, ref value, out ignored, 0);
            return value;
        }

        ///// <summary>
        ///// Set a system parameter which is a boolean.
        ///// </summary>
        ///// <param name="param">The parameter to set.</param>
        ///// <param name="value">The value to set.</param>
        //private void SetBoolParameter(JET_param param, bool value)
        //{
        //    if (value)
        //    {
        //        Api.JetSetSystemParameter(this.instance, this.sesid, param, 1, null);
        //    }
        //    else
        //    {
        //        Api.JetSetSystemParameter(this.instance, this.sesid, param, 0, null);
        //    }
        //}

        /// <summary>Get a system parameter which is a boolean.</summary>
        /// <param name="param">The parameter to get.</param>
        /// <returns>The value of the parameter.</returns>
        private bool GetBoolParameter(JET_param param)
        {
            int value = 0;
            string ignored;
            Api.JetGetSystemParameter(this.instance, this.sesid, param, ref value, out ignored, 0);
            return value != 0;
        }
    }
}