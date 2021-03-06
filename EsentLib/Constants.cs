﻿namespace EsentLib
{
    internal static class Constants
    {
        /// <summary>The name of the DLL that the methods should be loaded from.</summary>
        internal const string EsentDll = "esent.dll";

        /// <summary>The length of the prefix used to name files used by the database engine.</summary>
        public const int BaseNameLength = 3;
        /// <summary>Maximum number of fixed columns allowed in a table.</summary>
        public const int ColumnsFixedMost = 127;
        /// <summary>Maximum size for columns which are not JET_coltyp.LongBinary or JET_coltyp.LongText.</summary>
        public const int ColumnMost = 255;
        /// <summary>Maximum number of columns allowed in a table.</summary>
        public const int ColumnsMost = 65248;
        /// <summary>Maximum number of tagged columns allowed in a table.</summary>
        public const int ColumnsTaggedMost = 64993;
        /// <summary>Maximum number of variable-length columns allowedin a table.</summary>
        public const int ColumnsVarMost = 128;
        /// <summary>The maximum length of a locale name (LOCALE_NAME_MAX_LENGTH from winnt.h).</summary>
        public const int LocaleNameMaxLength = 85;
        /// <summary>Maximum size of a table/column/index name.</summary>
        public const int NameMost = 64;
        /// <summary>The number of pages that gives the smallest possible temporary database.</summary>
        public const int PageTempDBSmallest = 14;
    }
}
