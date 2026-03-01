using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace HumanResourcesManager.DAL.Repositories
{
    public class ContractRepository : IContractRepository
    {
        private readonly HumanManagerContext _context;

        public ContractRepository(HumanManagerContext context)
        {
            _context = context;
        }

        public List<Contract> GetAll()
        {
            var data = _context.Contracts
                .Include(c => c.Employee)
                .ToList();

            Console.WriteLine("DB COUNT: " + data.Count);

            return data;
        }

        public Contract? GetById(int id)
        {
            return _context.Contracts
                .Include(c => c.Employee)
                .FirstOrDefault(c => c.ContractId == id);
        }

        public void Add(Contract contract)
        {
            _context.Contracts.Add(contract);
        }

        public void Update(Contract contract)
        {
            _context.Contracts.Update(contract);
        }

        public void Delete(int id)
        {
            var contract = _context.Contracts.Find(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}