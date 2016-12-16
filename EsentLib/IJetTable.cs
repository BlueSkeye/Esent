using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EsentLib.Jet;

namespace EsentLib
{
    /// <summary></summary>
    public interface IJetTable
    {
        /// <summary>Add a new column to an existing table.</summary>
        /// <param name="column">The name of the column.</param>
        /// <param name="columndef">The definition of the column.</param>
        /// <param name="defaultValue">The default value of the column.</param>
        /// <param name="defaultValueSize">The size of the default value.</param>
        IJetColumn AddColumn(string column, JET_COLUMNDEF columndef, byte[] defaultValue, int defaultValueSize);

        /// <summary>Close an open table.</summary>
        void Close();

        /// <summary>Walks each index of a table to exactly compute the number
        /// of entries in an index, and the number of distinct keys in an index.
        /// This information, together with the number of database pages allocated
        /// for an index and the current time of the computation is stored in index
        /// metadata in the database. This data can be subsequently retrieved with
        /// information operations.</summary>
        void ComputeStatistics();

        /// <summary>Deletes a column from a database table.</summary>
        /// <param name="column">The name of the column to be deleted.</param>
        /// <param name="grbit">Optional deletion flag.</param>
        void DeleteColumn(string column, DeleteColumnGrbit grbit = DeleteColumnGrbit.None);

        /// <summary>Deletes an index from a database table.</summary>
        /// <param name="index">The name of the index to be deleted.</param>
        void DeleteIndex(string index);
    }
}
