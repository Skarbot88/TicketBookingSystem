namespace TicketBookingSystemApi.Dtos
{
    /// <summary>Ticket availability summary for a single event.</summary>
    /// <param name="Id">The event's id.</param>
    /// <param name="Name">The event's display name.</param>
    /// <param name="StartsAt">UTC date/time the event starts.</param>
    /// <param name="TotalSeats">Total number of seats/tickets configured for the event.</param>
    /// <param name="AvailableCount">Tickets currently available to reserve, including reservations that have expired (older than 10 minutes).</param>
    /// <param name="ReservedCount">Tickets currently reserved and not yet expired.</param>
    /// <param name="SoldCount">Tickets that have been purchased.</param>
    public record EventSummaryResponse(
        int Id,
        string Name,
        DateTime StartsAt,
        int TotalSeats,
        int AvailableCount,
        int ReservedCount,
        int SoldCount);
}
