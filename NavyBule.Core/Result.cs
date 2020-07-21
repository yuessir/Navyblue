using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rhema.Core
{
    /// <summary>
    /// Class Result.
    /// </summary>
    [Serializable]
    public class Result
    {

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>The code.</value>
        public virtual double Code { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Result"/> is success.
        /// </summary>
        /// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
        public virtual bool Success { get; set; }

        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        /// <value>The errors.</value>
        [JsonProperty("errors")]
        public string Errors { get; set; }
        /// <summary>
        /// Gets or sets the grouping errors.
        /// </summary>
        /// <value>The grouping errors.</value>
        public Dictionary<string, List<string>> GroupingErrors { get; set; }

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>The exception.</value>
        //[JsonIgnore]
       // public virtual Exception Exception { get; set; }
    }

    /// <summary>
    /// Class Result.
    /// Implements the <see cref="Result" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Result" />
    public class Result<T> : Result
    {

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public T Data { get; set; }
    }

  
    public class PageResult<T> : Result<T>
    {
        public int Total { get; set; }
    }

}
