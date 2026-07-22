using TicketBookingSystemApi.Models;

namespace TicketBookingSystemApi.Interfaces
{
    public interface IEventRepository
    {
        Task<Event?> GetWithTicketsAsync(int eventId);
    }
}
