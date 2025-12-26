using DistributedOrderSystem.Infrastructure.Data;
using DistributedOrderSystem.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DistributedOrderSystem.Workers
{
    public class OrderProcessingWorker : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<OrderProcessingWorker> _logger;

        public OrderProcessingWorker(IServiceProvider services, ILogger<OrderProcessingWorker> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var pendingOrders = await db.Orders
                    .Where(o => o.Status == OrderStatus.Pending)
                    .ToListAsync(stoppingToken);

                foreach (var order in pendingOrders)
                {
                    _logger.LogInformation($"Processing Order {order.Id}");
                    order.Status = OrderStatus.Processing;
                }

                await db.SaveChangesAsync();
                await Task.Delay(10000, stoppingToken); 
            }
        }
    }
}
