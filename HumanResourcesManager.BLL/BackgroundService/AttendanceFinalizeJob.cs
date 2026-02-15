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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;

            // 20:10 mỗi ngày
            if (now.Hour == 20 && now.Minute == 10)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var service = scope.ServiceProvider
                        .GetRequiredService<IAttendanceService>();

                    service.FinalizeDailyAttendance();
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
