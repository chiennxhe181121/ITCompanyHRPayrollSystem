using HumanResourcesManager.BLL.DTOs;

using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Repositories;

namespace HumanResourcesManager.BLL.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _repo;

        public ContractService(IContractRepository repo)
        {
            _repo = repo;
        }

        public List<ContractDTO> GetAll()
        {
            return _repo.GetAll()
                .Select(c => new ContractDTO
                {
                    ContractId = c.ContractId,
                    EmployeeId = c.EmployeeId,
                    EmployeeName = c.Employee.FullName,
                    ContractType = c.ContractType,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    BasicSalary = c.BasicSalary,
                    IsActive = c.IsActive
                }).ToList();
        }

        public ContractDTO? GetById(int id)
        {
            var c = _repo.GetById(id);
            if (c == null) return null;

            return new ContractDTO
            {
                ContractId = c.ContractId,
                EmployeeId = c.EmployeeId,
                EmployeeName = c.Employee.FullName,
                ContractType = c.ContractType,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                BasicSalary = c.BasicSalary,
                IsActive = c.IsActive
            };
        }

        public void Create(ContractDTO dto)
        {
            var contract = new Contract
            {
                EmployeeId = dto.EmployeeId,
                ContractType = dto.ContractType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                BasicSalary = dto.BasicSalary,
                IsActive = true
            };

            _repo.Add(contract);
            _repo.Save();
        }

        public void Update(ContractDTO dto)
        {
            var contract = _repo.GetById(dto.ContractId);

            if (contract == null) return;

           
            contract.ContractType = dto.ContractType;
            contract.StartDate = dto.StartDate;
            contract.EndDate = dto.EndDate;
            contract.BasicSalary = dto.BasicSalary;
            contract.IsActive = dto.IsActive;

            _repo.Save();
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
            _repo.Save();
        }

        public void SoftDelete(int id)
        {
            var contract = _repo.GetById(id);

            if (contract == null) return;

            contract.IsActive = false;
            _repo.Save();
        }
    }
}