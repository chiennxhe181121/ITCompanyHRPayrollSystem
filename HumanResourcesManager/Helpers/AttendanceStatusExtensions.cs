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
                AttendanceStatus.CompletedWork => "Đủ công",
                AttendanceStatus.InsufficientWork => "Thiếu công",
                AttendanceStatus.MissingCheckOut => "Quên check-out",
                AttendanceStatus.ApprovedLeave => "Nghỉ phép",
                AttendanceStatus.Absent => "Vắng",
                AttendanceStatus.Holiday => "Ngày nghỉ lễ",
                _ => "Không xác định"
            };
        }
    }
}
