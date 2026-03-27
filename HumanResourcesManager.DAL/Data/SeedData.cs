using HumanResourcesManager.DAL.Enum;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;

namespace HumanResourcesManager.DAL.Data
{
    public static class SeedData
    {
        public static void Initialize(HumanManagerContext context)
        {
            // ❌ KHÔNG dùng EnsureCreated khi có Migration
             //context.Database.EnsureCreated();

            // ===================== ROLE =====================
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { RoleCode = "EMP", RoleName = "Employee", Status = Constants.Active },
                    new Role { RoleCode = "HR", RoleName = "HR", Status = Constants.Active },
                    new Role { RoleCode = "ADMIN", RoleName = "Admin", Status = Constants.Active },
                    new Role { RoleCode = "MANAGER", RoleName = "Manager", Status = Constants.Active }
                );
                context.SaveChanges();
            }

            // ===================== DEPARTMENT =====================
            if (!context.Departments.Any())
            {
                context.Departments.AddRange(
                    new Department { DepartmentName = "HR", Status = Constants.Active }, // Hoàng thêm status để xóa mềm
                    new Department { DepartmentName = "IT", Status = Constants.Active },
                    new Department { DepartmentName = "Finance", Status = Constants.Active }
                );
                context.SaveChanges();
            }

            // ===================== POSITION =====================
            if (!context.Positions.Any())
            {
                context.Positions.AddRange(
                    new Position { PositionName = "HR Executive", BaseSalary = 8000000, Status = Constants.Active, CreatedAt = DateTime.Now },// Hoàng thêm status và createdat 
                    new Position { PositionName = "Software Engineer", BaseSalary = 15000000, Status = Constants.Active, CreatedAt = DateTime.Now },
                    new Position { PositionName = "Accountant", BaseSalary = 12000000, Status = Constants.Active, CreatedAt = DateTime.Now }
                );
                context.SaveChanges();
            }

            // ===================== USER ACCOUNT =====================
            if (!context.UserAccounts.Any())
            {
                var adminRoleId = context.Roles.First(r => r.RoleCode == "ADMIN").RoleId;
                var hrRoleId = context.Roles.First(r => r.RoleCode == "HR").RoleId;
                var empRoleId = context.Roles.First(r => r.RoleCode == "EMP").RoleId;
                var managerRoleId = context.Roles.First(r => r.RoleCode == "MANAGER").RoleId;

                context.UserAccounts.AddRange(
                    new UserAccount
                    {
                        Username = "admin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        RoleId = adminRoleId,
                        Status = Constants.Active
                    },
                    new UserAccount
                    {
                        Username = "hr",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        RoleId = hrRoleId,
                        Status = Constants.Active
                    },
                    new UserAccount
                    {
                        Username = "emp",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        RoleId = empRoleId,
                        Status = Constants.Active
                    },
                    new UserAccount
                    {
                        Username = "manager",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        RoleId = managerRoleId,
                        Status = Constants.Active
                    }
                );
                context.SaveChanges();
            }

