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
            Validation<T>();
            string queryString = BuildInsertQueryString<T>(obj);
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

       
        private string BuildInsertQueryString<T>(T obj)
        {

            var attribute = typeof(T).GetCustomAttributes(true).Where(s => s.GetType() == typeof(Table)).FirstOrDefault() as Table;
            string tableName = attribute != null ? attribute.TableName : obj.GetType().Name;
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
                if (property.PropertyType.Name == "String" || property.PropertyType.Name == "Type" || property.PropertyType.Name == "Guid")
                {
                    queryString += $"'{property.GetValue(obj, null)}',";
                }
                else if (property.PropertyType.Name == "DateTime")
                {
                    var date = Convert.ToDateTime(property.GetValue(obj, null));
                    DateTime updatedDate = date;
                    if (date.Year < 1753)
                    {
                        updatedDate = date.AddYears(1753 - date.Year);
                    }
                    queryString += $"'{updatedDate.ToString("YYYY-MM-DD hh:mm:ss")}',";
                }
                else if (property.PropertyType.Name == "Boolean")
                {
                    queryString += $"{Convert.ToByte(property.GetValue(obj, null))},";
                }
                else if (property.PropertyType.BaseType.Name == "Enum")
                {
                    queryString += $"'{property.GetValue(obj, null)}',";
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
