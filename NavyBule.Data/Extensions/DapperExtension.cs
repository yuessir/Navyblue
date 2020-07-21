using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Rhema.Core.Infrastructure;

namespace Rhema.Data.Extensions
{
    public static partial  class DapperExtension
    {
        public static int Insert<T>(this IDbConnection conn, ISqlBuilder builder, T model, IDbTransaction tran = null, int? commandTimeout = null)
        {
           
            return conn.Execute(builder.GetInsertSql<T>(), model, tran, commandTimeout);
        }
        public static int Update<T>(this IDbConnection conn, ISqlBuilder builder, T model,  string updateFields = null, bool useProperties = false, IDbTransaction tran = null, int? commandTimeout = null)
        {
            return  conn.Execute(builder.GetUpdateSql<T>(updateFields, useProperties), model, tran, commandTimeout);
        }
        public static int InsertOrUpdate<T>(this IDbConnection conn, ISqlBuilder builder, T model, string updateFields = null,bool useProperties = false, bool update = true, IDbTransaction tran = null, int? commandTimeout = null)
        {
           
            int effectRow = 0;
            dynamic total = conn.ExecuteScalar<dynamic>(builder.GetExistsKeySql<T>(), model, tran, commandTimeout);
            if (total > 0)
            {
                if (update)
                {
                    effectRow += Update(conn, builder, model, updateFields, useProperties, tran, commandTimeout);
                }
            }
            else
            {
                effectRow += Insert(conn, builder, model, tran, commandTimeout);
            }

            return effectRow;
        }
        public static IEnumerable<T> GetByWhere<T>(this IDbConnection conn, ISqlBuilder builder, string where, object param = null, string returnFields = null, string orderBy = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            return conn.Query<T>(builder.GetByWhereSql<T>(where, returnFields, orderBy), param, tran, true, commandTimeout);
        }
        public static IEnumerable<T> GetAll<T>(this IDbConnection conn, ISqlBuilder builder, string returnFields = null, string orderBy = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            return conn.Query<T>(builder.GetAllSql<T>(returnFields, orderBy), null, tran, true, commandTimeout);
        }
    }
}
