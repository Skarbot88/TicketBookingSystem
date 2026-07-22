using Microsoft.EntityFrameworkCore;
using TicketBookingSystemApi.Data;
using TicketBookingSystemApi.Models;

namespace TicketingApi.Data;

public static class DbSeeder
{
    // Runtime seeding rather than EF's HasData: HasData is baked into a migration
    // snapshot, which doesn't work well here since StartsAt is computed relative to
    // "now" every time (DateTime.UtcNow.AddDays(7)).
    public static async Task SeedAsync(TicketBookingDataContext db)
    {
        if (await db.Events.AnyAsync())
        {
            return;
        }

        var @event = new Event
        {
            Id = 1,
            Name = "Live Coding Lounge – Friday Night",
            StartsAt = DateTime.UtcNow.AddDays(7),
            TotalSeats = 50,
            Tickets = Enumerable.Range(1, 50)
                .Select(_ => new Ticket { Status = TicketStatus.Available })
                .ToList()
        };

        db.Events.Add(@event);
        await db.SaveChangesAsync();
    }
}