            // ===================== EMPLOYEE =====================
            if (!context.Employees.Any())
            {
                // 🔑 Lấy UserAccount đã seed
                var adminUser = context.UserAccounts.First(u => u.Username == "admin");
                var hrUser = context.UserAccounts.First(u => u.Username == "hr");
                var empUser = context.UserAccounts.First(u => u.Username == "emp");
                var managerUser = context.UserAccounts.First(u => u.Username == "manager");

                context.Employees.AddRange(
                    new Employee
                    {
                        EmployeeCode = "EMP001",
                        FullName = "Hoàng Admin",
                        Email = "a.nguyen@company.com",
                        Phone = "0901234567",
                        DepartmentId = 1,
                        PositionId = 1,
                        Status = Constants.Active,
                        ImgAvatar = null,

                        // ✅ LINK 1–1 BẰNG NAVIGATION
                        UserAccount = adminUser
                    },
                    new Employee
                    {
                        EmployeeCode = "EMP002",
                        FullName = "Quang Nhân Sự",
                        Email = "b.tran@company.com",
                        Phone = "0912345678",
                        DepartmentId = 2,
                        PositionId = 2,
                        Status = Constants.Active,
                        ImgAvatar = null,

                        UserAccount = hrUser
                    },
                    new Employee
                    {
                        EmployeeCode = "EMP003",
                        FullName = "Chiến Nhân Viên",
                        Email = "c.le@company.com",
                        Phone = "0923456789",
                        DepartmentId = 3,
                        PositionId = 3,
                        Status = Constants.Active,
                        ImgAvatar = null,

                        UserAccount = empUser
                    },
                      new Employee
                      {
                          EmployeeCode = "EMP004",
                          FullName = "Giang Quản Lý",
                          Email = "d.nguyen@company.com",
                          Phone = "0923456789",
                          DepartmentId = 3,
                          PositionId = 3,
                          Status = Constants.Active,
                          ImgAvatar = null,

                          UserAccount = managerUser
                      }

                );
                context.SaveChanges();
            }

            // ===================== ALLOWANCE =====================
            if (!context.Allowances.Any())
            {
                context.Allowances.AddRange(
                    new Allowance { AllowanceName = "Phụ cấp Ăn Trưa", Amount = 500000, Status = Constants.Active },
                    new Allowance { AllowanceName = "Phụ cấp Đi Lại", Amount = 300000, Status = Constants.Active }
                );
                context.SaveChanges();
            }

            // ===================== EMPLOYEE ALLOWANCE =====================
            if (!context.EmployeeAllowances.Any())
            {
                context.EmployeeAllowances.AddRange(
                    new EmployeeAllowance { EmployeeId = 1, AllowanceId = 1 },
                    new EmployeeAllowance { EmployeeId = 2, AllowanceId = 1 },
                    new EmployeeAllowance { EmployeeId = 2, AllowanceId = 2 }
                );
                context.SaveChanges();
            }

            // ===================== PAYROLL =====================
            if (!context.Payrolls.Any())
            {
                var currentYear = DateTime.Today.Year;

                var payrolls = new List<Payroll>
    {
        // EMP003 (user đang test chính)
        new Payroll
        {
            EmployeeId = 3,
            Month = 12,
            Year = 2025,
            BasicSalary = 12000000,
            TotalOT = 500000,
            TotalAllowance = 800000,
            MissingMinutesPenalty = 100000,
            NetSalary = 12200000,
            CreatedDate = new DateTime(currentYear, 1, 31)
        },
        new Payroll
        {
            EmployeeId = 3,
            Month = 11,
            Year = 2025,
            BasicSalary = 12000000,
            TotalOT = 700000,
            TotalAllowance = 800000,
            MissingMinutesPenalty = 0,
            NetSalary = 13500000,
            CreatedDate = new DateTime(currentYear, 2, 28)
        },
        

        // EMP002
        new Payroll
        {
            EmployeeId = 4,
            Month = 12,
            Year = 2025,
            BasicSalary = 15000000,
            TotalOT = 2000000,
            TotalAllowance = 800000,
            MissingMinutesPenalty = 0,
            NetSalary = 17800000,
            CreatedDate = new DateTime(currentYear, 2, 28)
        }
    };

                context.Payrolls.AddRange(payrolls);
                context.SaveChanges();

                // ===================== PAYROLL DETAIL =====================
                var details = new List<PayrollDetail>();

                foreach (var p in payrolls)
                {
                    details.AddRange(new List<PayrollDetail>
{
    new PayrollDetail
    {
        PayrollId = p.PayrollId,
        Description = "[05/03] Đi trễ 15 phút (-50,000)",
        Amount = -50000
    },
    new PayrollDetail
    {
        PayrollId = p.PayrollId,
        Description = "[06/03] Về sớm 30 phút (-100,000)",
        Amount = -100000
    },
    new PayrollDetail
    {
        PayrollId = p.PayrollId,
        Description = "[08/03] OT 2 giờ (+200,000)",
        Amount = 200000
    },
    new PayrollDetail
    {
        PayrollId = p.PayrollId,
        Description = "[10/03] Phụ cấp ăn trưa (+30,000)",
        Amount = 30000
    }
});
                }

                context.PayrollDetails.AddRange(details);
                context.SaveChanges();
            }

