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
                    new Role { RoleCode = "EMP", RoleName = "Employee", Status = CommonStatus.Active },
                    new Role { RoleCode = "HR", RoleName = "HR", Status = CommonStatus.Active },
                    new Role { RoleCode = "ADMIN", RoleName = "Admin", Status = CommonStatus.Active }
                );
                context.SaveChanges();
            }

            // ===================== DEPARTMENT =====================
            if (!context.Departments.Any())
            {
                context.Departments.AddRange(
                    new Department { DepartmentName = "HR" },
                    new Department { DepartmentName = "IT" },
                    new Department { DepartmentName = "Finance" }
                );
                context.SaveChanges();
            }

            // ===================== POSITION =====================
            if (!context.Positions.Any())
            {
                context.Positions.AddRange(
                    new Position { PositionName = "HR Executive", BaseSalary = 8000000 },
                    new Position { PositionName = "Software Engineer", BaseSalary = 15000000 },
                    new Position { PositionName = "Accountant", BaseSalary = 12000000 }
                );
                context.SaveChanges();
            }

            // ===================== EMPLOYEE =====================
            if (!context.Employees.Any())
            {
                context.Employees.AddRange(
                    new Employee
                    {
                        EmployeeCode = "EMP001",
                        FullName = "A Hoang dep zai",
                        Email = "a.nguyen@company.com",
                        Phone = "0901234567",
                        DepartmentId = 1,
                        PositionId = 1,
                        Status = CommonStatus.Active,
                        ImgAvatar = null
                    },
                    new Employee
                    {
                        EmployeeCode = "EMP002",
                        FullName = "Tran Thi B",
                        Email = "b.tran@company.com",
                        Phone = "0912345678",
                        DepartmentId = 2,
                        PositionId = 2,
                        Status = CommonStatus.Active,
                        ImgAvatar = null
                    },
                    new Employee
                    {
                        EmployeeCode = "EMP003",
                        FullName = "Le Van C",
                        Email = "c.le@company.com",
                        Phone = "0923456789",
                        DepartmentId = 3,
                        PositionId = 3,
                        Status = CommonStatus.Active,
                        ImgAvatar = null
                    }
                );
                context.SaveChanges();
            }

            // ===================== USER ACCOUNT =====================
            if (!context.UserAccounts.Any())
            {
                var adminRoleId = context.Roles.First(r => r.RoleCode == "ADMIN").RoleId;
                var hrRoleId = context.Roles.First(r => r.RoleCode == "HR").RoleId;
                var empRoleId = context.Roles.First(r => r.RoleCode == "EMP").RoleId;

                context.UserAccounts.AddRange(
                    new UserAccount
                    {
                        EmployeeId = 1,
                        Username = "admin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        RoleId = adminRoleId,
                        Status = CommonStatus.Active
                    },
                    new UserAccount
                    {
                        EmployeeId = 2,
                        Username = "hr",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        RoleId = hrRoleId,
                        Status = CommonStatus.Active
                    },
                    new UserAccount
                    {
                        EmployeeId = 3,
                        Username = "emp",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        RoleId = empRoleId,
                        Status = CommonStatus.Active
                    }
                );
                context.SaveChanges();
            }

            // ===================== ALLOWANCE =====================
            if (!context.Allowances.Any())
            {
                context.Allowances.AddRange(
                    new Allowance { AllowanceName = "Lunch Allowance", Amount = 500000 },
                    new Allowance { AllowanceName = "Transport Allowance", Amount = 300000 }
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
                        LatePenalty = 0,
                        NetSalary = 8500000
                    },
                    new Payroll
                    {
                        EmployeeId = 2,
                        BasicSalary = 15000000,
                        TotalOT = 2000000,
                        TotalAllowance = 800000,
                        LatePenalty = 0,
                        NetSalary = 17800000
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
