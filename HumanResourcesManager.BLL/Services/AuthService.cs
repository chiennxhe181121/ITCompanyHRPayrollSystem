using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


// @HoangDH 
namespace HumanResourcesManager.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly HumanManagerContext _context;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IADEmployeeRepository _empRepo;

        public AuthService(
            HumanManagerContext context,
            IUserAccountRepository userAccountRepository,
            IADEmployeeRepository empRepo
             )
        {
            _context = context;
            _userAccountRepository = userAccountRepository;
            _empRepo = empRepo;
        }

        // Tạo Mã Nhân Viên: VD: EMP260001
        private string GenerateEmployeeCode()
        {
            string yearPrefix = DateTime.Now.ToString("yy");
            string prefix = $"EMP{yearPrefix}";

            string? lastCode = _empRepo.GetLastEmployeeCode(prefix);
            int nextSequence = 1;

            if (!string.IsNullOrEmpty(lastCode))
            {
                string numberPart = lastCode.Substring(prefix.Length);
                if (int.TryParse(numberPart, out int currentSequence))
                {
                    nextSequence = currentSequence + 1;
                }
            }

            return $"{prefix}{nextSequence.ToString("D4")}";
        }


        public void Register(RegisterDTO dto)
        {
            if (_context.UserAccounts.Any(u => u.Username == dto.Username))
                throw new Exception("Username already exists");

            if (_context.Employees.Any(e => e.Email == dto.Email))
                throw new Exception("Email already exists");

            var roleId = _context.Roles.First(r => r.RoleCode == "EMP").RoleId;

            var user = new UserAccount
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = roleId,
                Status = Constants.Active
            };

            var employee = new Employee
            {
                //EmployeeCode = "EMP" + DateTime.Now.ToString("yyMMddHHmmss"),
                EmployeeCode = GenerateEmployeeCode(),
                FullName = dto.FullName,
                Email = dto.Email,
                Gender = dto.Gender,
                Phone = "0000000000",
                DateOfBirth = new DateTime(2000, 1, 1),
                HireDate = DateTime.Now,
                Status = Constants.Active,
                DepartmentId = 1,
                PositionId = 1,

                UserAccount = user
            };

            _context.Employees.Add(employee);
            _context.SaveChanges();
        }


        public UserSessionDTO? Login(LoginDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.LoginKey) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return null;
            }

            var user = _context.UserAccounts
                .Include(u => u.Employee)
                .Include(u => u.Role)
                .FirstOrDefault(u =>
                    u.Status == Constants.Active &&
                    (
                        u.Username == dto.LoginKey ||
                        (u.Employee != null && u.Employee.Email == dto.LoginKey)
                    )
                );

            if (user == null)
                return null;

            //if (!user.IsActive) return null; // user bị khóa coi như login fail

            if (user.PasswordHash == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            return new UserSessionDTO
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.Employee != null
                    ? user.Employee.FullName
                    : user.Username, 

                RoleCode = user.Role.RoleCode,
                RoleName = user.Role.RoleName,
                ImgAvatar = user.Employee?.ImgAvatar
            };
        }

        public void ResetPassword(string email, string newPass, string confirmPass)
        {
            if (newPass != confirmPass)
                throw new Exception("Password confirmation does not match");

            var user = _context.Employees
                .Include(e => e.UserAccount)
                .Where(e => e.Email == email)
                .Select(e => e.UserAccount)
                .FirstOrDefault(u => u != null);

            if (user == null)
                throw new Exception("Email not found");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPass);
            _context.SaveChanges();
        }


        public UserSessionDTO LoginWithGoogle(string email, string fullName)
        {
            var account = _context.UserAccounts
                .Include(u => u.Employee)
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Username == email);

            if (account == null)
            {
                var empRoleId = _context.Roles
                    .Where(r => r.RoleCode == "EMP")
                    .Select(r => r.RoleId)
                    .First();

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

                var user = new UserAccount
                {
                    Username = email,                
                    //PasswordHash = null,             
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                    RoleId = empRoleId,
                    Status = Constants.Active,
                    Employee = employee              

                };

                _context.UserAccounts.Add(user);
                _context.SaveChanges();

                account = _context.UserAccounts
                    .Include(u => u.Employee)
                    .Include(u => u.Role)
                    .First(u => u.UserId == user.UserId);
            }

            return new UserSessionDTO
            {
                UserId = account.UserId,
                Username = account.Username,
                FullName = account.Employee!.FullName,
                RoleCode = account.Role!.RoleCode,
                RoleName = account.Role.RoleName,
                ImgAvatar = account.Employee?.ImgAvatar  
            };
        }



    }
}
