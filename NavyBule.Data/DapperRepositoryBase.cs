using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rhema.Core.Domain;
using Rhema.Core.Infrastructure;
using Rhema.Data.Extensions;

namespace Rhema.Data
{
    /// <summary>
    /// Class DapperRepositoryBase.
    /// </summary>
    public abstract class DapperRepositoryBase 
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DapperRepositoryBase{T}"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        protected DapperRepositoryBase(IConfiguration configuration, ILogger logger)
        {
           
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the database session.
        /// </summary>
        /// <value>The database session.</value>
        protected IDbSession DbSession => DbSessionFactory.CreateSession(_configuration, DataType, ConnStrKey);

        /// <summary>
        /// Database type（MSSQL,Oracle...）
        /// </summary>
        protected abstract DatabaseType DataType { get; }

        protected ISqlBuilder SqlBuilder => SqlBuilderFactory.GetBuilder(DbSession.Connection);
        /// <summary>
        /// Connection key
        /// </summary>
        protected abstract string ConnStrKey { get; }

        /// <summary>
        /// Table name(default is class name,if it is not,should be override in the child class)
        /// </summary>
        [Obsolete]
        protected abstract string TableName { get; set; }
        /// <summary>
        /// SQLs the mapper set type map.
        /// </summary>
        /// <typeparam name="TEntity">The type of the t entity.</typeparam>
        protected void SqlMapperSetTypeMap<TEntity>()
        {
            var map = new CustomPropertyTypeMap(typeof(TEntity),
                (type, columnName) => type.GetProperties().FirstOrDefault(prop => GetDescriptionFromAttribute(prop) == columnName));
            SqlMapper.SetTypeMap(typeof(TEntity), map);
        }

        /// <summary>
        /// Gets the description from attribute.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns></returns>
        protected virtual string GetDescriptionFromAttribute(MemberInfo member)
        {
            if (member == null) return null;
            var attrib = (DescriptionAttribute)Attribute.GetCustomAttribute(member, typeof(DescriptionAttribute), false);
            return attrib?.Description;
        }


        /// <summary>
        /// insert as an asynchronous operation.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="buffered">if set to <c>true</c> [buffered].</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="useTransaction">if set to <c>true</c> [use transaction].</param>
        /// <returns>Task&lt;System.Int32&gt;.</returns>
        public virtual async Task<int> InsertAsync(string sql, object param = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, bool useTransaction = false)
        {
            if (string.IsNullOrEmpty(sql))
                return 0;

            IDbSession session = DbSession;

            IEnumerable<int> result;
            try
            {
                if (useTransaction)
                {
                    session.BeginTrans();

                    result = await session.Connection.QueryAsync<int>(sql, param, session.Transaction, commandTimeout, commandType);

                    session.Commit();
                }
                else
                {
                    result = await session.Connection.QueryAsync<int>(sql, param, null, commandTimeout, commandType);
                }
            }
            catch (Exception ex)
            {
                if (useTransaction)
                {
                    session.Rollback();
                }

                // log
                _logger.LogError(ex.Message);


                result = new List<int>();
            }
            finally
            {
                session.Dispose();
            }

            return result.SingleOrDefault();
        }
        public virtual async Task<int> DeleteAsync<T>( object ids, bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null, bool useTransaction = false)
        {
           
            IDbSession session = DbSession;
            int result;
            try
            {
                if (useTransaction)
                {
                    session.BeginTrans();
                    result = await session.Connection.DeleteAsync<T>(SqlBuilder, ids, session.Transaction, commandTimeout);
                    session.Commit();
                }
                else
                {
                    result = await session.Connection.DeleteAsync<T>(SqlBuilder, ids, null, commandTimeout);
                }
            }
            catch (Exception ex)
            {
                if (useTransaction)
                {
                    session.Rollback();
                }

                // log
                _logger.LogError(ex.Message);
                return 0;
            }
            finally
            {
                session.Dispose();
            }
            return result;
        }
        public virtual async Task<int> DeleteByIdsAsync<T>( object ids, bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null, bool useTransaction = false)
        {
   
            IDbSession session = DbSession;
            int result;
            try
            {
                if (useTransaction)
                {
                    session.BeginTrans();
                    result = await session.Connection.DeleteByIdsAsync<T>(SqlBuilder, ids, session.Transaction, commandTimeout);
                    session.Commit();
                }
                else
                {
                    result = await session.Connection.DeleteByIdsAsync<T>(SqlBuilder, ids, null, commandTimeout);
                }
            }
            catch (Exception ex)
            {
                if (useTransaction)
                {
                    session.Rollback();
                }

                // log
                _logger.LogError(ex.Message);
                return 0;
            }
            finally
            {
                session.Dispose();
            }
            return result;
        }
        public virtual async Task<int> InsertOrUpdateAsync<T>(T model, string updateFields = null, bool update = true, bool useProperties = false, bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null, bool useTransaction = false)
        {
            if (model == null)
                return 0;
            IDbSession session = DbSession;
            int result;
            try
            {
                if (useTransaction)
                {
                    session.BeginTrans();
                    result = await session.Connection.InsertOrUpdateAsync<T>(SqlBuilder, model, updateFields, useProperties, update, session.Transaction, commandTimeout);
                    session.Commit();
                }
                else
                {
                    result = await session.Connection.InsertOrUpdateAsync<T>(SqlBuilder, model, updateFields, useProperties, update, null, commandTimeout);
                }
            }
            catch (Exception ex)
            {
                if (useTransaction)
                {
                    session.Rollback();
                }

                // log
                _logger.LogError(ex.Message);
                return 0;
            }
            finally
            {
                session.Dispose();
            }
            return result;
        }
        public virtual async Task<int> UpdateAsync<T>(T model, string updateFields = null, bool useProperties = false, bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null, bool useTransaction = false)
        {
            if (model == null)
                return 0;
            IDbSession session = DbSession;
            int result;
            try
            {
                if (useTransaction)
                {
                    session.BeginTrans();
                    result = await session.Connection.UpdateAsync<T>(SqlBuilder, model, updateFields, useProperties, session.Transaction, commandTimeout);
                    session.Commit();
                }
                else
                {
                    result = await session.Connection.UpdateAsync<T>(SqlBuilder, model, updateFields, useProperties, null,commandTimeout);
                }
            }
            catch (Exception ex)
            {
                if (useTransaction)
                {
                    session.Rollback();
                }

                // log
                _logger.LogError(ex.Message);
                return 0;
            }
            finally
            {
                session.Dispose();
            }
            return result;
        }
        /// <summary>
        /// insert as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="buffered">if set to <c>true</c> [buffered].</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="useTransaction">if set to <c>true</c> [use transaction].</param>
        /// <returns>Task&lt;System.Int32&gt;.</returns>
        public virtual async Task<int> InsertAsync<T>(T model, object param = null, bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null, bool useTransaction = false)
        {
            if (model==null)
                return 0;
            IDbSession session = DbSession;
            int result;
            try
            {
                if (useTransaction)
                {
                    session.BeginTrans();
                    result = await session.Connection.InsertAsync<T>(SqlBuilder, model, session.Transaction, commandTimeout);
                    session.Commit();
                }
                else
                {
                    result = await session.Connection.InsertAsync<T>(SqlBuilder, model,null , commandTimeout);
                }
            }
            catch (Exception ex)
            {
                if (useTransaction)
                {
                    session.Rollback();
                }

                // log
                _logger.LogError(ex.Message);
                return 0;
            }
            finally
            {
                session.Dispose();
            }
            return result;
        }

        public virtual async Task<int> GetSequenceNextAsync(string sequence, IDbTransaction tran = null, int? commandTimeout = null)
        {
            IDbSession session = DbSession;
            try
            {
             
                return await session.Connection.GetSequenceNextAsync<int>(SqlBuilder, sequence, tran, commandTimeout);
            }
            catch (Exception e)
            {
                return 0;

            }
            finally { session.Dispose(); }
           

        }
        public virtual async Task<int> GetIdentityNextAsync(string sql, object param = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            IDbSession session = DbSession;
            try
            {

                return await session.Connection.ExecuteScalarAsync<int>(sql, param, transaction: tran, commandTimeout: commandTimeout);
            }
            catch (Exception e)
            {
                return 0;

            }
            finally { session.Dispose(); }


        }

        /// <summary>
        /// update as an asynchronous operation.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="buffered">if set to <c>true</c> [buffered].</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="useTransaction">if set to <c>true</c> [use transaction].</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        public virtual async Task<bool> UpdateAsync(string sql, object param = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, bool useTransaction = false)
        {
            if (string.IsNullOrEmpty(sql))
                return false;

            IDbSession session = DbSession;

            try
            {
                if (useTransaction)
                {
                    session.BeginTrans();

                    await session.Connection.QueryAsync(sql, param, session.Transaction, commandTimeout, commandType);
                    session.Commit();
                }
                else
                {
                    await session.Connection.QueryAsync(sql, param, null, commandTimeout, commandType);
                }

                return true;
            }
            catch (Exception ex)
            {
                if (useTransaction)
                {
                    session.Rollback();
                }

                // log
                _logger.LogError(ex.Message);

                return false;
            }
            finally
            {
                session.Dispose();
            }
        }

        /// <summary>
        /// Deletes the specified entity identifier.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="useTransaction">if set to <c>true</c> [use transaction].</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        public virtual async Task<bool> Delete(int entityId, string predicate = "", object param = null, int? commandTimeout = null,
            bool useTransaction = false)
        {
            if (entityId == 0 && string.IsNullOrEmpty(predicate))
                return false;

            StringBuilder builder = new StringBuilder($"DELETE FROM {TableName}");
            builder.Append(" WHERE ");

            builder.Append(string.IsNullOrEmpty(predicate) ? $"Id = {entityId};" : predicate);

            return await Execute(builder.ToString(), param, commandTimeout, CommandType.Text, useTransaction) > 0;
        }

