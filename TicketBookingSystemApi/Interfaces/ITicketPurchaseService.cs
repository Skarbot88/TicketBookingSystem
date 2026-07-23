using TicketBookingSystemApi.Repositories;

namespace TicketBookingSystemApi.Interfaces
{
    public interface ITicketPurchaseService
    {
        Task<PurchaseResult> PurchaseAsync(int ticketId, string holderName);
    }
}
