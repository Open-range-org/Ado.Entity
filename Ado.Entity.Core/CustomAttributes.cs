using System;
using System.Collections.Generic;
using System.Text;

namespace Ado.Entity.Core
{
    public class Primary : Attribute
    {
    }
    public class Unique : Attribute
    {
    }
    public class Table : Attribute
    {
        public string TableName { get; set; }
        public Table(string tableName)
        {
            TableName = tableName;
        }
    }
    public class Column : Attribute
    {
        public string Name { get; set; }
        public Column(string name)
        {
            Name = name;
        }
    }
}
