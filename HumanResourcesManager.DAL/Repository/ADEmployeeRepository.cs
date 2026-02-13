using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared; 
using Microsoft.EntityFrameworkCore;


// HoangDH
namespace HumanResourcesManager.DAL.Repository
{
    public class ADEmployeeRepository : IADEmployeeRepository
    {
        private readonly HumanManagerContext _context;

        public ADEmployeeRepository(HumanManagerContext context)
        {
            _context = context;
        }

        public IEnumerable<Employee> GetAll()
        {
            return _context.Employees
                .Include(e => e.Department)  
                .Include(e => e.Position)    
                .OrderByDescending(e => e.HireDate) 
                .OrderByDescending(e => e.EmployeeId) 
                .ToList();
        }

        public string? GetLastEmployeeCode(string prefix)
        {
            return _context.Employees
                .Where(e => e.EmployeeCode.StartsWith(prefix))
                .OrderByDescending(e => e.EmployeeCode)
                .Select(e => e.EmployeeCode)
                .FirstOrDefault();
        }

        public void Add(Employee employee)
        {
            _context.Employees.Add(employee);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public bool ExistsEmail(string email)
        {
            return _context.Employees.Any(e => e.Email == email);
        }

        public bool ExistsPhone(string phone)
        {
            return _context.Employees.Any(e => e.Phone == phone);
        }

        public IEnumerable<Department> GetDepartments()
        {
            return _context.Departments.Where(d => d.Status == Constants.Active).ToList();
        }

        public IEnumerable<Position> GetPositions()
        {
            return _context.Positions.Where(p => p.Status == Constants.Active).ToList();
        }

        public Employee? GetById(int id)
        {
            return _context.Employees
                .Include(e => e.UserAccount) 
                .FirstOrDefault(e => e.EmployeeId == id);
        }

        public void Update(Employee employee)
        {
            _context.Employees.Update(employee);
        }

        public bool ExistsEmail(string email, int excludeId)
        {
            return _context.Employees.Any(e => e.Email == email && e.EmployeeId != excludeId);
        }

        public bool ExistsPhone(string phone, int excludeId)
        {
            return _context.Employees.Any(e => e.Phone == phone && e.EmployeeId != excludeId);
        }

        public int GetRoleIdByCode(string roleCode)
        {
            var role = _context.Roles.FirstOrDefault(r => r.RoleCode == roleCode);
            return role != null ? role.RoleId : 0; 
        }
    }
}