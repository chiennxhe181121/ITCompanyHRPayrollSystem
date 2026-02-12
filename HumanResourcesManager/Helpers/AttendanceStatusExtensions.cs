using HumanResourcesManager.DAL.Enum;

namespace HumanResourcesManager.Helpers
{
    public static class AttendanceStatusExtensions
    {
        public static string ToDisplayName(this AttendanceStatus status)
        {
            return status switch
            {
                AttendanceStatus.Normal => "Đủ công",
                AttendanceStatus.InsufficientWork => "Thiếu công",
                AttendanceStatus.MissingCheckIn => "Thiếu Check-in",
                AttendanceStatus.MissingCheckOut => "Thiếu Check-out",
                AttendanceStatus.ApprovedLeave => "Nghỉ có phép",
                AttendanceStatus.AWOL => "Nghỉ không phép",
                _ => "Không xác định"
            };
        }
    }
}
