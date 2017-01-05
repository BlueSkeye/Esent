using System;

using EsentLib.Jet;
using EsentLib.Jet.Types;

namespace EsentLib.Api.Flags
{
    /// <summary>Options for temporary table creation, with <see cref="IJetSession.OpenTemporaryTable"/>
    /// </summary>
    [Flags]
    public enum TemporaryTableCreationFlags
    {
        /// <summary>Default options.</summary>
        None = 0,

        /// <summary>This option requests that the temporary table be flexible enough to permit
        /// the use of JetSeek to lookup records by index key. If this  functionality it not
        /// required then it is best to not request it. If this functionality is not requested
        /// then the temporary table manager may be  able to choose a strategy for managing the
        /// temporary table that will result in improved performance.</summary>
        Indexed = 0x1,

        /// <summary>This option requests that records with duplicate index keys be removed 
        /// from the final set of records in the temporary table. Prior to Windows Server 2003,
        /// the database engine always assumed this option to be in effect due to the fact that
        /// all clustered indexes must also be a primary key and thus must be unique. As of
        /// Windows Server 2003, it is now possible to create a temporary table that does NOT 
        /// remove duplicates when the <see cref="TemporaryTableCreationFlags.ForwardOnly"/>option is also
        /// specified. It is not possible to know which duplicate will win and which duplicates 
        /// will be discarded in general. However, when the <see cref="ErrorOnDuplicateInsertion"/>
        /// option is requested then the first record with a given index key to be inserted
        /// into the temporary table will always win.</summary>
        Unique = 0x2,

        /// <summary>This option requests that the temporary table be flexible enough to allow
        /// records that have previously been inserted to be subsequently changed. If this
        /// functionality it not required then it is best to not request it. If this functionality
        /// is not requested then the temporary table manager may be able to choose a strategy
        /// for managing the temporary table that will result in improved performance.</summary>
        Updatable = 0x4,

        /// <summary>This option requests that the temporary table be flexible enough to allow
        /// records to be scanned in arbitrary order and direction using
        /// <see cref="LegacyApi.JetMove(JET_SESID,JET_TABLEID,int,MoveGrbit)"/>. If this
        /// functionality it not required then it is best to not request it. If this functionality
        /// is not requested then the temporary table manager may be able to choose a strategy
        /// for managing the  temporary table that will result in improved performance.</summary>
        Scrollable = 0x8,

        /// <summary>This option requests that NULL key column values sort closer to the end
        /// of the index than non-NULL key column values.</summary>
        SortNullsHigh = 0x10,

        /// <summary>This option forces the temporary table manager to abandon any attempt
        /// to choose a clever strategy for managing the temporary table that will result
        /// in enhanced performance.</summary>
        ForceMaterialization = 0x20,

        /// <summary>This option requests that any attempt to insert a record with the same 
        /// index key as a previously inserted record will immediately fail with <see cref="JET_err.KeyDuplicate"/>. If this option is not requested then a duplicate 
        /// may be detected immediately and fail or may be silently removed later depending
        /// on the strategy chosen by the database engine to implement the temporary table
        /// based on the requested functionality. If this functionality it not required then
        /// it is best to not request it. If this functionality is not requested then the
        /// temporary table manager may be able to choose a strategy for managing the temporary
        /// table that will result in improved performance.</summary>
        ErrorOnDuplicateInsertion = 0x20,

        /// <summary>This option requests that the temporary table only be created if thetemporary
        /// table manager can use the implementation optimized for intermediate query results. If
        /// any characteristic of the temporary table would prevent the use of this optimization
        /// then the operation will fail with JET_errCannotMaterializeForwardOnlySort. A side
        /// effect of this option is to allow the temporary table to contain records with duplicate
        /// index keys. See <see cref="TemporaryTableCreationFlags.Unique"/> for more information.</summary>
        ForwardOnly = 0x40,

        // --------- //
        // WINDOWS 7 //
        // --------- //
        /// <summary>Permit only intrinsic LV's (so materialisation is not required simply
        /// because a TT has an LV column).</summary>
        IntrinsicLVsOnly = 0x80,

        // --------- //
        // WINDOWS 8 //
        // --------- //
        /// <summary>This option requests that the temporary table sort columns of type
        /// JET_coltypGUID according to .Net Guid sort order.</summary>        
        TTDotNetGuid = 0x100,
    }
}
