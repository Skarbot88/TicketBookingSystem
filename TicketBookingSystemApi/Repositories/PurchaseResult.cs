using global::TicketBookingSystemApi.Dtos;
using global::TicketBookingSystemApi.Models.enums;

namespace TicketBookingSystemApi.Repositories
{

    public class PurchaseResult
    {
        public bool Success { get; }
        public TicketPurchaseResponse? Ticket { get; }
        public PurchaseFailureReason? Error { get; }

        private PurchaseResult(bool success, TicketPurchaseResponse? ticket, PurchaseFailureReason? error)
        {
            Success = success;
            Ticket = ticket;
            Error = error;
        }

        public static PurchaseResult Ok(TicketPurchaseResponse ticket) => new(true, ticket, null);
        public static PurchaseResult Fail(PurchaseFailureReason error) => new(false, null, error);
    }
}
