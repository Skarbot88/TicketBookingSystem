using TicketBookingSystemApi.Dtos;
using TicketBookingSystemApi.Interfaces;

namespace TicketBookingSystemApi.Services
{
    public class TicketReservationService(
    ITicketRepository ticketRepository,
    IEventRepository eventRepository,
    ILogger<TicketReservationService> logger) : ITicketReservationService
    {
        public async Task<ReservationResult> ReserveAsync(int eventId, string holderName)
        {
            if (!await eventRepository.ExistsAsync(eventId))
            {
                logger.LogWarning("Reserve failed - event {EventId} not found", eventId);
                return ReservationResult.Fail(ReservationError.EventNotFound);
            }

            var now = DateTime.UtcNow;
            var cutoff = now.AddMinutes(-10);

            var ticket = await ticketRepository.ReserveNextAvailableAsync(eventId, holderName, now, cutoff);

            if (ticket is null)
            {
                logger.LogInformation("Reserve failed - no tickets available for event {EventId}", eventId);
                return ReservationResult.Fail(ReservationError.NoTicketsAvailable);
            }

            logger.LogInformation("Reserved ticket {TicketId} for event {EventId} to {HolderName}", ticket.Id, eventId, holderName);

            var response = new TicketReservationResponse(
                ticket.Id,
                ticket.EventId,
                ticket.HolderName!,
                ticket.ReservedAt!.Value,
                ticket.Status.ToString());

            return ReservationResult.Ok(response);
        }
    }
}
