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
            var attribute = typeof(T).GetCustomAttributes(true).Where(s => s.GetType() == typeof(Table)).FirstOrDefault() as Table;
            string tableName = attribute != null ? attribute.TableName : obj.GetType().Name;
            if (tableName != _type)
            {
                _type = tableName;
                var sqlSchema = GetDataByQuery<SqlSchema>($"select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '{tableName}'").OrderBy(s => s.Ordinal).ToList();
                LoadMetaData(sqlSchema);
            }
            Validation<T>();
            string queryString = BuildUpdateQueryString<T>(obj, tableName);
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
        private string BuildUpdateQueryString<T>(T obj, string tableName)
        {

            var primaryKey = typeof(T).GetProperties().Where(a => a.GetCustomAttributes(true).Where(s => s.GetType() == typeof(Primary)).Count() == 1).FirstOrDefault();
            var queryString = $"UPDATE {tableName} SET ";
            var properties = obj.GetType().GetProperties();

            string filterQuery = string.Empty;
            foreach (var property in properties)
            {
                var propAttribute = property.GetCustomAttributes(typeof(Column), false).FirstOrDefault() as Column;
                string columnName = propAttribute != null ? propAttribute.Name : property.Name;
                string columnType = _schimaDictionary[columnName] != null ? _schimaDictionary[columnName].DataType : "varchar";
                if (columnName == primaryKey.Name)
                {
                    if (columnType == "varchar" || columnType == "char" || columnType == "nchar" || columnType == "nvarchar" || columnType == "uniqueidentifier")
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
                    if (columnType == "varchar" || columnType == "char" || columnType == "nchar" || columnType == "nvarchar")
                    {
                        queryString += $"[{columnName}]='{property.GetValue(obj, null)}',";
                    }
                    else if (columnType.Contains("date"))
                    {
                        var date = Convert.ToDateTime(property.GetValue(obj, null));
                        string dateString = string.Empty;
                        DateTime updatedDate = date;
                        if (date.Year < 1753)
                        {
                            updatedDate = date.AddYears(1753 - date.Year);
                        }
                        if (columnType == "date")
                        {
                            dateString = $"'{updatedDate.ToString("YYYY-MM-DD")}',";
                        }
                        else if (columnType == "datetime")
                        {
                            dateString = $"'{updatedDate.ToString("yyyy-MM-dd HH:mm:ss.fff")}',";
                        }
                        else if (columnType == "datetime2")
                        {
                            dateString = $"'{updatedDate.ToString("YYYY-MM-DD hh:mm:ss.ffffff")}',";
                        }
                        else
                        {
                            dateString= $"'{updatedDate.ToString("YYYY-MM-DD hh:mm:ss")}',";
                        }
                        queryString += $"[{columnName}]={dateString}";
                    }
                    else if (columnType == "bit")
                    {
                        queryString += $"[{columnName}]={Convert.ToByte(property.GetValue(obj, null))},";
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
