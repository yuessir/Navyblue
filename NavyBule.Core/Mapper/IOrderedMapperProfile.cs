using System;
using System.Collections.Generic;
using System.Text;

namespace Rhema.Core.Mapper
{
    public interface IOrderedMapperProfile
    {
        /// <summary>
        /// Gets order of this configuration implementation
        /// </summary>
        int Order { get; }
    }
}
