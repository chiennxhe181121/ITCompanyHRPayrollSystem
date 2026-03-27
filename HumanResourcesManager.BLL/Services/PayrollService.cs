using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.DTOs.Employee;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Enum;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Repositories;
using HumanResourcesManager.DAL.Shared;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection.Metadata;
using PayrollDetailDTO = HumanResourcesManager.BLL.DTOs.PayrollDetailDTO;

namespace HumanResourcesManager.BLL.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _repo;
        private readonly IContractRepository _contractRepo;
        private readonly IOTRepository _otRepo;
        private readonly IEmployeeRepository _empRepo;
        private readonly IAllowanceRepository _allowanceRepo;
        private readonly IAttendanceRepository _attendanceRepo;

        public PayrollService(
            IPayrollRepository repo,
            IContractRepository contractRepo,
            IOTRepository otRepo,
            IEmployeeRepository empRepo,
            IAllowanceRepository allowanceRepo,
            IAttendanceRepository attendanceRepo)
        {
            _repo = repo;
            _contractRepo = contractRepo;
            _otRepo = otRepo;
            _empRepo = empRepo;
            _allowanceRepo = allowanceRepo;
            _attendanceRepo = attendanceRepo;
        }

        // ================= LIST =================
        public async Task<List<PayrollDTO>> GetAllAsync()
        {
            var payrolls = await _repo.GetAllAsync(); // dùng cái bạn đã có
            return payrolls.Select(p => new PayrollDTO
            {
                PayrollId = p.PayrollId,
                EmployeeId = p.EmployeeId,
                EmployeeName = p.Employee.FullName,
                Month = p.Month,
                Year = p.Year,             
                BasicSalary = p.BasicSalary,
                TotalOT = p.TotalOT,
                TotalAllowance = p.TotalAllowance,
                MissingMinutesPenalty = p.MissingMinutesPenalty,
                NetSalary = p.NetSalary,
                CreatedDate = p.CreatedDate,
                PayrollDetails = p.PayrollDetails.Select(d => new PayrollDetailDTO
                {
                    PayrollDetailId = d.PayrollDetailId,
                    Type = d.Type,
                    Description = d.Description,
                    Amount = d.Amount
                }).ToList()
            }).ToList();
        }

        // ================= GET BY ID =================
        public async Task<PayrollDTO> GetByIdAsync(int payrollId)
        {
            var payroll = await _repo.GetByIdAsync(payrollId);
            if (payroll == null) return null;

            var details = payroll.PayrollDetails;

            var dto = new PayrollDTO
            {
                PayrollId = payroll.PayrollId,
                EmployeeId = payroll.EmployeeId,
                EmployeeName = payroll.Employee.FullName,
                Month = payroll.Month,
                Year = payroll.Year,

                BasicSalary = payroll.BasicSalary,
                TotalOT = payroll.TotalOT,
                TotalAllowance = payroll.TotalAllowance,
                NetSalary = payroll.NetSalary,

                // ================= GROUP =================
                AbsentDeduction = details
                    .Where(d => d.Description.Contains("Không đi làm"))
                    .Sum(d => d.Amount),

                MissingCheckoutPenalty = details
                    .Where(d => d.Description.Contains("check-out"))
                    .Sum(d => d.Amount),

                InsufficientWorkPenalty = details
                    .Where(d => d.Description.Contains("Làm thiếu"))
                    .Sum(d => d.Amount),

                // ================= DETAILS =================
                AbsentDetails = details
                    .Where(d => d.Description.Contains("Không đi làm"))
                    .Select(d => new PenaltyDetailDTO
                    {
                        Reason = d.Description,
                        Amount = d.Amount
                    }).ToList(),

                MissingCheckoutDetails = details
                    .Where(d => d.Description.Contains("check-out"))
                    .Select(d => new PenaltyDetailDTO
                    {
                        Reason = d.Description,
                        Amount = d.Amount
                    }).ToList(),

                InsufficientDetails = details
                    .Where(d => d.Description.Contains("Làm thiếu"))
                    .Select(d => new PenaltyDetailDTO
                    {
                        Reason = d.Description,
                        Amount = d.Amount
                    }).ToList(),

                // ================= COUNT =================
                AbsentDays = details.Count(d => d.Description.Contains("Không đi làm")),

                MissingCheckoutDays = details.Count(d => d.Description.Contains("check-out")),

                TotalMissingMinutes = details
                    .Where(d => d.Description.Contains("Làm thiếu"))
                    .Sum(d => ExtractMinutes(d.Description)) , // parse từ text

                // ✅ QUAN TRỌNG NHẤT (THÊM DÒNG NÀY)
        PayrollDetails = details.Select(d => new PayrollDetailDTO
        {
            Description = d.Description,
            Amount = d.Amount,
            Type = d.Type
        }).ToList()

            };


            return dto;
        }
        private int ExtractMinutes(string description)
        {
            var match = System.Text.RegularExpressions.Regex.Match(description, @"\d+");
            return match.Success ? int.Parse(match.Value) : 0;
        }

        // ================= CREATE =================
        public async Task CreateAsync(int employeeId, int month, int year)
        {
            // 🔥 1. Generate lại payroll (CHUẨN NHẤT)
            var dto = await GeneratePayrollForEmployeeAsync(employeeId, month, year);

            // 🔥 2. Map sang entity
            var entity = new Payroll
            {
                EmployeeId = dto.EmployeeId,
                Month = dto.Month,
                Year = dto.Year,
                BasicSalary = dto.BasicSalary,
                TotalOT = dto.TotalOT,
                TotalAllowance = dto.TotalAllowance,
                MissingMinutesPenalty = dto.MissingMinutesPenalty,
                NetSalary = dto.NetSalary, // ✅ FIX: lấy trực tiếp
                CreatedDate = DateTime.Now,
                PayrollDetails = new List<PayrollDetail>()
            };

            // 🔥 3. Add details (chắc chắn có)
            foreach (var d in dto.PayrollDetails)
            {
                if (d.Amount == 0) continue; // ❌ bỏ qua nếu = 0

                entity.PayrollDetails.Add(new PayrollDetail
                {
                    Description = d.Description,
                    Amount = d.Amount,
                    Type = d.Type
                });
            }

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
        }

        // ================= UPDATE =================
        public async Task UpdateAsync(PayrollDTO dto)
        {
            var existing = await _repo.GetByIdAsync(dto.PayrollId!.Value);
            if (existing == null) return;

            Console.WriteLine("=== DEBUG UPDATE PAYROLL FIXED ===");
            Console.WriteLine($"PayrollId: {dto.PayrollId}");
            Console.WriteLine($"BasicSalary từ FE: {dto.BasicSalary}");
            Console.WriteLine($"TotalOT từ FE: {dto.TotalOT}");

            // 1️⃣ Update BasicSalary và TotalOT
            existing.BasicSalary = dto.BasicSalary;
            existing.TotalOT = dto.TotalOT;

            // 2️⃣ Đồng bộ PayrollDetails từ DTO
            if (dto.PayrollDetails != null && dto.PayrollDetails.Any())
            {
                Console.WriteLine("--- Đồng bộ các dòng điều chỉnh ---");

                var detailDict = existing.PayrollDetails
                    .ToDictionary(
                        d => (d.Type, Description: (d.Description ?? "").Trim()),
                        d => d,
                        new PayrollDetailTupleComparer()
                    );

                foreach (var d in dto.PayrollDetails)
                {
                    string desc = d.Description?.Trim() ?? "";
                    var key = (d.Type, Description: desc);

                    if (detailDict.TryGetValue(key, out var existingDetail))
                    {
                        if (existingDetail.Amount != d.Amount)
                            existingDetail.Amount = d.Amount;
                    }
                    else
                    {
                        // Chỉ thêm nếu Amount != 0
                        if (d.Amount != 0)
                        {
                            existing.PayrollDetails.Add(new PayrollDetail
                            {
                                Type = d.Type,
                                Description = desc,
                                Amount = d.Amount
                            });
                        }
                    }
                }
            }

            // 3️⃣ Tính TotalAllowance từ Earning
            decimal totalAllowance = existing.PayrollDetails
                .Where(d => d.Type == PayrollDetailType.Earning)
                .Sum(d => GetSignedAmount(d.Description, d.Amount));

            existing.TotalAllowance = Math.Max(0, totalAllowance);
            Console.WriteLine($"TotalAllowance tính lại: {existing.TotalAllowance}");

            // 4️⃣ Tính NetSalary hoàn chỉnh
            decimal netSalary = existing.BasicSalary
                                + existing.TotalOT
                                + existing.TotalAllowance
                                + existing.PayrollDetails
                                    .Where(d => d.Type == PayrollDetailType.Deduction)
                                    .Sum(d => -GetSignedAmount(d.Description, d.Amount));

            existing.NetSalary = netSalary;
            Console.WriteLine($"NetSalary tính lại: {existing.NetSalary}");

            // 5️⃣ Lưu vào database
            await _repo.UpdateAsync(existing);
            await _repo.SaveChangesAsync();

            Console.WriteLine("=== UPDATE PAYROLL DONE ===");
        }

        // ================= Helper duy nhất =================
        // Áp dụng cho cả DTO và entity
        private decimal GetSignedAmount(string? description, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(description)) return amount;

            string desc = description.ToLower().Trim();

            

            return amount;
        }

        // Custom comparer để dùng cho Dictionary với tuple key, ignore case cho string
        public class PayrollDetailTupleComparer : IEqualityComparer<(PayrollDetailType Type, string Description)>
        {
            public bool Equals((PayrollDetailType Type, string Description) x, (PayrollDetailType Type, string Description) y)
            {
                return x.Type == y.Type && string.Equals(x.Description, y.Description, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode((PayrollDetailType Type, string Description) obj)
            {
                return HashCode.Combine(obj.Type, obj.Description?.ToLowerInvariant());
            }
        }

        // ================= DELETE =================
        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
            await _repo.SaveChangesAsync();
        }

        // ================= PayrollAudit =================


        public async Task<List<PayrollAuditSimpleDTO>> AuditPayrollSimpleAsync(int payrollId)
        {
            var existing = await _repo.GetByIdAsync(payrollId);
            if (existing == null)
            {
                Console.WriteLine($"[Audit] PayrollId {payrollId} không tồn tại.");
                return null;
            }

            var regenerated = await GeneratePayrollForEmployeeAsync(
                existing.EmployeeId, existing.Month, existing.Year);

            Console.WriteLine($"[Audit] Bắt đầu audit payrollId: {payrollId}");
            Console.WriteLine($"[Audit] EmployeeId: {existing.EmployeeId}, Tháng: {existing.Month}/{existing.Year}");

            var result = new List<PayrollAuditSimpleDTO>();

            // ====================== DEBUG CHI TIẾT ======================
            Console.WriteLine("\n=== CHI TIẾT CŨ ===");
            foreach (var d in existing.PayrollDetails.OrderBy(x => x.Description))
                Console.WriteLine($"Old -> Type: {d.Type,-12} | Amount: {d.Amount,12:N0} | Desc: {d.Description}");

            Console.WriteLine("\n=== CHI TIẾT MỚI ===");
            foreach (var d in regenerated.PayrollDetails.OrderBy(x => x.Description))
                Console.WriteLine($"New -> Type: {d.Type,-12} | Amount: {d.Amount,12:N0} | Desc: {d.Description}");

            // ====================== HÀM HỖ TRỢ ======================
            string GetKey(object detail)
            {
                var desc = detail switch
                {
                    PayrollDetail e => e.Description,
                    PayrollDetailDTO dto => dto.Description,
                    _ => ""
                };
                return ExtractDateFromDescription(desc)?.ToString("dd/MM")
                       ?? ExtractAllowanceName(desc)
                       ?? "Unknown";
            }

            PayrollDetailType GetDetailType(object detail)
            {
                return detail switch
                {
                    PayrollDetail e => e.Type,
                    PayrollDetailDTO dto => dto.Type,
                    _ => PayrollDetailType.Earning
                };
            }

            decimal GetAmount(object detail)
            {
                return detail switch
                {
                    PayrollDetail e => e.Amount,
                    PayrollDetailDTO dto => dto.Amount,
                    _ => 0
                };
            }

            // ====================== TÍNH NET THEO KEY ======================
            var oldByKey = existing.PayrollDetails
                .GroupBy(d => GetKey(d))
                .ToDictionary(g => g.Key, g => new
                {
                    Earning = g.Where(x => GetDetailType(x) == PayrollDetailType.Earning).Sum(GetAmount),
                    Deduction = g.Where(x => GetDetailType(x) == PayrollDetailType.Deduction).Sum(GetAmount)
                });

            var newByKey = regenerated.PayrollDetails
                .GroupBy(d => GetKey(d))
                .ToDictionary(g => g.Key, g => new
                {
                    Earning = g.Where(x => GetDetailType(x) == PayrollDetailType.Earning).Sum(GetAmount),
                    Deduction = g.Where(x => GetDetailType(x) == PayrollDetailType.Deduction).Sum(GetAmount)
                });

            Console.WriteLine("\n=== SO SÁNH NET THEO KEY ===");

            var allKeys = oldByKey.Keys.Union(newByKey.Keys).OrderBy(k => k).ToList();

            foreach (var key in allKeys)
            {
                var old = oldByKey.GetValueOrDefault(key, new { Earning = 0m, Deduction = 0m });
                var nw = newByKey.GetValueOrDefault(key, new { Earning = 0m, Deduction = 0m });

                decimal oldNet = old.Earning - old.Deduction;
                decimal newNet = nw.Earning - nw.Deduction;
                decimal diff = newNet - oldNet;

                Console.WriteLine($"Key: {key,-10} | OldNet: {oldNet,12:N0} (E:{old.Earning,8:N0} D:{old.Deduction,8:N0}) | " +
                                  $"NewNet: {newNet,12:N0} (E:{nw.Earning,8:N0} D:{nw.Deduction,8:N0}) | Diff: {diff,10:N0}");

                if (Math.Abs(diff) < 0.01m)
                {
                    Console.WriteLine($"   → Diff = 0 → Bỏ qua");
                    continue;
                }

                // ====================== LOGIC MỚI - CHÍNH XÁC HƠN ======================
                string action;
                PayrollDetailType auditType;
                string descType;

                bool isIncreasingDeduction = (nw.Deduction > old.Deduction);   // Tăng khấu trừ
                bool isDecreasingEarning = (nw.Earning < old.Earning);       // Giảm thu nhập

                if (isIncreasingDeduction || isDecreasingEarning)
                {
                    action = "Điều chỉnh tăng";
                    auditType = PayrollDetailType.Deduction;   // Tăng trừ = Deduction
                    descType = "khấu trừ";
                }
                else
                {
                    action = "Điều chỉnh giảm";
                    auditType = PayrollDetailType.Earning;     // Giảm trừ hoặc tăng thu = Earning
                    descType = (nw.Earning > old.Earning) ? "thu nhập/phụ cấp" : "khấu trừ";
                }

                var auditItem = new PayrollAuditSimpleDTO
                {
                    PayrollId = payrollId,
                    Type = auditType,
                    Description = $"{action} [{key}] các khoản {descType}",
                    Amount = Math.Abs(diff)
                };

                result.Add(auditItem);

                Console.WriteLine($"   → THÊM AUDIT: {action} | Type: {auditType} | Amount: {auditItem.Amount:N0}");
                Console.WriteLine($"       Description: {auditItem.Description}");
            }

            Console.WriteLine($"\n=== HOÀN THÀNH - Tìm thấy {result.Count} thay đổi ===");

            return result;
        }

        // Helper: Lấy tên phụ cấp từ description (ví dụ: "Phụ cấp - Ăn trưa" => "Ăn trưa")
        private string ExtractAllowanceName(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return "Không có tên";

            // Nếu có dấu "-", lấy phần sau dấu "-".
            var parts = description.Split('-');
            if (parts.Length > 1)
                return parts[1].Trim();

            return description.Trim();
        }
        private DateTime? ExtractDateFromDescription(string description)
        {
            var match = System.Text.RegularExpressions.Regex.Match(description, @"\[(\d{2}/\d{2})\]");
            if (!match.Success) return null;

            var dateStr = match.Groups[1].Value; // "dd/MM"
            if (DateTime.TryParseExact(dateStr, "dd/MM", null, System.Globalization.DateTimeStyles.None, out var date))
                return date;
            return null;
        }

        // ================= GENERATE PAYROLL =================
        public async Task<PayrollDTO> GeneratePayrollForEmployeeAsync(int employeeId, int month, int year)
        {
            // 1. Basic salary từ contract
            var contract = await _contractRepo.GetActiveContractByEmployeeAsync(employeeId);
            decimal basicSalary = contract?.BasicSalary ?? 0;

            // 2. Tính OT
            var otRequests = await _otRepo.GetApprovedOTsAsync(employeeId, month, year);
            decimal totalOT = 0;
            foreach (var ot in otRequests)
            {
                if (ot.OTAttendance != null)
                    totalOT += (decimal)ot.OTAttendance.ActualOTHours * HourlyRate(employeeId);
            }

            // 3. Lấy allowance
            var allowances = await _allowanceRepo.GetActiveAllowancesByEmployeeAsync(employeeId);
            decimal totalAllowance = allowances.Sum(a => a.Amount);

            // 4. Attendance
            var attendances = await _attendanceRepo.GetAttendancesAsync(employeeId, month, year);

            var workingDays = attendances
                .Where(a => a.Status != AttendanceStatus.Holiday
                         && a.Status != AttendanceStatus.Weekend)
                .ToList();

            int totalWorkingDays = workingDays.Count;

            decimal dailySalary = totalWorkingDays > 0
                ? basicSalary / totalWorkingDays
                : 0;

            // ================= INIT =================
            decimal totalPenalty = 0;
            int totalMissingMinutes = 0;
            int absentDays = 0;
            int missingCheckoutDays = 0;

            var absentDetails = new List<PenaltyDetailDTO>();
            var missingCheckoutDetails = new List<PenaltyDetailDTO>();
            var insufficientDetails = new List<PenaltyDetailDTO>();

            // ================= PAYROLL DETAILS =================
            var payrollDetails = new List<PayrollDetailDTO>
    {
        
        new PayrollDetailDTO
        {
            Description = "Tăng ca",
            Amount = totalOT,
            Type = PayrollDetailType.Earning
        }
    };

            // ✅ Tách từng allowance theo tên
            foreach (var a in allowances ?? new List<Allowance>())
            {
                payrollDetails.Add(new PayrollDetailDTO
                {
                    Description = $"Phụ cấp - {a.AllowanceName}",
                    Amount = a.Amount,
                    Type = PayrollDetailType.Earning
                });
            }

            // ================= LOOP ATTENDANCE =================
            foreach (var a in workingDays)
            {
                switch (a.Status)
                {
                    case AttendanceStatus.Absent:
                        var absentAmount = dailySalary;
                        totalPenalty += absentAmount;
                        absentDays++;

                        absentDetails.Add(new PenaltyDetailDTO
                        {
                            WorkDate = a.WorkDate,
                            Reason = "Ngày nghỉ",
                            Amount = absentAmount
                        });

                        payrollDetails.Add(new PayrollDetailDTO
                        {
                            Description = $"[{a.WorkDate:dd/MM}] Không đi làm",
                            Amount = absentAmount,
                            Type = PayrollDetailType.Deduction
                        });
                        break;

                    case AttendanceStatus.MissingCheckOut:
                        var missingAmount = dailySalary / 2;
                        totalPenalty += missingAmount;
                        missingCheckoutDays++;

                        missingCheckoutDetails.Add(new PenaltyDetailDTO
                        {
                            WorkDate = a.WorkDate,
                            Reason = "Thiếu check-out",
                            Amount = missingAmount
                        });

                        payrollDetails.Add(new PayrollDetailDTO
                        {
                            Description = $"[{a.WorkDate:dd/MM}] Thiếu check-out",
                            Amount = missingAmount,
                            Type = PayrollDetailType.Deduction
                        });
                        break;

                    case AttendanceStatus.InsufficientWork:
                        if (a.MissingMinutes > 0)
                        {
                            var amount = a.MissingMinutes * MissingPenaltyPerMinute();

                            totalPenalty += amount;
                            totalMissingMinutes += a.MissingMinutes;

                            insufficientDetails.Add(new PenaltyDetailDTO
                            {
                                WorkDate = a.WorkDate,
                                Reason = $"Làm thiếu ({a.MissingMinutes} phút)",
                                Amount = amount
                            });

                            payrollDetails.Add(new PayrollDetailDTO
                            {
                                Description = $"[{a.WorkDate:dd/MM}] Làm thiếu {a.MissingMinutes} phút",
                                Amount = amount,
                                Type = PayrollDetailType.Deduction
                            });
                        }
                        break;
                }
            }

            // ✅ Sort: earning trước → theo description
            payrollDetails = payrollDetails
                .OrderBy(d => d.Type)
                .ThenBy(d => d.Description)
                .ToList();

            // ================= DTO =================
            var payroll = new PayrollDTO
            {
                EmployeeId = employeeId,
                Month = month,
                Year = year,

                BasicSalary = basicSalary,
                TotalOT = totalOT,
                TotalAllowance = totalAllowance,

                AbsentDeduction = absentDays * dailySalary,
                MissingCheckoutPenalty = missingCheckoutDays * (dailySalary / 2),
                InsufficientWorkPenalty = totalMissingMinutes * MissingPenaltyPerMinute(),

                MissingMinutesPenalty = totalPenalty,

                TotalWorkingDays = totalWorkingDays,
                AbsentDays = absentDays,
                MissingCheckoutDays = missingCheckoutDays,
                TotalMissingMinutes = totalMissingMinutes,

                NetSalary = basicSalary + totalOT + totalAllowance - totalPenalty,

                AbsentDetails = absentDetails,
                MissingCheckoutDetails = missingCheckoutDetails,
                InsufficientDetails = insufficientDetails,

                PayrollDetails = payrollDetails
            };

            return payroll;
        }

        // ================= GET ALL EMPLOYEES =================
        public async Task<List<EmployeeDTO>> GetAllEmployeesWithoutPayrollAsync(int month, int year)
        {
            // 1. Lấy tất cả employee
            var employees = _empRepo.GetAll()
                .OrderBy(e => e.FullName)
                .ToList();

            // 2. Lấy tất cả payroll đã tạo trong tháng đó
            var existingPayrolls = await _repo.GetAllAsync(); // hoặc repo.GetByMonthYearAsync(month, year)
            var createdEmployeeIds = existingPayrolls
                .Where(p => p.Month == month && p.Year == year)
                .Select(p => p.EmployeeId)
                .ToHashSet();

            // 3. Lọc ra employee chưa có payroll
            var filteredEmployees = employees
                .Where(e => !createdEmployeeIds.Contains(e.EmployeeId))
                .ToList();

            // 4. Map sang DTO
            return filteredEmployees.Select(e => new EmployeeDTO
            {
                EmployeeId = e.EmployeeId,
                FullName = e.FullName
            }).ToList();
        }

        // ================= HELPERS =================
        private decimal HourlyRate(int employeeId)
            => Constants.DEFAULT_HOURLY_RATE;

        private decimal MissingPenaltyPerMinute()
            => Constants.MISSING_PENALTY_PER_MINUTE;

        public EmployeePayrollViewDTO GetPayrolls(int employeeId, int page, int pageSize, int? month, int? year)
        {
            var query = _repo.GetQueryableByEmployee(employeeId);

            if (month.HasValue)
                query = query.Where(x => x.Month == month);

            if (year.HasValue)
                query = query.Where(x => x.Year == year);

            int totalRecords = query.Count();

            var data = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PayrollRowDTO
                {
                    PayrollId = x.PayrollId,
                    Month = x.Month,
                    Year = x.Year,
                    BasicSalary = x.BasicSalary,
                    TotalOT = x.TotalOT,
                    TotalAllowance = x.TotalAllowance,
                    MissingMinutesPenalty = x.MissingMinutesPenalty,
                    NetSalary = x.NetSalary,
                    CreatedDate = x.CreatedDate
                })
                .ToList();

            return new EmployeePayrollViewDTO
            {
                Records = data,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                SelectedMonth = month,
                SelectedYear = year
            };
        }

        public EmployeePayrollDetailDTO GetPayrollDetail(int payrollId, int employeeId)
        {
            var payroll = _repo.GetPayrollDetail(payrollId, employeeId);

            if (payroll == null) return null;

            return new EmployeePayrollDetailDTO
            {
                PayrollId = payroll.PayrollId,
                Month = payroll.Month,
                Year = payroll.Year,
                BasicSalary = payroll.BasicSalary,
                TotalOT = payroll.TotalOT,
                TotalAllowance = payroll.TotalAllowance,
                MissingMinutesPenalty = payroll.MissingMinutesPenalty,
                NetSalary = payroll.NetSalary,
                Details = payroll.PayrollDetails
                    .Select(d => new PayrollDetailDTO
                    {
                        Type = d.Type,
                        Description = d.Description,
                        Amount = d.Amount
                    }).ToList()
            };
        }

        public decimal GetCurrentSalary(int employeeId)
        {
            var payroll = _repo.GetLatestPayroll(employeeId);

            return payroll?.NetSalary ?? 0;
        }
    }
}