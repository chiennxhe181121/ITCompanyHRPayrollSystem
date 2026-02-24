using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.DAL.Interfaces
{
    public interface ILeaveTypeRepository
    {
        LeaveType? GetById(int id);
        IEnumerable<LeaveType> GetAll();
        void Add(LeaveType entity);
        void Update(LeaveType entity);
        void Delete(int id);
        void Save();
    }
}

