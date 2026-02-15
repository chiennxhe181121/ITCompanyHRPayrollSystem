using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Enum;
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
    public bool ExistsActiveRequest(int employeeId, DateTime fromDate, DateTime toDate)
    {
        return _context.LeaveRequests.Any(lr =>
            lr.EmployeeId == employeeId &&
            (lr.Status == RequestStatus.Pending || lr.Status == RequestStatus.Approved) &&

            // Check overlap
            lr.FromDate <= toDate &&
            lr.ToDate >= fromDate
        );
    }

    public void Update(LeaveRequest entity)
    {
        _context.LeaveRequests.Update(entity);
    }

    public void Add(LeaveRequest entity)
    {
        _context.LeaveRequests.Add(entity);
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}
