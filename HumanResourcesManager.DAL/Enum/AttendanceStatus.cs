using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.DAL.Enum
{
    public enum AttendanceStatus
    {
        Normal,
        MissingCheckIn,
        MissingCheckOut,
        ApprovedLeave, // nghỉ có phép
        AWOL, // nghỉ không phép
        InsufficientWork
    }
}
