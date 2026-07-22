using TicketBookingSystemApi.Dtos;
using TicketBookingSystemApi.Interfaces;
using TicketBookingSystemApi.Models;

namespace TicketBookingSystemApi.Services
{
    public class EventService(IEventRepository eventRepository) : IEventService
    {
        public async Task<EventSummaryResponse?> GetSummaryAsync(int eventId)
        {
            var @event = await eventRepository.GetWithTicketsAsync(eventId);
            if (@event is null)
            {
                return null;
            }

            var cutoff = DateTime.UtcNow.AddMinutes(-10);

            var available = @event.Tickets.Count(t =>
                t.Status == TicketStatus.Available ||
                (t.Status == TicketStatus.Reserved && t.ReservedAt < cutoff));

            var reserved = @event.Tickets.Count(t =>
                t.Status == TicketStatus.Reserved && t.ReservedAt >= cutoff);

            var sold = @event.Tickets.Count(t => t.Status == TicketStatus.Sold);

            return new EventSummaryResponse(
                @event.Id,
                @event.Name,
                @event.StartsAt,
                @event.TotalSeats,
                available,
                reserved,
                sold);
        }
    }
}