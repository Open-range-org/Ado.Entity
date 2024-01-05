using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data;
using Npgsql;
using System.Collections;
using System.Collections.Generic;

namespace Ado.Entity.Core.PGSql
{
    public partial class Connection : IConnection
    {

        public DataSet GetDataSetByQuery(string queryString)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var ds = new DataSet();
                    if (con.State != ConnectionState.Open)
                    using (NpgsqlCommand objSqlCommand = new NpgsqlCommand(queryString, con))
                    {
                        objSqlCommand.CommandType = CommandType.Text;
                            var adapter = new NpgsqlDataAdapter(objSqlCommand);
                            adapter.Fill(ds);
                            return ds;
                     }
                }
                catch (Exception ex)
                {
                    return null;
                }
                finally
                {
                    con.Close();
                }
            }
            return null;
        }

    }
    public static class SqlMap
    {

        public static List<T> MapDataWithModel<T>(this DataSet dataSet, int index = 0)
        {
            var table = dataSet.Tables[index];
            List<T> dtoList = new List<T>();
            try
            {
                dtoList.AddRange(Map<T>(table));

            }
            catch (Exception ex)
            {
                return dtoList;
            }
            return dtoList;
        }
        public static T MapSingleDataWithModel<T>(this DataSet dataSet,int index=0)
        {
            var table = dataSet.Tables[index];
            List<T> dtoList = new List<T>();
            try
            {
                dtoList.AddRange(Map<T>(table));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return dtoList.FirstOrDefault();
        }

        private static List<T> Map<T>(DataTable dataTable)
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
        private static T Execute<T>(DataColumnCollection column, DataRow row)
        {
            if (typeof(T).IsValueType)
            {
                return (T)Convert.ChangeType(row[0], typeof(T));
            }
            else
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
                            var val = ConvertObject(property.PropertyType.Name, row[index]);
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
        }
        private static object ConvertObject(string propType, object val)
        {
            object convertedValue = null;
            //if (prop.PropertyType.IsClass)
            //{
            //    convertedValue = Convert.ChangeType(val, Type.GetType(prop.PropertyType.Name));
            //}
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
                case "Int32[]":
                    if (val.ToString() == "") { convertedValue = Array.Empty<int>(); }
                    else { convertedValue = (int[])val; }
                    break;
                case "String[]":
                    if (val.ToString() == "") { convertedValue = Array.Empty<string>(); }
                    else { 
                        if(val.GetType().Name == "Object[][]")
                        {
                            IEnumerable enumerable = val as IEnumerable;
                            if(enumerable != null)
                            {
                                var list=new List<string>();
                                foreach (object element in enumerable)
                                {
                                    list.AddRange(((IEnumerable)element).Cast<object>()
                                     .Select(x => x.ToString()).ToList());
                                }
                                convertedValue = list.ToArray();
                            }
                        }
                        else
                        {
                            convertedValue = ((IEnumerable)val).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();
                        } 
                    }
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