        /// <summary>
        /// Executes the specified SQL.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="useTransaction">if set to <c>true</c> [use transaction].</param>
        /// <returns>Task&lt;System.Int32&gt;.</returns>
        public virtual async Task<int> Execute(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null,
            bool useTransaction = true)
        {
            if (string.IsNullOrEmpty(sql))
                return 0;

            IDbSession session = DbSession;

            try
            {
                if (useTransaction)
                {
                    session.BeginTrans();

                    int rowsAffected = await session.Connection.ExecuteAsync(sql, param, session.Transaction, commandTimeout, commandType);
                    session.Commit();

                    return rowsAffected;
                }
                else
                {
                    return await session.Connection.ExecuteAsync(sql, param, null, commandTimeout, commandType);
                }
            }
            catch (Exception ex)
            {
                // log
                _logger.LogError(ex.Message);

                if (useTransaction)
                {
                    session.Rollback();
                }

                return 0;
            }
            finally
            {
                session.Dispose();
            }
        }
        /// <summary>
        /// Gets the by.
        /// </summary>
        /// <typeparam name="TEntity">The type of the t entity.</typeparam>
        /// <param name="sql">The SQL.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="buffered">if set to <c>true</c> [buffered].</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="useTransaction">if set to <c>true</c> [use transaction].</param>
        /// <returns>Task&lt;TEntity&gt;.</returns>
        public virtual async Task<TEntity> GetBy<TEntity>(string sql, object param = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, bool useTransaction = false)
        {
            SqlMapperSetTypeMap<TEntity>();

            if (string.IsNullOrEmpty(sql))
                return default(TEntity);

            IDbSession session = DbSession;

            var result = await session.Connection.QueryAsync<TEntity>(sql, param, null, commandTimeout, commandType);

            session.Dispose();

            return result.FirstOrDefault();
        }

