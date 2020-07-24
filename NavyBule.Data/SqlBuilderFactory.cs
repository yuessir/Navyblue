using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Rhema.Core.Infrastructure;

namespace Rhema.Data
{
    internal class SqlBuilderFactory
    {
   
        private static readonly ISqlBuilder Oracle = new OracleBuilder(new Query());

        public static ISqlBuilder GetBuilder(IDbConnection conn)
        {
            string dbType = conn.GetType().Name;
            if (dbType.Equals("OracleConnection"))
            {
                return Oracle;
            }
            else
            {
                throw new Exception("Unknown DbType:" + dbType);
            }
     
        }

    }
}
