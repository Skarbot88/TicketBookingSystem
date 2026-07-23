using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketBookingSystemApi.Data;
using TicketBookingSystemApi.Interfaces;
using TicketBookingSystemApi.Models;
using TicketBookingSystemApi.Models.enums;

namespace TicketBookingSystemApi.Repositories
{
    public class TicketRepository(TicketBookingDataContext db, IConfiguration configuration, ILogger<TicketRepository> logger) : ITicketRepository
    {
        private readonly int maxAttempts = int.TryParse(configuration["TicketReservation:MaxAtttempts"], out var tries) ? tries : 5; 
        public async Task<Ticket?> ReserveNextAvailableAsync(int eventId, string holderName, DateTime now, DateTime cutoff)
        {
            var triedIds = new HashSet<int>();

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                
                var candidateId = await db.Tickets
                    .Where(t => t.EventId == eventId
                        && !triedIds.Contains(t.Id)
                        && (t.Status == TicketStatus.Available
                            || (t.Status == TicketStatus.Reserved && t.ReservedAt < cutoff)))
                    .OrderBy(t => t.Id)
                    .Select(t => (int?)t.Id)
                    .FirstOrDefaultAsync();

                if (candidateId is null)
                {
                    return null;
                }

                
                var affected = await db.Tickets
                    .Where(t => t.Id == candidateId
                        && (t.Status == TicketStatus.Available
                            || (t.Status == TicketStatus.Reserved && t.ReservedAt < cutoff)))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(t => t.Status, TicketStatus.Reserved)
                        .SetProperty(t => t.HolderName, holderName)
                        .SetProperty(t => t.ReservedAt, now));

                if (affected == 1)
                {
                    return await db.Tickets.AsNoTracking().FirstAsync(t => t.Id == candidateId);
                }

                logger.LogInformation(
                    "Reserve attempt {Attempt} lost the race for ticket {TicketId} on event {EventId}, retrying",
                    attempt, candidateId, eventId);

                triedIds.Add(candidateId.Value);
            }

            return null;
 
        }

        public async Task<TicketPurchaseOutcome> PurchaseAsync(int ticketId, string holderName, DateTime cutoff)
        {
            var affected = await db.Tickets
                .Where(t => t.Id == ticketId
                    && t.Status == TicketStatus.Reserved
                    && t.HolderName == holderName
                    && t.ReservedAt >= cutoff)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, TicketStatus.Sold));

            if (affected == 1)
            {
                var sold = await db.Tickets.AsNoTracking().FirstAsync(t => t.Id == ticketId);
                return TicketPurchaseOutcome.Ok(sold);
            }

            // affected == 0: the atomic decision is already final. Everything below is
            // purely to figure out *why*, for a helpful error message - it doesn't
            // affect correctness.
            var ticket = await db.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket is null)
            {
                return TicketPurchaseOutcome.Fail(PurchaseFailureReason.TicketNotFound);
            }

            if (ticket.Status == TicketStatus.Reserved && ticket.ReservedAt < cutoff)
            {
                return TicketPurchaseOutcome.Fail(PurchaseFailureReason.Expired);
            }

            if (ticket.Status != TicketStatus.Reserved)
            {
                return TicketPurchaseOutcome.Fail(PurchaseFailureReason.NotReserved);
            }

            return TicketPurchaseOutcome.Fail(PurchaseFailureReason.WrongHolder);
        }
    }
}
