using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.DAL.Data
{
    public static class SeedData
    {
        public static void Initialize(HumanManagerContext context)
        {
            context.Database.EnsureCreated();

            // ===================== Department =====================
            if (!context.Departments.Any())
            {
                context.Departments.AddRange(
                    new Department { DepartmentName = "HR" },
                    new Department { DepartmentName = "IT" },
                    new Department { DepartmentName = "Finance" }
                );
                context.SaveChanges();
            }

            // ===================== Position =====================
            if (!context.Positions.Any())
            {
                context.Positions.AddRange(
                    new Position { PositionName = "HR Executive", BaseSalary = 8000000 },
                    new Position { PositionName = "Software Engineer", BaseSalary = 15000000 },
                    new Position { PositionName = "Accountant", BaseSalary = 12000000 }
                );
                context.SaveChanges();
            }

            // ===================== Employee =====================
            if (!context.Employees.Any())
            {
                context.Employees.AddRange(
    new Employee
    {
        EmployeeCode = "EMP001",
        FullName = "Nguyen Van A",
        Phone = "0901234567",
        Email = "a.nguyen@company.com",
        DepartmentId = 1,
        PositionId = 1
    },
    new Employee
    {
        EmployeeCode = "EMP002",
        FullName = "Tran Thi B",
        Phone = "0912345678",
        Email = "b.tran@company.com",
        DepartmentId = 2,
        PositionId = 2
    },
    new Employee
    {
        EmployeeCode = "EMP003",
        FullName = "Le Van C",
        Phone = "0923456789",
        Email = "c.le@company.com",
        DepartmentId = 3,
        PositionId = 3
    }
);


                context.SaveChanges();
            }

            // ===================== Allowance =====================
            if (!context.Allowances.Any())
            {
                context.Allowances.AddRange(
                    new Allowance { AllowanceName = "Lunch Allowance", Amount = 500000 },
                    new Allowance { AllowanceName = "Transport Allowance", Amount = 300000 }
                );
                context.SaveChanges();
            }

            // ===================== EmployeeAllowance =====================
            if (!context.EmployeeAllowances.Any())
            {
                context.EmployeeAllowances.AddRange(
                    new EmployeeAllowance { EmployeeId = 1, AllowanceId = 1 },
                    new EmployeeAllowance { EmployeeId = 2, AllowanceId = 1 },
                    new EmployeeAllowance { EmployeeId = 2, AllowanceId = 2 }
                );
                context.SaveChanges();
            }

            // ===================== Payroll =====================
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
