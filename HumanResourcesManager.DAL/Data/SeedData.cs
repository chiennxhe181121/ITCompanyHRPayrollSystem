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
            // context.Database.EnsureCreated();

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
                context.Payrolls.AddRange(
                    new Payroll
                    {
                        EmployeeId = 1,
                        BasicSalary = 8000000,
                        TotalOT = 0,
                        TotalAllowance = 500000,
                        MissingMinutesPenalty = 0,
                        NetSalary = 8500000
                    },
                    new Payroll
                    {
                        EmployeeId = 2,
                        BasicSalary = 15000000,
                        TotalOT = 2000000,
                        TotalAllowance = 800000,
                        MissingMinutesPenalty = 0,
                        NetSalary = 17800000
                    }
                );
                context.SaveChanges();
            }

            // ===================== ATTENDANCE =====================
            if (!context.Attendances.Any())
            {
                var employeeId = 3; // EMP003

                var today = DateTime.Today;

                var attendances = new List<Attendance>
    {
        // 1. Làm đủ giờ
        new Attendance
        {
            EmployeeId = employeeId,
            WorkDate = today.AddDays(-9),
            CheckIn = new TimeSpan(8, 0, 0),
            CheckOut = new TimeSpan(17, 0, 0),
            MissingMinutes = 0,
            Status = AttendanceStatus.CompletedWork
        },

        // 2. Đi trễ nhẹ nhưng vẫn đủ giờ
        new Attendance
        {
            EmployeeId = employeeId,
            WorkDate = today.AddDays(-8),
            CheckIn = new TimeSpan(8, 10, 0),
            CheckOut = new TimeSpan(17, 10, 0),
            MissingMinutes = 10,
            Status = AttendanceStatus.CompletedWork
        },

        // 3. Thiếu 1 tiếng
        new Attendance
        {
            EmployeeId = employeeId,
            WorkDate = today.AddDays(-7),
            CheckIn = new TimeSpan(8, 0, 0),
            CheckOut = new TimeSpan(16, 0, 0),
            MissingMinutes = 60,
            Status = AttendanceStatus.InsufficientWork
        },

        // 4. Quên check-out
        new Attendance
        {
            EmployeeId = employeeId,
            WorkDate = today.AddDays(-6),
            CheckIn = new TimeSpan(8, 5, 0),
            CheckOut = null,
            MissingMinutes = 0,
            Status = AttendanceStatus.MissingCheckOut
        },

        // 5. Nghỉ có phép
        new Attendance
        {
            EmployeeId = employeeId,
            WorkDate = today.AddDays(-5),
            CheckIn = null,
            CheckOut = null,
            MissingMinutes = 0,
            Status = AttendanceStatus.ApprovedLeave
        },

        // 6. Vắng mặt
        new Attendance
        {
            EmployeeId = employeeId,
            WorkDate = today.AddDays(-4),
            CheckIn = null,
            CheckOut = null,
            MissingMinutes = 0,
            Status = AttendanceStatus.Absent
        },

        // 7. Làm đủ giờ
        new Attendance
        {
            EmployeeId = employeeId,
            WorkDate = today.AddDays(-3),
            CheckIn = new TimeSpan(8, 0, 0),
            CheckOut = new TimeSpan(17, 0, 0),
            MissingMinutes = 0,
            Status = AttendanceStatus.CompletedWork
        },

        // 8. Thiếu 30 phút
        new Attendance
        {
            EmployeeId = employeeId,
            WorkDate = today.AddDays(-2),
            CheckIn = new TimeSpan(8, 0, 0),
            CheckOut = new TimeSpan(16, 30, 0),
            MissingMinutes = 30,
            Status = AttendanceStatus.InsufficientWork
        },

        // 9. Quên check-out
        new Attendance
        {
            EmployeeId = employeeId,
            WorkDate = today.AddDays(-1),
            CheckIn = new TimeSpan(8, 15, 0),
            CheckOut = null,
            MissingMinutes = 0,
            Status = AttendanceStatus.MissingCheckOut
        },

        // 10. Làm đủ giờ hôm nay
        new Attendance
        {
            EmployeeId = employeeId,
            WorkDate = today,
            CheckIn = new TimeSpan(8, 0, 0),
            CheckOut = new TimeSpan(17, 0, 0),
            MissingMinutes = 0,
            Status = AttendanceStatus.CompletedWork
        } ,

        // 11. Ngày nghỉ lễ
        new Attendance
        {
            EmployeeId = employeeId,
            WorkDate = new DateTime(DateTime.Today.Year - 1, 12, 25),
            CheckIn = null,
            CheckOut = null,
            MissingMinutes = 0,
            Status = AttendanceStatus.Holiday
        }
        };

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
        new LeaveType
        {
            LeaveName = "Unpaid Leave",
            IsPaid = false
        },
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

        }
    }
}
