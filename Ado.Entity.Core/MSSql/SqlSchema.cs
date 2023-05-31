using System;


namespace Ado.Entity.Core.MSSql
{
    [Table("COLUMNS")]
    internal class SqlSchema: AdoBase
    {
        [Primary]
        [Column("COLUMN_NAME")]
        public string ColumnName { get; set; }

        [Column("TABLE_NAME")]
        public string TableName { get; set; }

        [Column("IS_NULLABLE")]
        public string IsNullable { get; set; }

        [Column("DATA_TYPE")]
        public string DataType { get; set; }

        [Column("CHARACTER_MAXIMUM_LENGTH")]
        public string CharLength { get; set; }

        [Column("NUMERIC_PRECISION")]
        public string NumaricPrecision { get; set; }

        [Column("NUMERIC_SCALE")]
        public string NumaricScale { get; set; }
        [Column("ORDINAL_POSITION")]
        public string Ordinal { get; set; }
    }
}
