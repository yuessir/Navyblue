using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using DBMemThresholdMonitorTask.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NavyBule.Core.Domain;
using NavyBule.Data;

namespace DBMemThresholdMonitorTask
{
    public class OracleServer : OracleRepositoryBase<BaseEntity>
    {
        public string DbKey { get; set; }
        public OracleServer(string dbKey, IConfiguration configuration, ILogger logger) : base(configuration, logger)
        {
            DbKey = dbKey;
        }
        protected override string ConnStrKey => DbKey;
        public bool IsConnected => DbSession.Connection != null;

        public async Task<SharedPoolSize> GetSharedPoolSize()
        {
            return await GetBy<SharedPoolSize>(@"SELECT NAME, BYTES / 1024 / 1024 SIZEBYMB FROM V$SGAINFO WHERE NAME ='Shared Pool Size'");

        }
        public async Task<SharedPoolRemainingSize> GetSharedPoolRemainingSize()
        {
            return await GetBy<SharedPoolRemainingSize>(@"select POOL,NAME,bytes/1024/1024 SIZEBYMB
from v$sgastat
where name ='free memory' and pool='shared pool'");

        }

    }
    public class OracleRepositoryBase<T> : DapperRepositoryBase<T> where T : BaseEntity
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MssqlRepositoryBase{T}"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public OracleRepositoryBase(IConfiguration configuration, ILogger logger) : base(configuration, logger)
        {
        }

        /// <summary>
        /// Database type（MSSQL,Oracle...）
        /// </summary>
        /// <value>The type of the data.</value>
        protected sealed override DatabaseType DataType => DatabaseType.Oracle;



        /// <inheritdoc />
        /// <summary>
        /// override the connection key
        /// </summary>
        protected override string ConnStrKey => "";

        /// <summary>
        /// override the table name
        /// </summary>
        protected override string TableName
        {
            get => typeof(T).Name;
            set => value = typeof(T).Name;
        }
    }
    /// <summary>
    /// Class MssqlRepositoryBase.
    /// Implements the <see cref="DapperRepositoryBase" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="DapperRepositoryBase" />
    public class MssqlRepositoryBase<T> : DapperRepositoryBase<T> where T : BaseEntity
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MssqlRepositoryBase{T}"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public MssqlRepositoryBase(IConfiguration configuration, ILogger logger) : base(configuration, logger)
        {
        }

        /// <summary>
        /// Database type（MSSQL,Oracle...）
        /// </summary>
        /// <value>The type of the data.</value>
        protected sealed override DatabaseType DataType => DatabaseType.Mssql;



        /// <inheritdoc />
        /// <summary>
        /// override the connection key
        /// </summary>
        protected override string ConnStrKey => "B2BDB";

        /// <summary>
        /// override the table name
        /// </summary>
        protected override string TableName
        {
            get => typeof(T).Name;
            set => value = typeof(T).Name;
        }
    }

    public interface IDBTargetRepository
    {
        Task<IEnumerable<Template>> GetTemplate();
        Task<IEnumerable<ThresholdInfo>> GetThreshold();

        Task<int> Log(MonitorLog log);
    }
    public class DBTargetRepository : MssqlRepositoryBase<BaseEntity>, IDBTargetRepository
    {

        public DBTargetRepository(IConfiguration configuration, ILogger<DBTargetRepository> logger) : base(configuration, logger)
        {
        }

        public async Task<IEnumerable<Template>> GetTemplate()
        {
            return await GetList<Template>(@"SELECT [Id]
      ,[Subject]
      ,[Content]
        ,[TemplateName]
  FROM [dbo].[Template]");

        }

        public async Task<IEnumerable<ThresholdInfo>> GetThreshold()
        {
            return await GetList<ThresholdInfo>(@"SELECT [Id]
      ,[ServerName]
      ,[DbType]
      ,[Threshold]
      ,[ServiceName]
  FROM [dbo].[ThresholdInfo]");
        }

        public async Task<int> Log(MonitorLog log)
        {
            return await InsertAsync(@$"INSERT INTO [dbo].[MonitorLog]
                       ([ServerName]
                   ,[Usage]
               ,[CreateDate]
               ,[Threshold])
           VALUES
               ('{log.ServerName}'
               ,{log.Usage}
               ,SYSDATETIME()
               ,{log.Threshold})", useTransaction: true);
        }
    }
}
