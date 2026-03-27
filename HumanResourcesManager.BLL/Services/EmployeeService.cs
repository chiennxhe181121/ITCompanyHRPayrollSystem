using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.DTOs.Employee;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Repositories;
using Microsoft.AspNetCore.Http;
using Volo.Abp;

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

        public IEnumerable<EmployeeDTO> GetTeamMembers(int managerUserId)
        {
            var manager = _repo.GetByUserId(managerUserId);
            if (manager == null)
            {
                return Enumerable.Empty<EmployeeDTO>();
            }

            return _repo.GetByDepartment(manager.DepartmentId, manager.EmployeeId)
                .Select(e => new EmployeeDTO
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
                    DepartmentName = e.Department?.DepartmentName,
                    PositionId = e.PositionId,
                    PositionName = e.Position?.PositionName
                });
        }

        public int CountManagedEmployees(int managerUserId)
        {
            var manager = _repo.GetByUserId(managerUserId);
            if (manager == null)
            {
                return 0;
            }

            return _repo.CountByDepartment(manager.DepartmentId, manager.EmployeeId);
        }

        public int CountPendingOvertimeRequests(int managerUserId)
        {
            var manager = _repo.GetByUserId(managerUserId);
            if (manager == null)
            {
                return 0;
            }

            return _repo.CountPendingOvertimeRequestsForManager(manager.EmployeeId);
        }

        public int CountOvertimeSchedules(int managerUserId)
        {
            var manager = _repo.GetByUserId(managerUserId);
            if (manager == null)
            {
                return 0;
            }

            return _repo.CountOvertimeSchedulesForManager(manager.EmployeeId);
        }

        public int CountCompletedOTSchedules(int managerUserId)
        {
            var manager = _repo.GetByUserId(managerUserId);
            if (manager == null)
            {
                return 0;
            }

            return _repo.CountCompletedOTSchedulesForManager(manager.EmployeeId);
        }

        public IEnumerable<EmployeeDTO> GetEmployeesWithoutDepartment()
        {
            return _repo.GetEmployeesWithoutDepartment()
                .Select(e => new EmployeeDTO
                {
                    EmployeeId = e.EmployeeId,
                    EmployeeCode = e.EmployeeCode,
                    FullName = e.FullName,
                    Email = e.Email,
                    Phone = e.Phone,
                    PositionId = e.PositionId,
                    PositionName = e.Position?.PositionName,
                    Status = e.Status
                });
        }

        public bool AddEmployeeToManagerTeam(int managerUserId, int employeeId, out string message)
        {
            message = string.Empty;

            var manager = _repo.GetByUserId(managerUserId);
            if (manager == null)
            {
                message = "Không tìm thấy thông tin quản lý.";
                return false;
            }

            var success = _repo.AssignEmployeeToDepartment(employeeId, manager.DepartmentId);
            if (!success)
            {
                message = "Chỉ có thể thêm nhân viên chưa thuộc phòng ban nào.";
                return false;
            }

            message = "Thêm nhân viên vào phòng ban thành công.";
            return true;
        }

        public bool ChangeTeamMemberStatus(int managerUserId, int employeeId, int status, out string message)
        {
            message = string.Empty;

            if (status != 0 && status != 1)
            {
                message = "Trạng thái không hợp lệ.";
                return false;
            }

            var manager = _repo.GetByUserId(managerUserId);
            if (manager == null)
            {
                message = "Không tìm thấy thông tin quản lý.";
                return false;
            }

            var teamMember = _repo.GetByDepartment(manager.DepartmentId, manager.EmployeeId)
                .FirstOrDefault(e => e.EmployeeId == employeeId);

            if (teamMember == null)
            {
                message = "Nhân viên không thuộc nhóm quản lý của bạn.";
                return false;
            }

            var ok = _repo.SetStatus(employeeId, status);
            if (!ok)
            {
                message = "Không thể cập nhật trạng thái nhân viên.";
                return false;
            }

            message = status == 1
                ? "Đã kích hoạt nhân viên thành công."
                : "Đã chuyển nhân viên sang trạng thái ngừng hoạt động.";

            return true;
        }


        public EmployeeOwnerProfileDTO? GetOwnProfile(int userId)
        {
            var e = _repo.GetByUserId(userId);

            return new EmployeeOwnerProfileDTO
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

        private void ValidateUpdateOwnProfile(
            int userId,
            Employee employee,
            EmployeeOwnerProfileDTO dto,
            IFormFile? avatarFile
        )
        {
            // ===== EMAIL UNIQUE =====
            if (_repo.ExistsByEmail(dto.Email, userId))
                throw new BusinessException("EmailAlreadyExists")
                {
                    Details = "Email (" + dto.Email + ") đã tồn tại, vui lòng sử dụng email khác"
                };

            // ===== PHONE UNIQUE =====
            if (_repo.ExistsByPhone(dto.Phone, userId))
                throw new BusinessException("PhoneAlreadyExists")
                {
                    Details = "Số điện thoại (" + dto.Phone + ") đã tồn tại, vui lòng sử dụng số khác"
                };

            // ===== DATE OF BIRTH =====
            var today = DateTime.Today;

            // 1. Ngày sinh ở tương lai
            if (dto.DateOfBirth > today)
                throw new BusinessException("DateOfBirthInFuture")
                {
                    Details = "Ngày sinh không được lớn hơn ngày hiện tại"
                };

            // 2. Tuổi >= 18
            var age = today.Year - dto.DateOfBirth.Year;
            if (dto.DateOfBirth.Date > today.AddYears(-age))
                age--;

            if (age < 18)
                throw new BusinessException("EmployeeTooYoung")
                {
                    Details = "Nhân viên phải từ 18 tuổi trở lên"
                };

            // 3. Quá già (data không hợp lệ)
            if (age > 100)
                throw new BusinessException("DateOfBirthInvalid")
                {
                    Details = "Ngày sinh không hợp lệ"
                };
        }

        public async Task<Employee?> UpdateOwnProfile(
            int userId,
            EmployeeOwnerProfileDTO dto,
            IFormFile? avatarFile
        )
        {
            var employee = _repo.GetByUserId(userId);
            if (employee == null)
                return null;

            // VALIDATE HERE
            ValidateUpdateOwnProfile(userId, employee, dto, avatarFile);

            // ===== AVATAR BUSINESS LOGIC =====
            if (dto.RemoveAvatar)
            {
                employee.ImgAvatar = null;
            }
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

            // ===== UPDATE PROFILE =====
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
