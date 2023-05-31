using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;

namespace Ado.Entity.Core.MSSql
{
    public partial class Connection:IConnection
    {
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
                var propAttribute = property.GetCustomAttributes(typeof(Column), false).FirstOrDefault() as Column;
                string columnName = propAttribute != null ? propAttribute.Name : property.Name;
                var index = column.IndexOf(columnName);

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
                    else if (property.PropertyType.Name == "Guid")
                    {
                        Guid guid = Guid.Empty;
                        try
                        {
                            guid = new Guid(row[index].ToString());
                        }
                        catch
                        { }
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
