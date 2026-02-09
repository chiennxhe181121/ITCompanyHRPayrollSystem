using HumanResourcesManager.DAL.Models;


// @HoangDH 
namespace HumanResourcesManager.DAL.Interfaces
{
    public interface IUserAccountRepository
    {

        UserAccount? GetByEmployeeEmail(string email);
        UserAccount AddGoogleAccount(Employee employee);

        // new 
        IEnumerable<UserAccount> GetAll();


        UserAccount? GetById(int id);

        UserAccount Add(UserAccount user);

        void Update(UserAccount user);

        void Save();

        // ✅ NEW
        bool ExistsByUsername(string username);
        bool ExistsByEmail(string email);
        Role? GetRoleByCode(string roleCode);

    }
}
