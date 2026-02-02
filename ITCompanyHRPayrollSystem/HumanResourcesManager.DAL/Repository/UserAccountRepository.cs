using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace HumanResourcesManager.DAL.Repositories
{
    public class UserAccountRepository : IUserAccountRepository
    {
        private readonly HumanManagerContext _context;

        public UserAccountRepository(HumanManagerContext context)
        {
            _context = context;
        }

        public UserAccount? GetByEmployeeEmail(string email)
        {
            return _context.UserAccounts
                .Include(x => x.Employee)
                .Include(x => x.Role)
                .FirstOrDefault(x => x.Employee.Email == email
                                  && x.Status == 1);
        }

        public UserAccount AddGoogleAccount(Employee employee)
        {
            var role = _context.Roles.First(r => r.RoleCode == "EMP");

            var account = new UserAccount
            {
            
                Username = employee.Email,
                  // ✅ hash password mặc định
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                RoleId = role.RoleId,
                Status = 1
            };

            _context.UserAccounts.Add(account);
            _context.SaveChanges();

            return account;
        }
    }
}
