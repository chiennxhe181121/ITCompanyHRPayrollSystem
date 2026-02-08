using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.DTOs.Common;
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

        public UserAccountService(IUserAccountRepository repo)
        {
            _repo = repo;
        }

        public List<UserAccountDTO> GetAllAccounts()
        {
            return _repo.GetAll()
                .Select(x => new UserAccountDTO
                {
                    UserId = x.UserId,
                    Username = x.Username,
                    FullName = x.Employee != null ? x.Employee.FullName : "(Chưa gán nhân viên)",
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
                    EmployeeCode = "EMP" + DateTime.Now.ToString("yyMMddHHmmss"),
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


        public void Update(UserAccountUpdateDTO dto)
        {
            var user = _repo.GetById(dto.UserId)
                ?? throw new Exception("User not found");

            var role = _repo.GetRoleByCode(dto.RoleCode)
                ?? throw new Exception("Role not found");

            user.RoleId = role.RoleId;
            user.Status = dto.Status;

            _repo.Update(user);
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

        public void SetInactive(int id)
        {
            var user = _repo.GetById(id)
                ?? throw new Exception("User not found");

            user.Status = Constants.Inactive;

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


    }
}