        /// <summary>
        /// get by where as an asynchronous operation.
        /// </summary>
        /// <typeparam name="TEntity">The type of the t entity.</typeparam>
        /// <param name="whereClause">The where clause, optional.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="returnFields">The return fields.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="tran">The tran.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="useTransaction">if set to <c>true</c> [use transaction].</param>
        /// <returns>Task&lt;IEnumerable&lt;TEntity&gt;&gt;.</returns>
        public virtual async Task<IEnumerable<TEntity>> GetByWhereAsync<TEntity>(string whereClause, object param = null, string returnFields = null, string orderBy = null, IDbTransaction tran = null, int? commandTimeout = null
            , bool useTransaction = false)
        {
            SqlMapperSetTypeMap<TEntity>();
      
            IDbSession session = DbSession;
             try
            {
                IEnumerable<TEntity> results;
                if (useTransaction)
                {
                    session.BeginTrans();

                    results = await session.Connection.GetByWhereAsync<TEntity>(SqlBuilder, whereClause, param, returnFields, orderBy, session.Transaction, commandTimeout);

                    session.Commit();
                }
                else
                {
                    results = await session.Connection.GetByWhereAsync<TEntity>(SqlBuilder, whereClause, param, returnFields, orderBy, null, commandTimeout);

                }
                return results.ToList();

            }
            catch (Exception ex)
            {
                // log
                _logger.LogError(ex.Message);

                if (useTransaction)
                {
                    session.Rollback();
                }

                return null;
            }
            finally
            {
                session.Dispose();
            }
        }
        /// <summary>
        /// get all as an asynchronous operation.
        /// </summary>
        /// <typeparam name="TEntity">The type of the t entity.</typeparam>
        /// <param name="returnFields">The return fields.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="tran">The tran.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="useTransaction">if set to <c>true</c> [use transaction].</param>
        /// <returns>Task&lt;IEnumerable&lt;TEntity&gt;&gt;.</returns>
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(string returnFields = null, string orderBy = null, IDbTransaction tran = null, int? commandTimeout = null
            , bool useTransaction = false)
        {
            SqlMapperSetTypeMap<TEntity>();

            IDbSession session = DbSession;
            try
            {
                IEnumerable<TEntity> results;
                if (useTransaction)
                {
                    session.BeginTrans();

                    results = await session.Connection.GetAllAsync<TEntity>(SqlBuilder, returnFields, orderBy, session.Transaction, commandTimeout);

                    session.Commit();
                }
                else
                {
                    results = await session.Connection.GetAllAsync<TEntity>(SqlBuilder, returnFields, orderBy, null, commandTimeout);

                }
                return results.ToList();

            }
            catch (Exception ex)
            {
                // log
                _logger.LogError(ex.Message);

                if (useTransaction)
                {
                    session.Rollback();
                }

                return null;
            }
            finally
            {
                session.Dispose();
            }
        }
        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <typeparam name="TEntity">The type of the t entity.</typeparam>
        /// <param name="sql">The SQL.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="buffered">if set to <c>true</c> [buffered].</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="useTransaction">if set to <c>true</c> [use transaction].</param>
        /// <returns>Task&lt;IEnumerable&lt;TEntity&gt;&gt;.</returns>
        public virtual async Task<IEnumerable<TEntity>> GetList<TEntity>(string sql, object param = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, bool useTransaction = false)
        {

            SqlMapperSetTypeMap<TEntity>();
            if (string.IsNullOrEmpty(sql))
                return null;

            IDbSession session = DbSession;

            try
            {
                IEnumerable<TEntity> results;
                if (useTransaction)
                {
                    session.BeginTrans();

                    results = await session.Connection.QueryAsync<TEntity>(sql, param, session.Transaction, commandTimeout, commandType);
                    session.Commit();
                }
                else
                {
                    results = await session.Connection.QueryAsync<TEntity>(sql, param, null, commandTimeout, commandType);
                }
                return results.ToList();

            }
            catch (Exception ex)
            {
                // log
                _logger.LogError(ex.Message);

                if (useTransaction)
                {
                    session.Rollback();
                }

                return null;
            }
            finally
            {
                session.Dispose();
            }
        }

    }
    /// <summary>
    /// Class DapperRepositoryBase.
    /// Implements the <see cref="IDapperRepository{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="IDapperRepository{T}" />
    public abstract class DapperRepositoryBase<T> : DapperRepositoryBase, IDapperRepository, IDapperRepository<T> where T : BaseEntity
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        /// <summary>
        /// Initializes a new instance of the <see cref="DapperRepositoryBase{T}"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        protected DapperRepositoryBase(IConfiguration configuration, ILogger logger) : base(configuration, logger)
        {
            SqlMapperSetTypeMap<T>();
        }

        /// <summary>
        /// Get a piece of data based on relevant conditions
        /// </summary>
        /// <param name="sql">sql query or stored procedure</param>
        /// <param name="param">parameters</param>
        /// <param name="buffered">buffer data or not，more detail：https://dapper-tutorial.net/buffered</param>
        /// <param name="commandTimeout">command time out</param>
        /// <param name="commandType">command type (sql or stored procedure)</param>
        /// <param name="useTransaction">open transaction or not</param>
        /// <returns>data</returns>
        public virtual async Task<T> GetBy(string sql, object param = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, bool useTransaction = false)
        {
            if (string.IsNullOrEmpty(sql))
                return null;

            IDbSession session = DbSession;

            var result = await session.Connection.QueryAsync<T>(sql, param, null, commandTimeout, commandType);

            session.Dispose();

            return result.FirstOrDefault();
        }



        /// <summary>
        /// get data list on relevant conditions
        /// </summary>
        /// <param name="sql">sql query or stored procedure</param>
        /// <param name="param">parameters</param>
        /// <param name="buffered">buffer data or not，more detail：https://dapper-tutorial.net/buffered</param>
        /// <param name="commandTimeout">command time out</param>
        /// <param name="commandType">command type (sql or stored procedure)</param>
        /// <param name="useTransaction">open transaction or not</param>
        /// <returns>data list</returns>
        public virtual async Task<IEnumerable<T>> GetList(string sql, object param = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, bool useTransaction = false)
        {
           
            if (string.IsNullOrEmpty(sql))
                return null;

            IEnumerable<T> results;

            IDbSession session = DbSession;
            if (useTransaction)
            {
                session.BeginTrans();

                results = await session.Connection.QueryAsync<T>(sql, param, session.Transaction, commandTimeout, commandType);
                session.Commit();
            }
            else
            {
                results = await session.Connection.QueryAsync<T>(sql, param, null, commandTimeout, commandType);
            }

            session.Dispose();

            return results.ToList();
        }


    
    }
}
