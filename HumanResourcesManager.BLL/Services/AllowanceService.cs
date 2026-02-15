using HumanResourcesManager.BLL.DTOs.Allowance;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;


// HoangDH
namespace HumanResourcesManager.BLL.Services
{
    public class AllowanceService : IAllowanceService
    {
        private readonly IAllowanceRepository _repo;

        public AllowanceService(IAllowanceRepository repo)
        {
            _repo = repo;
        }

        public List<AllowanceDTO> GetAll()
        {
            return _repo.GetAll().Select(a => new AllowanceDTO
            {
                AllowanceId = a.AllowanceId,
                AllowanceName = a.AllowanceName,
                Amount = a.Amount,
                Status = a.Status
            }).ToList();
        }

        public AllowanceDTO? GetById(int id)
        {
            var a = _repo.GetById(id);
            if (a == null) return null;
            return new AllowanceDTO
            {
                AllowanceId = a.AllowanceId,
                AllowanceName = a.AllowanceName,
                Amount = a.Amount,
                Status = a.Status
            };
        }

        public bool Create(AllowanceDTO dto)
        {
            try
            {
                var entity = new Allowance
                {
                    AllowanceName = dto.AllowanceName,
                    Amount = dto.Amount,
                    Status = Constants.Active 
                };
                _repo.Add(entity);
                _repo.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Update(AllowanceDTO dto)
        {
            var entity = _repo.GetById(dto.AllowanceId);
            if (entity == null) return false;

            entity.AllowanceName = dto.AllowanceName;
            entity.Amount = dto.Amount;
            entity.Status = dto.Status;

            _repo.Update(entity);
            _repo.Save();
            return true;
        }

        public bool ToggleStatus(int id)
        {
            var entity = _repo.GetById(id);
            if (entity == null) return false;

            entity.Status = (entity.Status == Constants.Active) ? 0 : Constants.Active;

            _repo.Update(entity);
            _repo.Save();
            return true;
        }
    }
}