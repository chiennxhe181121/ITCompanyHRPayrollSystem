using System;

namespace HumanResourcesManager.BLL.DTOs.Manager
{
    public class OTScheduleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        
        public int DepartmentId { get; set; }
        public int ManagerId { get; set; }
        public string ManagerName { get; set; } = string.Empty;

        // Trạng thái: 0 = Mở (Open), 1 = Chốt/Hoàn thành (Closed/Completed), 2 = Hủy (Cancelled)
        public int Status { get; set; }

        public DateTime CreatedAt { get; set; }

        // Số lượng người đã đăng ký
        public int RegisteredCount { get; set; }
        
        // Trạng thái đối với nhân viên đang xem (đã đăng ký hay chưa)
        public bool IsRegisteredByCurrentUser { get; set; }
    }
}
