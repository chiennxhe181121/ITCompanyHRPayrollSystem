using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.DTOs.Employee;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Enum;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

            // Map lại như trong GeneratePayrollForEmployeeAsync
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
                AbsentDeduction = payroll.PayrollDetails
                    .Where(d => d.Description.Contains("Absent")).Sum(d => d.Amount),
                MissingCheckoutPenalty = payroll.PayrollDetails
                    .Where(d => d.Description.Contains("CheckOut")).Sum(d => d.Amount),
                InsufficientWorkPenalty = payroll.PayrollDetails
                    .Where(d => d.Description.Contains("Insufficient")).Sum(d => d.Amount),
                NetSalary = payroll.NetSalary,

                // Tách các nhóm chi tiết
                AbsentDetails = payroll.PayrollDetails
                    .Where(d => d.Description.Contains("Absent"))
                    .Select(d => new PenaltyDetailDTO
                    {
                        
                        Reason = d.Description,
                        Amount = d.Amount
                    }).ToList(),

                MissingCheckoutDetails = payroll.PayrollDetails
                    .Where(d => d.Description.Contains("CheckOut"))
                    .Select(d => new PenaltyDetailDTO
                    {
                       
                        Reason = d.Description,
                        Amount = d.Amount
                    }).ToList(),

                InsufficientDetails = payroll.PayrollDetails
                    .Where(d => d.Description.Contains("Insufficient"))
                    .Select(d => new PenaltyDetailDTO
                    {
                        
                        Reason = d.Description,
                        Amount = d.Amount
                    }).ToList(),

                AbsentDays = payroll.PayrollDetails.Count(d => d.Description.Contains("Absent")),
                MissingCheckoutDays = payroll.PayrollDetails.Count(d => d.Description.Contains("CheckOut")),
                TotalMissingMinutes = payroll.PayrollDetails
                    .Where(d => d.Description.Contains("Insufficient"))
                    .Sum(d => (int)d.Amount) // Amount lưu phút
            };

            return dto;
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

            existing.EmployeeId = dto.EmployeeId;
            existing.Month = dto.Month;
            existing.Year = dto.Year;
            existing.BasicSalary = dto.BasicSalary;
            existing.TotalOT = dto.TotalOT;
            existing.TotalAllowance = dto.TotalAllowance;
            existing.MissingMinutesPenalty = dto.MissingMinutesPenalty;

            existing.PayrollDetails.Clear();
            foreach (var d in dto.PayrollDetails)
            {
                existing.PayrollDetails.Add(new PayrollDetail
                {
                    Description = d.Description,
                    Amount = d.Amount,
                    Type = d.Type // ✅ THÊM
                });
            }

            existing.NetSalary = existing.PayrollDetails.Sum(d =>
    d.Type == PayrollDetailType.Earning ? d.Amount : -d.Amount
);

            await _repo.UpdateAsync(existing);
            await _repo.SaveChangesAsync();
        }

        // ================= DELETE =================
        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
            await _repo.SaveChangesAsync();
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

            // 3. Tính allowance
            var allowances = await _allowanceRepo.GetActiveAllowancesByEmployeeAsync(employeeId);
            decimal totalAllowance = allowances.Sum(a => a.Amount);

            // 4. Tính attendance + penalty

            var attendances = await _attendanceRepo.GetAttendancesAsync(employeeId, month, year);

            // bỏ holiday + weekend
            var workingDays = attendances
                .Where(a => a.Status != AttendanceStatus.Holiday
                         && a.Status != AttendanceStatus.Weekend)
                .ToList();

            int totalWorkingDays = workingDays.Count;

            decimal dailySalary = totalWorkingDays > 0
                ? basicSalary / totalWorkingDays
                : 0;

            var penaltyDetails = new List<PenaltyDetailDTO>();

            // ================= INIT =================
            var absentDetails = new List<PenaltyDetailDTO>();
            var missingCheckoutDetails = new List<PenaltyDetailDTO>();
            var insufficientDetails = new List<PenaltyDetailDTO>();

            decimal totalPenalty = 0;
            int totalMissingMinutes = 0;
            int absentDays = 0;
            int missingCheckoutDays = 0;

            // ================= LOOP =================
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
                            Reason = "Absent",
                            Amount = absentAmount
                        });
                        break;

                    case AttendanceStatus.MissingCheckOut:
                        var missingCheckoutAmount = dailySalary / 2;
                        totalPenalty += missingCheckoutAmount;
                        missingCheckoutDays++;

                        missingCheckoutDetails.Add(new PenaltyDetailDTO
                        {
                            WorkDate = a.WorkDate,
                            Reason = "Missing CheckOut",
                            Amount = missingCheckoutAmount
                        });
                        break;

                    case AttendanceStatus.InsufficientWork:
                        var amount = a.MissingMinutes * MissingPenaltyPerMinute();

                        if (amount > 0)
                        {
                            totalPenalty += amount;
                            totalMissingMinutes += a.MissingMinutes;

                            insufficientDetails.Add(new PenaltyDetailDTO
                            {
                                WorkDate = a.WorkDate,
                                Reason = $"Insufficient ({a.MissingMinutes} mins)",
                                Amount = amount
                            });
                        }
                        break;
                }
            }

            var payroll = new PayrollDTO
            {
                EmployeeId = employeeId,
                Month = month,
                Year = year,

                BasicSalary = basicSalary,
                TotalOT = totalOT,
                TotalAllowance = totalAllowance,

                // ✅ penalty tách rõ
                AbsentDeduction = absentDays * dailySalary,
                MissingCheckoutPenalty = missingCheckoutDays * (dailySalary / 2),
                InsufficientWorkPenalty = totalMissingMinutes * MissingPenaltyPerMinute(),

                MissingMinutesPenalty = totalPenalty,

                TotalWorkingDays = totalWorkingDays,
                AbsentDays = absentDays,
                MissingCheckoutDays = missingCheckoutDays,
                TotalMissingMinutes = totalMissingMinutes,

                NetSalary = basicSalary + totalOT + totalAllowance - totalPenalty,

                // ✅ 3 nhóm riêng
                AbsentDetails = absentDetails,
                MissingCheckoutDetails = missingCheckoutDetails,
                InsufficientDetails = insufficientDetails,

                

                // ✅ FIX: đủ thành phần lương
                PayrollDetails = new List<PayrollDetailDTO>
    {
        new PayrollDetailDTO
        {
            Description = "Basic Salary",
            Amount = basicSalary,
            Type = PayrollDetailType.Earning
        },
        new PayrollDetailDTO
        {
            Description = "Over Time",
            Amount = totalOT,
            Type = PayrollDetailType.Earning
        },
        new PayrollDetailDTO
        {
            Description = "Allowance",
            Amount = totalAllowance,
            Type = PayrollDetailType.Earning
        },
        // ✅ tách penalty ra 3 loại
    new PayrollDetailDTO
    {
        Description = "Absent Deduction",
        Amount = absentDays * dailySalary,
        Type = PayrollDetailType.Deduction
    },
    new PayrollDetailDTO
    {
        Description = "Missing CheckOut",
        Amount = missingCheckoutDays * (dailySalary / 2),
        Type = PayrollDetailType.Deduction
    },
    new PayrollDetailDTO
    {
        Description = "Insufficient Work",
        Amount = totalMissingMinutes * MissingPenaltyPerMinute(),
        Type = PayrollDetailType.Deduction
    }
    }
            };

            Console.WriteLine(attendances.Count);

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
        private decimal HourlyRate(int employeeId) => 50000;
        private decimal MissingPenaltyPerMinute() => 1000;

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

    }
}