using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.DTOs.Common;
using HumanResourcesManager.BLL.DTOs.UserAccount;
using HumanResourcesManager.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// @HoangDH 
namespace HumanResourcesManager.BLL.Interfaces
{
    public interface IUserAccountService
    {
        // ===== BASIC =====
        List<UserAccountDTO> GetAllAccounts();
        UserAccountDTO GetById(int id);
        // ===== CRUD =====
        void Create(UserAccountCreateDTO dto);
        void Update(UserAccountUpdateDTO dto);
        void ResetPassword(UserAccountResetPasswordDTO dto);
        void SetInactive(int id);
        void SetActive(int id);

        // ===== SEARCH + FILTER + PAGING =====
        PagedResult<UserAccountDTO> SearchAccounts(
            string? keyword,
            string? roleCode,
            string? status,
            int page,
            int pageSize
        );

    }

}
