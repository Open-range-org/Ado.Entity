using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace Ado.Entity.Core.PGSql
{
    public partial class Connection : IConnection
    {

        public bool AddEntryByQuery(string queryString)
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
        public bool AddEntryByModel<T>(List<T> models)
        {
            Validation<T>();
            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                        models.ForEach(model =>
                        {
                            var queryString = BuildInserQuery<T>(model);
                            NpgsqlCommand objSqlCommand = new NpgsqlCommand(queryString, con);
                            objSqlCommand = AddParameters(objSqlCommand, model);
                            objSqlCommand.ExecuteNonQuery();
                        });
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
        public bool AddEntryByModel<T>(T model)
        {
            Validation<T>();
            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var queryString= BuildInserQuery<T>(model);
                    if (con.State != ConnectionState.Open)
                    {
                        NpgsqlCommand objSqlCommand = new NpgsqlCommand(queryString, con);
                        objSqlCommand = AddParameters(objSqlCommand,model);
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
        private NpgsqlCommand AddParameters<T>(NpgsqlCommand cmd, T model)
        {
            var modelType = model.GetType();
            var properties = modelType.GetProperties();
            foreach (var property in properties)
            {
                
                var propAttribute = property.GetCustomAttributes(typeof(Column), false).FirstOrDefault() as Column;
                string columnName = propAttribute != null ? propAttribute.Name : property.Name;
                object val = property.GetValue(model, null);
                if (property.PropertyType == typeof(string) && val==null)
                {
                    val = string.Empty;
                }
                var dataTypeAttribute = property.GetCustomAttributes(typeof(DataType), false).FirstOrDefault() as DataType;
                if(dataTypeAttribute != null)
                {
                    cmd.Parameters.AddWithValue(columnName, dataTypeAttribute.Value, val);
                }
                else
                {
                    cmd.Parameters.AddWithValue(columnName, val);
                }

            }
            return cmd;
        }
        private static string BuildInserQuery<T>(T model)
        {
            var modelType = model.GetType();

            var classAttribute = modelType.GetCustomAttributes(typeof(Table), false).FirstOrDefault() as Table;
            string tableName = classAttribute != null ? classAttribute.TableName : modelType.Name;
            string CombinedQuery = string.Empty, query2 = string.Empty, query3 = string.Empty;
            string query1 = $"INSERT INTO {tableName} (";
            var properties = modelType.GetProperties();
            foreach (var property in properties)
            {
                var propAttribute = property.GetCustomAttributes(typeof(Column), false).FirstOrDefault() as Column;
                var ignoreAttribute = property.GetCustomAttributes(typeof(Ignore), false).FirstOrDefault() as Ignore;
                bool isIgnored = ignoreAttribute != null;
                if (!isIgnored)
                {
                    string columnName = propAttribute != null ? propAttribute.Name : property.Name;
                    query2 += $"{columnName},";
                    query3 += $"@{columnName},";
                }
            }
            query2=query2.Remove(query2.Length - 1);
            query3 = query3.Remove(query3.Length - 1);
            CombinedQuery = $"{query1} {query2}) VALUES ({query3})";
            return CombinedQuery;
        }
    }
}

