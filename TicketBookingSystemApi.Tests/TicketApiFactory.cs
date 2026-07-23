using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketBookingSystemApi.Data;
using TicketBookingSystemApi.Models;

namespace TicketBookingSystemApi.Tests
{
    public class TicketApiFactory : WebApplicationFactory<Program>
    {
        //persist connection through the lifetime of this class
        private readonly SqliteConnection _connection = new("DataSource=:memory:;Cache=Shared");

        public TicketApiFactory() => _connection.Open();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<TicketBookingDataContext>));
                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<TicketBookingDataContext>(options => options.UseSqlite(_connection));
            });
        }

        public async Task<int> SeedEventAsync(params Ticket[] tickets)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TicketBookingDataContext>();

            var @event = new Event
            {
                Name = "Test Event",
                StartsAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = tickets.Length,
                Tickets = tickets.ToList()
            };

            db.Events.Add(@event);
            await db.SaveChangesAsync();

            return @event.Id;
        }

        public async Task<Event> SeedEventReturningEntityAsync(params Ticket[] tickets)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TicketBookingDataContext>();

            var @event = new Event
            {
                Name = "Test Event",
                StartsAt = DateTime.UtcNow.AddDays(1),
                TotalSeats = tickets.Length,
                Tickets = tickets.ToList()
            };

            db.Events.Add(@event);
            await db.SaveChangesAsync();

            return @event;
        }

        public Task<int> SeedEventWithSingleAvailableTicketAsync() =>
            SeedEventAsync(new Ticket { Status = TicketStatus.Available });

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _connection.Dispose();
        }
    }
}
