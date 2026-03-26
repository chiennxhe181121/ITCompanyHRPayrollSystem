using HumanResourcesManager.BLL.DTOs.Employee;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Interfaces;

namespace HumanResourcesManager.BLL.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _repo;

        public PayrollService(IPayrollRepository repo)
        {
            _repo = repo;
        }

        public EmployeePayrollViewDTO GetPayrolls(int employeeId, int page, int pageSize, int? month, int? year)
        {
            var query = _repo.GetQueryableByEmployee(employeeId);

            if (month.HasValue)
                query = query.Where(x => x.Month == month);

            if (year.HasValue)
                query = query.Where(x => x.Year == year);

            int totalRecords = query.Count();

            var data = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PayrollRowDTO
                {
                    PayrollId = x.PayrollId,
                    Month = x.Month,
                    Year = x.Year,
                    BasicSalary = x.BasicSalary,
                    TotalOT = x.TotalOT,
                    TotalAllowance = x.TotalAllowance,
                    MissingMinutesPenalty = x.MissingMinutesPenalty,
                    NetSalary = x.NetSalary,
                    CreatedDate = x.CreatedDate
                })
                .ToList();

            return new EmployeePayrollViewDTO
            {
                Records = data,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                SelectedMonth = month,
                SelectedYear = year
            };
        }
    }
}
