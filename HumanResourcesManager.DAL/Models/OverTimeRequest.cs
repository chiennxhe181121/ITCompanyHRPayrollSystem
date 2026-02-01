using System;
using System.Collections.Generic;

namespace HumanResourcesManager.DAL.Models
{
    public class OverTimeRequest
    {
        public int Id { get; set; }

        // Nhân viên được giao OT
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        // Ngày OT (nguồn chuẩn)
        public DateTime WorkDate { get; set; }

        // Thời gian OT đăng ký
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }

        // Nội dung OT
        public string Reason { get; set; } = string.Empty;
        public string TaskRef { get; set; } = string.Empty;

        // Manager giao OT (cũng là Employee)
        public int ManagerId { get; set; }
        public Employee Manager { get; set; } = null!;

        // Nhân viên phản hồi
        // null = chưa phản hồi | true = đồng ý | false = từ chối
        public bool? EmployeeAccepted { get; set; }

        // Assigned | Approved | Declined | Cancelled
        public long Status { get; set; }

        public OTAttendance? OTAttendance { get; set; }
    }
}
