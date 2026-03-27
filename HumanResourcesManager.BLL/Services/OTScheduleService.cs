using System;
using System.Collections.Generic;
using System.Linq;
using HumanResourcesManager.BLL.DTOs.Manager;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;

namespace HumanResourcesManager.BLL.Services
{
    public class OTScheduleService : IOTScheduleService
    {
        private readonly IOTScheduleRepository _scheduleRepo;
        private readonly IEmployeeRepository _employeeRepo;

        public OTScheduleService(IOTScheduleRepository scheduleRepo, IEmployeeRepository employeeRepo)
        {
            _scheduleRepo = scheduleRepo;
            _employeeRepo = employeeRepo;
        }

        private static DateTime GetScheduleStartDateTime(DateTime startDate, TimeSpan startTime)
            => startDate.Date.Add(startTime);

        // EndDate được tính LÀ ngày OT (end-inclusive).
        private static DateTime GetScheduleEndDateTime(DateTime endDate, TimeSpan endTime)
            => endDate.Date.Add(endTime);

        private static bool IsTimeOverlap(TimeSpan aStart, TimeSpan aEnd, TimeSpan bStart, TimeSpan bEnd)
            => aStart < bEnd && bStart < aEnd;

        private static bool IsWeekend(DateTime date)
            => date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

        private static bool TryApplyAndValidateOtTimeWindow(OTScheduleCreateDTO dto, out string message)
        {
            message = string.Empty;

            var isWeekend = IsWeekend(dto.StartDate.Date);
            var minStart = isWeekend ? Constants.OTWeekendStart : Constants.OTWeekdayStart;
            var maxHours = isWeekend ? Constants.OTWeekendMaxHours : Constants.OTWeekdayMaxHours;

            if (dto.DurationHours < 1 || dto.DurationHours > maxHours)
            {
                message = isWeekend
                    ? $"Thứ 7/Chủ nhật chỉ được tạo OT tối đa {Constants.OTWeekendMaxHours} tiếng."
                    : $"Ngày trong tuần chỉ được tạo OT tối đa {Constants.OTWeekdayMaxHours} tiếng.";
                return false;
            }

            if (dto.StartTime < minStart)
            {
                message = isWeekend
                    ? $"Thứ 7/Chủ nhật chỉ được bắt đầu từ {Constants.OTWeekendStart:hh\\:mm}."
                    : $"Ngày trong tuần chỉ được bắt đầu từ {Constants.OTWeekdayStart:hh\\:mm}.";
                return false;
            }

            var startDateTime = dto.StartDate.Date.Add(dto.StartTime);
            var calculatedEndDateTime = startDateTime.AddHours(dto.DurationHours);
            var cutoffDateTime = dto.StartDate.Date.AddDays(1).Add(Constants.OTNextDayCutoff);

            if (calculatedEndDateTime > cutoffDateTime)
            {
                message = $"Ca OT chỉ được kéo dài đến {Constants.OTNextDayCutoff:hh\\:mm} sáng ngày hôm sau.";
                return false;
            }

            dto.EndTime = calculatedEndDateTime.TimeOfDay;
            return true;
        }

        private static int CalculateDurationHours(TimeSpan startTime, TimeSpan endTime)
        {
            var duration = endTime - startTime;
            if (duration <= TimeSpan.Zero)
            {
                duration = duration.Add(TimeSpan.FromHours(24));
            }

            var hours = (int)Math.Round(duration.TotalHours, MidpointRounding.AwayFromZero);
            return Math.Clamp(hours, 1, Constants.OTWeekendMaxHours);
        }

        // Runtime status for OT schedule:
        // 5 = Upcoming, 0 = Active, 3 = Expired
        private static int ResolveRuntimeStatus(DateTime now, DateTime startDate, TimeSpan startTime, DateTime endDate, TimeSpan endTime)
        {
            var startDateTime = GetScheduleStartDateTime(startDate, startTime);
            var endDateTime = GetScheduleEndDateTime(endDate, endTime);

            if (now < startDateTime)
            {
                return 5;
            }

            // Hết hạn ngay tại thời điểm endTime
            if (now >= endDateTime)
            {
                return 3;
            }

            return 0;
        }

