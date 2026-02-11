using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// HoangDH
namespace HumanResourcesManager.DAL.Repository
{
    public class PositionRepository : IPositionRepository
    {
        private readonly HumanManagerContext _context;

        public PositionRepository(HumanManagerContext context)
        {
            _context = context;
        }

        public IEnumerable<Position> GetAll()
        {
            //return _context.Positions;
            return _context.Positions
                           .Include(x => x.Employees)
                           .ToList();
        }

        public Position? GetById(int id)
        {
            return _context.Positions.FirstOrDefault(p => p.PositionId == id);
        }

        public void Add(Position position)
        {
            _context.Positions.Add(position);
        }

        public void Update(Position position)
        {
            _context.Positions.Update(position);
        }

        public int Count()
        {
            return _context.Positions.Count();
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }

}
