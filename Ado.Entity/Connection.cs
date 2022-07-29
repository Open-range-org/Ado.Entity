using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

namespace Ado.Entity
{
    class Connection
    {
        private string ConnectionString;
        public Connection(string connectionString)
        {
            ConnectionString = connectionString;
        }
        public List<T> GetDataByQuery<T>(string queryFormat, string[] param = null)
        {
            List<T> dtoList = new List<T>();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                if (con.State != ConnectionState.Open)
                    con.Open();
                param = param ?? new string[] { };
                var queryString = string.Format(queryFormat, param);
                SqlCommand objSqlCommand = new SqlCommand(queryString, con);
                objSqlCommand.CommandType = CommandType.Text;

                try
                {
                    using (SqlDataReader reader = objSqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            T dto = GetObject<T>(reader);
                            dtoList.Add(dto);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return dtoList;
                }
            }
            return dtoList;
        }
        T GetObject<T>(SqlDataReader reader)
        {
            return (T)Activator.CreateInstance(typeof(T), reader);
        }
    }
}
