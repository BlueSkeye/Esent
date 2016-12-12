//-----------------------------------------------------------------------
// <copyright file="jet_sesparam.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace EsentLib.Jet
{
    /// <summary>ESENT session parameters.</summary>
    public enum JET_sesparam
    {
        /// <summary>This parameter is not meant to be used.</summary>
        Base = 4096,

        /// <summary>This parameter sets the grbits for commit. It is functionally the same
        /// as the system parameter JET_param.CommitDefault when used with an instance and a
        /// sesid. Note: JET_param.CommitDefault is not currently exposed in the ESE interop
        /// layer.</summary>
        CommitDefault = Base + 1,

        /// <summary>This parameter sets a user specific commit context that will be placed
        /// in the transaction log on commit to level 0.</summary>
        CommitGenericContext = Base + 2,

        //------------//
        // WINDOWS 10 //
        //------------//
        /// <summary>Gets the current number of nested levels of transactions begun. A value
        /// of zero indicates that the session is not currently in a transaction. This parameter
        /// is read-only.</summary>
        TransactionLevel = Base + 3,

        /// <summary>A client context of type <see cref="EsentLib.Jet.Windows10.JET_OPERATIONCONTEXT"/>
        /// that the engine uses to track and trace operations (such as IOs).</summary>
        OperationContext = Base + 4,

        /// <summary>A 32-bit integer ID that is logged in traces and can be used by clients
        /// to correlate ESE actions with their activity.</summary>
        CorrelationID = Base + 5
    }
}