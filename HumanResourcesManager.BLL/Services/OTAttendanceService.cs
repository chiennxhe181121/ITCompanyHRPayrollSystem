using System.Linq;
using HumanResourcesManager.BLL.DTOs.Common;
using HumanResourcesManager.BLL.DTOs.Employee;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;

namespace HumanResourcesManager.BLL.Services
{
    public class OTAttendanceService : IOTAttendanceService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IOTRepository _otRepository;
        private readonly IOTAttendanceRepository _otAttendanceRepository;

        public OTAttendanceService(
            IEmployeeRepository employeeRepository,
            IOTRepository otRepository,
            IOTAttendanceRepository otAttendanceRepository)
        {
            _employeeRepository = employeeRepository;
            _otRepository = otRepository;
            _otAttendanceRepository = otAttendanceRepository;
        }

        public async Task<List<OverTimeRequest>> GetTodayOTs(int userId)
        {
            var employee = _employeeRepository.GetByUserId(userId);
            if (employee == null) return new List<OverTimeRequest>();

            var today = GetVietnamNow().Date;
            var list = await _otRepository.GetEmployeeOTByDateAsync(employee.EmployeeId, today);

            return list
                .Where(o => o.EmployeeAccepted == true
                            && o.Status != 3
                            && o.Status != Constants.Cancelled)
                .ToList();
        }

        public async Task<OTTodayAttendanceViewDTO?> GetTodayOTAttendance(int userId, int overTimeRequestId)
        {
            var employee = _employeeRepository.GetByUserId(userId);
            if (employee == null) return null;

            var ot = await _otRepository.GetByIdAsync(overTimeRequestId);
            if (ot == null || ot.EmployeeId != employee.EmployeeId) return null;

            var now = GetVietnamNow();
            var today = now.Date;
            if (ot.WorkDate.Date != today) return null;

            var checkIn = ot.OTAttendance != null && ot.OTAttendance.CheckIn != default ? ot.OTAttendance.CheckIn : (TimeSpan?)null;
            var checkOut = ot.OTAttendance != null && ot.OTAttendance.CheckOut != default ? ot.OTAttendance.CheckOut : (TimeSpan?)null;
            var missingMinutes = CalculateOTMissingMinutes(ot.StartTime, ot.EndTime, checkIn, checkOut);
            var resolvedStatus = ResolveOTAttendanceStatus(now, ot.WorkDate.Date, ot.StartTime, ot.EndTime, checkIn, checkOut, missingMinutes);

            var dto = new OTTodayAttendanceViewDTO
            {
                OverTimeRequestId = ot.Id,
                WorkDate = ot.WorkDate.Date,
                StartTime = ot.StartTime,
                EndTime = ot.EndTime,
                CheckInTime = checkIn,
                CheckOutTime = checkOut,
                CheckInImagePath = ot.OTAttendance?.CheckInImagePath,
                CheckOutImagePath = ot.OTAttendance?.CheckOutImagePath,
                Status = resolvedStatus
            };

            return dto;
        }

        public async Task<EmployeeOTAttendanceHistoryViewDTO> GetHistory(int userId, int page, int pageSize, int? month, int? year)
        {
            var employee = _employeeRepository.GetByUserId(userId);
            if (employee == null)
            {
                return new EmployeeOTAttendanceHistoryViewDTO
                {
                    CurrentPage = 1,
                    TotalPages = 1,
                    PageSize = pageSize <= 0 ? 10 : pageSize,
                    TotalRecords = 0,
                    SelectedMonth = month,
                    SelectedYear = year
                };
            }

            var (items, total) = await _otRepository.GetEmployeeOTHistoryAsync(employee.EmployeeId, page, pageSize, month, year);

            var totalPages = total == 0 ? 1 : (int)Math.Ceiling((double)total / pageSize);
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var now = GetVietnamNow();
            var rows = items.Select(o =>
                {
                    var checkIn = o.OTAttendance != null && o.OTAttendance.CheckIn != default ? o.OTAttendance.CheckIn : (TimeSpan?)null;
                    var checkOut = o.OTAttendance != null && o.OTAttendance.CheckOut != default ? o.OTAttendance.CheckOut : (TimeSpan?)null;
                    var missingMinutes = CalculateOTMissingMinutes(o.StartTime, o.EndTime, checkIn, checkOut);
                    var status = ResolveOTAttendanceStatus(now, o.WorkDate.Date, o.StartTime, o.EndTime, checkIn, checkOut, missingMinutes);

                    return new OTAttendanceRowDTO
                {
                    OverTimeRequestId = o.Id,
                    WorkDate = o.WorkDate.Date,
                    StartTime = o.StartTime,
                    EndTime = o.EndTime,
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    CheckInImagePath = o.OTAttendance?.CheckInImagePath,
                    CheckOutImagePath = o.OTAttendance?.CheckOutImagePath,
                    Status = status,
                    Reason = o.Reason,
                    MissingMinutes = missingMinutes
                };
                }).ToList();

            return new EmployeeOTAttendanceHistoryViewDTO
            {
                Records = rows,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalRecords = total,
                SelectedMonth = month,
                SelectedYear = year
            };
        }

