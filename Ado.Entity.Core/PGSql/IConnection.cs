using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Ado.Entity.Core.PGSql
{
    public interface IConnection
    {
        DataSet GetDataSetByQuery(string queryString);
        bool AddEntryByModel<T>(List<T> models);
        bool AddEntryByModel<T>(T model);
        bool AddEntryByQuery(string queryString);
        bool UpdateEntryByQuery(string queryString);
    }
}
