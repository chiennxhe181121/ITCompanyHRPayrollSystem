using System;
using System.Collections.Generic;

namespace HumanResourcesManager.DAL.Models
{
    public class OTSchedule
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Số lượng nhân viên cần
        public int Quantity { get; set; }

        // Thời gian OT
        public DateTime StartDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan EndTime { get; set; }

        // Trạng thái: 0 = Mở (Open), 1 = Chốt (Closed), 2 = Hủy (Cancelled), 4 = Hoàn thành (Completed)
        public int Status { get; set; }

        // Foreign Keys
        public int DepartmentId { get; set; }
        public int ManagerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public Department Department { get; set; } = null!;
        public Employee Manager { get; set; } = null!;

        // Các lượt đăng ký (anh xạ sang OverTimeRequest)
        public ICollection<OverTimeRequest> Registrations { get; set; } = new List<OverTimeRequest>();
    }
}