        public async Task<double> GetMonthActualOTHoursAsync(int userId, int month, int year)
        {
            var employee = _employeeRepository.GetByUserId(userId);
            if (employee == null) return 0;

            var ots = await _otRepository.GetApprovedOTsAsync(employee.EmployeeId, month, year);
            return ots.Sum(ResolveActualOtHours);
        }

        /// <summary>
        /// Giờ OT thực tế: dùng ActualOTHours nếu đã lưu; nếu chưa, tính từ check-in/out trong khung OT (giống logic payroll khi có đủ dữ liệu).
        /// </summary>
        private static double ResolveActualOtHours(OverTimeRequest ot)
        {
            if (ot.OTAttendance == null) return 0;

            var a = ot.OTAttendance;
            if (a.ActualOTHours > 0) return a.ActualOTHours;

            if (a.CheckIn == default || a.CheckOut == default || a.CheckOut <= a.CheckIn)
                return 0;

            var effectiveIn = a.CheckIn < ot.StartTime ? ot.StartTime : a.CheckIn;
            var effectiveOut = a.CheckOut > ot.EndTime ? ot.EndTime : a.CheckOut;
            if (effectiveOut <= effectiveIn) return 0;

            return Math.Max(0, (effectiveOut - effectiveIn).TotalHours);
        }

        public async Task<ServiceResult> CheckIn(int userId, OTCheckInDTO dto)
        {
            var employee = _employeeRepository.GetByUserId(userId);
            if (employee == null)
                return ServiceResult.Failure("Không tìm thấy nhân viên.");

            var ot = await _otRepository.GetByIdAsync(dto.OverTimeRequestId);
            if (ot == null || ot.EmployeeId != employee.EmployeeId)
                return ServiceResult.Failure("Không tìm thấy OT hoặc bạn không có quyền chấm công OT này.");

            if (ot.EmployeeAccepted != true || ot.Status == 3 || ot.Status == Constants.Cancelled)
                return ServiceResult.Failure("OT này không hợp lệ để chấm công.");

            var now = GetVietnamNow();
            if (ot.WorkDate.Date != now.Date)
                return ServiceResult.Failure("Chỉ có thể chấm công OT trong đúng ngày OT.");

            if (dto.CheckInTime < ot.StartTime || dto.CheckInTime > ot.EndTime)
                return ServiceResult.Failure("Không nằm trong khung giờ OT để check-in.");

            var attendance = _otAttendanceRepository.GetByOverTimeRequestId(ot.Id);
            if (attendance != null && attendance.CheckIn != default)
                return ServiceResult.Failure("Bạn đã check-in OT rồi.");

            string? imagePath = null;
            if (dto.CheckInImage != null && dto.CheckInImage.Length > 0)
            {
                var folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "img",
                    "employees",
                    employee.EmployeeId.ToString(),
                    "ot-attendance"
                );
                Directory.CreateDirectory(folderPath);

                var fileName = $"ot-{ot.Id}-check-in-{now:yyyyMMddHHmmss}.jpg";
                var filePath = Path.Combine(folderPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.CheckInImage.CopyToAsync(stream);

                imagePath = $"/img/employees/{employee.EmployeeId}/ot-attendance/{fileName}";
            }

            if (attendance == null)
            {
                attendance = new OTAttendance
                {
                    OverTimeRequestId = ot.Id,
                    CheckIn = dto.CheckInTime,
                    CheckInImagePath = imagePath,
                    Status = DAL.Enum.AttendanceStatus.Pending
                };
                _otAttendanceRepository.Add(attendance);
            }
            else
            {
                attendance.CheckIn = dto.CheckInTime;
                attendance.CheckInImagePath = imagePath;
                _otAttendanceRepository.Update(attendance);
            }

            _otAttendanceRepository.Save();
            return ServiceResult.Success("Check-in OT thành công.");
        }

