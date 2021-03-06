﻿using System.Collections.Generic;
using NavyBule.Core.Infrastructure;

namespace NavyBule.Core
{
    public class ConditionParameter<T>: ConditionParameter
    {
      
    }
    public class ConditionParameter
    {
        public string FieldName { get; set; }
        public ExpressionTypeDefault Comparison { get; set; }
        public object Val { get; set; }
        public ConditionType ConditionType { get; set; }
        public int Priority { get; set; }
        public string Tag { get; set; }
    }

}
