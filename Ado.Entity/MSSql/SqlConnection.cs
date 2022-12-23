using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;

namespace Ado.Entity
{
    public partial class Connection:IConnection
    {
        private string ConnectionString;
        Dictionary<string, SqlSchema> _schimaDictionary = new Dictionary<string, SqlSchema>();
        private string _type = string.Empty;
        public Connection(string connectionString)
        {
            ConnectionString = connectionString;
        }
        private void Validation<T>()
        {
            if (typeof(T).BaseType != typeof(AdoBase))
            {
                throw new NotImplementedException("AdoBase class is not inharited in Entity");
            }
        }

        private void LoadMetaData(List<SqlSchema> schemaList)
        {
            schemaList.ForEach(s => {
                _schimaDictionary.Add(s.ColumnName, s);
            });
        }

    }
}
