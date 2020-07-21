using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rhema.Core.Service
{
    public class DateTimeHelper : IDateTimeHelper
    {
        public TimeZoneInfo FindTimeZoneById(string id)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        public virtual DateTime ConvertToUserTime(DateTime dt)
        {
            return ConvertToUserTime(dt, dt.Kind);
        }
        public virtual DateTime ConvertToUserTime(DateTime dt, DateTimeKind sourceDateTimeKind)
        {
            dt = DateTime.SpecifyKind(dt, sourceDateTimeKind);
            if (TimeZoneInfo.Local.IsInvalidTime(dt))
                return dt;

            var currentUserTimeZoneInfo = CurrentTimeZone;
            return TimeZoneInfo.ConvertTime(dt, currentUserTimeZoneInfo);
        }

        private TimeZoneInfo _currentTimeZone;
        public virtual TimeZoneInfo CurrentTimeZone
        {
            get
            {
                if (_currentTimeZone != null)
                {
                    return _currentTimeZone;
                }
                //default time zone
                return _currentTimeZone = FindTimeZoneById("Taipei Standard Time");

            }
            set => _currentTimeZone = value;
        }
    }
}
