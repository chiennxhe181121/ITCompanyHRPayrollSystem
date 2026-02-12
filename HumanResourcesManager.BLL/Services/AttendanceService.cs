using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Enum;
using HumanResourcesManager.DAL.Interfaces;

namespace HumanResourcesManager.BLL.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public AttendanceService(
            IAttendanceRepository attendanceRepository,
            IEmployeeRepository employeeRepository)
        {
            _attendanceRepository = attendanceRepository;
            _employeeRepository = employeeRepository;
        }

        public EmployeeAttendanceViewDTO GetEmployeeAttendance(int currentUserId, int page, int pageSize)
        {
            var employee = _employeeRepository.GetByUserId(currentUserId);

            if (employee == null)
            {
                throw new Exception("Employee not found for current user.");
            }

            var employeeId = employee.EmployeeId;

            // 1️⃣ Validate pageSize
            if (pageSize <= 0)
                pageSize = 10;

            // 2️⃣ Tổng record
            var totalRecords = _attendanceRepository.CountByEmployeeId(employeeId);

            // 3️⃣ Tính totalPages
            var totalPages = totalRecords == 0
                ? 1
                : (int)Math.Ceiling((double)totalRecords / pageSize);

            // 4️⃣ Validate page
            if (page < 1)
                page = 1;

            if (page > totalPages)
                page = totalPages;

            // 5️⃣ Lấy data từ repo
            var attendances = _attendanceRepository
                .GetByEmployeeId(employeeId, page, pageSize);

            // 6️⃣ Map sang DTO
            var records = attendances.Select(a => new AttendanceRowDTO
            {
                WorkDate = a.WorkDate,
                CheckIn = a.CheckIn,
                CheckOut = a.CheckOut,
                MissingMinutes = a.MissingMinutes,
                CheckInImagePath = a.CheckInImagePath,
                CheckOutImagePath = a.CheckOutImagePath,
                Status = GetStatusText(a.Status)
            }).ToList();

            // 7️⃣ Trả về ViewDTO
            return new EmployeeAttendanceViewDTO
            {
                Records = records,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        private string GetStatusText(AttendanceStatus status)
        {
            return status switch
            {
                AttendanceStatus.Normal => "Đủ công",
                AttendanceStatus.InsufficientWork => "Thiếu công",
                AttendanceStatus.MissingCheckIn => "Thiếu Check-in",
                AttendanceStatus.MissingCheckOut => "Thiếu Check-out",
                AttendanceStatus.ApprovedLeave => "Nghỉ có phép",
                AttendanceStatus.AWOL => "Nghỉ không phép",
                _ => "Không xác định"
            };
        }
    }
}