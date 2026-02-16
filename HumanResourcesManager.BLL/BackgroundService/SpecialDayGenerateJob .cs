using HumanResourcesManager.BLL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class SpecialDayGenerateJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public SpecialDayGenerateJob(IServiceProvider serviceProvider)
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

    private DateTime? _lastRunDate;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = GetVietnamNow();

            if (_lastRunDate != now.Date &&
                now.TimeOfDay >= new TimeSpan(0, 5, 0))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var service = scope.ServiceProvider
                        .GetRequiredService<IAttendanceService>();

                    service.GenerateSpecialDayAttendance(now.Date);
                }

                _lastRunDate = now.Date;
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
