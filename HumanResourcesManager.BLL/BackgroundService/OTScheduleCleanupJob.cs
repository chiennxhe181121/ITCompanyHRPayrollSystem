using HumanResourcesManager.BLL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HumanResourcesManager.BLL.BackgroundService
{
    public class OTScheduleCleanupJob : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public OTScheduleCleanupJob(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<IOTScheduleService>();
                    
                    try
                    {
                        service.CancelExpiredSchedules();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in OTScheduleCleanupJob: {ex.Message}");
                    }
                }

                // Chạy mỗi 5 phút một lần để check & đóng/hủy các Lịch quá hạn
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
