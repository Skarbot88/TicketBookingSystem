using TicketBookingSystemApi.Dtos;
using TicketBookingSystemApi.Interfaces;
using TicketBookingSystemApi.Repositories;
namespace TicketBookingSystemApi.Services
{
    public class TicketPurchaseService(
        ITicketRepository ticketRepository,
        ILogger<TicketPurchaseService> logger) : ITicketPurchaseService
    {
        public async Task<PurchaseResult> PurchaseAsync(int ticketId, string holderName)
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-10);
            var outcome = await ticketRepository.PurchaseAsync(ticketId, holderName, cutoff);

            if (!outcome.Success)
            {
                logger.LogInformation("Purchase failed for ticket {TicketId}: {Reason}", ticketId, outcome.FailureReason);
                return PurchaseResult.Fail(outcome.FailureReason!.Value);
            }

            logger.LogInformation("Ticket {TicketId} purchased by {HolderName}", ticketId, holderName);

            var response = new TicketPurchaseResponse(
                outcome.Ticket!.Id,
                outcome.Ticket.EventId,
                outcome.Ticket.HolderName!,
                outcome.Ticket.Status.ToString());

            return PurchaseResult.Ok(response);
        }
    }
}
