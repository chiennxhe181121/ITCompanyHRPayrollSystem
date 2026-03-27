using System;

namespace HumanResourcesManager.DAL.Shared
{
    /// <summary>
    /// Giờ nghiệp vụ Việt Nam (UTC+7). Dùng thay cho DateTime.Now khi so khung OT/ngày làm
    /// để không phụ thuộc múi giờ Windows/IIS hoặc container Linux.
    /// </summary>
    public static class VietnamClock
    {
        private static readonly TimeZoneInfo Zone = ResolveZone();

        private static TimeZoneInfo ResolveZone()
        {
            foreach (var id in new[] { "SE Asia Standard Time", "Asia/Ho_Chi_Minh" })
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }

            return TimeZoneInfo.CreateCustomTimeZone(
                "Vietnam+7",
                TimeSpan.FromHours(7),
                "Vietnam (UTC+7, fallback)",
                "Vietnam (UTC+7, fallback)");
        }

        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone);

        public static DateTime Today => Now.Date;
    }
}
