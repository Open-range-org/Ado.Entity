using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;

namespace Ado.Entity
{
    public class Connection:IConnection
    {
        private string ConnectionString;
        public Connection(string connectionString)
        {
            ConnectionString = connectionString;
        }
        private void Validation<T>()
        {
            if(typeof(T).BaseType != typeof(AdoBase))
            {
                throw new NotImplementedException("AdoBase class is not inharited in Entity");
            }
        }

        /// <summary>
        /// Returns data based on the query string
        /// </summary>
        /// <param name="queryFormat">Query string to get data from table</param>
        /// <returns>List of Object based od the query string</returns>
       public List<T> GetDataByQuery<T>(string queryFormat)
        {
            return GetDataByQuery<T>(queryFormat, null);
        }
        
        /// <summary>
        /// Returns data based on the query string
        /// </summary>
        /// <param name="queryFormat">Query string to get data from table</param>
        /// <param name="param">Optional parameter to pass for query string.</param>
        /// <returns>List of Object based od the query string</returns>
        public List<T> GetDataByQuery<T>(string queryFormat, string[] param = null)
        {
            Validation<T>();
            List<T> dtoList = new List<T>();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                try
                {
                    if (con.State != ConnectionState.Open)
                        con.Open();
                    param = param ?? new string[] { };
                    var queryString = string.Format(queryFormat, param);
                    SqlCommand objSqlCommand = new SqlCommand(queryString, con);
                    objSqlCommand.CommandType = CommandType.Text;
                    using (SqlDataReader reader = objSqlCommand.ExecuteReader())
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(reader);
                        dtoList.AddRange(Map<T>(dataTable));
                    }
                }
                catch (Exception ex)
                {
                    return dtoList;
                }
                finally
                {
                    con.Close();
                }
            }
            return dtoList;
        }
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
                var propAttribute = property.GetCustomAttributes(typeof(Column),false).FirstOrDefault() as Column;
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
                        queryString += $"[{columnName}]={property.GetValue(obj,null)},";
                    }
                }


            }
            queryString = queryString.Remove(queryString.Length - 1, 1) + $" {filterQuery}";
            return queryString;
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
                    queryString += $"'{updatedDate}',";
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
        private T GetObject<T>(SqlDataReader reader)
        {
            return (T)Activator.CreateInstance(typeof(T), reader);
        }
        private List<T> Map<T>(DataTable dataTable)
        {

            List<T> _objList = new List<T>();
            foreach (DataRow row in dataTable.Rows)
            {
                T _obj = Execute<T>(dataTable.Columns, row);
                if (_obj != null)
                {
                    _objList.Add(_obj);
                }
            }
            return _objList;
        }
        private T Execute<T>(DataColumnCollection column, DataRow row)
        {

            var _object = (T)Activator.CreateInstance(typeof(T));
            var properties = _object.GetType().GetProperties();
            foreach (var property in properties)
            {
                var x = property.GetCustomAttributes(true).Count() > 0 ? property.GetCustomAttributes(true)[0].GetType().Name : null;
                var index = column.IndexOf(property.Name);
                if (index != -1)
                {
                    if (property.PropertyType.IsClass || property.PropertyType.IsPrimitive)
                    {
                        var val = ConvertObject(property, row[index]);
                        property.SetValue(_object, val, null);
                    }
                    else if (property.PropertyType.IsEnum)
                    {
                        var val = Enum.Parse(property.PropertyType, row[index].ToString(), true);
                        property.SetValue(_object, val, null);
                    }
                    else if (property.PropertyType.Name=="Guid")
                    {
                        Guid guid = Guid.Empty;
                        try
                        {
                            guid = new Guid(row[index].ToString());
                        }
                        catch
                        {}
                        property.SetValue(_object, guid, null);
                    }
                    else
                    {
                        try
                        {
                            property.SetValue(_object, row[index], null);
                        }
                        catch
                        {
                            property.SetValue(_object, default(T), null);
                        }
                    }
                }
            }
            return _object;
        }
        private object ConvertObject(PropertyInfo prop, object val)
        {
            object convertedValue = null;
            //if (prop.PropertyType.IsClass)
            //{
            //    convertedValue = Convert.ChangeType(val, Type.GetType(prop.PropertyType.Name));
            //}
            var propType = prop.PropertyType.Name;
            switch (propType)
            {
                case nameof(TypeCode.String):
                    convertedValue = val.ToString();
                    break;
                case nameof(TypeCode.Int16):
                    short s;
                    short.TryParse(val.ToString(), out s);
                    convertedValue = s;
                    break;
                case nameof(TypeCode.Int32):
                    int i;
                    int.TryParse(val.ToString(), out i);
                    convertedValue = i;
                    break;
                case nameof(TypeCode.Int64):
                    long l;
                    long.TryParse(val.ToString(), out l);
                    convertedValue = l;
                    break;
                case nameof(TypeCode.Boolean):
                    bool b;
                    Boolean.TryParse(val.ToString(), out b);
                    convertedValue = b;
                    break;
                case nameof(TypeCode.Decimal):
                    Decimal dc;
                    Decimal.TryParse(val.ToString(), out dc);
                    convertedValue = dc;
                    break;
                case nameof(TypeCode.Double):
                    Double db;
                    Double.TryParse(val.ToString(), out db);
                    convertedValue = db;
                    break;
                case nameof(TypeCode.Byte):
                    Byte by;
                    Byte.TryParse(val.ToString(), out by);
                    convertedValue = by;
                    break;
                case nameof(TypeCode.Single):
                    float f;
                    float.TryParse(val.ToString(), out f);
                    convertedValue = f;
                    break;
                case nameof(TypeCode.DateTime):
                    DateTime d;
                    DateTime.TryParse(val.ToString(), out d);
                    convertedValue = d;
                    break;
                case nameof(System.Guid):
                    convertedValue = new Guid(val.ToString());
                    break;
                case nameof(System.Type):
                    convertedValue = Type.GetType(val.ToString());
                    break;

                default:
                    convertedValue = val.ToString();
                    break;
            }
            return convertedValue;
        }


    }
}
