using TicketBookingSystemApi.Dtos;

namespace TicketBookingSystemApi.Interfaces
{
    public interface IEventService
    {
        Task<EventSummaryResponse?> GetSummaryAsync(int eventId);
    }
}
