using TicketBookingSystemApi.Dtos;

namespace TicketBookingSystemApi.Services
{
    public enum ReservationError
    {
        EventNotFound,
        NoTicketsAvailable
    }

    public class ReservationResult
    {
        public bool Success { get; }
        public TicketReservationResponse? Ticket { get; }
        public ReservationError? Error { get; }

        private ReservationResult(bool success, TicketReservationResponse? ticket, ReservationError? error)
        {
            Success = success;
            Ticket = ticket;
            Error = error;
        }

        public static ReservationResult Ok(TicketReservationResponse ticket) => new(true, ticket, null);
        public static ReservationResult Fail(ReservationError error) => new(false, null, error);
    }
}
