using HumanResourcesManager.DAL.Enum;
using System;

namespace HumanResourcesManager.DAL.Models
{
    public class OTAttendance
    {
        public int Id { get; set; }

        // Liên kết OT Request
        public int OverTimeRequestId { get; set; }
        public OverTimeRequest OverTimeRequest { get; set; } = null!;

        // Thời gian OT thực tế
        public TimeSpan CheckIn { get; set; }
        public TimeSpan CheckOut { get; set; }
        public string? CheckInImagePath { get; set; }
        public string? CheckOutImagePath { get; set; }

        // Số giờ OT thực tế (tính tự động)
        public double ActualOTHours { get; private set; }
        public AttendanceStatus Status { get; set; }        
    }
}
