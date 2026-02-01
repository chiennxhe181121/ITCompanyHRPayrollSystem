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
                // 1️⃣ Check Username
                if (_context.UserAccounts.Any(u => u.Username == dto.Username))
                    throw new Exception("Username already exists");

                // 2️⃣ Check Email (Employee)
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
                    u.Status == CommonStatus.Active &&
                    (
                        u.Username == dto.LoginKey ||
                        u.Employee.Email == dto.LoginKey
                    )
                );

            if (user == null)
                return null;

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




        /// <summary>
        ///     new
        /// </summary>
        /// <returns></returns>
        //public string GenerateOtp()
        //{
        //    return new Random().Next(100000, 999999).ToString();
        //}

        //public void ResetPasswordByEmail(string email, string newPassword, string confirmPassword)
        //{
        //    if (newPassword != confirmPassword)
        //        throw new Exception("Confirm password does not match new password");

        //    var user = _context.UserAccounts
        //        .Include(u => u.Employee)
        //        .FirstOrDefault(u => u.Employee.Email == email);

        //    if (user == null)
        //        throw new Exception("Email not found");

        //    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        //    _context.SaveChanges();
        //}


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



    }
}
