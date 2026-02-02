using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.DAL.Interfaces
{
    public interface IUserAccountRepository
    {
        UserAccount? GetByEmployeeEmail(string email);
        UserAccount AddGoogleAccount(Employee employee);
    }
}
