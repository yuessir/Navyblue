using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NavyBule.Core.Infrastructure;

namespace NavyBule.Core.Util
{
    public static class CommonHelper
    {
        public static string ConvertEnum(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            var result = string.Empty;
            foreach (var c in str)
                if (c.ToString() != c.ToString().ToLower())
                    result += " " + c.ToString();
                else
                    result += c.ToString();

            //ensure no spaces (e.g. when the first letter is upper case)
            result = result.TrimStart();
            return result;
        }
        /// <summary>
        /// Gets or sets the default file provider
        /// </summary>
        public static IRmaFileProvider DefaultFileProvider { get; set; }
    }
}