        private static DateTime? GetNextOccurrenceStart(DateTime now, DateTime scheduleStartDate, DateTime scheduleEndDate, TimeSpan startTime)
        {
            if (now.Date < scheduleStartDate.Date)
            {
                return scheduleStartDate.Date.Add(startTime);
            }

            if (now.Date > scheduleEndDate.Date)
            {
                return null;
            }

            var todayOccurrence = now.Date.Add(startTime);
            if (now < todayOccurrence)
            {
                return todayOccurrence;
            }

            var tomorrow = now.Date.AddDays(1);
            if (tomorrow <= scheduleEndDate.Date)
            {
                return tomorrow.Add(startTime);
            }

            return null;
        }

        public bool CreateOTSchedule(int managerUserId, OTScheduleCreateDTO dto, out string message)
        {
            message = string.Empty;
            var manager = _employeeRepo.GetByUserId(managerUserId);
            if (manager == null || manager.DepartmentId <= 0)
            {
                message = "Không tìm thấy thông tin quản lý hoặc quản lý chưa thuộc phòng ban nào.";
                return false;
            }

            // Chỉ cho phép tạo OT theo từng ngày: EndDate = StartDate
            dto.EndDate = dto.StartDate.Date;

            // EndDate là end-inclusive => cho phép EndDate == StartDate (OT 1 ngày)
            if (dto.EndDate.Date < dto.StartDate.Date)
            {
                message = $"Ngày kết thúc phải từ {dto.StartDate:dd/MM/yyyy} trở đi.";
                return false;
            }

            if (!TryApplyAndValidateOtTimeWindow(dto, out message))
            {
                return false;
            }

            var now = DateTime.Now;
            var initialStatus = ResolveRuntimeStatus(now, dto.StartDate, dto.StartTime, dto.EndDate, dto.EndTime);

            // Không cho phép tạo lịch OT đã hết hạn (retroactive creation)
            if (initialStatus == 3)
            {
                var endDateTime = GetScheduleEndDateTime(dto.EndDate, dto.EndTime);
                message = $"Không thể tạo lịch OT vì thời gian OT đã qua. Vui lòng chọn thời gian kết thúc sau thời điểm hiện tại.";
                return false;
            }

            var schedule = new OTSchedule
            {
                Name = dto.Name,
                Description = dto.Description,
                Quantity = dto.Quantity,
                StartDate = dto.StartDate.Date,
                EndDate = dto.EndDate.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                ManagerId = manager.EmployeeId,
                DepartmentId = manager.DepartmentId,
                Status = initialStatus,
                CreatedAt = DateTime.Now
            };

            _scheduleRepo.Add(schedule);
            _scheduleRepo.Save();

            message = initialStatus == 5 ? "Tạo Lịch OT thành công (Trạng thái: Sắp tới)." : "Tạo Lịch OT thành công (Trạng thái: Đang hoạt động).";
            return true;
        }

        public IEnumerable<OTScheduleDTO> GetManagerSchedules(int managerUserId)
        {
            var manager = _employeeRepo.GetByUserId(managerUserId);
            if (manager == null) return Enumerable.Empty<OTScheduleDTO>();

            var list = _scheduleRepo.GetByManager(manager.EmployeeId);
            return list.Select(s => new OTScheduleDTO
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Quantity = s.Quantity,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                DepartmentId = s.DepartmentId,
                ManagerId = s.ManagerId,
                ManagerName = s.Manager?.FullName ?? "",
                Status = s.Status,
                CreatedAt = s.CreatedAt,
                RegisteredCount = s.Registrations.Where(x => x.Status != 3).GroupBy(x => x.EmployeeId).Count()
            });
        }

