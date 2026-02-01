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
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan CheckOutTime { get; set; }

        // Số giờ OT thực tế (tính tự động)
        public double ActualOTHours { get; private set; }

        // CheckedIn | CheckedOut | Invalid
        public long Status { get; set; }

        public string? Note { get; set; }
        
    }
}
