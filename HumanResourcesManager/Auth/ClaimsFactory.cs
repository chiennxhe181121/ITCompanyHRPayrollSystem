using System.Security.Claims;
using HumanResourcesManager.BLL.DTOs;

namespace HumanResourcesManager.Auth
{
    public static class ClaimsFactory
    {
        public static List<Claim> Create(UserSessionDTO session)
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, session.UserId.ToString()),
                new Claim(ClaimTypes.Name, session.Username),
                new Claim(ClaimTypes.Role, session.RoleCode),
                new Claim("FullName", session.FullName)
            };
        }
    }
}
