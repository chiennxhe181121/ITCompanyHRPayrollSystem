using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.BLL.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;

        public EmployeeService(IEmployeeRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<EmployeeDTO> GetAll()
        {
            return _repo.GetAll().Select(e => new EmployeeDTO
            {
                EmployeeId = e.EmployeeId,
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName,
                Gender = e.Gender,
                DateOfBirth = e.DateOfBirth,
                Email = e.Email,
                Phone = e.Phone,
                Address = e.Address,
                ImgAvatar = e.ImgAvatar,
                HireDate = e.HireDate,
                Status = e.Status,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.Department.DepartmentName,
                PositionId = e.PositionId,
                PositionName = e.Position.PositionName
            });
        }

        public EmployeeDTO? GetById(int id)
        {
            var e = _repo.GetById(id);
            if (e == null) return null;

            return new EmployeeDTO
            {
                EmployeeId = e.EmployeeId,
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName,
                Gender = e.Gender,
                DateOfBirth = e.DateOfBirth,
                Email = e.Email,
                Phone = e.Phone,
                Address = e.Address,
                ImgAvatar = e.ImgAvatar,
                HireDate = e.HireDate,
                Status = e.Status,
                DepartmentId = e.DepartmentId,
                PositionId = e.PositionId
            };
        }

        public void Create(EmployeeDTO dto)
        {
            var emp = new Employee
            {
                EmployeeCode = dto.EmployeeCode,
                FullName = dto.FullName,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                ImgAvatar = dto.ImgAvatar,
                HireDate = dto.HireDate,
                Status = 1,
                DepartmentId = dto.DepartmentId,
                PositionId = dto.PositionId
            };

            _repo.Add(emp);
            _repo.Save();
        }

        public void Update(EmployeeDTO dto)
        {
            var emp = _repo.GetById(dto.EmployeeId);
            if (emp == null) return;

            emp.FullName = dto.FullName;
            emp.Gender = dto.Gender;
            emp.Email = dto.Email;
            emp.Phone = dto.Phone;
            emp.Address = dto.Address;
            emp.ImgAvatar = dto.ImgAvatar;
            emp.DepartmentId = dto.DepartmentId;
            emp.PositionId = dto.PositionId;
            emp.Status = dto.Status;

            _repo.Update(emp);
            _repo.Save();
        }

        public void Delete(int id)
        {
            _repo.SoftDelete(id);
            _repo.Save();
        }
    }
}
