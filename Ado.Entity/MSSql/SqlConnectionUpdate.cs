using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;

namespace Ado.Entity
{
    public partial class Connection : IConnection
    {
        /// <summary>
        /// Updates list of object in table
        /// </summary>
        /// <param name="objList">List of object which we want to add in table</param>
        /// <returns>Boolean value if transaction is successful</returns>
        public bool UpdateEntry<T>(List<T> objList)
        {
            try
            {
                foreach (T obj in objList)
                {
                    UpdateEntry<T>(obj);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return true;
        }
        /// <summary>
        /// Updates object in table
        /// </summary>
        /// <param name="obj">List of object which we want to add in table</param>
        /// <returns>Boolean value if transaction is successful</returns>
        public bool UpdateEntry<T>(T obj)
        {
            Validation<T>();
            string queryString = BuildUpdateQueryString<T>(obj);
            SqlTransaction objTrans = null;
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                try
                {
                    objTrans = con.BeginTransaction();
                    SqlCommand objSqlCommand = new SqlCommand(queryString, con, objTrans);
                    objSqlCommand.ExecuteNonQuery();
                    objTrans.Commit();
                }
                catch (Exception ex)
                {
                    objTrans.Rollback();
                    throw new Exception(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
            return true;
        }
        private string BuildUpdateQueryString<T>(T obj)
        {

            var primaryKey = typeof(T).GetProperties().Where(a => a.GetCustomAttributes(true).Where(s => s.GetType() == typeof(Primary)).Count() == 1).FirstOrDefault();
            var attribute = typeof(T).GetCustomAttributes(true).Where(s => s.GetType() == typeof(Table)).FirstOrDefault() as Table;
            string tableName = attribute != null ? attribute.TableName : obj.GetType().Name;
            var queryString = $"UPDATE {tableName} SET ";
            var properties = obj.GetType().GetProperties();

            string filterQuery = string.Empty;
            foreach (var property in properties)
            {
                var propAttribute = property.GetCustomAttributes(typeof(Column), false).FirstOrDefault() as Column;
                string columnName = propAttribute != null ? propAttribute.Name : property.Name;
                if (columnName == primaryKey.Name)
                {
                    if (property.PropertyType.Name == "String" || property.PropertyType.Name == "Guid")
                    {
                        filterQuery += $"Where {columnName}='{property.GetValue(obj, null)}'";
                    }
                    else
                    {
                        filterQuery += $"Where {columnName}={property.GetValue(obj, null)}";
                    }
                }
                else
                {
                    if (property.PropertyType.Name == "String" || property.PropertyType.Name == "Type" || property.PropertyType.Name == "Guid" || property.PropertyType.Name == "DateTime")
                    {
                        queryString += $"[{columnName}]='{property.GetValue(obj, null)}',";
                    }
                    else if (property.PropertyType.Name == "Boolean")
                    {
                        queryString += $"[{columnName}]={Convert.ToByte(property.GetValue(obj, null))},";
                    }
                    else if (property.PropertyType.BaseType.Name == "Enum")
                    {
                        queryString += $"[{columnName}]='{property.GetValue(obj, null)}',";
                    }
                    else
                    {
                        queryString += $"[{columnName}]={property.GetValue(obj, null)},";
                    }
                }


            }
            queryString = queryString.Remove(queryString.Length - 1, 1) + $" {filterQuery}";
            return queryString;
        }

    }
}
