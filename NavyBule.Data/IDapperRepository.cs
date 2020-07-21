using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Rhema.Core.Domain;

namespace Rhema.Data
{
    /// <summary>
    /// Interface IDapperRepository
    /// </summary>
    public interface IDapperRepository
    {

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
        Task<TEntity> GetBy<TEntity>(string sql, object param = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, bool useTransaction = false);

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
        Task<IEnumerable<TEntity>> GetList<TEntity>(string sql, object param = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, bool useTransaction = false);
      


    }
    /// <summary>
    /// Interface IDapperRepository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDapperRepository<T>  where T : BaseEntity
    {
        /// <summary>
        /// Get a piece of data based on relevant conditions
        /// </summary>
        /// <param name="sql">sql query or stored procedure</param>
        /// <param name="param">parameters</param>
        /// <param name="buffered">buffer data or not，more detail：https://dapper-tutorial.net/buffered </param>
        /// <param name="commandTimeout">command time out</param>
        /// <param name="commandType">command type (sql or stored procedure)</param>
        /// <param name="useTransaction">open transaction or not</param>
        /// <returns>data</returns>
        Task<T> GetBy(string sql, object param = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, bool useTransaction = false);


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
        Task<IEnumerable<T>> GetList(string sql, object param = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, bool useTransaction = false);


        ///// <summary>
        ///// insert data
        ///// </summary>
        ///// <param name="sql">sql query or stored procedure</param>
        ///// <param name="param">parameters</param>
        ///// <param name="buffered">buffer data or not，more detail：https://dapper-tutorial.net/buffered </param>
        ///// <param name="commandTimeout">command time out</param>
        ///// <param name="commandType">command type (sql or stored procedure)</param>
        ///// <param name="useTransaction">open transaction or not</param>
        ///// <returns>insert id</returns>
        //Task<int> Insert(string sql, object param = null, bool buffered = true, int? commandTimeout = null,
        //    CommandType? commandType = null, bool useTransaction = false);

        ///// <summary>
        ///// update data
        ///// </summary>
        ///// <param name="sql">sql query or stored procedure</param>
        ///// <param name="param">parameters</param>
        ///// <param name="buffered">buffer data or not，more detail：https://dapper-tutorial.net/buffered </param>
        ///// <param name="commandTimeout">command time out</param>
        ///// <param name="commandType">command type (sql or stored procedure)</param>
        ///// <param name="useTransaction">open transaction or not</param>
        ///// <returns>update result</returns>
        //Task<bool> Update(string sql, object param = null, bool buffered = true, int? commandTimeout = null,
        //    CommandType? commandType = null, bool useTransaction = false);

        ///// <summary>
        ///// delete data on relevant conditions
        ///// </summary>
        ///// <param name="entityId">primary key</param>
        ///// <param name="predicate"> where condition</param>
        ///// <param name="param">parameters</param>
        ///// <param name="commandTimeout">command time out</param>
        ///// <param name="useTransaction">open transaction or not</param>
        ///// <returns>delete result</returns>
        //Task<bool> Delete(int entityId, string predicate = "", object param = null, int? commandTimeout = null, bool useTransaction = false);

        ///// <summary>
        ///// execute sql
        ///// </summary>
        ///// <param name="sql">sql query or stored procedure</param>
        ///// <param name="param">parameters</param>
        ///// <param name="commandTimeout">command time out</param>
        ///// <param name="commandType">command type (sql or stored procedure)</param>
        ///// <param name="useTransaction">open transaction or not</param>
        ///// <returns>affected rows</returns>
        //Task<int> Execute(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null,
        //    bool useTransaction = true);
    }
}
