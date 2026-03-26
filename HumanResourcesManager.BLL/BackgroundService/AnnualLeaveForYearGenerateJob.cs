using HumanResourcesManager.BLL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class AnnualLeaveGenerateJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public AnnualLeaveGenerateJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private DateTime GetVietnamNow()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
        );
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = GetVietnamNow();

            // 🎯 00:00 - 00:10 ngày 01/01 mỗi năm
            if (now.Month == 1 &&
                now.Day == 1 &&
                now.TimeOfDay < TimeSpan.FromMinutes(10))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var service = scope.ServiceProvider
                        .GetRequiredService<IAnnualLeaveBalanceService>();

                    service.GenerateAnnualLeaveForYear(now.Year);
                }

                // tránh chạy nhiều lần trong 10 phút đầu năm
                await Task.Delay(
                    TimeSpan.FromMinutes(15),
                    stoppingToken);
            }

            await Task.Delay(
                TimeSpan.FromSeconds(30),
                stoppingToken);
        }
    }
}