//-----------------------------------------------------------------------
// <copyright file="Update.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Text;

using EsentLib.Api;
using EsentLib.Implementation;
using EsentLib.Jet;

namespace EsentLib
{
    /// <summary>A class that encapsulates an update on a JET_TABLEID.</summary>
    [CLSCompliant(false)]
    public class JetCursor : EsentResource, ICursor
    {
        /// <summary>Initializes a new instance of the Update class. This automatically begins
        /// an update. The update will be cancelled if not explicitly saved.</summary>
        /// <param name="session">The session to start the transaction for.</param>
        /// <param name="table">The tableid to prepare the update for.</param>
        /// <param name="prep">The type of update.</param>
        internal JetCursor(IJetSession session, JetTable table, JET_prep prep)
        {
            if (JET_prep.Cancel == prep) {
                throw new ArgumentException("Cannot create an Update for JET_prep.Cancel", "prep");
            }
            this.sesid = session;
            this.table = table;
            this.prep = prep;
            ResourceWasAllocated();
        }

        /// <summary>Get the name of the current index of a given cursor.</summary>
        /// <remarks>This accessor is also used to later re-select that index as the current
        /// index. It can also be used to discover the properties of that index using
        /// JetGetTableIndexInfo. The returned name of the index will be an empty string if
        /// the current index is the clustered index and no primary index was explicitly
        /// defined.</remarks>
        public string CurrentIndex
        {
            get
            {
                Tracing.TraceFunctionCall("CurrentIndex");
                int maxNameLength = Constants.NameMost + 1;
                StringBuilder name = new StringBuilder(maxNameLength);
                int returnCode = NativeMethods.JetGetCurrentIndex(this.sesid.Id, this.table.Id,
                    name, checked((uint)maxNameLength));
                Tracing.TraceResult(returnCode);
                EsentExceptionHelper.Check(returnCode);
                string indexName = name.ToString();
                return StringCache.TryToIntern(indexName);
            }
        }

        /// <summary>Cancel the update.</summary>
        public void Cancel()
        {
            this.CheckObjectIsNotDisposed();
            if (!this.HasResource) { throw new InvalidOperationException("Not in an update"); }
            Tracing.TraceFunctionCall("Cancel");
            int returnCode = NativeMethods.JetPrepareUpdate(sesid.Id, this.table.Id,
                unchecked((uint)JET_prep.Cancel));
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            this.ResourceWasReleased();
        }

        /// <summary>Positions a cursor to an index entry for the record that is associated
        /// with the specified bookmark. The bookmark can be used with any index defined over
        /// a table. The bookmark for a record can be retrieved using
        /// <see cref="IJetInstance.JetGetBookmark"/>.</summary>
        /// <param name="bookmark">The bookmark used to position the cursor.</param>
        /// <param name="bookmarkSize">The size of the bookmark.</param>
        public void GotoBookmark(byte[] bookmark, int bookmarkSize)
        {
            Tracing.TraceFunctionCall("GotoBookmark");
            Helpers.CheckNotNull(bookmark, "bookmark");
            Helpers.CheckDataSize(bookmark, bookmarkSize, "bookmarkSize");
            int returnCode = NativeMethods.JetGotoBookmark(this.sesid.Id, this.table.Id,
                bookmark, checked((uint)bookmarkSize));
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return;
        }

        /// <summary>Called when the transaction is being disposed while active. This should
        /// rollback the transaction.</summary>
        protected override void ReleaseResource()
        {
            this.Cancel();
        }

        /// <summary>Update the tableid.</summary>
        /// <param name="bookmark">Returns the bookmark of the updated record. This can be null.</param>
        /// <param name="bookmarkSize">The size of the bookmark buffer.</param>
        /// <returns>Returns the actual size of the bookmark.</returns>
        /// <remarks>Save is the final step in performing an insert or an update. The update is
        /// begun by calling creating an Update object and then by calling JetSetColumn or
        /// JetSetColumns one or more times to set the record state. Finally, Update is called
        /// to complete the update operation. Indexes are updated only by Update or and not
        /// during JetSetColumn or JetSetColumns.</remarks>
        public int Save(byte[] bookmark = null, int bookmarkSize = 0)
        {
            this.CheckObjectIsNotDisposed();
            if (!this.HasResource) { throw new InvalidOperationException("Not in an update"); }
            try { return this.table.Update(this.sesid, bookmark); }
            finally { this.ResourceWasReleased(); }
        }

        /// <summary>Update the tableid and position the tableid on the record that was modified.
        /// This can be useful when inserting a record because by default the tableid remains
        /// in its old location.</summary>
        /// <remarks>Save is the final step in performing an insert or an update. The update
        /// is begun by calling creating an Update object and then by calling JetSetColumn or
        /// JetSetColumns one or more times to set the record state. Finally, Update is called
        /// to complete the update operation. Indexes are updated only by Update or and not
        /// during JetSetColumn or JetSetColumns.</remarks>
        public void SaveAndGotoBookmark()
        {
            byte[] bookmark = null;
            try {
                bookmark = MemoryCache.BookmarkCache.Allocate();
                int actualBookmarkSize = Save(bookmark, bookmark.Length);
                GotoBookmark(bookmark, actualBookmarkSize);
            }
            finally { if (bookmark != null) { MemoryCache.BookmarkCache.Free(ref bookmark); } }
        }

