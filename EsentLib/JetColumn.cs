using System;
using EsentLib.Api;

using EsentLib.Implementation;
using EsentLib.Jet;

namespace EsentLib
{
    /// <summary></summary>
    [CLSCompliant(false)]
    public class JetColumn : IJetColumn
    {
        internal JetColumn(IJetTable owner, JET_COLUMNID id)
        {
            _owner = owner;
            _id = id;
        }

        /// <summary></summary>
        public uint Characteristics { get; private set; }

        /// <summary></summary>
        public ushort CodePage { get; private set; }

        /// <summary></summary>
        public string DefaultValue { get; private set; }

        /// <summary></summary>
        public JET_COLUMNID Id { get; private set; }

        /// <summary></summary>
        public int MaximumLength { get; private set; }

        /// <summary></summary>
        public string Name { get; private set; }

        /// <summary></summary>
        public IJetTable Owner { get; private set; }

        /// <summary></summary>
        public JET_coltyp Type { get; private set; }

        internal static JetColumn FromColumnList(IJetTable owner, JET_COLUMNLIST metadata,
            IJetCursor dataSource)
        {
            int columnId = dataSource.RetrieveColumnAsInt32(metadata.columnidcolumnname).Value;
            return new JetColumn(owner, new JET_COLUMNID(columnId)) {
                Name = dataSource.RetrieveColumnAsString(metadata.columnidcolumnname),
                CodePage = dataSource.RetrieveColumnAsUInt16(metadata.columnidCp) ?? 0,
                DefaultValue = dataSource.RetrieveColumnAsString(metadata.columnidDefault),
                Id = new JET_COLUMNID(dataSource.RetrieveColumnAsInt32(metadata.columnidcolumnid) ?? 0),
                MaximumLength = dataSource.RetrieveColumnAsInt32(metadata.columnidcbMax) ?? 0,
                Type = (JET_coltyp)dataSource.RetrieveColumnAsInt32(metadata.columnidcoltyp),
                Characteristics = dataSource.RetrieveColumnAsUInt32(metadata.columnidgrbit) ?? 0
            };
        }

        /// <summary>Provides a human readable desription of this column.</summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("#({0}) {1} : {2}(0x{3:X}), CP={4}, ML={5}, DEF='{6}'",
                Id.Value, Name, Type, Characteristics, CodePage, MaximumLength,
                DefaultValue ?? "<NULL>");
        }

        private JET_COLUMNID _id;
        private IJetTable _owner;
    }
}
