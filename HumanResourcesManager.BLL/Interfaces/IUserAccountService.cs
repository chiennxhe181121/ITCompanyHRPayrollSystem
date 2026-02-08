using HumanResourcesManager.BLL.DTOs;
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
        List<UserAccountDTO> GetAllAccounts();
        // 8/2 

        UserAccountDTO GetById(int id);

        void Create(UserAccountCreateDTO dto);
        void Update(UserAccountUpdateDTO dto);
        void ResetPassword(UserAccountResetPasswordDTO dto);
        void SetInactive(int id);




    }

}
