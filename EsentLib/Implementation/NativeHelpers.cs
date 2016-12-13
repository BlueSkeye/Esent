using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using EsentLib.Jet;
using EsentLib.Jet.Types;

namespace EsentLib.Implementation
{
    internal static class NativeHelpers
    {
        /// <summary>Get a system parameter which is a boolean.</summary>
        /// <param name="instance"></param>
        /// <param name="param">The parameter to get.</param>
        /// <returns>The value of the parameter.</returns>
        internal static bool GetBoolParameter(JET_INSTANCE instance, JET_param param)
        {
            IntPtr rawValue = new IntPtr();
            string trashString;
            GetParameter(instance, JET_SESID.Nil, param, ref rawValue, out trashString, 0);
            return (0 != rawValue.ToInt32());
        }

        /// <summary>Get a system parameter which is an integer.</summary>
        /// <param name="param">The parameter to get.</param>
        /// <returns>The value of the parameter.</returns>
        internal static int GetInt32Parameter(JET_param param)
        {
            return GetInt32Parameter(JET_INSTANCE.Nil, param);
        }

        /// <summary>Get a system parameter which is an integer.</summary>
        /// <param name="instance"></param>
        /// <param name="param">The parameter to get.</param>
        /// <returns>The value of the parameter.</returns>
        internal static int GetInt32Parameter(JET_INSTANCE instance, JET_param param)
        {
            IntPtr rawValue = new IntPtr();
            string trashString;
            GetParameter(instance, JET_SESID.Nil, param, ref rawValue, out trashString, 0);
            return rawValue.ToInt32();
        }

        /// <summary>Get a system parameter which is an IntPtr.</summary>
        /// <param name="instance"></param>
        /// <param name="param">The parameter to get.</param>
        /// <returns>The value of the parameter.</returns>
        internal static IntPtr GetIntPtrParameter(JET_INSTANCE instance, JET_param param)
        {
            IntPtr value = IntPtr.Zero;
            string ignored;
            Api.JetGetSystemParameter(instance, JET_SESID.Nil, param, ref value, out ignored, 0);
            return value;
        }

        /// <summary>Gets database configuration options.</summary>
        /// <param name="instance">The instance to retrieve the options from.</param>
        /// <param name="sesid">The session to use.</param>
        /// <param name="paramid">The parameter to get.</param>
        /// <param name="paramValue">Returns the value of the parameter, if the value is an
        /// integer.</param>
        /// <param name="paramString">Returns the value of the parameter, if the value is a
        /// string.</param>
        /// <param name="maxParam">The maximum size of the parameter string.</param>
        /// <returns>An ESENT warning code.</returns>
        /// <remarks><see cref="JET_param.ErrorToString"/> passes in the error number in the
        /// paramValue, which is why it is a ref parameter and not an out parameter.</remarks>
        /// <returns>An error or warning.</returns>
        private static void GetParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid,
            ref IntPtr paramValue, out string paramString, int maxParam)
        {
            Tracing.TraceFunctionCall("GetParameter");
            Helpers.CheckNotNegative(maxParam, "maxParam");
            uint bytesMax = checked((uint)(maxParam * sizeof(char)));
            var sb = new StringBuilder(maxParam);
            int err = NativeMethods.JetGetSystemParameterW(instance.Value, sesid.Value,
                (uint)paramid, ref paramValue, sb, bytesMax);
            Tracing.TraceResult(err);
            paramString = sb.ToString();
            paramString = StringCache.TryToIntern(paramString);
            Tracing.TraceResult(err);
            EsentExceptionHelper.Check(err);
        }

        /// <summary>Get a system parameter which is a string.</summary>
        /// <param name="instance"></param>
        /// <param name="param">The parameter to get.</param>
        /// <returns>The value of the parameter.</returns>
        internal static string GetStringParameter(JET_INSTANCE instance, JET_param param)
        {
            IntPtr ignored = IntPtr.Zero;
            string value;
            GetParameter(instance, JET_SESID.Nil, param, ref ignored, out value, 1024);
            return value;
        }

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
        internal static void SetParameter(JET_INSTANCE instance, JET_param paramid, int paramValue)
        {
            SetParameter(instance, JET_SESID.Nil, paramid, new IntPtr(paramValue));
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
            unsafe {
                int returnCode = NativeMethods.JetSetSystemParameterW(&instance.Value,
                    sesid.Value, (uint)paramid, IntPtr.Zero, paramString);
                Tracing.TraceResult(returnCode);
                EsentExceptionHelper.Check(returnCode);
            }
        }

        private static readonly IntPtr OneForBoolean = new IntPtr(1);
    }
}
