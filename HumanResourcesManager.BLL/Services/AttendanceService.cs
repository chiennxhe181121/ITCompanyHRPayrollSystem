using HumanResourcesManager.BLL.DTOs.Employee;
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

        public EmployeeAttendanceViewDTO GetEmployeeAttendance(
            int currentUserId,
            int page,
            int pageSize,
            int? month,
            int? year,
            AttendanceStatus? status)
        {
            var employee = _employeeRepository.GetByUserId(currentUserId);

            if (employee == null)
                throw new Exception("Employee not found.");

            var employeeId = employee.EmployeeId;

            if (pageSize <= 0)
                pageSize = 10;

            if (page < 1)
                page = 1;

            // 🔥 1️⃣ Lấy query gốc
            var query = _attendanceRepository
                .GetQueryableByEmployeeId(employeeId);

            // 🔥 2️⃣ Filter theo tháng
            if (month.HasValue && month > 0)
                query = query.Where(a => a.WorkDate.Month == month.Value);

            // 🔥 3️⃣ Filter theo năm
            if (year.HasValue && year > 0)
                query = query.Where(a => a.WorkDate.Year == year.Value);

            // 🔥 4️⃣ Filter theo status (nên dùng enum int)
            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            // 🔥 5️⃣ Sắp xếp
            query = query.OrderByDescending(a => a.WorkDate);

            // 🔥 6️⃣ Tổng record sau khi filter
            var totalRecords = query.Count();

            var totalPages = totalRecords == 0
                ? 1
                : (int)Math.Ceiling((double)totalRecords / pageSize);

            if (page > totalPages)
                page = totalPages;

            // 🔥 7️⃣ Paging
            var attendances = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 🔥 8️⃣ Map DTO
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

            return new EmployeeAttendanceViewDTO
            {
                Records = records,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalRecords = totalRecords,

                // giữ lại filter để view render lại
                SelectedMonth = month,
                SelectedYear = year,
                SelectedStatus = status
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