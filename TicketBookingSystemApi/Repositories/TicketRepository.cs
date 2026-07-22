using Microsoft.EntityFrameworkCore;
using TicketBookingSystemApi.Data;
using TicketBookingSystemApi.Interfaces;
using TicketBookingSystemApi.Models;

namespace TicketBookingSystemApi.Repositories
{
    public class TicketRepository(TicketBookingDataContext db) : ITicketRepository
    {
        public async Task<Ticket?> ReserveNextAvailableAsync(int eventId, string holderName, DateTime now, DateTime cutoff)
        {
            var ticket = await db.Tickets
                .Where(t => t.EventId == eventId &&
                    (t.Status == TicketStatus.Available ||
                     (t.Status == TicketStatus.Reserved && t.ReservedAt < cutoff)))
                .OrderBy(t => t.Id)
                .FirstOrDefaultAsync();

            if (ticket is null)
            {
                return null;
            }

            ticket.Status = TicketStatus.Reserved;
            ticket.HolderName = holderName;
            ticket.ReservedAt = now;

            await db.SaveChangesAsync();

            return ticket;
        }
    }
}
