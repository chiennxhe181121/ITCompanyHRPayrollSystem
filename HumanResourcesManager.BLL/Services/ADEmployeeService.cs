using HumanResourcesManager.BLL.DTOs.ADEmployee;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;
using BCrypt.Net;

// HoangDH 
namespace HumanResourcesManager.BLL.Services
{
    public class ADEmployeeService : IADEmployeeService
    {
        private readonly IADEmployeeRepository _empRepo;
        private readonly IUserAccountRepository _userRepo; 

        public ADEmployeeService(IADEmployeeRepository empRepo, IUserAccountRepository userRepo)
        {
            _empRepo = empRepo;
            _userRepo = userRepo;
        }

        public List<Employee> GetAllEmployees()
        {
            return _empRepo.GetAll().ToList();
        }

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


        public List<DepartmentSelectDTO> GetDepartments()
        {
            return _empRepo.GetDepartments()
                .Select(d => new DepartmentSelectDTO { DepartmentId = d.DepartmentId, DepartmentName = d.DepartmentName })
                .ToList();
        }

        public List<PositionSelectDTO> GetPositions()
        {
            return _empRepo.GetPositions()
                .Select(p => new PositionSelectDTO
                {
                    PositionId = p.PositionId,
                    PositionName = p.PositionName,
                    BaseSalary = p.BaseSalary
                })
                .ToList();
        }



        public ADEmployeeUpdateDTO? GetById(int id)
        {
            var emp = _empRepo.GetById(id);
            if (emp == null) return null;

            return new ADEmployeeUpdateDTO
            {
                EmployeeId = emp.EmployeeId,
                FullName = emp.FullName,
                Gender = emp.Gender,
                DateOfBirth = emp.DateOfBirth,
                Email = emp.Email,
                Phone = emp.Phone,
                Address = emp.Address,
                ImgAvatar = emp.ImgAvatar,
                HireDate = emp.HireDate,
                DepartmentId = emp.DepartmentId,
                PositionId = emp.PositionId,

                HasAccount = emp.UserAccount != null,
                IsCreateAccount = false 
            };
        }

        public bool UpdateEmployee(ADEmployeeUpdateDTO dto, out string message)
        {
            message = "";

            if (_empRepo.ExistsEmail(dto.Email, dto.EmployeeId))
            {
                message = "DUPLICATE_EMAIL";
                return false;
            }
            if (_empRepo.ExistsPhone(dto.Phone, dto.EmployeeId))
            {
                message = "DUPLICATE_PHONE";
                return false;
            }

            var emp = _empRepo.GetById(dto.EmployeeId);
            if (emp == null) { message = "Nhân viên không tồn tại"; return false; }

            emp.FullName = dto.FullName;
            emp.Gender = dto.Gender;
            emp.DateOfBirth = dto.DateOfBirth;
            emp.Email = dto.Email;
            emp.Phone = dto.Phone;
            emp.Address = dto.Address;
            emp.HireDate = dto.HireDate;
            emp.DepartmentId = dto.DepartmentId;
            emp.PositionId = dto.PositionId;

            if (!string.IsNullOrEmpty(dto.ImgAvatar))
            {
                emp.ImgAvatar = dto.ImgAvatar;
            }

            if (emp.UserAccount == null && dto.IsCreateAccount)
            {
                string finalPassword = !string.IsNullOrEmpty(dto.Password) ? dto.Password : "12345678";

                var newAccount = new UserAccount
                {
                    Username = dto.Email, 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(finalPassword),
                    RoleId = 2, 
                    Status = Constants.Active,
                    Employee = emp
                };
                emp.UserAccount = newAccount;
            }

            try
            {
                _empRepo.Update(emp);
                _empRepo.Save();
                return true;
            }
            catch (Exception ex)
            {
                message = "Lỗi hệ thống: " + ex.Message;
                return false;
            }
        }


        public bool CreateEmployee(ADEmployeeCreateDTO dto, out string message)
        {
            message = "";
            if (_empRepo.ExistsEmail(dto.Email)) { message = "DUPLICATE_EMAIL"; return false; }
            if (_empRepo.ExistsPhone(dto.Phone)) { message = "DUPLICATE_PHONE"; return false; }

            var emp = new Employee
            {
                EmployeeCode = GenerateEmployeeCode(),
                FullName = dto.FullName,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                ImgAvatar = dto.ImgAvatar,
                HireDate = dto.HireDate,
                DepartmentId = dto.DepartmentId,
                PositionId = dto.PositionId,
                Status = Constants.Active
            };

            if (dto.IsCreateAccount)
            {
                int roleId = _empRepo.GetRoleIdByCode("EMP");

                if (roleId == 0)
                {
                    message = "SYSTEM_ERROR: Role 'EMP' chưa được định nghĩa trong Database Role.";
                    return false;
                }

                string finalUsername = !string.IsNullOrEmpty(dto.Username) ? dto.Username : dto.Email;
                string finalPassword = !string.IsNullOrEmpty(dto.Password) ? dto.Password : "12345678";

                emp.UserAccount = new UserAccount
                {
                    Username = finalUsername,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(finalPassword),
                    RoleId = roleId, 
                    Status = Constants.Active
                };
            }

            try
            {
                _empRepo.Add(emp);
                _empRepo.Save();
                return true;
            }
            catch (Exception ex)
            {
                message = "SYSTEM_ERROR: " + ex.Message;
                return false;
            }
        }


        //  Logic chặn Inactive bản thân
        public void SetStatus(int id, int status, int? currentUserId)
        {
            var emp = _empRepo.GetById(id);
            if (emp == null) throw new Exception("Nhân viên không tồn tại.");

            if (status == 0)
            {
                if (emp.UserId != null && emp.UserId == currentUserId)
                {
                    throw new InvalidOperationException("Bạn không thể tự vô hiệu hóa hồ sơ của chính mình!");
                }
            }

            emp.Status = status;

            if (status == 0 && emp.UserAccount != null)
            {
                emp.UserAccount.Status = 0; 
            }
            else if (status == 1 && emp.UserAccount != null)
            {
                emp.UserAccount.Status = 1; 
            }

            _empRepo.Update(emp);
            _empRepo.Save();
        }


    }
}