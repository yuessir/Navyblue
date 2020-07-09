using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NavyBule.Core.Service
{
    public interface IDateTimeHelper
    {
        TimeZoneInfo CurrentTimeZone { get; set; }
        TimeZoneInfo FindTimeZoneById(string id);
        DateTime ConvertToUserTime(DateTime dt);
        DateTime ConvertToUserTime(DateTime dt, DateTimeKind sourceDateTimeKind);
    }
}
