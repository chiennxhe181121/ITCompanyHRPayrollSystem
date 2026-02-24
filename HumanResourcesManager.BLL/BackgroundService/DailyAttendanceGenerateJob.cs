using HumanResourcesManager.BLL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class DailyAttendanceGenerateJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public DailyAttendanceGenerateJob(IServiceProvider serviceProvider)
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = GetVietnamNow();

            // ⏰ 00:05 - 00:10 mỗi ngày
            if (now.TimeOfDay >= new TimeSpan(0, 5, 0) &&
                now.TimeOfDay < new TimeSpan(0, 10, 0))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var service = scope.ServiceProvider
                        .GetRequiredService<IAttendanceService>();

                    service.GenerateDailyAttendance(now.Date);
                }

                // tránh chạy nhiều lần trong 5 phút đó
                await Task.Delay(TimeSpan.FromMinutes(6), stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}