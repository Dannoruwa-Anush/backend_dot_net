using System;

namespace WebApplication1.Utils.Helpers
{
    public static class TimeZoneHelper
    {
        private static readonly TimeZoneInfo _sriLankaTimeZone;

        static TimeZoneHelper()
        {
            try
            {
                // Works on Windows
                _sriLankaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                // Works on Linux/macOS
                _sriLankaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Colombo");
            }
        }

        public static DateTime NowInSriLanka()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _sriLankaTimeZone);
        }

        public static DateTime ToSriLankaTime(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc), _sriLankaTimeZone);
        }

        public static DateTime ToUtcFromSriLanka(DateTime localDateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, _sriLankaTimeZone);
        }
    }
}