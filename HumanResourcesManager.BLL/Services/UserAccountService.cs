using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.DTOs.Common;
using HumanResourcesManager.BLL.DTOs.Employee;
using HumanResourcesManager.BLL.DTOs.UserAccount;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// HoangDH
namespace HumanResourcesManager.BLL.Services
{
    public class UserAccountService : IUserAccountService
    {
        private readonly IUserAccountRepository _repo;
        private readonly IADEmployeeRepository _empRepo;

        public UserAccountService(IUserAccountRepository repo, IADEmployeeRepository empRepo)
        {
            _repo = repo;
            _empRepo = empRepo;
        }

        // Tạo Mã Nhân Viên: VD: EMP260001
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

        public List<UserAccountDTO> GetAllAccounts()
        {
            return _repo.GetAll()
                .Select(x => new UserAccountDTO
                {
                    UserId = x.UserId,
                    Username = x.Username,
                    FullName = x.Employee != null ? x.Employee.FullName : "(Chưa gán nhân viên)",
                    // 👇 THÊM DÒNG NÀY ĐỂ LẤY ID NHÂN VIÊN
                    //EmployeeId = x.Employee != null ? x.Employee.EmployeeId : null,
                    //// 👇 THÊM DÒNG NÀY (Lấy ảnh từ bảng Employee)
                    //Avatar = x.Employee != null ? x.Employee.ImgAvatar : null,
                    Email = x.Employee?.Email,
                    RoleName = x.Role.RoleName,
                    Status = x.Status,
                    HasEmployee = x.Employee != null
                })
                .ToList();
        }

        public UserAccountDTO GetById(int id)
        {
            var user = _repo.GetById(id) ?? throw new Exception("User not found");

            return new UserAccountDTO
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.Employee?.FullName ?? "",
                Email = user.Employee?.Email,
                RoleCode = user.Role.RoleCode,
                RoleName = user.Role.RoleName,
                Status = user.Status,
                HasEmployee = user.Employee != null
            };
        }

        public void Create(UserAccountCreateDTO dto)
        {
            if (_repo.ExistsByUsername(dto.Username))
                throw new Exception("Username đã tồn tại");

            if (!string.IsNullOrEmpty(dto.Email) &&
                _repo.ExistsByEmail(dto.Email))
                throw new Exception("Email đã tồn tại");

            var user = new UserAccount
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = dto.RoleId,
                Status = Constants.Active
            };

            // tạo employee nếu nhập đủ info
            if (!string.IsNullOrEmpty(dto.FullName) &&
                !string.IsNullOrEmpty(dto.Email))
            {
                user.Employee = new Employee
                {
                    //EmployeeCode = "EMP" + DateTime.Now.ToString("yyMMddHHmmss"),
                    EmployeeCode = GenerateEmployeeCode(),
                    FullName = dto.FullName,
                    Email = dto.Email,
                    Phone = "0000000000",
                    Status = Constants.Active,
                    HireDate = DateTime.Now,
                    DepartmentId = 1,
                    PositionId = 1
                };
            }

            _repo.Add(user);
            _repo.Save();
        }

        public void Update(UserAccountUpdateDTO dto, int currentUserId)
        {
            var targetUser = _repo.GetById(dto.UserId)
                ?? throw new Exception("Tài khoản không tồn tại");

            var currentUser = _repo.GetById(currentUserId)
                ?? throw new Exception("Không xác định được người thực hiện");

            var newRole = _repo.GetRoleByCode(dto.RoleCode)
                ?? throw new Exception("Role không hợp lệ");

            if (targetUser.UserId != currentUser.UserId && targetUser.RoleId == currentUser.RoleId)
            {
                throw new Exception($"Bạn không thể chỉnh sửa đồng nghiệp cùng cấp ({targetUser.Role.RoleName})!");
            }

            if (targetUser.UserId == currentUser.UserId)
            {
                if (targetUser.RoleId != newRole.RoleId)
                {
                    throw new Exception("Bạn không thể tự thay đổi quyền hạn (Role) của chính mình. Hãy nhờ Admin khác.");
                }

            }

            targetUser.RoleId = newRole.RoleId;

            // targetUser.Status = dto.Status; 

            _repo.Update(targetUser);
            _repo.Save();
        }

        public void ResetPassword(UserAccountResetPasswordDTO dto)
        {
            var user = _repo.GetById(dto.UserId)
                ?? throw new Exception("User not found");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            _repo.Update(user);
            _repo.Save();
        }

        public void SetActive(int id)
        {
            var user = _repo.GetById(id)
                ?? throw new Exception("User not found");

            user.Status = Constants.Active;

            _repo.Update(user);
            _repo.Save();
        }
        public void SetInactive(int targetUserId, int currentUserId)
        {
            if (targetUserId == currentUserId)
                throw new Exception("Bạn không thể tự khóa chính mình");

            var targetUser = _repo.GetById(targetUserId)
                ?? throw new Exception("User not found");

            var currentUser = _repo.GetById(currentUserId)
                ?? throw new Exception("Current user not found");

            // ❌ Không cho khóa người cùng role
            //if (targetUser.Role.RoleCode == currentUser.Role.RoleCode)
            //{
            //    throw new Exception("Bạn không thể khóa đồng nghiệp cùng vai trò");
            //}
            if (targetUser.RoleId == currentUser.RoleId)
            {
                throw new Exception("Bạn không thể khóa đồng nghiệp cùng vai trò");
            }


            targetUser.Status = Constants.Inactive;

            _repo.Update(targetUser);
            _repo.Save();
        }

        public PagedResult<UserAccountDTO> SearchAccounts(
            string? keyword,
            string? roleCode,
            string? status,
            int page,
            int pageSize)
        {
            var query = _repo.GetAll().AsQueryable();

            // SEARCH keyword
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(x =>
                    x.Username.ToLower().Contains(keyword) ||
                    (x.Employee != null && (
                        x.Employee.FullName.ToLower().Contains(keyword) ||
                        x.Employee.Email.ToLower().Contains(keyword)
                    ))
                );
            }

            // FILTER ROLE
            if (!string.IsNullOrWhiteSpace(roleCode))
            {
                query = query.Where(x => x.Role.RoleCode == roleCode);
            }

            // FILTER STATUS
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "Active")
                    query = query.Where(x => x.Status == Constants.Active);
                else if (status == "Inactive")
                    query = query.Where(x => x.Status == Constants.Inactive);
            }

            int totalItems = query.Count();

            var items = query
                //.OrderByDescending(x => x.UserId)
                .OrderBy(x => x.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new UserAccountDTO
                {
                    UserId = x.UserId,
                    Username = x.Username,
                    FullName = x.Employee != null ? x.Employee.FullName : "",
                    Email = x.Employee != null ? x.Employee.Email : "",
                    RoleName = x.Role.RoleName,
                    Status = x.Status
                })
                .ToList();

            return new PagedResult<UserAccountDTO>
            {
                Items = items,
                TotalItems = totalItems
            };
        }

        public ServiceResult ChangePassword(int userId, ChangePasswordDTO dto)
        {
            var user = _repo.GetById(userId);
            if (user == null)
                return ServiceResult.Failure("Không tìm thấy người dùng.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return ServiceResult.Failure("Mật khẩu hiện tại không đúng.");

            if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
                return ServiceResult.Failure("Mật khẩu mới không được trùng với mật khẩu hiện tại.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            _repo.Save();

            return ServiceResult.Success("Đổi mật khẩu thành công.");
        }
    }
}