using System;
using System.Runtime;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;

namespace Ado.Entity.MSSql
{
    public partial class Connection:IConnection
    {

       
        /// <summary>
        /// Adds list of object in table
        /// </summary>
        /// <param name="objList">List of object which we want to add in table</param>
        /// <returns>Boolean value if transaction is successful</returns>
        public bool AddEntry<T>(List<T> objList)
        {
            try
            {
                foreach (T obj in objList)
                {
                    AddEntry<T>(obj);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return true;
        }
        /// <summary>
        /// Adds object in table
        /// </summary>
        /// <param name="obj">Object which we want to add in table</param>
        /// <returns>Boolean value if transaction is successful</returns>
        public bool AddEntry<T>(T obj)
        {
            var attribute = typeof(T).GetCustomAttributes(true).Where(s => s.GetType() == typeof(Table)).FirstOrDefault() as Table;
            string tableName = attribute != null ? attribute.TableName : obj.GetType().Name;
            if(tableName != _type)
            {
                _type = tableName;
                var sqlSchema = GetDataByQuery<SqlSchema>($"select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '{tableName}'").OrderBy(s => s.Ordinal).ToList();
                LoadMetaData(sqlSchema);
            }
            Validation<T>();
            string queryString = BuildInsertQueryString<T>(obj, tableName);
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
       
        private string BuildInsertQueryString<T>(T obj,string tableName)
        {

            
            string identityStart = $"SET IDENTITY_INSERT {tableName} ON ;";
            string identityEnd = $"SET IDENTITY_INSERT {tableName} OFF ;";
            string queryString = $"insert into {tableName} (";
            var properties = obj.GetType().GetProperties();
            foreach (var property in properties)
            {
                var propAttribute = property.GetCustomAttributes(typeof(Column), false).FirstOrDefault() as Column;
                string columnName = propAttribute != null ? propAttribute.Name : property.Name;
                queryString += $"[{columnName}],";
            }
            queryString = queryString.Remove(queryString.Length - 1, 1) + ") VALUES (";
            foreach (var property in properties)
            {
                var propAttribute = property.GetCustomAttributes(typeof(Column), false).FirstOrDefault() as Column;
                string columnName = propAttribute != null ? propAttribute.Name : property.Name;
                string columnType = _schimaDictionary[columnName] != null ? _schimaDictionary[columnName].DataType : "varchar";
                if (columnType == "varchar" || columnType == "char" || columnType == "nchar" || columnType == "nvarchar"||columnType == "uniqueidentifier")
                {
                    queryString += $"'{property.GetValue(obj, null)}',";
                }
                else if (columnType.Contains("date"))
                {
                    var date = Convert.ToDateTime(property.GetValue(obj, null));
                    DateTime updatedDate = date;
                    if (date.Year < 1753)
                    {
                        updatedDate = date.AddYears(1753 - date.Year);
                    }
                    if (columnType == "date")
                    {
                        queryString += $"'{updatedDate.ToString("YYYY-MM-DD")}',";
                    }
                    else if (columnType == "datetime")
                    {
                        queryString += $"'{updatedDate.ToString("yyyy-MM-dd HH:mm:ss.fff")}',";
                    }
                    else if (columnType == "datetime2")
                    {
                        queryString += $"'{updatedDate.ToString("YYYY-MM-DD hh:mm:ss.ffffff")}',";
                    }
                    else
                    {
                        queryString += $"'{updatedDate.ToString("YYYY-MM-DD hh:mm:ss")}',";
                    }
                }
                else if (columnType == "bit")
                {
                    queryString += $"{Convert.ToByte(property.GetValue(obj, null))},";
                }
                else
                {
                    queryString += $"{property.GetValue(obj, null)},";
                }
            }
            queryString = queryString.Remove(queryString.Length - 1, 1) + $");";
            string query = $"  IF (OBJECTPROPERTY(OBJECT_ID('{tableName}'), 'TableHasIdentity') > 0) {Environment.NewLine} BEGIN {Environment.NewLine}";
            query += $" {identityStart} {Environment.NewLine} {queryString} {Environment.NewLine} {identityEnd} {Environment.NewLine} END {Environment.NewLine}";
            query += $" ELSE {Environment.NewLine} BEGIN {Environment.NewLine} {queryString}{Environment.NewLine} END";
            return query;
        }


    }
}
