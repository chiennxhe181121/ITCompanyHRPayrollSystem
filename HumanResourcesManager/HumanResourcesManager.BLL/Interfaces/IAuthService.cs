using HumanResourcesManager.BLL.DTOs;

namespace HumanResourcesManager.BLL.Interfaces
{
    public interface IAuthService
    {
        void Register(RegisterDTO dto);
        UserSessionDTO? Login(LoginDTO dto);
    }
}
