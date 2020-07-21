using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using Rhema.Core.Domain;

namespace Rhema.Data
{

    /// <summary>
    /// Class ERPSessionFactory.
    /// </summary>
    public class ERPSessionFactory
    {

        private static ISapConn CreateConnection(IConfiguration configuration, ERPType dataType = ERPType.ECC6, string connStrKey = "")
        {
            SapConn conn;
            switch (dataType)
            {
                case ERPType.ECC6:
                    conn = new SapConn(connStrKey);
                    break;
                default:
                    conn = new SapConn(connStrKey);
                    break;
            }

            return conn;
        }



        public static ISapConn CreateSession(IConfiguration configuration, ERPType databaseType, string key)
        {
            var isDefined = Enum.IsDefined(typeof(ERPType), databaseType);
            if (!isDefined)
            {
                throw new NotSupportedException();
            }
            return CreateConnection(configuration, databaseType, key);

        }
    }
}
