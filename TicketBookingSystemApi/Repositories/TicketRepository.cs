using Microsoft.EntityFrameworkCore;
using TicketBookingSystemApi.Data;
using TicketBookingSystemApi.Interfaces;
using TicketBookingSystemApi.Models;
using TicketBookingSystemApi.Models.enums;

namespace TicketBookingSystemApi.Repositories
{
    public class TicketRepository(TicketBookingDataContext db) : ITicketRepository
    {
        public async Task<Ticket?> ReserveNextAvailableAsync(int eventId, string holderName, DateTime now, DateTime cutoff)
        {
            await using var transaction = await db.Database.BeginTransactionAsync();

            var candidate = await db.Tickets
                .FromSqlInterpolated($@"
                    SELECT TOP (1) Id, EventId, HolderName, Status, ReservedAt
                    FROM Tickets WITH (UPDLOCK, ROWLOCK, READPAST)
                    WHERE EventId = {eventId}
                      AND (Status = {(int)TicketStatus.Available}
                           OR (Status = {(int)TicketStatus.Reserved} AND ReservedAt < {cutoff}))
                    ORDER BY Id")
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (candidate is null)
            {
                return null;
            }

            await db.Tickets
                .Where(t => t.Id == candidate.Id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.Status, TicketStatus.Reserved)
                    .SetProperty(t => t.HolderName, holderName)
                    .SetProperty(t => t.ReservedAt, now));

            await transaction.CommitAsync();

            candidate.Status = TicketStatus.Reserved;
            candidate.HolderName = holderName;
            candidate.ReservedAt = now;
            return candidate;
        }

        public async Task<TicketPurchaseOutcome> PurchaseAsync(int ticketId, string holderName, DateTime cutoff)
        {
            await using var transaction = await db.Database.BeginTransactionAsync();

            var ticket = await db.Tickets
                .FromSqlInterpolated($@"
                    SELECT Id, EventId, HolderName, Status, ReservedAt
                    FROM Tickets WITH (UPDLOCK, ROWLOCK)
                    WHERE Id = {ticketId}")
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (ticket is null)
            {
                return TicketPurchaseOutcome.Fail(PurchaseFailureReason.TicketNotFound);
            }

            if (ticket.Status != TicketStatus.Reserved)
            {
                return TicketPurchaseOutcome.Fail(PurchaseFailureReason.NotReserved);
            }

            if (ticket.ReservedAt < cutoff)
            {
                return TicketPurchaseOutcome.Fail(PurchaseFailureReason.Expired);
            }

            if (ticket.HolderName != holderName)
            {
                return TicketPurchaseOutcome.Fail(PurchaseFailureReason.WrongHolder);
            }

            await db.Tickets
                .Where(t => t.Id == ticketId)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, TicketStatus.Sold));

            await transaction.CommitAsync();

            ticket.Status = TicketStatus.Sold;
            return TicketPurchaseOutcome.Ok(ticket);
        }
    }
}
