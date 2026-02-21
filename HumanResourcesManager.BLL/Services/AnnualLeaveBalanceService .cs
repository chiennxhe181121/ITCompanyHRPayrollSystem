using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;

namespace HumanResourcesManager.BLL.Services
{
    public class AnnualLeaveBalanceService : IAnnualLeaveBalanceService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IAnnualLeaveBalanceRepositry _balanceRepo;

        public AnnualLeaveBalanceService(
            IEmployeeRepository employeeRepo,
            IAnnualLeaveBalanceRepositry balanceRepo)
        {
            _employeeRepo = employeeRepo;
            _balanceRepo = balanceRepo;
        }
        public double GetRemainingDays(int employeeId, int year)
        {
            return _balanceRepo.GetRemainingDaysAsync(employeeId, year).Result;
        }

        private DateTime GetVietnamNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
            );
        }

        public void GenerateAnnualLeaveForYear(int year)
        {
            var now = GetVietnamNow();

            var employees = _employeeRepo
                .GetAll()
                .Where(e => e.Status == Constants.Active)
                .ToList();

            foreach (var emp in employees)
            {
                // Nếu đã generate năm này rồi thì bỏ qua
                if (_balanceRepo.Exists(emp.EmployeeId, year))
                    continue;

                double carryOver = 0;

                // Lấy tối đa 3 năm gần nhất chưa expired
                var previousBalances = _balanceRepo
                    .GetAll()
                    .Where(x => x.EmployeeId == emp.EmployeeId
                                && x.Year < year
                                && !x.IsExpired)
                    .OrderByDescending(x => x.Year)
                    .Take(3)
                    .ToList();

                foreach (var prev in previousBalances)
                {
                    if (prev.RemainingDays > 0)
                    {
                        carryOver += prev.RemainingDays;
                    }
                }

                var entitled = Constants.AnnualLeavePerYear;

                var balance = new AnnualLeaveBalance
                {
                    EmployeeId = emp.EmployeeId,
                    Year = year,
                    EntitledDays = entitled,
                    UsedDays = 0,
                    RemainingDays = entitled + carryOver,
                    CreatedDate = now,
                    IsExpired = false
                };

                _balanceRepo.Add(balance);

                // Đánh dấu 3 năm cũ là expired
                foreach (var prev in previousBalances)
                {
                    prev.IsExpired = true;
                    _balanceRepo.Update(prev);
                }
            }

            _balanceRepo.Save();
        }
    }
}