        /// <summary>Set the current index of a cursor.</summary>
        /// <param name="index">The name of the index to be selected. If this is null or empty
        /// the primary index will be selected.</param>
        /// <param name="grbit">Set index options.</param>
        /// <param name="itagSequence">Sequence number of the multi-valued column value which
        /// will be used to position the cursor on the new index. This parameter is only used
        /// in conjunction with <see cref="SetCurrentIndexGrbit.NoMove"/>. When this parameter
        /// is not present or is set to zero, its value is presumed to be 1.</param>
        /// <returns>An error if the call fails.</returns>
        public void SetCurrentIndex(string index, SetCurrentIndexGrbit grbit = SetCurrentIndexGrbit.None,
            int itagSequence = 1)
        {
            Tracing.TraceFunctionCall("SetCurrentIndex");
            int returnCode = (SetCurrentIndexGrbit.None == grbit)
                ? NativeMethods.JetSetCurrentIndex(sesid.Id, table.Id, index)
                : (1 == itagSequence)
                    ? NativeMethods.JetSetCurrentIndex2(sesid.Id, table.Id, index, (uint)grbit)
                    : NativeMethods.JetSetCurrentIndex3(sesid.Id, table.Id, index, (uint)grbit, checked((uint)itagSequence));
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return;
        }

        /// <summary>Set the current index of a cursor.</summary>
        /// <param name="indexid">The id of the index to select. This id can be obtained using
        /// JetGetIndexInfo or JetGetTableIndexInfo with the <see cref="JET_IdxInfo.IndexId"/>
        /// option.</param>
        /// <param name="grbit">Set index options.</param>
        /// <param name="itagSequence">Sequence number of the multi-valued column value which
        /// will be used to position the cursor on the new index. This parameter is only used
        /// in conjunction with <see cref="SetCurrentIndexGrbit.NoMove"/>. When this parameter
        /// is not present or is set to zero, its value is presumed to be 1.</param>
        /// <returns>An error if the call fails.</returns>
        public void SetCurrentIndex(JET_INDEXID indexid, SetCurrentIndexGrbit grbit = SetCurrentIndexGrbit.None,
            int itagSequence = 1)
        {
            Tracing.TraceFunctionCall("SetCurrentIndex");
            // A null index name is valid here -- it will set the table to the primary index
            int returnCode = NativeMethods.JetSetCurrentIndex4(sesid.Id, table.Id, null, ref indexid,
                (uint)grbit, checked((uint)itagSequence));
            Tracing.TraceResult(returnCode);
            EsentExceptionHelper.Check(returnCode);
            return;
        }

        /// <summary>Returns a <see cref="T:System.String"/> that represents the current
        /// <see cref="JetCursor"/>.</summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current
        /// <see cref="JetCursor"/>.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Update ({0})", this.prep);
        }

        /// <summary>The JetUpdate function performs an update operation including inserting
        /// a new row into a table or updating an existing row. Deleting a table row is
        /// performed by calling<see cref="IJetInstance.JetDelete"/>.</summary>
        /// <param name="bookmark">Returns the bookmark of the updated record. This can be null.
        /// </param>
        /// <param name="bookmarkSize">The size of the bookmark buffer.</param>
        /// <param name="actualBookmarkSize">Returns the actual size of the bookmark.</param>
        /// <remarks>JetUpdate is the final step in performing an insert or an update. The
        /// update is begun by calling <see cref="IJetTable.PrepareUpdate"/> and then by
        /// calling JetSetColumn one or more times to set the record state. Finally, JetUpdate
        /// is called to complete the update operation. Indexes are updated only by JetUpdate
        /// or and not during JetSetColumn.</remarks>
        /// <returns>An error if the call fails.</returns>
        public int Update(byte[] bookmark, int bookmarkSize, out int actualBookmarkSize)
        {
            Tracing.TraceFunctionCall("JetUpdate");
            Helpers.CheckDataSize(bookmark, bookmarkSize, "bookmarkSize");
            uint bytesActual;
            int returnCode = NativeMethods.JetUpdate(sesid.Id, table.Id, bookmark,
                checked((uint)bookmarkSize), out bytesActual);
            Tracing.TraceResult(returnCode);
            actualBookmarkSize = Helpers.GetActualSize(bytesActual);
            return returnCode;
        }

        /// <summary>The underlying JET_SESID.</summary>
        private readonly IJetSession sesid;
        /// <summary>The underlying JET_TABLEID.</summary>
        private readonly JetTable table;
        /// <summary>The type of update.</summary>
        private readonly JET_prep prep;
    }
}