        public async Task<ServiceResult> CheckOut(int userId, OTCheckOutDTO dto)
        {
            var employee = _employeeRepository.GetByUserId(userId);
            if (employee == null)
                return ServiceResult.Failure("Không tìm thấy nhân viên.");

            var ot = await _otRepository.GetByIdAsync(dto.OverTimeRequestId);
            if (ot == null || ot.EmployeeId != employee.EmployeeId)
                return ServiceResult.Failure("Không tìm thấy OT hoặc bạn không có quyền chấm công OT này.");

            if (ot.EmployeeAccepted != true || ot.Status == 3 || ot.Status == Constants.Cancelled)
                return ServiceResult.Failure("OT này không hợp lệ để chấm công.");

            var now = GetVietnamNow();
            if (ot.WorkDate.Date != now.Date)
                return ServiceResult.Failure("Chỉ có thể chấm công OT trong đúng ngày OT.");

            if (dto.CheckOutTime < ot.StartTime || dto.CheckOutTime > ot.EndTime)
                return ServiceResult.Failure("Không nằm trong khung giờ OT để check-out.");

            var attendance = _otAttendanceRepository.GetByOverTimeRequestId(ot.Id);
            if (attendance == null || attendance.CheckIn == default)
                return ServiceResult.Failure("Bạn chưa check-in OT.");

            if (attendance.CheckOut != default)
                return ServiceResult.Failure("Bạn đã check-out OT rồi.");

            if (dto.CheckOutTime <= attendance.CheckIn)
                return ServiceResult.Failure("Check-out OT phải sau check-in OT.");

            string? imagePath = null;
            if (dto.CheckOutImage != null && dto.CheckOutImage.Length > 0)
            {
                var folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "img",
                    "employees",
                    employee.EmployeeId.ToString(),
                    "ot-attendance"
                );
                Directory.CreateDirectory(folderPath);

                var fileName = $"ot-{ot.Id}-check-out-{now:yyyyMMddHHmmss}.jpg";
                var filePath = Path.Combine(folderPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.CheckOutImage.CopyToAsync(stream);

                imagePath = $"/img/employees/{employee.EmployeeId}/ot-attendance/{fileName}";
            }

            attendance.CheckOut = dto.CheckOutTime;
            attendance.CheckOutImagePath = imagePath;

            // Align with normal attendance logic: CompletedWork vs InsufficientWork based on missing minutes
            var missingMinutes = CalculateOTMissingMinutes(ot.StartTime, ot.EndTime, attendance.CheckIn, attendance.CheckOut);
            attendance.Status = missingMinutes > 0 ? DAL.Enum.AttendanceStatus.InsufficientWork : DAL.Enum.AttendanceStatus.CompletedWork;

            _otAttendanceRepository.Update(attendance);
            _otAttendanceRepository.Save();

            return ServiceResult.Success("Check-out OT thành công.");
        }

        private static DateTime GetVietnamNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
            );
        }

        private static int CalculateOTMissingMinutes(TimeSpan start, TimeSpan end, TimeSpan? checkIn, TimeSpan? checkOut)
        {
            var expected = (int)Math.Max(0, (end - start).TotalMinutes);
            if (!checkIn.HasValue || !checkOut.HasValue)
                return expected;

            // Clamp inside OT window
            var effectiveIn = checkIn.Value < start ? start : checkIn.Value;
            var effectiveOut = checkOut.Value > end ? end : checkOut.Value;

            var actual = (int)Math.Max(0, (effectiveOut - effectiveIn).TotalMinutes);
            var missing = expected - actual;
            return missing > 0 ? missing : 0;
        }

        private static DAL.Enum.AttendanceStatus ResolveOTAttendanceStatus(
            DateTime now,
            DateTime workDate,
            TimeSpan start,
            TimeSpan end,
            TimeSpan? checkIn,
            TimeSpan? checkOut,
            int missingMinutes)
        {
            // If already has checkout -> conclude like normal attendance
            if (checkIn.HasValue && checkOut.HasValue)
            {
                return missingMinutes > 0
                    ? DAL.Enum.AttendanceStatus.InsufficientWork
                    : DAL.Enum.AttendanceStatus.CompletedWork;
            }

            // Missing checkout detection (same semantics as AttendanceStatus.MissingCheckOut)
            if (checkIn.HasValue && !checkOut.HasValue)
            {
                var isPastWorkDay = now.Date > workDate.Date;
                var isPastEndTimeSameDay = now.Date == workDate.Date && now.TimeOfDay > end;
                return (isPastWorkDay || isPastEndTimeSameDay)
                    ? DAL.Enum.AttendanceStatus.MissingCheckOut
                    : DAL.Enum.AttendanceStatus.Pending;
            }

            return DAL.Enum.AttendanceStatus.Pending;
        }
    }
}

