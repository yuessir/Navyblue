using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using Rhema.Core.Domain;

namespace Rhema.Data
{


    /// <summary>
    /// Class DbSessionFactory.
    /// </summary>
    public class DbSessionFactory
    {
        //todo refactor
        private static IDbConnection CreateConnection(IConfiguration configuration, DatabaseType dataType = DatabaseType.Oracle, string connStrKey = "")
        {
            IDbConnection conn;
            switch (dataType)
            {

                case DatabaseType.Oracle:
                    conn = new OracleConnection(GetConnectionString(connStrKey, configuration));
                    break;
                case DatabaseType.Mssql:
                    conn = new SqlConnection(GetConnectionString(connStrKey, configuration));
                    break;
                
                default:
                    conn = new OracleConnection(GetConnectionString(connStrKey, configuration));
                    break;
            }

            try
            {
                conn.Open();

                
            }
            catch (Exception e)
            {
          
                throw e;
            }
            return conn;
        }

        private static string GetConnectionString(string connKey, IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(connKey))
                return string.Empty;
            return configuration.GetConnectionString(connKey);

        }

        public static IDbSession CreateSession(IConfiguration configuration, DatabaseType databaseType, string key)
        {
            var isDefined = Enum.IsDefined(typeof(DatabaseType), databaseType);
            if (!isDefined)
            {
                throw new NotSupportedException();
            }
            IDbConnection conn = CreateConnection(configuration, databaseType, key);
            IDbSession session = new DbSession(conn);
            return session;
        }
    }
}
