using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.DAL.Repositories
{
    public interface IContractRepository
    {
        List<Contract> GetAll();
        Contract? GetById(int id);
        void Add(Contract contract);
        void Update(Contract contract);
        void Delete(int id);
        void Save();
        Task<Contract?> GetActiveContractByEmployeeAsync(int employeeId);
    }
}