//-----------------------------------------------------------------------
// <copyright file="jet_colinfo.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace EsentLib.Jet
{
    /// <summary>Info levels for retrieving column info.</summary>
    internal enum JET_ColInfo
    {
        /// <summary>Default option. Retrieves a JET_COLUMNDEF.</summary>
        Default = 0,

        /// <summary>Retrieves a JET_COLUMNLIST structure, containing all the columns in the
        /// table.</summary>
        List = 1,

        /// <summary>Retrieves a JET_COLUMNBASE structure.</summary>
        Base = 4,

        /// <summary>Retrieves a JET_COLUMNDEF, the szColumnName argument is interpreted
        /// as a pointer to a columnid.</summary>
        ByColid = 6,

        // ----- //
        // VISTA //
        // ----- //
        /// <summary>Retrieve the JET_COLBASE using the column id.</summary>
        BaseByColid = 8,

        /// <summary>For lists, only return non-derived columns (if the table is derived from
        /// a template).</summary>
        GrbitNonDerivedColumnsOnly = int.MinValue, // 0x80000000,

        /// <summary>For lists, only return the column name and columnid of each column.
        /// </summary>
        GrbitMinimalInfo = 0x40000000,

        /// <summary>For lists, sort returned column list by columnid (default is to sort
        /// list by column name).</summary>
        GrbitSortByColumnid = 0x20000000,
    }
}
