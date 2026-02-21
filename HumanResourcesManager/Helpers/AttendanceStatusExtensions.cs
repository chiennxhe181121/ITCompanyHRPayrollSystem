using HumanResourcesManager.DAL.Enum;

namespace HumanResourcesManager.Helpers
{
    public static class AttendanceStatusExtensions
    {
        public static string ToDisplayName(this AttendanceStatus status)
        {
            return status switch
            {
                AttendanceStatus.Pending => "Chờ kết luận",
                AttendanceStatus.CompletedWork => "Hoàn thành",
                AttendanceStatus.InsufficientWork => "Thiếu giờ",
                AttendanceStatus.MissingCheckOut => "Thiếu check-out",
                AttendanceStatus.ApprovedLeave => "Nghỉ phép",
                AttendanceStatus.Absent => "Vắng",
                AttendanceStatus.Holiday => "Nghỉ lễ",
                AttendanceStatus.Weekend => "Nghỉ cuối tuần",
                _ => "Không xác định"
            };
        }
    }
}
