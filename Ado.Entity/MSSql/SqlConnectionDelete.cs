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


        /// <summary>
        /// Deletes list of object in table
        /// </summary>
        /// <param name="objList">List of object which we want to add in table</param>
        /// <returns>Boolean value if transaction is successful</returns>
        public bool DeleteEntry<T>(List<T> objList)
        {
            try
            {
                foreach (T obj in objList)
                {
                    DeleteEntry<T>(obj);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return true;
        }
        /// <summary>
        /// Deletes object in table
        /// </summary>
        /// <param name="obj">Object which we want to add in table</param>
        /// <returns>Boolean value if transaction is successful</returns>
        public bool DeleteEntry<T>(T obj)
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
            string queryString = BuildDeleteQueryString<T>(obj, tableName);
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
        private string BuildDeleteQueryString<T>(T obj, string tableName)
        {
            var primaryKey = typeof(T).GetProperties().Where(a => a.GetCustomAttributes(true).Where(s => s.GetType() == typeof(Primary)).Count() == 1).FirstOrDefault();
            var propAttribute = primaryKey.GetCustomAttributes(typeof(Column), false).FirstOrDefault() as Column;
            string columnName = propAttribute != null ? propAttribute.Name : primaryKey.Name;
            string columnType = _schimaDictionary[columnName] != null ? _schimaDictionary[columnName].DataType : "varchar";

            string _key = string.Empty;
            if (columnType == "varchar" || columnType == "char" || columnType == "nchar" || columnType == "nvarchar")
            {
                _key = $"'{primaryKey.GetValue(obj, null)}'";
            }
            else
            {
                _key = $"{primaryKey.GetValue(obj, null)}";
            }

            string query = $"delete from {tableName} where [{columnName}]={_key}";
            
            return query;
        }
    }
}
