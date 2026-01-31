using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;
using Microsoft.EntityFrameworkCore;

namespace HumanResourcesManager.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly HumanManagerContext _context;

        public AuthService(HumanManagerContext context)
        {
            _context = context;
        }

        public void Register(RegisterDTO dto)
        {
            try
            {
                if (_context.UserAccounts.Any(u => u.Username == dto.Username))
                    throw new Exception("Username already exists");

                var employee = new Employee
                {
                    EmployeeCode = "EMP" + DateTime.Now.ToString("yyMMddHHmmss"),
                    FullName = dto.FullName,
                    Email = dto.Email,
                    Gender = dto.Gender,

                    Phone = "0000000000",
                    DateOfBirth = new DateTime(2000, 1, 1),

                    HireDate = DateTime.Now,
                    Status = CommonStatus.Active,
                    DepartmentId = 1,
                    PositionId = 1
                };

                _context.Employees.Add(employee);
                _context.SaveChanges();

                var empRoleId = _context.Roles
                    .First(r => r.RoleCode == "EMP").RoleId;

                var user = new UserAccount
                {
                    EmployeeId = employee.EmployeeId,
                    Username = dto.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    RoleId = empRoleId,
                    Status = CommonStatus.Active
                };

                _context.UserAccounts.Add(user);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception(ex.InnerException?.Message ?? "Database error");
            }
        }


        public UserSessionDTO? Login(LoginDTO dto)
        {
            var user = _context.UserAccounts
                .Include(u => u.Employee)
                .Include(u => u.Role)
                .FirstOrDefault(u =>
                    u.Username == dto.Username &&
                    u.Status == CommonStatus.Active);

            if (user == null) return null;

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            return new UserSessionDTO
            {
                UserId = user.UserId,
                EmployeeId = user.EmployeeId,
                Username = user.Username,
                FullName = user.Employee.FullName,
                RoleCode = user.Role.RoleCode,
                RoleName = user.Role.RoleName
            };
        }
    }
}
