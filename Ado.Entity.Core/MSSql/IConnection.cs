using System;
using System.Collections.Generic;
using System.Text;

namespace Ado.Entity.Core.MSSql
{
    public interface IConnection
    {
        List<T> GetDataByQuery<T>(string queryFormat, string[] param = null);
        List<T> GetDataByQuery<T>(string queryFormat);
        bool AddEntry<T>(T obj);
        bool UpdateEntry<T>(T obj);
        bool AddEntry<T>(List<T> objList);
        bool UpdateEntry<T>(List<T> objList);
    }
}
