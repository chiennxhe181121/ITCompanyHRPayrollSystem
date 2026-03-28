using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Enum;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;
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

        public IEnumerable<Employee> GetByDepartment(int departmentId, int? excludeEmployeeId = null)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Where(e => e.Status != -1 && e.DepartmentId == departmentId);

            if (excludeEmployeeId.HasValue)
            {
                query = query.Where(e => e.EmployeeId != excludeEmployeeId.Value);
            }

            return query
                .OrderBy(e => e.FullName)
                .ToList();
        }

        public int CountByDepartment(int departmentId, int? excludeEmployeeId = null)
        {
            var query = _context.Employees
                .Where(e => e.Status != -1 && e.DepartmentId == departmentId);

            if (excludeEmployeeId.HasValue)
            {
                query = query.Where(e => e.EmployeeId != excludeEmployeeId.Value);
            }

            return query.Count();
        }

        public int CountPendingOvertimeRequestsForManager(int managerEmployeeId)
        {
            return _context.OverTimeRequests.Count(x =>
                x.ManagerId == managerEmployeeId &&
                x.Status == (long)RequestStatus.Pending);
        }

        public int CountOvertimeSchedulesForManager(int managerEmployeeId)
        {
            var now = VietnamClock.Now;
            return _context.OTSchedules.Count(x =>
                x.ManagerId == managerEmployeeId &&
                x.CreatedAt.Month == now.Month &&
                x.CreatedAt.Year == now.Year);
        }

        public int CountCompletedOTSchedulesForManager(int managerEmployeeId)
        {
            var now = VietnamClock.Now;
            return _context.OTSchedules.Count(x =>
                x.ManagerId == managerEmployeeId &&
                x.Status == 4 &&
                x.CreatedAt.Month == now.Month &&
                x.CreatedAt.Year == now.Year);
        }

        public IEnumerable<Employee> GetEmployeesWithoutDepartment()
        {
            return _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Where(e => e.Status != -1 && (e.Department == null || e.DepartmentId == 0 || e.Department.DepartmentName == "[TEST ONLY] - CHƯA PHÂN LOẠI"))
                .OrderBy(e => e.FullName)
                .ToList();
        }

        public bool AssignEmployeeToDepartment(int employeeId, int departmentId)
        {
            var employee = _context.Employees.Include(e => e.Department).FirstOrDefault(e => e.EmployeeId == employeeId && e.Status != -1);
            if (employee == null)
            {
                return false;
            }

            // Cho phép chuyển phòng ban nếu nhân viên chưa có phòng (Id=0) hoặc đang ở phòng ban chờ đặc biệt
            if (employee.DepartmentId > 0 && employee.Department?.DepartmentName != "[TEST ONLY] - CHƯA PHÂN LOẠI")
            {
                return false;
            }

            employee.DepartmentId = departmentId;
            _context.Employees.Update(employee);
            _context.SaveChanges();
            return true;
        }

        public Employee? GetById(int id)
        {
            return _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefault(e => e.EmployeeId == id && e.Status != -1);
        }

        public Employee? GetByUserId(int userId)
        {
            return _context.UserAccounts
                .Include(u => u.Employee)
                    .ThenInclude(e => e.Department)
                .Include(u => u.Employee)
                    .ThenInclude(e => e.Position)
                .Where(u => u.UserId == userId)
                .Select(u => u.Employee)
                .FirstOrDefault();
        }

        public bool ExistsByEmail(string email, int excludeUserId)
        {
            return _context.Employees.Any(e =>
                e.Email == email &&
                e.UserId != excludeUserId &&
                e.Status != -1
            );
        }
        public bool ExistsByPhone(string phone, int excludeUserId)
        {
            return _context.Employees.Any(e =>
                e.Phone == phone &&
                e.UserId != excludeUserId &&
                e.Status != -1
            );
        }

        public void Add(Employee employee)
        {
            _context.Employees.Add(employee);
        }

        public void Update(Employee employee)
        {
            _context.Employees.Update(employee);
        }

        public bool SetStatus(int employeeId, int status)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.EmployeeId == employeeId && e.Status != -1);
            if (employee == null)
            {
                return false;
            }

            employee.Status = status;
            _context.Employees.Update(employee);
            _context.SaveChanges();
            return true;
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
