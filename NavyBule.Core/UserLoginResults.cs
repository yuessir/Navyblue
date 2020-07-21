using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rhema.Core
{

    /// <summary>
    /// Enum UserLoginResults
    /// </summary>
    public enum UserLoginResults
    {
        /// <summary>
        /// Login successful
        /// </summary>
        Successful = 1,

        /// <summary>
        /// The user account not exist
        /// </summary>
        UserAccountNotExist = 2,

        /// <summary>
        /// Wrong password
        /// </summary>
        WrongPassword = 3,

        /// <summary>
        /// Account have not been activated
        /// </summary>
        NotActive = 4,


        /// <summary>
        /// The deleted
        /// </summary>
        Deleted = 5,


        /// <summary>
        /// The not registered
        /// </summary>
        NotRegistered = 6,

        /// <summary>
        /// Locked out
        /// </summary>
        LockedOut = 7
    }
}
