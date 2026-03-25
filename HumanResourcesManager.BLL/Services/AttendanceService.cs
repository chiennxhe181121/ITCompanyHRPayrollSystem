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
        private readonly ILeaveRequestRepository _leaveRequestRepo;

        public AttendanceService(
            IAttendanceRepository attendanceRepository,
            IEmployeeRepository employeeRepository,
            ILeaveRequestRepository leaveRequestRepo)
        {
            _attendanceRepository = attendanceRepository;
            _employeeRepository = employeeRepository;
            _leaveRequestRepo = leaveRequestRepo;
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
        public int CountAttendanceDays(int currentUserId, int month, int year)
        {
            var employee = _employeeRepository.GetByUserId(currentUserId);

            return employee == null
                ? throw new Exception("Employee not found")
                : _attendanceRepository.CountAttendanceDays(employee.EmployeeId, month, year);
        }

        public async Task<ServiceResult> CheckIn(int userId, CheckInDTO dto)
        {
            if (dto.CheckInTime < Constants.CheckInFrom ||
                dto.CheckInTime > Constants.CheckInTo)
            {
                return ServiceResult.Failure("Không nằm trong khung giờ check-in.");
            }

            var employee = _employeeRepository.GetByUserId(userId);
            if (employee == null)
                return ServiceResult.Failure("Không tìm thấy nhân viên.");

            var today = GetVietnamNow().Date;

            var attendance = _attendanceRepository
                .GetByEmployeeAndWorkDate(employee.EmployeeId, today);

            if (attendance == null)
                return ServiceResult.Failure("Attendance hôm nay chưa được tạo.");

            // Không cho check-in nếu không phải ngày làm việc
            if (attendance.Status == AttendanceStatus.Weekend)
                return ServiceResult.Failure("Hôm nay là cuối tuần.");

            if (attendance.Status == AttendanceStatus.Holiday)
                return ServiceResult.Failure("Hôm nay là ngày nghỉ lễ.");

            if (attendance.Status == AttendanceStatus.ApprovedLeave)
                return ServiceResult.Failure("Ngày này bạn nghỉ có phép.");

            if (attendance.CheckIn.HasValue)
                return ServiceResult.Failure("Bạn đã Check-in rồi.");

            // ===== LƯU ẢNH =====
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

            // ===== UPDATE =====
            attendance.CheckIn = dto.CheckInTime;
            attendance.CheckInImagePath = imagePath;

            _attendanceRepository.Update(attendance);
            _attendanceRepository.Save();

            return ServiceResult.Success("Check-in thành công.");
        }

        public async Task<ServiceResult> CheckOut(int userId, CheckOutDTO dto)
        {
            if (dto.CheckOutTime < Constants.CheckOutFrom ||
                dto.CheckOutTime > Constants.CheckOutTo)
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
                return ServiceResult.Failure("Attendance hôm nay chưa được tạo.");

            if (attendance.Status == AttendanceStatus.Weekend)
                return ServiceResult.Failure("Hôm nay là cuối tuần.");

            if (attendance.Status == AttendanceStatus.Holiday)
                return ServiceResult.Failure("Hôm nay là ngày nghỉ lễ.");

            if (attendance.Status == AttendanceStatus.ApprovedLeave)
                return ServiceResult.Failure("Ngày này bạn nghỉ có phép.");

            if (!attendance.CheckIn.HasValue)
                return ServiceResult.Failure("Bạn chưa Check-in.");

            if (attendance.CheckOut.HasValue)
                return ServiceResult.Failure("Bạn đã Check-out rồi.");

            if (dto.CheckOutTime <= attendance.CheckIn.Value)
                return ServiceResult.Failure("Check-out phải sau Check-in.");

            // ===== LƯU ẢNH =====
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

            // ===== TÍNH GIỜ LÀM =====
            var workStart = Constants.WorkStart; // 08:00
            var workEnd = Constants.WorkEnd;     // 17:00

            if (attendance.CheckIn is not TimeSpan checkIn)
                return ServiceResult.Failure("Bạn chưa Check-in.");

            if (dto.CheckOutTime is not TimeSpan checkOut)
                return ServiceResult.Failure("Chưa nhập check-out time.");

            // 🔥 Clamp check-in
            var effectiveCheckIn =
                checkIn < workStart
                    ? workStart
                    : checkIn;

            // 🔥 Clamp check-out
            var effectiveCheckOut =
                checkOut > workEnd
                    ? workEnd
                    : checkOut;

            // 🔥 Validate
            if (effectiveCheckOut <= effectiveCheckIn)
                return ServiceResult.Failure("Thời gian làm việc không hợp lệ.");

            // 🔥 Tính phút
            var totalMinutes =
                (int)(effectiveCheckOut - effectiveCheckIn).TotalMinutes;

            // 🔥 Trừ nghỉ trưa
            var actualWorkMinutes = totalMinutes - Constants.BREAK_MINUTES;

            if (actualWorkMinutes < 0)
                actualWorkMinutes = 0;

            // 🔥 Missing
            var missingMinutes =
                Constants.STANDARD_WORK_MINUTES - actualWorkMinutes;

            if (missingMinutes < 0)
                missingMinutes = 0;

            // 🔥 Status
            AttendanceStatus status =
                missingMinutes > 0
                    ? AttendanceStatus.InsufficientWork
                    : AttendanceStatus.CompletedWork;

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

        public void GenerateDailyAttendance(DateTime today)
        {
            // Nếu đã tạo rồi thì không tạo lại
            var existing = _attendanceRepository.GetByDate(today);

            if (existing.Any())
                return;

            bool isWeekend =
                today.DayOfWeek == DayOfWeek.Saturday ||
                today.DayOfWeek == DayOfWeek.Sunday;

            bool isFixedHoliday = Constants.FixedHolidays
                .Any(h => h.Day == today.Day && h.Month == today.Month);

            var tetDays = LunarHelper.GetTetHolidayDates(today.Year);

            bool isTetHoliday = tetDays
                .Any(d => d.Date == today.Date);

            bool isHoliday = isFixedHoliday || isTetHoliday;

            var employees = _employeeRepository
                .GetAll()
                .Where(e => e.Status == Constants.Active)
                .ToList();

            foreach (var emp in employees)
            {
                var approvedLeave = _leaveRequestRepo
                    .GetAll()
                    .FirstOrDefault(l =>
                        l.EmployeeId == emp.EmployeeId &&
                        l.Status == RequestStatus.Approved &&
                        today.Date >= l.FromDate.Date &&
                        today.Date <= l.ToDate.Date
                    );

                AttendanceStatus status;

                if (approvedLeave != null)
                {
                    status = AttendanceStatus.ApprovedLeave;
                }
                else if (isHoliday)
                {
                    status = AttendanceStatus.Holiday;
                }
                else if (isWeekend)
                {
                    status = AttendanceStatus.Weekend;
                }
                else
                {
                    status = AttendanceStatus.Pending;
                }

                _attendanceRepository.Add(new Attendance
                {
                    EmployeeId = emp.EmployeeId,
                    WorkDate = today,
                    Status = status,
                    MissingMinutes = 0
                });
            }

            _attendanceRepository.Save();
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

            foreach (var attendance in attendances)
            {
                if (attendance.Status != AttendanceStatus.Pending)
                    continue;

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

                    attendance.Status =
                        duration.TotalHours >= 8
                            ? AttendanceStatus.CompletedWork
                            : AttendanceStatus.InsufficientWork;
                }

                _attendanceRepository.Update(attendance);
            }

            _attendanceRepository.Save();
        }
    }
}