using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavyBule.Core.Attribute
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : System.Attribute
    {
        public string Name { get; set; }
        public bool UsePropertyName { get; set; }
    }
}
