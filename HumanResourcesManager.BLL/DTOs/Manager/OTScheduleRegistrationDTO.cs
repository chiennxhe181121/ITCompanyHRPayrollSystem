using System;

namespace HumanResourcesManager.BLL.DTOs.Manager
{
    public class OTScheduleRegistrationDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public int Status { get; set; } // 1: Approved/Active, 3: Cancelled
        public DateTime RegisteredAt { get; set; } // We don't have this in OverTimeRequest but maybe we can use Time created if any. For now leave it.
    }
}
