using HumanResourcesManager.BLL.DTOs.Common;
using HumanResourcesManager.BLL.DTOs.Department;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;
using Microsoft.EntityFrameworkCore;

// HoangDH
namespace HumanResourcesManager.BLL.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly HumanManagerContext _context;

        public DepartmentService(HumanManagerContext context)
        {
            _context = context;
        }

        public List<DepartmentListDTO> GetAll(string? keyword)
        {
            var query = _context.Departments
                .Include(d => d.Employees)
                .Where(d => d.Status != Constants.Deleted);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(d =>
                    d.DepartmentName.Contains(keyword));
            }

            return query.Select(d => new DepartmentListDTO
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName,
                Description = d.Description,
                Status = d.Status,
                EmployeeCount = d.Employees.Count(e => e.Status == Constants.Active)
            }).ToList();
        }

        public DepartmentCreateUpdateDTO? GetById(int id)
        {
            var d = _context.Departments
                .FirstOrDefault(x => x.DepartmentId == id && x.Status != Constants.Deleted);

            if (d == null) return null;

            return new DepartmentCreateUpdateDTO
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName,
                Description = d.Description
            };
        }

        public bool Create(DepartmentCreateUpdateDTO dto)
        {
            if (_context.Departments.Any(d => d.DepartmentName == dto.DepartmentName))
                return false;

            _context.Departments.Add(new Department
            {
                DepartmentName = dto.DepartmentName,
                Description = dto.Description,
                Status = Constants.Active
            });

            _context.SaveChanges();
            return true;
        }


        public bool Update(DepartmentCreateUpdateDTO dto)
        {
            var d = _context.Departments
                .FirstOrDefault(x => x.DepartmentId == dto.DepartmentId);

            if (d == null) return false;

            bool isDuplicate = _context.Departments.Any(x =>
                x.DepartmentName == dto.DepartmentName &&
                x.DepartmentId != dto.DepartmentId);

            if (isDuplicate)
                return false;

            d.DepartmentName = dto.DepartmentName;
            d.Description = dto.Description;

            _context.SaveChanges();
            return true;
        }


        public bool Delete(int id)
        {
            var d = _context.Departments
                .Include(x => x.Employees)
                .FirstOrDefault(x => x.DepartmentId == id);

            if (d == null) return false;

            if (d.Employees.Any(e => e.Status == Constants.Active))
                return false;

            d.Status = Constants.Inactive;
            _context.SaveChanges();
            return true;
        }

        public void SetActive(int id)
        {
            var d = _context.Departments.FirstOrDefault(x => x.DepartmentId == id);
            if (d == null) return;

            d.Status = Constants.Active;
            _context.SaveChanges();
        }

        public PagedResult<DepartmentListDTO> Search(
              string? keyword,
              int? status,
              int page,
              int pageSize)
        {
            if (page < 1) page = 1;

            var query = _context.Departments
                .Include(d => d.Employees)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(d =>
                    d.DepartmentName.Contains(keyword));
            }

            if (status.HasValue)
            {
                query = query.Where(d => d.Status == status.Value);
            }

            int totalItems = query.Count();

            var items = query
                 //.OrderByDescending(d => d.DepartmentId)
                 .OrderBy(d => d.DepartmentId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DepartmentListDTO
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    Description = d.Description,
                    Status = d.Status,
                    EmployeeCount = d.Employees.Count(e => e.Status == 1)
                })
                .ToList();

            return new PagedResult<DepartmentListDTO>
            {
                Items = items,
                TotalItems = totalItems
            };
        }

        public int Count()
        {
            return _context.Departments.Count();
        }
    }
}