        public IEnumerable<OTScheduleDTO> GetDepartmentOpenSchedules(int employeeUserId)
        {
            var emp = _employeeRepo.GetByUserId(employeeUserId);
            if (emp == null || emp.DepartmentId <= 0) return Enumerable.Empty<OTScheduleDTO>();

            var list = _scheduleRepo.GetByDepartment(emp.DepartmentId);
            
            return list.Where(s => s.Status == 0 || s.Status == 5) // Hiện cả Open và Upcoming
                .Select(s => new OTScheduleDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Quantity = s.Quantity,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    DepartmentId = s.DepartmentId,
                    ManagerId = s.ManagerId,
                    ManagerName = s.Manager?.FullName ?? "",
                    Status = s.Status,
                    CreatedAt = s.CreatedAt,
                    RegisteredCount = s.Registrations.Where(x => x.Status != 3).GroupBy(x => x.EmployeeId).Count(),
                    IsRegisteredByCurrentUser = s.Registrations.Any(r => r.EmployeeId == emp.EmployeeId && r.Status != 3) // 3 = Cancelled
                });
        }

        public bool RegisterOT(int employeeUserId, int scheduleId, out string message)
        {
            message = string.Empty;
            var emp = _employeeRepo.GetByUserId(employeeUserId);
            if (emp == null)
            {
                message = "Không tìm thấy thông tin nhân viên.";
                return false;
            }

            var schedule = _scheduleRepo.GetById(scheduleId);
            if (schedule == null || (schedule.Status != 0 && schedule.Status != 5))
            {
                message = "Lịch OT không tồn tại, đã hủy hoặc đã kết thúc.";
                return false;
            }

            if (schedule.DepartmentId != emp.DepartmentId)
            {
                message = "Bạn không thuộc phòng ban của Lịch OT này.";
                return false;
            }

            // Đếm theo số người đăng ký duy nhất
            var uniqueRegistrationsCount = schedule.Registrations.Where(r => r.Status != 3).GroupBy(r => r.EmployeeId).Count();
            if (uniqueRegistrationsCount >= schedule.Quantity)
            {
                message = "Lịch OT này đã đủ số lượng đăng ký dự kiến.";
                return false;
            }

            if (schedule.Registrations.Any(r => r.EmployeeId == emp.EmployeeId && r.Status != 3))
            {
                message = "Bạn đã đăng ký Lịch OT này rồi.";
                return false;
            }

            var now = DateTime.Now;
            var startDateTime = GetScheduleStartDateTime(schedule.StartDate, schedule.StartTime);
            if (now >= startDateTime)
            {
                message = $"Không thể đăng ký vì lịch OT đã bắt đầu. Mốc bắt đầu: {startDateTime:dd/MM/yyyy HH:mm}. Hiện tại: {now:dd/MM/yyyy HH:mm}.";
                return false;
            }

            // 1) Chặn OT trùng thời gian hành chính (08:00-17:00 theo Constants)
            if (IsTimeOverlap(schedule.StartTime, schedule.EndTime, Constants.WorkStart, Constants.WorkEnd))
            {
                message = $"Không thể đăng ký: khung giờ OT ({schedule.StartTime:hh\\:mm}–{schedule.EndTime:hh\\:mm}) trùng với thời gian hành chính ({Constants.WorkStart:hh\\:mm}–{Constants.WorkEnd:hh\\:mm}).";
                return false;
            }

            // 2) Chặn OT chồng lấn với OT khác của chính nhân viên (theo từng ngày trong range)
            var overlap = _scheduleRepo.HasEmployeeOvertimeOverlap(emp.EmployeeId, schedule.StartDate, schedule.EndDate, schedule.StartTime, schedule.EndTime);
#if DEBUG
            try
            {
                var existing = _scheduleRepo.GetEmployeeOverTimeRequestsInRange(emp.EmployeeId, schedule.StartDate, schedule.EndDate).ToList();
                Console.WriteLine($"[OT-REGISTER] empId={emp.EmployeeId} scheduleId={schedule.Id} range={schedule.StartDate:yyyy-MM-dd}..{schedule.EndDate:yyyy-MM-dd} time={schedule.StartTime:hh\\:mm}-{schedule.EndTime:hh\\:mm} overlap={overlap} existingCount={existing.Count}");
                foreach (var r in existing.OrderBy(x => x.WorkDate).ThenBy(x => x.StartTime).Take(20))
                {
                    var isOverlap = r.StartTime < schedule.EndTime && schedule.StartTime < r.EndTime;
                    Console.WriteLine($"[OT-EXISTING] id={r.Id} workDate={r.WorkDate:yyyy-MM-dd} time={r.StartTime:hh\\:mm}-{r.EndTime:hh\\:mm} status={r.Status} scheduleId={r.OTScheduleId} overlapWithNew={isOverlap}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[OT-REGISTER] debug log failed: " + ex.Message);
            }
#endif
            if (overlap)
            {
                message = "Không thể đăng ký: bạn đã có OT khác bị trùng thời gian trong khoảng ngày đăng ký.";
                return false;
            }

            // EndDate là end-inclusive => tạo đăng ký cho các ngày: [StartDate, EndDate]
            for (var date = schedule.StartDate.Date; date <= schedule.EndDate.Date; date = date.AddDays(1))
            {
                var existingForDate = schedule.Registrations.FirstOrDefault(r => r.EmployeeId == emp.EmployeeId && r.WorkDate.Date == date && r.Status == 3);
                if (existingForDate != null)
                {
                    existingForDate.Status = 1; // Approved
                    existingForDate.EmployeeAccepted = true;
                }
                else
                {
                    var otRequest = new OverTimeRequest
                    {
                        EmployeeId = emp.EmployeeId,
                        WorkDate = date,
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime,
                        Reason = schedule.Name,
                        TaskRef = "Lịch OT Chung",
                        ManagerId = schedule.ManagerId,
                        Status = 1, // Approved
                        EmployeeAccepted = true,
                        OTScheduleId = schedule.Id
                    };
                    schedule.Registrations.Add(otRequest);
                }
            }

            _scheduleRepo.Update(schedule);
            _scheduleRepo.Save();

            message = "Đăng ký Lịch OT thành công.";
            return true;
        }

        public bool CancelRegistration(int employeeUserId, int scheduleId, out string message)
        {
            message = string.Empty;
            var emp = _employeeRepo.GetByUserId(employeeUserId);
            if (emp == null)
            {
                message = "Không tìm thấy thông tin nhân viên.";
                return false;
            }

            var schedule = _scheduleRepo.GetById(scheduleId);
            if (schedule == null)
            {
                message = "Lịch OT không tồn tại.";
                return false;
            }

            var regsToCancel = schedule.Registrations.Where(r => r.EmployeeId == emp.EmployeeId && r.Status != 3).ToList();
            if (!regsToCancel.Any())
            {
                message = "Bạn chưa đăng ký Lịch OT này hoặc đăng ký đã bị hủy.";
                return false;
            }

            // Chặn hủy: Không cho hủy khi đã bắt đầu (ngày bắt đầu + giờ bắt đầu)
            var startDateTime = GetScheduleStartDateTime(schedule.StartDate, schedule.StartTime);
            if (DateTime.Now >= startDateTime)
            {
                message = $"Không thể hủy vì lịch OT đã bắt đầu. Mốc bắt đầu: {startDateTime:dd/MM/yyyy HH:mm}. Hiện tại: {DateTime.Now:dd/MM/yyyy HH:mm}.";
                return false;
            }

            foreach (var reg in regsToCancel)
            {
                reg.Status = 3; // Cancelled
                reg.EmployeeAccepted = false;
            }

            _scheduleRepo.Update(schedule);
            _scheduleRepo.Save();

            message = "Đã hủy đăng ký thành công.";
            return true;
        }

        public bool RemoveEmployeeFromSchedule(
            int managerUserId,
            int scheduleId,
            int employeeId,
            out string message,
            out (string Email, string Name)? removedEmployee)
        {
            message = string.Empty;
            removedEmployee = null;

            var manager = _employeeRepo.GetByUserId(managerUserId);
            if (manager == null)
            {
                message = "Không tìm thấy thông tin quản lý.";
                return false;
            }

            var schedule = _scheduleRepo.GetById(scheduleId);
            if (schedule == null)
            {
                message = "Lịch OT không tồn tại.";
                return false;
            }

            if (schedule.ManagerId != manager.EmployeeId)
            {
                message = "Bạn không có quyền thao tác với Lịch OT này.";
                return false;
            }

            if (schedule.Status == 2 || schedule.Status == 4)
            {
                message = "Không thể thao tác với lịch OT đã hủy hoặc đã hoàn thành.";
                return false;
            }

            var emp = _employeeRepo.GetById(employeeId);
            if (emp == null)
            {
                message = "Không tìm thấy thông tin nhân viên.";
                return false;
            }

            if (emp.DepartmentId != schedule.DepartmentId)
            {
                message = "Nhân viên này không thuộc phòng ban của lịch OT.";
                return false;
            }

            var regsToCancel = schedule.Registrations
                .Where(r => r.EmployeeId == employeeId && r.Status != 3)
                .ToList();

            if (!regsToCancel.Any())
            {
                message = "Nhân viên này hiện không còn tham gia lịch OT hoặc đã được hủy trước đó.";
                return false;
            }

            // Manager xóa hẳn đăng ký OT khỏi hệ thống (không để trạng thái "đã hủy").
            _scheduleRepo.RemoveRegistrations(regsToCancel);
            _scheduleRepo.Save();

            removedEmployee = (emp.Email ?? string.Empty, emp.FullName);

            message = $"Đã xóa \"{emp.FullName}\" khỏi danh sách tham gia OT.";
            return true;
        }

        public void CancelExpiredSchedules()
        {
            var now = DateTime.Now;

            // 0. Nếu đã tới thời gian OT mà không ai đăng ký/nhận → tự hết hạn
            var startedOpenSchedules = _scheduleRepo.GetStartedOpenSchedules(now);
            foreach (var schedule in startedOpenSchedules)
            {
                var hasAnyActiveRegistration = schedule.Registrations
                    .Any(r => r.Status != 3); // 3 = Cancelled

                if (!hasAnyActiveRegistration)
                {
                    schedule.Status = 3; // Expired
                    _scheduleRepo.Update(schedule);
                }
            }

            // 1. Kích hoạt các lịch Sắp tới (5 -> 0)
            var upcomingToActivate = _scheduleRepo.GetUpcomingToActivateSchedules(now);
            foreach (var schedule in upcomingToActivate)
            {
                var endDateTime = GetScheduleEndDateTime(schedule.EndDate, schedule.EndTime);
                if (now > endDateTime)
                {
                    schedule.Status = 3; // Hết hạn luôn nếu cực ngắn
                }
                else
                {
                    // Nếu tới giờ OT mà không ai nhận thì chuyển hết hạn
                    var hasAnyActiveRegistration = schedule.Registrations.Any(r => r.Status != 3);
                    schedule.Status = hasAnyActiveRegistration ? 0 : 3;
                }
                _scheduleRepo.Update(schedule);
            }

            // 2. Kết thúc các lịch đã qua thời gian (Status 0 hoặc 5 sang Status 3)
            var expiredList = _scheduleRepo.GetExpiredOpenSchedules(now);
            foreach (var schedule in expiredList)
            {
                schedule.Status = 3;
                _scheduleRepo.Update(schedule);
            }

            // 3. (Sửa lỗi) Chuyển Đang hoạt động về Sắp tới nếu thực tế ở tương lai
            var schedulesToDeactivate = _scheduleRepo.GetSchedulesToDeactivate(now);
            foreach (var schedule in schedulesToDeactivate)
            {
                schedule.Status = 5;
                _scheduleRepo.Update(schedule);
            }

            _scheduleRepo.Save();
        }

        public bool CancelOTSchedule(int managerUserId, int scheduleId, out string message, out List<(string Email, string Name)> notifyList)
        {
            message = string.Empty;
            notifyList = new List<(string Email, string Name)>();
            var manager = _employeeRepo.GetByUserId(managerUserId);
            if (manager == null)
            {
                message = "Không tìm thấy thông tin quản lý.";
                return false;
            }

            var schedule = _scheduleRepo.GetById(scheduleId);
            if (schedule == null)
            {
                message = "Lịch OT không tồn tại.";
                return false;
            }

            if (schedule.ManagerId != manager.EmployeeId)
            {
                message = "Bạn không có quyền thao tác với Lịch OT này.";
                return false;
            }

            if (schedule.Status == 2)
            {
                message = "Lịch OT này đã bị hủy trước đó.";
                return false;
            }

            // Cho phép Manager hủy mọi lúc theo yêu cầu, chỉ cần lấy danh sách nhân viên đã đăng ký
            notifyList = schedule.Registrations
                .Where(r => r.Status != 3 && r.Employee != null && !string.IsNullOrWhiteSpace(r.Employee.Email))
                .Select(r => (r.Employee!.Email, r.Employee!.FullName))
                .Distinct()
                .ToList();

            schedule.Status = 2; // Cancelled
            // Cancel all registrations
            foreach (var reg in schedule.Registrations.Where(r => r.Status != 3))
            {
                reg.Status = 3;
                reg.EmployeeAccepted = false;
            }

            _scheduleRepo.Update(schedule);
            _scheduleRepo.Save();

            message = "Đã hủy lịch OT thành công.";
            return true;
        }

        public bool ReopenOTSchedule(int managerUserId, int scheduleId, out string message)
        {
            message = string.Empty;
            var manager = _employeeRepo.GetByUserId(managerUserId);
            if (manager == null)
            {
                message = "Không tìm thấy thông tin quản lý.";
                return false;
            }

            var schedule = _scheduleRepo.GetById(scheduleId);
            if (schedule == null)
            {
                message = "Lịch OT không tồn tại.";
                return false;
            }

            if (schedule.ManagerId != manager.EmployeeId)
            {
                message = "Bạn không có quyền thao tác với Lịch OT này.";
                return false;
            }

            if (schedule.Status != 2)
            {
                message = "Chỉ có thể mở lại Lịch OT đã bị hủy.";
                return false;
            }

            var startDateTime = GetScheduleStartDateTime(schedule.StartDate, schedule.StartTime);
            var endDateTime = GetScheduleEndDateTime(schedule.EndDate, schedule.EndTime);
            var now = DateTime.Now;

            if (now >= startDateTime && now <= endDateTime)
            {
                message = "Không thể mở lại khi đang trong thời gian OT.";
                return false;
            }
            if (now > endDateTime)
            {
                message = "Lịch OT đã kết thúc, không thể mở lại.";
                return false;
            }

            // Mở lại lịch -> xác định theo thời điểm hiện tại
            schedule.Status = ResolveRuntimeStatus(now, schedule.StartDate, schedule.StartTime, schedule.EndDate, schedule.EndTime);
            _scheduleRepo.Update(schedule);
            _scheduleRepo.Save();

            message = "Đã mở lại Lịch OT thành công.";
            return true;
        }

        public OTScheduleDetailDTO? GetOTScheduleDetails(int managerUserId, int scheduleId)
        {
            var manager = _employeeRepo.GetByUserId(managerUserId);
            if (manager == null) return null;

            var schedule = _scheduleRepo.GetById(scheduleId);
            if (schedule == null || schedule.ManagerId != manager.EmployeeId)
            {
                return null;
            }

            var detail = new OTScheduleDetailDTO
            {
                Id = schedule.Id,
                Name = schedule.Name,
                Description = schedule.Description,
                Quantity = schedule.Quantity,
                StartDate = schedule.StartDate,
                EndDate = schedule.EndDate,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                DepartmentId = schedule.DepartmentId,
                ManagerId = schedule.ManagerId,
                ManagerName = schedule.Manager?.FullName ?? "",
                Status = schedule.Status,
                CreatedAt = schedule.CreatedAt,
                RegisteredCount = schedule.Registrations.Where(r => r.Status != 3).GroupBy(r => r.EmployeeId).Count(),
                Registrations = schedule.Registrations
                    .GroupBy(r => r.EmployeeId)
                    .Select(g => {
                        var first = g.OrderByDescending(r => r.WorkDate).First();
                        var anyActive = g.Any(r => r.Status != 3);
                        return new OTScheduleRegistrationDTO
                        {
                            EmployeeId = g.Key,
                            EmployeeName = first.Employee?.FullName ?? "",
                            Position = first.Employee?.Position?.PositionName ?? "",
                            Avatar = first.Employee?.ImgAvatar ?? "",
                            Status = anyActive ? 1 : 3, // 1 = Active/Tham gia, 3 = Cancelled
                            RegisteredAt = schedule.CreatedAt
                        };
                    }).ToList()
            };

            return detail;
        }

        public OTScheduleCreateDTO? GetScheduleForEdit(int managerUserId, int scheduleId)
        {
            var manager = _employeeRepo.GetByUserId(managerUserId);
            if (manager == null) return null;

            var schedule = _scheduleRepo.GetById(scheduleId);
            if (schedule == null || schedule.ManagerId != manager.EmployeeId) return null;

            // Chỉ cho sửa khi đang Mở (0), hoặc Đã Hết Hạn (3)/(1) để gia hạn
            if (schedule.Status == 2 || schedule.Status == 4) return null;

            return new OTScheduleCreateDTO
            {
                Name = schedule.Name,
                Description = schedule.Description,
                Quantity = schedule.Quantity,
                StartDate = schedule.StartDate,
                EndDate = schedule.EndDate,
                StartTime = schedule.StartTime,
                DurationHours = CalculateDurationHours(schedule.StartTime, schedule.EndTime),
                EndTime = schedule.EndTime,
                Status = schedule.Status
            };
        }

        public bool UpdateOTSchedule(int managerUserId, int scheduleId, OTScheduleCreateDTO dto, out string message, out List<(string Email, string Name)> notifyList)
        {
            message = string.Empty;
            notifyList = new List<(string Email, string Name)>();

            // Chỉ cho phép OT theo từng ngày: EndDate luôn bằng StartDate
            dto.EndDate = dto.StartDate.Date;

            // EndDate là end-inclusive => cho phép EndDate == StartDate (OT 1 ngày)
            if (dto.EndDate.Date < dto.StartDate.Date)
            {
                message = $"Ngày kết thúc phải từ {dto.StartDate:dd/MM/yyyy} trở đi.";
                return false;
            }

            if (!TryApplyAndValidateOtTimeWindow(dto, out message))
            {
                return false;
            }

            var manager = _employeeRepo.GetByUserId(managerUserId);
            if (manager == null)
            {
                message = "Không tìm thấy thông tin quản lý.";
                return false;
            }

            var schedule = _scheduleRepo.GetById(scheduleId);
            if (schedule == null || schedule.ManagerId != manager.EmployeeId)
            {
                message = "Lịch OT không tồn tại hoặc bạn không có quyền chỉnh sửa.";
                return false;
            }

            if (schedule.Status == 2 || schedule.Status == 4)
            {
                message = "Không thể chỉnh sửa lịch OT đã hủy hoặc đã hoàn thành.";
                return false;
            }

            // Thu thập danh sách nhân viên đang active (chưa hủy) trong lịch này
            notifyList = schedule.Registrations
                .Where(r => r.Status != 3 && r.Employee != null && !string.IsNullOrWhiteSpace(r.Employee.Email))
                .Select(r => (r.Employee!.Email, r.Employee!.FullName))
                .Distinct()
                .ToList();

            schedule.Description = dto.Description;
            schedule.Quantity = dto.Quantity;

            var now = DateTime.Now;
            var firstOccurrenceStart = schedule.StartDate.Date.Add(schedule.StartTime);
            var isExpiredSchedule = schedule.Status == 3 || schedule.Status == 1;

            // 1. Khóa StartDate: Chỉ cho sửa nếu phiên OT đầu tiên chưa diễn ra
            bool canEditStart = now < firstOccurrenceStart;
            // Với lịch đã hết hạn: KHÔNG cho sửa ngày bắt đầu
            if (canEditStart && !isExpiredSchedule)
            {
                schedule.StartDate = dto.StartDate.Date;
            }

            // 2. Khóa StartTime/EndTime:
            // Chỉ cho sửa khi còn ít nhất 1 tiếng trước phiên OT gần nhất sắp diễn ra.
            var nextOccurrenceStart = GetNextOccurrenceStart(now, schedule.StartDate, schedule.EndDate, schedule.StartTime);
            // Với lịch đã hết hạn: cho phép sửa giờ bắt đầu/kết thúc tự do (chỉ cần StartTime < EndTime)
            bool canEditTimes = isExpiredSchedule || (nextOccurrenceStart.HasValue && now < nextOccurrenceStart.Value.AddHours(-1));
            bool timesChanged = false;

            var requestedTimesChanged = schedule.StartTime != dto.StartTime || schedule.EndTime != dto.EndTime;
            if (requestedTimesChanged && !canEditTimes)
            {
                if (nextOccurrenceStart.HasValue)
                {
                    message = "Bạn chỉ có thể sửa khung giờ OT trước giờ bắt đầu ít nhất 1 tiếng.";
                }
                else
                {
                    message = "Không thể sửa khung giờ OT lúc này. Vui lòng thử lại sau.";
                }
                return false;
            }

            if (canEditTimes)
            {
                if (requestedTimesChanged)
                {
                    timesChanged = true;
                }
                schedule.StartTime = dto.StartTime;
                schedule.EndTime = dto.EndTime;
            }

            schedule.EndDate = dto.EndDate.Date;

            // 3. Nếu giờ đổi, cập nhật các bản đăng ký OverTimeRequest chưa diễn ra
            if (timesChanged)
            {
                foreach (var reg in schedule.Registrations.Where(r => r.Status != 3))
                {
                    // Chỉ cập nhật cho các ngày chưa diễn ra hoặc phiên hôm nay chưa bắt đầu
                    var regStart = reg.WorkDate.Date.Add(dto.StartTime);
                    if (now < regStart.AddHours(-1))
                    {
                        reg.StartTime = dto.StartTime;
                        reg.EndTime = dto.EndTime;
                    }
                }
            }
            
            // Re-evaluate Status sau khi sửa (không đè trạng thái đặc biệt: 2=hủy, 4=hoàn thành)
            if (schedule.Status == 0 || schedule.Status == 5 || schedule.Status == 1 || schedule.Status == 3)
            {
                schedule.Status = ResolveRuntimeStatus(now, schedule.StartDate, schedule.StartTime, schedule.EndDate, schedule.EndTime);
            }

            _scheduleRepo.Update(schedule);
            _scheduleRepo.Save();

            message = "Cập nhật lịch OT thành công.";
            return true;
        }

        public bool MarkAsCompleted(int managerUserId, int scheduleId, out string message)
        {
            message = string.Empty;
            var manager = _employeeRepo.GetByUserId(managerUserId);
            if (manager == null)
            {
                message = "Không tìm thấy thông tin quản lý.";
                return false;
            }

            var schedule = _scheduleRepo.GetById(scheduleId);
            if (schedule == null || schedule.ManagerId != manager.EmployeeId)
            {
                message = "Lịch OT không tồn tại hoặc bạn không có quyền thao tác.";
                return false;
            }

            if (schedule.Status == 2)
            {
                message = "Lịch OT đã bị hủy, không thể đánh dấu hoàn thành.";
                return false;
            }

            schedule.Status = 4; // Completed
            _scheduleRepo.Update(schedule);
            _scheduleRepo.Save();

            message = "Đã đánh dấu hoàn thành lịch OT thành công.";
            return true;
        }
    }
}
