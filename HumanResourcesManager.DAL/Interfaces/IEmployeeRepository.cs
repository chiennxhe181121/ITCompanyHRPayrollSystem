using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.DAL.Interfaces
{
    public interface IEmployeeRepository
    {
        IEnumerable<Employee> GetAll();
        Employee? GetById(int id);
        Employee? GetByUserId(int id);
        bool ExistsByEmail(string email, int excludeUserId);
        bool ExistsByPhone(string phone, int excludeUserId);
        void Add(Employee employee);
        void Update(Employee employee);
        void SoftDelete(int id);
        void Save();
    }
}
