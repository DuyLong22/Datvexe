using BusTicketBooking.Controllers;
using BusTicketBooking.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class TicketCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public TicketCleanupService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BusDbContext>();
                var now = DateTime.Now;

                var expiredTickets = db.Tickets
                    .Where(t => t.Status == TicketStatus.Pending.ToString() && t.ExpireAt < now)
                    .ToList();

                if (expiredTickets.Any())
                {
                    foreach (var ticket in expiredTickets)
                        ticket.Status = TicketStatus.Cancelled.ToString();

                    db.SaveChanges();
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // kiểm tra mỗi phút
        }
    }
}
