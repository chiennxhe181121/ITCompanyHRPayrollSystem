using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Repositories;
using Microsoft.AspNetCore.Http;

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

        public EmployeeResponseDTO? GetOwnProfile(int userId)
        {
            var e = _repo.GetByUserId(userId);

            return new EmployeeResponseDTO
            {
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName,
                Gender = e.Gender,
                DateOfBirth = e.DateOfBirth,
                Email = e.Email,
                Phone = e.Phone,
                Address = e.Address,
                ImgAvatar = e.ImgAvatar,
                HireDate = e.HireDate,
                DepartmentName = e.Department.DepartmentName,
                PositionName = e.Position.PositionName
            };
        }

        public async Task<Employee?> UpdateOwnProfile(
               int userId,
               EmployeeRequestDTO dto,
               IFormFile? avatarFile
           )
        {
            var employee = _repo.GetByUserId(userId);
            if (employee == null)
                return null;

            // ===== AVATAR BUSINESS LOGIC =====

            // CASE 1: user bấm Remove
            if (dto.RemoveAvatar)
            {
                employee.ImgAvatar = null;
            }
            // CASE 2: upload avatar mới
            else if (avatarFile != null && avatarFile.Length > 0)
            {
                var folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot", "img", "employees", userId.ToString()
                );

                Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, "avatar.jpg");
                using var stream = new FileStream(filePath, FileMode.Create);
                await avatarFile.CopyToAsync(stream);

                employee.ImgAvatar = $"/img/employees/{userId}/avatar.jpg";
            }
            // CASE 3: không đụng avatar → giữ nguyên
            // (không cần code)

            // ===== UPDATE PROFILE FIELDS =====

            employee.FullName = dto.FullName;
            employee.Email = dto.Email;
            employee.Gender = dto.Gender;
            employee.DateOfBirth = dto.DateOfBirth;
            employee.Phone = dto.Phone;
            employee.Address = dto.Address;

            _repo.Update(employee);
            _repo.Save();

            return employee;
        }

    }
}
