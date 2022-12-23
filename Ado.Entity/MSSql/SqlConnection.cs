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

    }
}
