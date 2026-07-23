using TicketBookingSystemApi.Models;
using TicketBookingSystemApi.Repositories;

namespace TicketBookingSystemApi.Interfaces
{
    public interface ITicketRepository
    {
        Task<Ticket?> ReserveNextAvailableAsync(int eventId, string holderName, DateTime now, DateTime cutoff);
        Task<TicketPurchaseOutcome> PurchaseAsync(int ticketId, string holderName, DateTime cutoff);
    }
}
