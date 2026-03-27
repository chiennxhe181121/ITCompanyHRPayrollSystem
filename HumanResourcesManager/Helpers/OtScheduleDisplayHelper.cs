using System;

namespace HumanResourcesManager.Helpers
{
    public static class OtScheduleDisplayHelper
    {
        public static DateTime GetEffectiveEndDateTime(DateTime startDate, TimeSpan startTime, DateTime endDate, TimeSpan endTime)
        {
            if (startDate.Date == endDate.Date && endTime <= startTime)
                return startDate.Date.AddDays(1).Add(endTime);
            return endDate.Date.Add(endTime);
        }

        /// <summary>
        /// Khung giờ 12h: nếu kết thúc sang ngày mới thì hiển thị cả ngày cho đầu và cuối.
        /// </summary>
        public static string FormatScheduleTimeRange12h(DateTime startDate, TimeSpan startTime, DateTime endDate, TimeSpan endTime)
        {
            var startDt = startDate.Date.Add(startTime);
            var endDt = GetEffectiveEndDateTime(startDate, startTime, endDate, endTime);
            var startClock = startDt.ToString("h:mm tt");
            var endClock = endDt.ToString("h:mm tt");
            if (endDt.Date > startDt.Date)
                return $"{startClock} ({startDt:dd/MM/yyyy}) – {endClock} ({endDt:dd/MM/yyyy})";
            return $"{startClock} – {endClock}";
        }

        /// <summary>
        /// Khung giờ 24h (vd bảng lịch sử).
        /// </summary>
        public static string FormatScheduleTimeRange24h(DateTime startDate, TimeSpan startTime, DateTime endDate, TimeSpan endTime)
        {
            var startDt = startDate.Date.Add(startTime);
            var endDt = GetEffectiveEndDateTime(startDate, startTime, endDate, endTime);
            var startClock = startDt.ToString("HH:mm");
            var endClock = endDt.ToString("HH:mm");
            if (endDt.Date > startDt.Date)
                return $"{startClock} ({startDt:dd/MM/yyyy}) – {endClock} ({endDt:dd/MM/yyyy})";
            return $"{startClock} – {endClock}";
        }
    }
}
