using HumanResourcesManager.BLL.DTOs.Position;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;
using Microsoft.EntityFrameworkCore; 


// HoangDH
namespace HumanResourcesManager.BLL.Services
{
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _repo;


        public PositionService(IPositionRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<PositionListDTO> GetAll()
        {
            var data = _repo.GetAll();

            return data.Select(p => new PositionListDTO
            {
                PositionId = p.PositionId,
                PositionName = p.PositionName,
                Description = p.Description,
                BaseSalary = p.BaseSalary,
                Status = p.Status,
                EmployeeCount = p.Employees.Count(e => e.Status == Constants.Active)
            }).ToList();
        }

        public PositionCreateUpdateDTO? GetById(int id)
        {
            var p = _repo.GetById(id);
            if (p == null) return null;

            return new PositionCreateUpdateDTO
            {
                PositionId = p.PositionId,
                PositionName = p.PositionName,
                Description = p.Description,
                BaseSalary = p.BaseSalary,
                Status = p.Status 
            };
        }

        public bool Create(PositionCreateUpdateDTO dto)
        {
            if (_repo.GetAll().Any(p => p.PositionName.ToLower() == dto.PositionName.ToLower()))
                return false;

            var position = new Position
            {
                PositionName = dto.PositionName,
                Description = dto.Description,
                BaseSalary = dto.BaseSalary,
                Status = Constants.Active, 
                CreatedAt = DateTime.Now
            };

            _repo.Add(position);
            _repo.Save();
            return true;
        }

        public bool Update(PositionCreateUpdateDTO dto)
        {
            var position = _repo.GetById(dto.PositionId ?? 0);
            if (position == null) return false;

            position.PositionName = dto.PositionName;
            position.Description = dto.Description;
            position.BaseSalary = dto.BaseSalary;


            position.Status = dto.Status;

            position.UpdatedAt = DateTime.Now;

            _repo.Update(position);
            _repo.Save();
            return true;
        }

        //public void SoftDelete(int id)
        //{
        //    var position = _repo.GetById(id);
        //    if (position != null)
        //    {
        //        position.Status = Constants.Inactive;
        //        position.UpdatedAt = DateTime.Now;
        //        _repo.Update(position);
        //        _repo.Save();
        //    }
        //}
        public bool SoftDelete(int id) 
        {

            var hasActiveEmployee = _repo.GetAll()
                .Any(p => p.PositionId == id &&
                          p.Employees.Any(e => e.Status == Constants.Active));

            if (hasActiveEmployee)
            {
                return false; 
            }

            var position = _repo.GetById(id);
            if (position != null)
            {
                position.Status = Constants.Inactive;
                position.UpdatedAt = DateTime.Now;
                _repo.Update(position);
                _repo.Save();
                return true; 
            }

            return false; 
        }

        public void SetActive(int id)
        {
            var position = _repo.GetById(id);
            if (position != null)
            {
                position.Status = Constants.Active;
                position.UpdatedAt = DateTime.Now;
                _repo.Update(position);
                _repo.Save();
            }
        }

        public int Count() => _repo.Count();
    }
}