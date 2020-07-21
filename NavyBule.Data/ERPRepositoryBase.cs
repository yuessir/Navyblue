using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rhema.Core.Domain;

namespace Rhema.Data
{
    /// <summary>
    /// Class ERPRepositoryBase.
    /// </summary>
    public abstract class ERPRepositoryBase : IERPRepositoryBase
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        /// <summary>
        /// Initializes a new instance of the <see cref="ERPRepositoryBase"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        protected ERPRepositoryBase(IConfiguration configuration, ILogger logger)
        {

            _logger = logger;
            _configuration = configuration;

        }
        protected abstract ERPType DataType { get; }
        protected ISapConn DbSession => ERPSessionFactory.CreateSession(_configuration, DataType, ConnStrKey);
        protected abstract string ConnStrKey { get; }
        /// <summary>
        /// RFC execute as an asynchronous operation.
        /// </summary>
        /// <param name="rfcName">Name of the RFC.</param>
        /// <param name="d">The d.</param>
        /// <param name="rfcTableName">Name of the RFC table.</param>
        /// <param name="rfcTab">The RFC tab.</param>
        /// <returns>Task&lt;RFCFunction&gt;.</returns>
        public virtual async Task<RFCFunction> RfcExecuteAsync(string rfcName, Dictionary<string, string> d,
            List<string> rfcTableName = null, Func<RFCTable, RFCTable> rfcTab = null)
        {
            ISapConn session = DbSession;
            session.Open();
            var result=   await session.ConnectionSAP.RFCFunctionInit(rfcName).ExecuteAsync(d, rfcTableName, rfcTab);

            session.Dispose();
            return result;
        }

    }
    /// <summary>
    /// Class ERPHelper.
    /// </summary>
    public static class ERPHelper
    {
        /// <summary>
        /// RFCs the function initialize.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <param name="rfcName">Name of the RFC.</param>
        /// <returns>RFCFunction.</returns>
        public static RFCFunction RFCFunctionInit(this R3Connection conn, string rfcName)
        {
            var fun = conn.CreateFunction(rfcName);
            var tbs = fun.Tables;
            return fun;
        }

        /// <summary>
        /// Sets the RFC parameters.
        /// </summary>
        /// <param name="fun">The fun.</param>
        /// <param name="l">The l.</param>
        /// <returns>RFCFunction.</returns>
        public static RFCFunction SetRfcParameters(this RFCFunction fun, Action<RFCFunction> l)
        {

            l.Invoke(fun);
            return fun;
        }
        /// <summary>
        /// Runs the specified fun.
        /// </summary>
        /// <param name="fun">The fun.</param>
        /// <returns>RFCFunction.</returns>
        public static RFCFunction Run(this RFCFunction fun)
        {

            fun.Execute();
            return fun;
        }
        /// <summary>
        /// RFCs the execute.
        /// </summary>
        /// <param name="initFun">The initialize fun.</param>
        /// <param name="d">The d.</param>
        /// <param name="rfcTableName">Name of the RFC table.</param>
        /// <param name="rfcTab">The RFC tab.</param>
        /// <returns>RFCFunction.</returns>
        /// <exception cref="NotSupportedException">Not supported multi rfc table export yet.</exception>
        public static async Task<RFCFunction> ExecuteAsync(this RFCFunction initFun, Dictionary<string, string> d, List<string> rfcTableName = null, Func<RFCTable, RFCTable> rfcTab = null)
        {
            return await Task.Run(() =>
            {
                var fun = initFun.SetRfcParameters(x => x.Exports.ToList().ForEach(p =>
                    {
                        if (d.ContainsKey(p.Name))
                        {
                            p.ParamValue = d[p.Name];
                        }

                    }))
                    .Run();
               
                if (rfcTab != null && rfcTableName != null)
                {
                    if (rfcTableName.Count > 1)
                    {
                        //todo:optional,if needed
                        throw new NotSupportedException("Not supported multi rfc table export yet.");
                    }
                    fun.SetRfcParameters(q => q.Tables.Where(r => rfcTableName.Contains(r.Name)).ToList().ForEach(s =>
                    {
                        rfcTab.Invoke(s);
                    })).Run();
                    return fun;
                }
                return fun;
            });


        }
    }
}
