using Microsoft.EntityFrameworkCore;
using TicketBookingSystemApi.Models;

namespace TicketBookingSystemApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(TicketBookingDataContext db)
    {
        if (await db.Events.AnyAsync())
        {
            return;
        }

        var @event = new Event
        {
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