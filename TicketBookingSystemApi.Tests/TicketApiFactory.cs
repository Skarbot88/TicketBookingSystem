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
        private readonly string _connectionString =
            $"Data Source=TicketBookingTests_{Guid.NewGuid():N};Mode=Memory;Cache=Shared;Default Timeout=5";

        // Keep-alive only: SQLite drops a shared in-memory database once its last
        // connection closes. Never handed to the DbContext - requests get their own
        // connections so concurrent requests behave like independent real clients.
        private readonly SqliteConnection _keepAliveConnection;

        public TicketApiFactory()
        {
            _keepAliveConnection = new SqliteConnection(_connectionString);
            _keepAliveConnection.Open();
        }

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

                services.AddDbContext<TicketBookingDataContext>(options => options.UseSqlite(_connectionString));
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
            _keepAliveConnection.Dispose();
        }
    }
}
