using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ERPConnect;

namespace Rhema.Data
{
    /// <summary>
    /// Interface IDbSession
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IDbSession :IDisposable
    {
        IDbConnection Connection { get; }
        IDbTransaction Transaction { get; }

        IDbTransaction BeginTrans(IsolationLevel isolation = IsolationLevel.ReadCommitted);
        void Commit();
        void Rollback();

    }
    public interface ISapConn : IDisposable
    { 
        R3Connection ConnectionSAP { get; }
        void Open();
        void Close();

    }
}