            // ===================== ATTENDANCE =====================
            if (!context.Attendances.Any(a => a.EmployeeId == 3 || a.EmployeeId == 4))
            {
                var months = new List<(DateTime start, DateTime end, List<(int day, AttendanceStatus status, TimeSpan? checkIn, TimeSpan? checkOut, int missingMinutes)> specialDays)>
    {
        // Tháng 1
        (new DateTime(2026,1,1), new DateTime(2026,1,31), new List<(int, AttendanceStatus, TimeSpan?, TimeSpan?, int)>
        {
            (1, AttendanceStatus.Holiday, null, null, 0),
            (2, AttendanceStatus.Absent, null, null, 0),
            (5, AttendanceStatus.InsufficientWork, new TimeSpan(8,5,0), new TimeSpan(17,0,0), 5),
            (10, AttendanceStatus.InsufficientWork, new TimeSpan(8,10,0), new TimeSpan(17,0,0), 10),
            (15, AttendanceStatus.InsufficientWork, new TimeSpan(8,15,0), new TimeSpan(17,0,0), 15)
        }),
        // Tháng 2
        (new DateTime(2026,2,1), new DateTime(2026,2,28), new List<(int, AttendanceStatus, TimeSpan?, TimeSpan?, int)>
        {
            (2, AttendanceStatus.ApprovedLeave, null, null, 0),
            (5, AttendanceStatus.MissingCheckOut, new TimeSpan(8,0,0), null, 0),
            (10, AttendanceStatus.InsufficientWork, new TimeSpan(8,5,0), new TimeSpan(17,0,0), 5),
            (20, AttendanceStatus.InsufficientWork, new TimeSpan(8,10,0), new TimeSpan(17,0,0), 10),
            (16, AttendanceStatus.Holiday, null, null, 0),
            (17, AttendanceStatus.Holiday, null, null, 0),
            (18, AttendanceStatus.Holiday, null, null, 0),
            (19, AttendanceStatus.Holiday, null, null, 0),
            (20, AttendanceStatus.Holiday, null, null, 0)
        })
    };

                var attendances = new List<Attendance>();

                foreach (var month in months)
                {
                    var specialDaysEmp3 = month.specialDays;
                    var specialDaysEmp4 = new List<(int day, AttendanceStatus status, TimeSpan? checkIn, TimeSpan? checkOut, int missingMinutes)>(specialDaysEmp3);

                    for (var date = month.start; date <= month.end; date = date.AddDays(1))
                    {
                        var dayOfWeek = (int)date.DayOfWeek; // Sunday = 0, Saturday = 6

                        if (dayOfWeek == 0 || dayOfWeek == 6)
                        {
                            attendances.Add(new Attendance { EmployeeId = 3, WorkDate = date, Status = AttendanceStatus.Weekend });
                            attendances.Add(new Attendance { EmployeeId = 4, WorkDate = date, Status = AttendanceStatus.Weekend });
                            continue;
                        }

                        // EMP003
                        var specialEmp3 = specialDaysEmp3.FirstOrDefault(d => d.day == date.Day);
                        if (specialEmp3 != default)
                        {
                            attendances.Add(new Attendance
                            {
                                EmployeeId = 3,
                                WorkDate = date,
                                CheckIn = specialEmp3.checkIn,
                                CheckOut = specialEmp3.checkOut,
                                MissingMinutes = specialEmp3.missingMinutes,
                                Status = specialEmp3.status
                            });
                        }
                        else
                        {
                            attendances.Add(new Attendance
                            {
                                EmployeeId = 3,
                                WorkDate = date,
                                CheckIn = new TimeSpan(8, 0, 0),
                                CheckOut = new TimeSpan(17, 0, 0),
                                MissingMinutes = 0,
                                Status = AttendanceStatus.CompletedWork
                            });
                        }

                        // EMP004
                        var specialEmp4 = specialDaysEmp4.FirstOrDefault(d => d.day == date.Day);
                        if (specialEmp4 != default)
                        {
                            attendances.Add(new Attendance
                            {
                                EmployeeId = 4,
                                WorkDate = date,
                                CheckIn = specialEmp4.checkIn ?? new TimeSpan(8, 0, 0),
                                CheckOut = specialEmp4.checkOut ?? new TimeSpan(17, 0, 0),
                                MissingMinutes = specialEmp4.missingMinutes,
                                Status = specialEmp4.status
                            });
                        }
                        else
                        {
                            attendances.Add(new Attendance
                            {
                                EmployeeId = 4,
                                WorkDate = date,
                                CheckIn = new TimeSpan(8, 0, 0),
                                CheckOut = new TimeSpan(17, 0, 0),
                                MissingMinutes = 0,
                                Status = AttendanceStatus.CompletedWork
                            });
                        }
                    }
                }

                context.Attendances.AddRange(attendances);
                context.SaveChanges();
            }

