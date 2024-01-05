using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data;
using Npgsql;

namespace Ado.Entity.Core.PGSql
{
    public partial class Connection : IConnection
    {

        public bool UpdateEntryByQuery(string queryString)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var ds = new DataSet();
                    if (con.State != ConnectionState.Open)
                    using (NpgsqlCommand objSqlCommand = new NpgsqlCommand(queryString, con))
                    {
                            con.Open();
                            objSqlCommand.ExecuteNonQuery();
                            con.Close();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally
                {
                    con.Close();
                }
            }
        }

    }
}

