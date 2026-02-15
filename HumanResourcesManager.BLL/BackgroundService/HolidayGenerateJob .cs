using HumanResourcesManager.BLL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class HolidayGenerateJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public HolidayGenerateJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;

            // 00:05 mỗi ngày
            if (now.Hour == 0 && now.Minute == 5)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var service = scope.ServiceProvider
                        .GetRequiredService<IAttendanceService>();

                    service.GenerateHolidayAttendance();
                }

                // tránh chạy lặp trong cùng phút
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
