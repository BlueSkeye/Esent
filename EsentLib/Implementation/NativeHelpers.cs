using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EsentLib.Jet;
using EsentLib.Jet.Types;

namespace EsentLib.Implementation
{
    internal class NativeHelpers
    {
        /// <summary>Sets database configuration options.</summary>
        /// <param name="instance"></param>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="flag">The value of the parameter to set, if the parameter is an integer type.</param>
        /// <returns>An error or warning.</returns>
        internal static void SetParameter(JET_INSTANCE instance, JET_param paramid, bool flag)
        {
            SetParameter(instance, JET_SESID.Nil, paramid, flag ? OneForBoolean : IntPtr.Zero);
        }

        /// <summary>Sets database configuration options.</summary>
        /// <param name="instance"></param>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="paramValue">The value of the parameter to set, if the parameter is an integer type.</param>
        /// <returns>An error or warning.</returns>
        internal static void SetParameter(JET_INSTANCE instance, JET_param paramid, IntPtr paramValue)
        {
            SetParameter(instance, JET_SESID.Nil, paramid, paramValue);
        }

        /// <summary>Sets database configuration options.</summary>
        /// <param name="instance"></param>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="paramString">The value of the parameter to set, if the parameter is a string type.</param>
        /// <returns>An error or warning.</returns>
        internal static void SetParameter(JET_INSTANCE instance, JET_param paramid, string paramString)
        {
            SetParameter(instance, JET_SESID.Nil, paramid, paramString);
        }

        /// <summary>Sets database configuration options.</summary>
        /// <param name="instance"></param>
        /// <param name="sesid">The session to use.</param>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="paramValue">The value of the parameter to set, if the parameter is an integer type.</param>
        /// <returns>An error or warning.</returns>
        internal static void SetParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid,
            IntPtr paramValue)
        {
            Tracing.TraceFunctionCall("SetParameter(uint)");
            unsafe {
                int returnCode = NativeMethods.JetSetSystemParameterW(&instance.Value,
                    sesid.Value, (uint)paramid, paramValue, null);
                Tracing.TraceResult(returnCode);
                EsentExceptionHelper.Check(returnCode);
            }
        }

        /// <summary>Sets database configuration options.</summary>
        /// <param name="instance"></param>
        /// <param name="sesid">The session to use.</param>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="paramString">The value of the parameter to set, if the parameter is a string type.</param>
        /// <returns>An error or warning.</returns>
        internal static void SetParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid,
            string paramString)
        {
            Tracing.TraceFunctionCall("SetParameter(string)");
            unsafe
            {
                int returnCode = NativeMethods.JetSetSystemParameterW(&instance.Value,
                    sesid.Value, (uint)paramid, IntPtr.Zero, paramString);
                Tracing.TraceResult(returnCode);
                EsentExceptionHelper.Check(returnCode);
            }
        }

        private static readonly IntPtr OneForBoolean = new IntPtr(1);
    }
}
