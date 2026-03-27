// HumanResourcesManager.Tests/AuthServiceTests.cs
using HumanResourcesManager.BLL.Services;
using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HumanResourcesManager.Tests
{
    public class AuthServiceTests
    {
        private DbContextOptions<HumanManagerContext> _options;

        public AuthServiceTests()
        {
            _options = new DbContextOptionsBuilder<HumanManagerContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;
        }

        private Employee CreateEmployee(string fullName, string email, string empCode = "EMP0001", string phone = "0123456789")
        {
            return new Employee
            {
                EmployeeId = 1,
                EmployeeCode = empCode,
                FullName = fullName,
                Email = email,
                Phone = phone,
                Status = Constants.Active,
                Gender = true,
                DepartmentId = 1,
                PositionId = 1,
                HireDate = DateTime.Now,
                DateOfBirth = new DateTime(2000, 1, 1)
            };
        }

        private UserAccount CreateUser(string username, string password, Role role, Employee emp)
        {
            return new UserAccount
            {
                UserId = 1,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Status = Constants.Active,
                Role = role,
                Employee = emp
            };
        }

        private Role CreateRole(string code, string name)
        {
            return new Role
            {
                RoleId = 1,
                RoleCode = code,
                RoleName = name
            };
        }

        [Fact]
        public void Login_ValidUser_ReturnsUserSessionDTO()
        {
            using (var context = new HumanManagerContext(_options))
            {
                var role = CreateRole("ADMIN", "Admin");
                context.Roles.Add(role);

                var emp = CreateEmployee("John Doe", "john@example.com");
                context.Employees.Add(emp);

                var user = CreateUser("john", "123456", role, emp);
                context.UserAccounts.Add(user);

                context.SaveChanges();

                var authService = new AuthService(context, null, null);

                var dto = new LoginDTO { LoginKey = "john", Password = "123456" };

                var result = authService.Login(dto);

                Assert.NotNull(result);
                Assert.Equal(user.UserId, result.UserId);
                Assert.Equal(user.Username, result.Username);
                Assert.Equal(emp.FullName, result.FullName);
                Assert.Equal(role.RoleCode, result.RoleCode);
            }
        }

        [Fact]
        public void Login_InvalidPassword_ReturnsNull()
        {
            using (var context = new HumanManagerContext(_options))
            {
                var role = CreateRole("ADMIN", "Admin");
                context.Roles.Add(role);

                var emp = CreateEmployee("John Doe", "john@example.com");
                context.Employees.Add(emp);

                var user = CreateUser("john", "123456", role, emp);
                context.UserAccounts.Add(user);

                context.SaveChanges();

                var authService = new AuthService(context, null, null);

                var dto = new LoginDTO { LoginKey = "john", Password = "wrongpass" };

                var result = authService.Login(dto);

                Assert.Null(result);
            }
        }

        [Fact]
        public void Login_NonExistentUser_ReturnsNull()
        {
            using (var context = new HumanManagerContext(_options))
            {
                var authService = new AuthService(context, null, null);

                var dto = new LoginDTO { LoginKey = "nonexist", Password = "123456" };

                var result = authService.Login(dto);

                Assert.Null(result);
            }
        }

        [Fact]
        public void Login_ByEmail_ReturnsUserSessionDTO()
        {
            using (var context = new HumanManagerContext(_options))
            {
                var role = CreateRole("EMP", "Employee");
                context.Roles.Add(role);

                var emp = CreateEmployee("Jane Smith", "jane@example.com");
                context.Employees.Add(emp);

                var user = CreateUser("jane", "password", role, emp);
                context.UserAccounts.Add(user);

                context.SaveChanges();

                var authService = new AuthService(context, null, null);

                var dto = new LoginDTO { LoginKey = "jane@example.com", Password = "password" };

                var result = authService.Login(dto);

                Assert.NotNull(result);
                Assert.Equal(user.UserId, result.UserId);
                Assert.Equal(user.Username, result.Username);
                Assert.Equal(emp.FullName, result.FullName);
                Assert.Equal(role.RoleCode, result.RoleCode);
            }
        }
    }
}