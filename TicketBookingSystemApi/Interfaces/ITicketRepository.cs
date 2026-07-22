using TicketBookingSystemApi.Models;

namespace TicketBookingSystemApi.Interfaces
{
    public interface ITicketRepository
    {
        Task<Ticket?> ReserveNextAvailableAsync(int eventId, string holderName, DateTime now, DateTime cutoff);
    }
}
