using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NavyBule.Core.Infrastructure;
using NavyBule.Core.Util;

namespace NavyBule.Data.Extensions
{
    public static partial class DapperExtension
    {
        public static async Task<int> InsertAsync<T>(this IDbConnection conn, ISqlBuilder builder, T model, IDbTransaction tran = null, int? commandTimeout = null)
        {
          
            return await conn.ExecuteAsync(builder.GetInsertSql<T>(), model, tran, commandTimeout);
        }
        public static async Task<int> UpdateAsync<T>(this IDbConnection conn, ISqlBuilder builder, T model, string updateFields = null, bool useProperties = false, IDbTransaction tran = null, int? commandTimeout = null)
        {
    
            return await conn.ExecuteAsync(builder.GetUpdateSql<T>(updateFields, useProperties), model, tran, commandTimeout);
        }

        public static async Task<int> InsertOrUpdateAsync<T>(this IDbConnection conn, ISqlBuilder builder, T model, string updateFields = null, bool useProperties = false, bool update = true, IDbTransaction tran = null, int? commandTimeout = null)
        {
            return await Task.Run(() =>
            {
                return InsertOrUpdate<T>(conn, builder, model, updateFields, useProperties, update, tran, commandTimeout);
            });
        }
        public static async Task<IdType> GetSequenceNextAsync<IdType>(this IDbConnection conn, ISqlBuilder builder, string sequence, IDbTransaction tran = null, int? commandTimeout = null)
        {
        
            return await conn.ExecuteScalarAsync<IdType>(builder.GetSequenceNextSql(sequence), null, tran, commandTimeout);
        }
        public static async Task<int> DeleteByIdsAsync<T>(this IDbConnection conn, ISqlBuilder builder, object ids, IDbTransaction tran = null, int? commandTimeout = null)
        {
            if (CommonUtilExt.ObjectIsEmpty(ids))
                return 0;
            DynamicParameters dpar = new DynamicParameters();
            dpar.Add("ids", ids);
            return await conn.ExecuteAsync(builder.GetDeleteByIdsSql<T>(), dpar, tran, commandTimeout);
        }
        public static async Task<int> DeleteAsync<T>(this IDbConnection conn, ISqlBuilder builder, object id, IDbTransaction tran = null, int? commandTimeout = null)
        {
            return await conn.ExecuteAsync(builder.GetDeleteByIdSql<T>(), new { id = id }, tran, commandTimeout);
        }
    }
}
