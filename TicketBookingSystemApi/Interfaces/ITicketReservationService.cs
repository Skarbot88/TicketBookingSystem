using TicketBookingSystemApi.Services;

namespace TicketBookingSystemApi.Interfaces
{
    public interface ITicketReservationService
    {
        Task<ReservationResult> ReserveAsync(int eventId, string holderName);
    }
}
