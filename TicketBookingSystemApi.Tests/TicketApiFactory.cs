using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using TicketBookingSystemApi.Data;
using TicketBookingSystemApi.Models;

namespace TicketBookingSystemApi.Tests
{
    public class TicketApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private string _connectionString = string.Empty;

        async Task IAsyncLifetime.InitializeAsync()
        {
            var container = await SharedSqlServerContainer.GetAsync();

            var connectionStringBuilder = new SqlConnectionStringBuilder(container.GetConnectionString())
            {
                InitialCatalog = $"TicketBookingTests_{Guid.NewGuid():N}"
            };
            _connectionString = connectionStringBuilder.ConnectionString;
        }

        async Task IAsyncLifetime.DisposeAsync() => await base.DisposeAsync();

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

                services.AddDbContext<TicketBookingDataContext>(options => options.UseSqlServer(_connectionString));
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
    }

    // start it for the test suit then keep it running for all tests, to avoid the overhead of starting/stopping a container for each test
    internal static class SharedSqlServerContainer
    {
        private static readonly Lazy<Task<MsSqlContainer>> LazyContainer = new(async () =>
        {
            // Same image as docker-compose.yml, kept in sync deliberately.
            var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
            await container.StartAsync();
            return container;
        });

        public static Task<MsSqlContainer> GetAsync() => LazyContainer.Value;
    }
}
