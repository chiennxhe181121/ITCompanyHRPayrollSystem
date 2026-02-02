using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace HumanResourcesManager.DAL.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly HumanManagerContext _context;

        public EmployeeRepository(HumanManagerContext context)
        {
            _context = context;
        }

        public IEnumerable<Employee> GetAll()
        {
            return _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Where(e => e.Status != -1)
                .ToList();
        }

        public Employee? GetById(int id)
        {
            return _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefault(e => e.EmployeeId == id && e.Status != -1);
        }

        public void Add(Employee employee)
        {
            _context.Employees.Add(employee);
        }

        public void Update(Employee employee)
        {
            _context.Employees.Update(employee);
        }

        public void SoftDelete(int id)
        {
            var emp = _context.Employees.Find(id);
            if (emp != null)
            {
                emp.Status = -1;
                _context.Employees.Update(emp);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
