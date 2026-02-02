using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;



namespace HumanResourcesManager.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly HumanManagerContext _context;
        private readonly IUserAccountRepository _userAccountRepository;


        public AuthService(
            HumanManagerContext context,
            IUserAccountRepository userAccountRepository
             )
        {
            _context = context;
            _userAccountRepository = userAccountRepository;
        }

        public void Register(RegisterDTO dto)
        {
            try
            {
                // 1️. Check Username
                if (_context.UserAccounts.Any(u => u.Username == dto.Username))
                    throw new Exception("Username already exists");

                // 2️. Check Email (Employee)
                if (_context.Employees.Any(e => e.Email == dto.Email))
                    throw new Exception("Email already exists");

                var employee = new Employee
                {
                    EmployeeCode = "EMP" + DateTime.Now.ToString("yyMMddHHmmss"),
                    FullName = dto.FullName,
                    Email = dto.Email,
                    Gender = dto.Gender,

                    Phone = "0000000000",
                    DateOfBirth = new DateTime(2000, 1, 1),

                    HireDate = DateTime.Now,
                    Status = Constants.Active,
                    DepartmentId = 1,
                    PositionId = 1
                };

                _context.Employees.Add(employee);
                _context.SaveChanges();

                var empRoleId = _context.Roles
                    .First(r => r.RoleCode == "EMP").RoleId;

                var user = new UserAccount
                {
                   
                    Username = dto.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    RoleId = empRoleId,
                    Status = Constants.Active
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
                    u.Status == Constants.Active &&
                    (
                        u.Username == dto.LoginKey ||
                        u.Employee.Email == dto.LoginKey
                    )
                );

            if (user == null)
                return null;

            if (user.PasswordHash == null)
                return null; // Google account không login bằng password

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            return new UserSessionDTO
            {
                UserId = user.UserId,
            
                Username = user.Username,
                FullName = user.Employee.FullName,
                RoleCode = user.Role.RoleCode,
                RoleName = user.Role.RoleName
            };
        }


        public void ResetPassword(string email, string newPass, string confirmPass)
        {
            if (newPass != confirmPass)
                throw new Exception("Password confirmation does not match");

            var user = _context.UserAccounts
                .Include(u => u.Employee)
                .FirstOrDefault(u => u.Employee.Email == email);

            if (user == null)
                throw new Exception("Email not found");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPass);
            _context.SaveChanges();
        }



        // ===== LOGIN GOOGLE =====
        public UserSessionDTO LoginWithGoogle(string email, string fullName)
        {
            var account = _userAccountRepository.GetByEmployeeEmail(email);

            if (account == null)
            {
                var employee = new Employee
                {
                    EmployeeCode = "EMP" + DateTime.Now.ToString("yyMMddHHmmss"),
                    FullName = fullName,
                    Email = email,
                    Gender = true,

                    Phone = "0000000000",

                    DateOfBirth = new DateTime(2003, 1, 1),

                    HireDate = DateTime.Now,
                    Status = Constants.Active,
                    DepartmentId = 1,
                    PositionId = 1
                };

                _context.Employees.Add(employee);
                _context.SaveChanges();

                account = _userAccountRepository.AddGoogleAccount(employee);
            }

            if (account.Role == null)
                throw new Exception("User role not found");

            return new UserSessionDTO
            {
                UserId = account.UserId,
             
                Username = account.Username,
                FullName = account.Employee.FullName,
                RoleCode = account.Role.RoleCode,
                RoleName = account.Role.RoleName
            };
        }

    }
}
