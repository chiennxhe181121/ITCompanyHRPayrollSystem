using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Models;
using Microsoft.EntityFrameworkCore;

public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly HumanManagerContext _context;

    public LeaveRequestRepository(HumanManagerContext context)
    {
        _context = context;
    }

    public List<LeaveRequest> GetAll()
        => _context.LeaveRequests
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .ToList();

    public LeaveRequest? GetById(int id)
        => _context.LeaveRequests.Find(id);

    public void Update(LeaveRequest entity)
    {
        _context.LeaveRequests.Update(entity);
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}
