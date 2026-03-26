using HumanResourcesManager.BLL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class AttendanceFinalizeJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public AttendanceFinalizeJob(IServiceProvider serviceProvider)
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

            // 20:10 - 20: 15 mỗi ngày
            if (now.TimeOfDay >= new TimeSpan(20, 10, 0) &&
                now.TimeOfDay < new TimeSpan(20, 15, 0))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var service = scope.ServiceProvider
                        .GetRequiredService<IAttendanceService>();

                    service.FinalizeDailyAttendance(now);
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
