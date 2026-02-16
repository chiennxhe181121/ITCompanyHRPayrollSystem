using HumanResourcesManager.BLL.DTOs.Common;
using HumanResourcesManager.BLL.DTOs.Employee;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Enum;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;
using Microsoft.EntityFrameworkCore;

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
                Status = a.Status
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

        public TodayAttendanceViewDTO GetTodayAttendance(int currentUserId)
        {
            var employee = _employeeRepository.GetByUserId(currentUserId);

            if (employee == null)
                throw new Exception("Employee not found.");

            var today = DateTime.Today;

            var attendance = _attendanceRepository
                .GetQueryableByEmployeeId(employee.EmployeeId)
                .FirstOrDefault(a => a.WorkDate == today);

            return new TodayAttendanceViewDTO
            {
                WorkDate = today,
                CheckInTime = attendance?.CheckIn,
                CheckOutTime = attendance?.CheckOut,
                Status = attendance?.Status
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

            // ❗ Không cho check-in vào thứ 7 / chủ nhật
            if (today.DayOfWeek == DayOfWeek.Saturday ||
                today.DayOfWeek == DayOfWeek.Sunday)
            {
                return ServiceResult.Failure("Hôm nay là cuối tuần, không thể check-in.");
            }

            var existingAttendance = _attendanceRepository
                .GetByEmployeeAndWorkDate(employee.EmployeeId, today);

            if (existingAttendance != null)
            {
                if (existingAttendance.Status == AttendanceStatus.ApprovedLeave)
                    return ServiceResult.Failure("Ngày này bạn nghỉ có phép.");

                if (existingAttendance.Status == AttendanceStatus.Holiday)
                {
                    return ServiceResult.Failure("Hôm nay là ngày nghỉ lễ.");
                }

                if (existingAttendance.Status == AttendanceStatus.Pending
                    && !existingAttendance.CheckIn.HasValue)
                {
                    // cho phép ghi đè checkin
                }
                else
                {
                    return ServiceResult.Failure("Bạn đã chấm công ngày này rồi.");
                }
            }

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
                Status = AttendanceStatus.Pending,
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

            // ❗ Không cho check-out vào thứ 7 / chủ nhật
            if (today.DayOfWeek == DayOfWeek.Saturday ||
                today.DayOfWeek == DayOfWeek.Sunday)
            {
                return ServiceResult.Failure("Hôm nay là cuối tuần, không thể check-out.");
            }

            var attendance = _attendanceRepository
                .GetByEmployeeAndWorkDate(employee.EmployeeId, today);

            if (attendance != null
    && attendance.Status == AttendanceStatus.ApprovedLeave)
            {
                return ServiceResult.Failure("Ngày này bạn nghỉ có phép.");
            }

            if (attendance != null
    && attendance.Status == AttendanceStatus.Holiday)
            {
                return ServiceResult.Failure("Hôm nay là ngày nghỉ lễ.");
            }

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

                var fileName = $"check-out-{today:yyyyMMdd}.jpg";
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
                status = AttendanceStatus.CompletedWork;

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

        public void FinalizeDailyAttendance(DateTime now)
        {
            var cutoff = now.Date + Constants.CheckOutTo;

            if (now < cutoff)
                return;

            var workDate = now.Date;

            // 1️⃣ Lấy tất cả nhân viên
            var employees = _employeeRepository.GetAll().Where(e => e.Status == Constants.Active);

            // 2️⃣ Lấy tất cả attendance trong ngày
            var attendances = _attendanceRepository.GetByDate(workDate);

            foreach (var employee in employees)
            {
                var attendance = attendances
                    .FirstOrDefault(a => a.EmployeeId == employee.EmployeeId);

                // ❗ Nếu chưa có record → Absent
                if (attendance == null)
                {
                    _attendanceRepository.Add(new Attendance
                    {
                        EmployeeId = employee.EmployeeId,
                        WorkDate = workDate,
                        Status = AttendanceStatus.Absent
                    });

                    continue;
                }

                // ❗ Chỉ xử lý khi đang Pending
                if (attendance.Status != AttendanceStatus.Pending)
                {
                    continue; // Holiday, Weekend, ApprovedLeave... bỏ qua
                }

                // ❗ Finalize Pending
                if (attendance.CheckIn == null)
                {
                    attendance.Status = AttendanceStatus.Absent;
                }
                else if (attendance.CheckOut == null)
                {
                    attendance.Status = AttendanceStatus.MissingCheckOut;
                }
                else
                {
                    var duration = attendance.CheckOut.Value - attendance.CheckIn.Value;

                    attendance.Status = duration.TotalHours >= 8
                        ? AttendanceStatus.CompletedWork
                        : AttendanceStatus.InsufficientWork;
                }

                _attendanceRepository.Update(attendance);
            }

            _attendanceRepository.Save();
        }

        public void GenerateSpecialDayAttendance(DateTime today)
        {
            bool isWeekend =
                today.DayOfWeek == DayOfWeek.Saturday ||
                today.DayOfWeek == DayOfWeek.Sunday;

            bool isFixedHoliday = Constants.FixedHolidays
                .Any(h => h.Day == today.Day && h.Month == today.Month);

            bool isTetHoliday = LunarHelper
                .GetTetHolidayDates(today.Year)
                .Contains(today);

            if (!isWeekend && !isFixedHoliday && !isTetHoliday)
                return;

            var employees = _employeeRepository
                .GetAll()
                .Where(e => e.Status == Constants.Active)
                .ToList();

            // Lấy tất cả attendance của ngày đó 1 lần
            var existingAttendances = _attendanceRepository
                .GetByDate(today)
                .ToDictionary(a => a.EmployeeId);

            foreach (var emp in employees)
            {
                if (existingAttendances.ContainsKey(emp.EmployeeId))
                    continue;

                _attendanceRepository.Add(new Attendance
                {
                    EmployeeId = emp.EmployeeId,
                    WorkDate = today,
                    Status = isWeekend
                        ? AttendanceStatus.Weekend
                        : AttendanceStatus.Holiday,
                    MissingMinutes = 0
                });
            }

            _attendanceRepository.Save();
        }
    }
}