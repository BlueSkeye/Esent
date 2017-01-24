using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EsentLib.Api;
using EsentLib.Jet;

namespace EsentLib.Implementation
{
    internal class JetIndex : IJetIndex
    {
        internal JetIndex(IJetTable owner)
        {
            Owner = owner;
        }

        /// <summary></summary>
        public List<IJetIndexColumn> Columns { get; private set; }

        /// <summary></summary>
        public uint EntriesCount { get; private set; }

        /// <summary></summary>
        public uint KeyCount { get; private set; }

        /// <summary></summary>
        public string Name { get; private set; }

        /// <summary></summary>
        public IJetTable Owner { get; private set; }

        internal void AddColumn(Column column)
        {
            Columns.Add(column);
        }

        internal static JetIndex FromIndexList(IJetTable owner, JET_INDEXLIST metadata, JetCursor dataSource,
            out uint columnsCount)
        {
            columnsCount = dataSource.RetrieveColumnAsUInt32(metadata.columnidcColumn).Value;
            return new JetIndex(owner) {
                Columns = new List<IJetIndexColumn>(),
                EntriesCount = dataSource.RetrieveColumnAsUInt32(metadata.columnidcEntry).Value,
                KeyCount = dataSource.RetrieveColumnAsUInt32(metadata.columnidcKey).Value,
                Name = dataSource.RetrieveColumnAsString(metadata.columnidindexname),

                //CodePage = dataSource.RetrieveColumnAsUInt16(metadata.columnidCp) ?? 0,
                //DefaultValue = dataSource.RetrieveColumnAsString(metadata.columnidDefault),
                //Id = new JET_COLUMNID(dataSource.RetrieveColumnAsUInt32(metadata.columnidcolumnid) ?? 0),
                //MaximumLength = dataSource.RetrieveColumnAsInt32(metadata.columnidcbMax) ?? 0,
                //Type = (JET_coltyp)dataSource.RetrieveColumnAsInt32(metadata.columnidcoltyp),
                //Characteristics = dataSource.RetrieveColumnAsUInt32(metadata.columnidgrbit) ?? 0
            };
        }

        internal class Column : IJetIndexColumn
        {
            private Column()
            {
                return;
            }

            /// <summary></summary>
            public JET_COLUMNID Id { get; private set; }

            /// <summary></summary>
            public string Name { get; private set; }

            internal static JetIndex.Column FromIndexList(JetIndex owner, JET_INDEXLIST metadata,
                JetCursor dataSource)
            {
                Column result = new Column() {
                    Id = new JET_COLUMNID(dataSource.RetrieveColumnAsInt32(metadata.columnidcolumnid).Value),
                    Name = dataSource.RetrieveColumnAsString(metadata.columnidcolumnname),

                    //CodePage = dataSource.RetrieveColumnAsUInt16(metadata.columnidCp) ?? 0,
                    //DefaultValue = dataSource.RetrieveColumnAsString(metadata.columnidDefault),
                    //Id = new JET_COLUMNID(dataSource.RetrieveColumnAsUInt32(metadata.columnidcolumnid) ?? 0),
                    //MaximumLength = dataSource.RetrieveColumnAsInt32(metadata.columnidcbMax) ?? 0,
                    //Type = (JET_coltyp)dataSource.RetrieveColumnAsInt32(metadata.columnidcoltyp),
                    //Characteristics = dataSource.RetrieveColumnAsUInt32(metadata.columnidgrbit) ?? 0
                };
                owner.AddColumn(result);
                return result;
            }
        }
    }
}
