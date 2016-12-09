using System;

namespace EsentLib.Jet
{
    /// <summary>The default exception thrown by the JetEngine on error.</summary>
    public class JetEngineException : ApplicationException
    {
        internal JetEngineException(int errorCode, string message, params object[] args)
            : base(string.Format(message, args))
        {
            ErrorCode = errorCode;
            return;
        }

        /// <summary>Get the underlying native error code that triggered this exception.</summary>
        public int ErrorCode { get; private set; }
    }
}
