using HumanResourcesManager.BLL.DTOs;


namespace HumanResourcesManager.BLL.Services
{
    public interface IContractService
    {
        List<ContractDTO> GetAll();
        ContractDTO? GetById(int id);
        void Create(ContractDTO dto);
        void Update(ContractDTO dto);
        void Delete(int id);

        void SoftDelete(int id);
    }
}