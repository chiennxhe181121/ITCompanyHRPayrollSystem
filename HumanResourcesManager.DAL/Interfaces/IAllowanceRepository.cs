using HumanResourcesManager.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// HoangDH
namespace HumanResourcesManager.DAL.Interfaces
{
    public interface IAllowanceRepository
    {
        IEnumerable<Allowance> GetAll();
        Allowance? GetById(int id);
        void Add(Allowance allowance);
        void Update(Allowance allowance);
        void Save();
    }
}
