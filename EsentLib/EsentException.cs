//-----------------------------------------------------------------------
// <copyright file="EsentException.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace EsentLib
{
    using System;
    using System.Runtime.Serialization;
#if !MANAGEDESENT_SUPPORTS_SERIALIZATION
    using EsentLib;
    using SerializableAttribute = EsentLib.SerializableAttribute;
#endif

    /// <summary>
    /// Base class for ESENT exceptions.
    /// </summary>
    [Serializable]
    public abstract class EsentException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the EsentException class.
        /// </summary>
        protected EsentException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the EsentException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        protected EsentException(string message) 
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the EsentException class. This constructor
        /// is used to deserialize a serialized exception.
        /// </summary>
        /// <param name="info">The data needed to deserialize the object.</param>
        /// <param name="context">The deserialization context.</param>
        protected EsentException(SerializationInfo info, StreamingContext context)
#if MANAGEDESENT_SUPPORTS_SERIALIZATION
                : base(info, context)
#endif
        {
        }
    }
}