            // ===================== LEAVE TYPE =====================
            if (!context.LeaveTypes.Any())
            {
                var leaveTypes = new List<LeaveType>
    {
        new LeaveType
        {
            LeaveName = "Annual Leave",
            IsPaid = true
        },
        //new LeaveType
        //{
        //    LeaveName = "Unpaid Leave",
        //    IsPaid = false
        //},
        new LeaveType
        {
            LeaveName = "Maternity Leave",
            IsPaid = true
        }
    };

                context.LeaveTypes.AddRange(leaveTypes);
                context.SaveChanges();
            }

            // ===================== ANNUAL LEAVE BALANCE =====================
            if (!context.AnnualLeaveBalance.Any())
            {
                var currentYear = DateTime.Today.Year;

                var balances = new List<AnnualLeaveBalance>
    {
        // EMP001
        new AnnualLeaveBalance
        {
            EmployeeId = 1,
            Year = currentYear,
            EntitledDays = 12,
            UsedDays = 2,
            RemainingDays = 10,
            CreatedDate = new DateTime(currentYear, 1, 1)
        },

        // EMP002
        new AnnualLeaveBalance
        {
            EmployeeId = 2,
            Year = currentYear,
            EntitledDays = 12,
            UsedDays = 3,
            RemainingDays = 9,
            CreatedDate = new DateTime(currentYear, 1, 1)
        },

        // EMP003
        new AnnualLeaveBalance
        {
            EmployeeId = 3,
            Year = currentYear,
            EntitledDays = 12,
            UsedDays = 3,
            RemainingDays = 9,
            CreatedDate = new DateTime(currentYear, 1, 1)
        }
    };

                context.AnnualLeaveBalance.AddRange(balances);
                context.SaveChanges();
            }
            // ===================== CONTRACT =====================
            if (!context.Contracts.Any())
            {
                context.Contracts.AddRange(
                   
                    new Contract
                    {
                        EmployeeId = 2,
                        ContractType = "Official",
                        StartDate = DateTime.Today.AddMonths(-6),
                        EndDate = null,
                        BasicSalary = 15000000,
                        IsActive = true
                    },
                    new Contract
                    {
                        EmployeeId = 3,
                        ContractType = "Probation",
                        StartDate = DateTime.Today.AddMonths(-1),
                        EndDate = DateTime.Today.AddMonths(2),
                        BasicSalary = 12000000,
                        IsActive = true
                    },
                    new Contract
                    {
                        EmployeeId = 4,
                        ContractType = "Official",
                        StartDate = DateTime.Today.AddYears(-2),
                        EndDate = DateTime.Today.AddMonths(-3),
                        BasicSalary = 13000000,
                        IsActive = false
                    }
                );

                context.SaveChanges();
            }

