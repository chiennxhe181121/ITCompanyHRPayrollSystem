using HumanResourcesManager.DAL.Enum;

namespace HumanResourcesManager.Helpers
{
    public static class LeaveRequestStatusExtensions
    {
        public static string ToDisplayName(this RequestStatus status)
        {
            return status switch
            {
                RequestStatus.Pending => "Chờ duyệt",
                RequestStatus.Approved => "Đã duyệt",
                RequestStatus.Rejected => "Từ chối",
                RequestStatus.Cancelled => "Đã huỷ",
                _ => "Không xác định"
            };
        }
    }
}
