using System;

namespace WebApplication1.Utils.Helpers
{
    public static class TimeZoneHelper
    {
        private static readonly TimeZoneInfo SriLankaTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time");

        public static DateTime ToSriLankaTime(DateTime utcDateTime)
        {
            if (utcDateTime.Kind == DateTimeKind.Local)
                utcDateTime = utcDateTime.ToUniversalTime();

            return TimeZoneInfo.ConvertTimeFromUtc(
                utcDateTime.Kind == DateTimeKind.Utc
                    ? utcDateTime
                    : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc),
                SriLankaTimeZone);
        }
    }
}