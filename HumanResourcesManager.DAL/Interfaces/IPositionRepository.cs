using HumanResourcesManager.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// HoangDH
namespace HumanResourcesManager.DAL.Interfaces
{
    public interface IPositionRepository
    {
        IEnumerable<Position> GetAll();
        Position? GetById(int id);
        void Add(Position position);
        void Update(Position position);
        int Count();
        void Save();
    }
}
