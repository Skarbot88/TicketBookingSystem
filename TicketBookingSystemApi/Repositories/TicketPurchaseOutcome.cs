using global::TicketBookingSystemApi.Models;
using TicketBookingSystemApi.Models.enums;

namespace TicketBookingSystemApi.Repositories
{

    public class TicketPurchaseOutcome
    {
        public bool Success { get; }
        public Ticket? Ticket { get; }
        public PurchaseFailureReason? FailureReason { get; }

        private TicketPurchaseOutcome(bool success, Ticket? ticket, PurchaseFailureReason? reason)
        {
            Success = success;
            Ticket = ticket;
            FailureReason = reason;
        }

        public static TicketPurchaseOutcome Ok(Ticket ticket) => new(true, ticket, null);
        public static TicketPurchaseOutcome Fail(PurchaseFailureReason reason) => new(false, null, reason);
    }
}