            // ===================== OT (OTSchedule, OverTimeRequest, OTAttendance) — test EmployeeId 3, ManagerId 4 =====================
            if (!context.OTSchedules.Any())
            {
                const int otEmployeeId = 3;
                const int otManagerId = 4;
                const int otDepartmentId = 3; // Finance — cùng phòng EMP003

                var futureOtDay = DateTime.Today.AddDays(14);
                var pastOtDay = DateTime.Today.AddDays(-5);

                var schedule = new OTSchedule
                {
                    Name = "Lịch OT test — Tổng kết quý (Finance)",
                    Description = "Seed: lịch OT chung để test đăng ký (EMP003 / Manager Giang)",
                    Quantity = 5,
                    StartDate = futureOtDay.Date,
                    EndDate = futureOtDay.Date,
                    StartTime = new TimeSpan(19, 30, 0),
                    EndTime = new TimeSpan(23, 30, 0),
                    Status = 5,
                    DepartmentId = otDepartmentId,
                    ManagerId = otManagerId,
                    CreatedAt = DateTime.Now
                };
                context.OTSchedules.Add(schedule);
                context.SaveChanges();

                var reqFromSchedule = new OverTimeRequest
                {
                    EmployeeId = otEmployeeId,
                    WorkDate = futureOtDay.Date,
                    StartTime = new TimeSpan(19, 30, 0),
                    EndTime = new TimeSpan(23, 30, 0),
                    Reason = "Hỗ trợ tổng kết quý theo lịch OT chung",
                    TaskRef = "Lịch OT Chung",
                    ManagerId = otManagerId,
                    OTScheduleId = schedule.Id,
                    EmployeeAccepted = true,
                    Status = 1
                };

                var reqPastWithAttendance = new OverTimeRequest
                {
                    EmployeeId = otEmployeeId,
                    WorkDate = pastOtDay.Date,
                    StartTime = new TimeSpan(19, 30, 0),
                    EndTime = new TimeSpan(22, 30, 0),
                    Reason = "Hỗ trợ release gấp (seed test OT)",
                    TaskRef = "PRJ-TEST-OT",
                    ManagerId = otManagerId,
                    OTScheduleId = null,
                    EmployeeAccepted = true,
                    Status = 1,
                    OTAttendance = new OTAttendance
                    {
                        CheckIn = new TimeSpan(19, 35, 0),
                        CheckOut = new TimeSpan(22, 20, 0),
                        Status = AttendanceStatus.CompletedWork
                    }
                };

                var reqFutureNoAttendance = new OverTimeRequest
                {
                    EmployeeId = otEmployeeId,
                    WorkDate = DateTime.Today.AddDays(3).Date,
                    StartTime = new TimeSpan(19, 30, 0),
                    EndTime = new TimeSpan(23, 0, 0),
                    Reason = "Hoàn thiện báo cáo (seed test OT)",
                    TaskRef = "RPT-TEST-OT",
                    ManagerId = otManagerId,
                    OTScheduleId = null,
                    EmployeeAccepted = true,
                    Status = 1
                };

                var reqCancelled = new OverTimeRequest
                {
                    EmployeeId = otEmployeeId,
                    WorkDate = DateTime.Today.AddDays(-10).Date,
                    StartTime = new TimeSpan(19, 30, 0),
                    EndTime = new TimeSpan(21, 30, 0),
                    Reason = "Đã hủy — đổi kế hoạch (seed test OT)",
                    TaskRef = "CANCEL-TEST-OT",
                    ManagerId = otManagerId,
                    OTScheduleId = null,
                    EmployeeAccepted = false,
                    Status = 3
                };

                context.OverTimeRequests.AddRange(
                    reqFromSchedule,
                    reqPastWithAttendance,
                    reqFutureNoAttendance,
                    reqCancelled);
                context.SaveChanges();
            }
        }
    }
}
