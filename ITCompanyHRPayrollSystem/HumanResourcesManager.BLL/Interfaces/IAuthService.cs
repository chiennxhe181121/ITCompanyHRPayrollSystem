using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.BLL.Interfaces
{
    public interface IAuthService
    {
        void Register(RegisterDTO dto);
        UserSessionDTO? Login(LoginDTO dto);
        void ResetPassword(string email, string newPass, string confirmPass);
        UserSessionDTO LoginWithGoogle(string email, string name);

    }
}
