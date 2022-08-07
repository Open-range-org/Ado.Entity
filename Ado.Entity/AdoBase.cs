using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Ado.Entity
{
    public class AdoBase
    {
        public AdoBase()
        {
            var properties = this.GetType().GetProperties();
            var primaryCount = properties.SelectMany(p => p.GetCustomAttributes(true)).Where(a => a.GetType() == typeof(Primary)).Count();
            if (primaryCount > 1)
            {
                throw new InvalidFilterCriteriaException("Only one primary attribute acceptable");
            }
        }
    }
}
