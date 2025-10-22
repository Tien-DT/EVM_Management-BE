using System;

namespace EVMManagement.BLL.Helpers
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// Converts a DateTime to UTC. If already UTC, returns as-is.
        /// If Kind is Unspecified, treats as UTC.
        /// If Kind is Local, converts to UTC.
        /// </summary>
        public static DateTime ToUtc(DateTime dateTime)
        {
            switch (dateTime.Kind)
            {
                case DateTimeKind.Utc:
                    return dateTime;
                case DateTimeKind.Local:
                    return dateTime.ToUniversalTime();
                case DateTimeKind.Unspecified:
                    return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                default:
                    return dateTime;
            }
        }

        /// <summary>
        /// Converts a nullable DateTime to UTC. Returns null if input is null.
        /// </summary>
        public static DateTime? ToUtc(DateTime? dateTime)
        {
            return dateTime.HasValue ? ToUtc(dateTime.Value) : null;
        }
    }
}
