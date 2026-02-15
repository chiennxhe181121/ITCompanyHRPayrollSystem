using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.DAL.Repository
{
    public class LeaveTypeRepository : ILeaveTypeRepository
    {
        private readonly HumanManagerContext _context;

        public LeaveTypeRepository(HumanManagerContext context)
        {
            _context = context;
        }

        public LeaveType? GetById(int id)
        {
            return _context.LeaveTypes.Find(id);
        }

        public IEnumerable<LeaveType> GetAll()
        {
            return _context.LeaveTypes.ToList();
        }

        public void Add(LeaveType entity)
        {
            _context.LeaveTypes.Add(entity);
        }

        public void Update(LeaveType entity)
        {
            _context.LeaveTypes.Update(entity);
        }

        public void Delete(int id)
        {
            var entity = _context.LeaveTypes.Find(id);
            if (entity != null)
            {
                _context.LeaveTypes.Remove(entity);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
