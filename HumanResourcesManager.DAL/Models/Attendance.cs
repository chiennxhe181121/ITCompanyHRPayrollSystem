using HumanResourcesManager.DAL.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.DAL.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public DateTime WorkDate { get; set; }

        public TimeSpan? CheckIn { get; set; }
        public TimeSpan? CheckOut { get; set; }

        public int MissingMinutes { get; set; }

        public string? CheckInImagePath { get; set; }
        public string? CheckOutImagePath { get; set; }

        public AttendanceStatus Status { get; set; }
    }

}
