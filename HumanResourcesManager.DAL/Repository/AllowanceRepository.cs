using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// HoangDH
namespace HumanResourcesManager.DAL.Repository
{
    public class AllowanceRepository : IAllowanceRepository
    {
        private readonly HumanManagerContext _context;

        public AllowanceRepository(HumanManagerContext context)
        {
            _context = context;
        }

        public IEnumerable<Allowance> GetAll() => _context.Allowances.ToList();

        public Allowance? GetById(int id) => _context.Allowances.Find(id);

        public void Add(Allowance allowance) => _context.Allowances.Add(allowance);

        public void Update(Allowance allowance) => _context.Allowances.Update(allowance);

        public void Save() => _context.SaveChanges();
    }
}
