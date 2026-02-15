using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.DAL.Models
{
    public class LeaveType
    {
        public int LeaveTypeId { get; set; }
        public string LeaveName { get; set; } = null!;
        public bool IsPaid { get; set; } // có tính lương hay không

        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    }

}
