using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rhema.Core.Attribute
{
    /// <summary>
    /// 忽略列(非数据库字段)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgoreAttribute : System.Attribute
    {

    }
}
