using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using Microsoft.EntityFrameworkCore;



// @HoangDH 
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


        // new methods can be added here as needed
        public IEnumerable<UserAccount> GetAll()
        {
            return _context.UserAccounts
                .Include(x => x.Role)
                .Include(x => x.Employee);
        }


        public UserAccount? GetById(int id)
        {
            return _context.UserAccounts
                .Include(x => x.Role)
                .Include(x => x.Employee)
                .FirstOrDefault(x => x.UserId == id && x.Status != -1);
        }

        public UserAccount Add(UserAccount user)
        {
            _context.UserAccounts.Add(user);
            return user;
        }

        public void Update(UserAccount user)
        {
            _context.UserAccounts.Update(user);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public bool ExistsByUsername(string username)
        {
            return _context.UserAccounts
                .Any(x => x.Username == username);
        }

        public bool ExistsByEmail(string email)
        {
            return _context.Employees
                .Any(e => e.Email == email);
        }

        public Role? GetRoleByCode(string roleCode)
        {
            return _context.Roles
                .FirstOrDefault(r => r.RoleCode == roleCode);
        }




    }
}
