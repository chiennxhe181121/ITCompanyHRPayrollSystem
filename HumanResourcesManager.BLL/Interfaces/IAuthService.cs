using HumanResourcesManager.BLL.DTOs;

namespace HumanResourcesManager.BLL.Interfaces
{
    public interface IAuthService
    {
        void Register(RegisterDTO dto);
        UserSessionDTO? Login(LoginDTO dto);

        //string GenerateOtp();
        //void ResetPasswordByEmail(string email, string newPassword, string confirmPassword);
        void ResetPassword(string email, string newPass, string confirmPass);
    }
}
