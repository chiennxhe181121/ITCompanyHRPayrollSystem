using HumanResourcesManager.BLL.DTOs.Common;
using HumanResourcesManager.BLL.DTOs.Employee;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Enum;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;

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

        public async Task<ServiceResult> CheckIn(int userId, CheckInDTO dto)
        {
            // Ràng buộc thời gian chấm công
            if (dto.CheckInTime < Constants.CheckInFrom || dto.CheckInTime > Constants.CheckInTo)
            {
                return ServiceResult.Failure("Không nằm trong khung giờ check-in.");
            }

            var employee = _employeeRepository.GetByUserId(userId);

            if (employee == null)
                return ServiceResult.Failure("Không tìm thấy nhân viên.");

            var today = GetVietnamNow().Date;

            var existingAttendance = _attendanceRepository
                .GetByEmployeeAndWorkDate(employee.EmployeeId, today);

            if (existingAttendance != null)
                return ServiceResult.Failure("Bạn đã chấm công ngày này rồi.");

            string? imagePath = null;

            if (dto.CheckInImage != null && dto.CheckInImage.Length > 0)
            {
                var folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "img",
                    "employees",
                    employee.EmployeeId.ToString(),
                    "attendance"
                );

                Directory.CreateDirectory(folderPath);

                var fileName = $"check-in-{today:yyyyMMdd}.jpg";
                var filePath = Path.Combine(folderPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.CheckInImage.CopyToAsync(stream);

                imagePath = $"/img/employees/{employee.EmployeeId}/attendance/{fileName}";
            }

            var newAttendance = new Attendance
            {
                EmployeeId = employee.EmployeeId,
                WorkDate = today,
                CheckIn = dto.CheckInTime,
                CheckInImagePath = imagePath,
                Status = AttendanceStatus.MissingCheckOut,
                MissingMinutes = 0
            };

            _attendanceRepository.Add(newAttendance);
            _attendanceRepository.Save();

            return ServiceResult.Success("Chấm công thành công!");
        }

        public async Task<ServiceResult> CheckOut(int userId, CheckOutDTO dto)
        {
            // Ràng buộc thời gian chấm công
            if (dto.CheckOutTime < Constants.CheckOutFrom || dto.CheckOutTime > Constants.CheckOutTo)
            {
                return ServiceResult.Failure("Không nằm trong khung giờ check-out.");
            }

            var employee = _employeeRepository.GetByUserId(userId);
            if (employee == null)
                return ServiceResult.Failure("Không tìm thấy nhân viên.");

            var today = GetVietnamNow().Date;

            var attendance = _attendanceRepository
                .GetByEmployeeAndWorkDate(employee.EmployeeId, today);

            if (attendance == null)
                return ServiceResult.Failure("Bạn chưa Check-in hôm nay.");

            if (attendance.CheckIn == null)
                return ServiceResult.Failure("Thiếu Check-in.");

            if (attendance.CheckOut != null)
                return ServiceResult.Failure("Bạn đã Check-out rồi.");

            // ===== LƯU ẢNH CHECKOUT =====
            string? imagePath = null;

            if (dto.CheckOutImage != null && dto.CheckOutImage.Length > 0)
            {
                var folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "img",
                    "employees",
                    employee.EmployeeId.ToString(),
                    "attendance"
                );

                Directory.CreateDirectory(folderPath);

                var fileName = $"check-in-{today:yyyyMMdd}.jpg";
                var filePath = Path.Combine(folderPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.CheckOutImage.CopyToAsync(stream);

                imagePath = $"/img/employees/{employee.EmployeeId}/attendance/{fileName}";
            }

            // ===== TÍNH THỜI GIAN LÀM VIỆC =====
            // ===== VALIDATE =====
            if (!attendance.CheckIn.HasValue)
                return ServiceResult.Failure("Chưa check-in.");

            if (!dto.CheckOutTime.HasValue)
                return ServiceResult.Failure("Chưa nhập check-out time.");

            // ===== TÍNH THỜI GIAN LÀM VIỆC =====
            var totalMinutes =
                (int)(dto.CheckOutTime.Value - attendance.CheckIn.Value).TotalMinutes;

            if (totalMinutes < 0)
                return ServiceResult.Failure("Check-out phải sau check-in.");

            // Trừ thời gian nghỉ
            var actualWorkMinutes = totalMinutes - Constants.BREAK_MINUTES;

            if (actualWorkMinutes < 0)
                actualWorkMinutes = 0;

            // Tính thiếu giờ
            var missingMinutes = Constants.STANDARD_WORK_MINUTES - actualWorkMinutes;

            if (missingMinutes < 0)
                missingMinutes = 0;

            // ===== SET STATUS =====
            AttendanceStatus status;

            if (missingMinutes > 0)
                status = AttendanceStatus.InsufficientWork;
            else
                status = AttendanceStatus.Normal;

            // ===== UPDATE =====
            attendance.CheckOut = dto.CheckOutTime;
            attendance.CheckOutImagePath = imagePath;
            attendance.MissingMinutes = missingMinutes;
            attendance.Status = status;

            _attendanceRepository.Update(attendance);
            _attendanceRepository.Save();

            return ServiceResult.Success("Check-out thành công.");
        }
        private DateTime GetVietnamNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
            );
        }

    }
}