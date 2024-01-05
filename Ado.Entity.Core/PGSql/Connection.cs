using System;
using System.Collections.Generic;
using Npgsql;

namespace Ado.Entity.Core.PGSql
{
    public partial class Connection : IConnection
    {
        private string _connectionString;
        private string _type = string.Empty;
        public Connection(string connectionString)
        {
            _connectionString = connectionString;
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

