using System;
using System.Collections.Generic;
using System.Linq;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Enum;
using HumanResourcesManager.DAL.Shared;
using HumanResourcesManager.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace HumanResourcesManager.DAL.Data
{
    public static class TestDataGenerator
    {
        public static void GenerateExtraData(HumanManagerContext context)
        {
            // 1. Ensure Roles & Position Exist
            var empRole = context.Roles.FirstOrDefault(r => r.RoleCode == "EMP");
            var managerRole = context.Roles.FirstOrDefault(r => r.RoleCode == "MANAGER");
            if (empRole == null || managerRole == null) return;

            var softwarePosition = context.Positions.FirstOrDefault() ?? new Position { PositionName = "Software Engineer", BaseSalary = 10000000, Status = Constants.Active, CreatedAt = DateTime.Now };
            if (softwarePosition.PositionId == 0) 
            {
                context.Positions.Add(softwarePosition);
                context.SaveChanges();
            }

            // 2. Ensure Special "Floating" Department Exists
            var floatingDept = context.Departments.FirstOrDefault(d => d.DepartmentName == "[TEST ONLY] - CHƯA PHÂN LOẠI");
            if (floatingDept == null)
            {
                floatingDept = new Department { DepartmentName = "[TEST ONLY] - CHƯA PHÂN LOẠI", Status = Constants.Active };
                context.Departments.Add(floatingDept);
                context.SaveChanges();
            }

            // 3. Ensure Other Departments Exist
            string[] deptNames = { "IT", "HR", "Finance", "Marketing", "Sales", "Customer Support" };
            foreach (var name in deptNames)
            {
                if (!context.Departments.Any(d => d.DepartmentName == name))
                {
                    context.Departments.Add(new Department { DepartmentName = name, Status = Constants.Active });
                }
            }
            context.SaveChanges();

            var depts = context.Departments.Where(d => d.DepartmentName != "[TEST ONLY] - CHƯA PHÂN LOẠI").ToList();

            // 4. Create "Floating" Employees (Assigned to the special department)
            for (int i = 1; i <= 5; i++)
            {
                string username = $"test_floating_{i}";
                string code = $"FL{i:D3}";
                
                if (!context.UserAccounts.Any(u => u.Username == username) && !context.Employees.Any(e => e.EmployeeCode == code))
                {
                    var user = new UserAccount
                    {
                        Username = username,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        RoleId = empRole.RoleId,
                        Status = Constants.Active
                    };
                    context.UserAccounts.Add(user);

                    var emp = new Employee
                    {
                        EmployeeCode = code,
                        FullName = $"NV Tự Do {i}",
                        Email = $"{username}@example.com",
                        Phone = $"09{Random.Shared.Next(10000000, 99999999)}",
                        DepartmentId = floatingDept.DepartmentId, // Gán vào phòng chờ thay vì 0
                        PositionId = softwarePosition.PositionId,
                        Status = Constants.Active,
                        UserAccount = user,
                        HireDate = DateTime.Now
                    };
                    context.Employees.Add(emp);
                }
            }
            context.SaveChanges();

            // 5. Ensure Managers & Employees for real departments
            foreach (var dept in depts)
            {
                // Manager
                string mUsername = $"manager_{dept.DepartmentName.ToLower().Replace(" ", "_")}";
                string mCode = $"MGR{dept.DepartmentId:D3}";
                if (!context.UserAccounts.Any(u => u.Username == mUsername) && !context.Employees.Any(e => e.EmployeeCode == mCode))
                {
                    var u = new UserAccount { Username = mUsername, PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), RoleId = managerRole.RoleId, Status = Constants.Active };
                    context.UserAccounts.Add(u);
                    context.Employees.Add(new Employee { EmployeeCode = mCode, FullName = $"Quản Lý {dept.DepartmentName}", Email = $"{mUsername}@example.com", Phone = $"09{Random.Shared.Next(10, 99)}888888", DepartmentId = dept.DepartmentId, PositionId = softwarePosition.PositionId, Status = Constants.Active, UserAccount = u, HireDate = DateTime.Now.AddYears(-1) });
                }

                // Employees
                for (int i = 1; i <= 3; i++)
                {
                    string eUsername = $"emp_{dept.DepartmentName.ToLower().Replace(" ", "_")}_{i}";
                    string eCode = $"E{dept.DepartmentId:D2}{i:D3}";
                    if (!context.UserAccounts.Any(u => u.Username == eUsername) && !context.Employees.Any(e => e.EmployeeCode == eCode))
                    {
                        var u = new UserAccount { Username = eUsername, PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), RoleId = empRole.RoleId, Status = Constants.Active };
                        context.UserAccounts.Add(u);
                        context.Employees.Add(new Employee { EmployeeCode = eCode, FullName = $"NV {dept.DepartmentName} {i}", Email = $"{eUsername}@example.com", Phone = $"09{Random.Shared.Next(10, 99)}666666", DepartmentId = dept.DepartmentId, PositionId = softwarePosition.PositionId, Status = Constants.Active, UserAccount = u, HireDate = DateTime.Now });
                    }
                }
            }
            context.SaveChanges();

            // 6. Generate random OT Schedules
            var allManagers = context.Employees.Where(e => e.UserAccount.RoleId == managerRole.RoleId).ToList();
            foreach (var mgr in allManagers)
            {
                if (context.OTSchedules.Count(s => s.ManagerId == mgr.EmployeeId) < 2)
                {
                    var startDateTime = DateTime.Today.Add(new TimeSpan(18, 0, 0));
                    var initialStatus = DateTime.Now < startDateTime ? 5 : 0;

                    context.OTSchedules.Add(new OTSchedule { 
                        Name = $"OT {mgr.Department?.DepartmentName} Urgent", 
                        Description = "Test data", 
                        Quantity = 5, 
                        StartDate = DateTime.Today, StartTime = new TimeSpan(18, 0, 0), EndDate = DateTime.Today, EndTime = new TimeSpan(21, 0, 0), 
                        DepartmentId = mgr.DepartmentId, ManagerId = mgr.EmployeeId, 
                        Status = initialStatus, CreatedAt = DateTime.Now 
                    });
                }
            }
            context.SaveChanges();
        }
    }
}
