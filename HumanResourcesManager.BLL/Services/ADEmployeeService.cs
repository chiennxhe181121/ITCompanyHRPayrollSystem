using BCrypt.Net;
using HumanResourcesManager.BLL.DTOs.ADEmployee;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http; 


// HoangDH 
namespace HumanResourcesManager.BLL.Services
{
    public class ADEmployeeService : IADEmployeeService
    {
        private readonly IADEmployeeRepository _empRepo;
        private readonly IUserAccountRepository _userRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ADEmployeeService(IADEmployeeRepository empRepo, IUserAccountRepository userRepo, IWebHostEnvironment webHostEnvironment)
        {
            _empRepo = empRepo;
            _userRepo = userRepo;
            _webHostEnvironment = webHostEnvironment;
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


        private string SaveAvatarImage(IFormFile file, int employeeId)
        {
            string webRootPath = _webHostEnvironment.WebRootPath;
            string folderPath = Path.Combine(webRootPath, "img", "employee", employeeId.ToString(), "avatar");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }


            string fileExtension = Path.GetExtension(file.FileName);
            string fileName = $"avatar{fileExtension}";
            string fullPath = Path.Combine(folderPath, fileName);


            var files = Directory.GetFiles(folderPath);
            foreach (var f in files) File.Delete(f);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return $"/img/employee/{employeeId}/avatar/{fileName}";
        }


        public bool CreateEmployee(ADEmployeeCreateDTO dto, out string message)
        {
            message = "";
            if (_empRepo.ExistsEmail(dto.Email)) { message = "DUPLICATE_EMAIL"; return false; }
            if (_empRepo.ExistsPhone(dto.Phone)) { message = "DUPLICATE_PHONE"; return false; }

            //  1: Tạo đối tượng (Ảnh tạm thời null)
            var emp = new Employee
            {
                EmployeeCode = GenerateEmployeeCode(),
                FullName = dto.FullName,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                HireDate = dto.HireDate,
                DepartmentId = dto.DepartmentId,
                PositionId = dto.PositionId,
                Status = Constants.Active,
                ImgAvatar = null
            };

            // Logic UserAccount
            if (dto.IsCreateAccount)
            {
                int roleId = _empRepo.GetRoleIdByCode("EMP");
                if (roleId == 0) { message = "SYSTEM_ERROR: Role 'EMP' not found"; return false; }

                string pass = !string.IsNullOrEmpty(dto.Password) ? dto.Password : "12345678";
                emp.UserAccount = new UserAccount
                {
                    Username = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(pass),
                    RoleId = roleId,
                    Status = Constants.Active
                };
            }

            try
            {
                // 2: Lưu DB lần 1 để lấy ID
                _empRepo.Add(emp);
                _empRepo.Save(); // --> Sinh ra emp.EmployeeId (Ví dụ: 10)

                // 3: Nếu có ảnh -> Lưu ảnh theo ID -> Update DB lần 2
                if (dto.AvatarFile != null)
                {
                    string avatarPath = SaveAvatarImage(dto.AvatarFile, emp.EmployeeId);

                    emp.ImgAvatar = avatarPath;
                    _empRepo.Update(emp);
                    _empRepo.Save(); // Chốt đơn đường dẫn ảnh
                }

                return true;
            }
            catch (Exception ex)
            {
                message = "SYSTEM_ERROR: " + ex.Message;
                return false;
            }
        }

        // --- 3. UPDATE (Sửa + Ảnh) ---
        public bool UpdateEmployee(ADEmployeeUpdateDTO dto, out string message)
        {
            message = "";
            if (_empRepo.ExistsEmail(dto.Email, dto.EmployeeId)) { message = "DUPLICATE_EMAIL"; return false; }
            if (_empRepo.ExistsPhone(dto.Phone, dto.EmployeeId)) { message = "DUPLICATE_PHONE"; return false; }

            var emp = _empRepo.GetById(dto.EmployeeId);
            if (emp == null) { message = "Not found"; return false; }

            // Update thông tin
            emp.FullName = dto.FullName;
            emp.Gender = dto.Gender;
            emp.DateOfBirth = dto.DateOfBirth;
            emp.Email = dto.Email;
            emp.Phone = dto.Phone;
            emp.Address = dto.Address;
            emp.HireDate = dto.HireDate;
            emp.DepartmentId = dto.DepartmentId;
            emp.PositionId = dto.PositionId;

            try
            {
                // Nếu có chọn ảnh mới -> Ghi đè ảnh cũ -> Update đường dẫn
                if (dto.AvatarFile != null)
                {
                    string newPath = SaveAvatarImage(dto.AvatarFile, emp.EmployeeId);
                    emp.ImgAvatar = newPath;
                }
                // Nếu dto.AvatarFile == null thì giữ nguyên emp.ImgAvatar cũ trong DB

                // Logic tạo thêm Account lúc sửa (nếu chưa có)
                if (emp.UserAccount == null && dto.IsCreateAccount)
                {
                    int roleId = _empRepo.GetRoleIdByCode("EMP");
                    string pass = !string.IsNullOrEmpty(dto.Password) ? dto.Password : "12345678";
                    emp.UserAccount = new UserAccount
                    {
                        Username = dto.Email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(pass),
                        RoleId = roleId,
                        Status = Constants.Active,
                        Employee = emp
                    };
                }

                _empRepo.Update(emp);
                _empRepo.Save();
                return true;
            }
            catch (Exception ex)
            {
                message = "SYSTEM_ERROR: " + ex.Message;
                return false;
            }
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